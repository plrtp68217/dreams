"""Prepare Unity audio from SoundDatabase without changing the external originals.

Usage: python AgentScripts/PrepareFastFoodAudio.py --ffmpeg PATH_TO_FFMPEG
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--ffmpeg', required=True)
    parser.add_argument('--source', type=Path, default=Path('C:/Users/moroz/Desktop/SoundDatabase'))
    args = parser.parse_args()
    project = Path(__file__).resolve().parent.parent
    output = project / 'Assets/Audio/FastFood'
    for folder in ('Ambience', 'Movement', 'Interactions', 'Kitchen'):
        (output / folder).mkdir(parents=True, exist_ok=True)
    manifest = []

    def ffmpeg(source, destination, options):
        subprocess.run([args.ffmpeg, '-hide_banner', '-loglevel', 'error', '-nostdin',
                        '-n', '-i', str(source), '-map', '0:a:0', '-vn',
                        *options, str(destination)], check=True)

    # Each file contains several recorded actions. Extract individual transients so
    # an animation event never starts an entire walk/run/jump recording.
    # Times were selected using FFmpeg silencedetect on the supplied recordings.
    transients = {
        'walk': ([0.36, 1.06, 1.76], 0.32),
        'run': ([0.15, 0.86, 1.53], 0.30),
        'jump': ([1.03, 2.30, 3.40], 0.36),
    }
    for source in sorted(args.source.iterdir()):
        if not source.is_file() or source.suffix.lower() not in ('.mp3', '.wav', '.mp4'):
            continue
        digest = hashlib.sha256(source.read_bytes()).hexdigest()
        action = next((key for key in transients if f'player_{key}_' in source.stem), None)
        if action:
            starts, duration = transients[action]
            for index, start in enumerate(starts, 1):
                destination = output / 'Movement' / f'fastfood_sfx_player_{action}_{index:02}.wav'
                trim = f'atrim=start={start}:duration={duration},asetpts=PTS-STARTPTS'
                analysis = subprocess.run([args.ffmpeg, '-hide_banner', '-nostdin', '-i', str(source),
                    '-map', '0:a:0', '-vn', '-af', trim + ',volumedetect', '-f', 'null', '-'],
                    capture_output=True, text=True, check=True)
                peak = float(re.search(r'max_volume: ([-\d.]+) dB', analysis.stderr)[1])
                gain = -6.0 - peak
                filters = (f'{trim},volume={gain:.2f}dB,afade=t=in:d=0.005,'
                           f'afade=t=out:st={duration - 0.035:.3f}:d=0.035')
                ffmpeg(source, destination, ['-af', filters, '-ac', '1', '-ar', '44100', '-c:a', 'pcm_s16le'])
                manifest.append(dict(source=source.name, output=str(destination.relative_to(project)),
                                     start=start, duration=duration, peak_gain_db=round(gain, 2), sha256=digest))
        else:
            folder = 'Ambience' if '_amb_' in source.name else ('Interactions' if '_item_pickup_' in source.name else 'Kitchen')
            suffix = '.ogg' if folder == 'Ambience' else ('.wav' if source.suffix == '.mp4' else source.suffix)
            destination = output / folder / (source.stem + suffix)
            if folder == 'Ambience':
                # Keep the four-minute stereo atmosphere compressed on disk and stream it in Unity.
                ffmpeg(source, destination, ['-af', 'afade=t=in:d=0.1,afade=t=out:st=239.9:d=0.1',
                                             '-c:a', 'libvorbis', '-q:a', '5'])
            elif source.suffix == '.mp4':
                ffmpeg(source, destination, ['-c:a', 'pcm_s16le', '-ar', '44100'])
            else:
                if destination.exists():
                    raise FileExistsError(destination)
                shutil.copyfile(source, destination)
            manifest.append(dict(source=source.name, output=str(destination.relative_to(project)), sha256=digest))
        if hashlib.sha256(source.read_bytes()).hexdigest() != digest:
            raise RuntimeError(f'External source changed: {source}')
    manifest_path = project / 'Logs/FastFoodAudioSession/preparation-manifest.json'
    manifest_path.parent.mkdir(parents=True, exist_ok=True)
    manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding='utf-8')
    print(f'Prepared {len(manifest)} audio clips; all external originals unchanged.')


if __name__ == '__main__':
    main()

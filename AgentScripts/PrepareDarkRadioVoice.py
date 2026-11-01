"""Extract complete short utterances at low-energy boundaries; never alter the source."""
import argparse
import array
import hashlib
import json
import math
from pathlib import Path
import wave


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--source', type=Path, default=Path(
        'C:/Users/moroz/Desktop/DEV/UNITY/DREAMS-DATABASES/VoiceDatabase/faceless_voice/03_Dark_Radio.wav'))
    args = parser.parse_args()
    # Selected from 5 ms RMS analysis: breath/syllable gaps, with 25–40 ms tails.
    # Keep connected syllables together rather than force equal-length slices.
    regions = [(0.580, 1.585), (1.585, 1.965), (3.800, 4.200), (4.200, 4.950),
               (4.950, 5.840), (7.375, 8.065), (8.065, 8.730), (10.185, 11.275),
               (11.305, 11.720), (14.255, 15.515), (17.415, 18.295),
               (18.345, 18.630), (18.755, 19.320)]
    project = Path(__file__).resolve().parent.parent
    prepare(args.source, project / 'Assets/Audio/Voices/FacelessDarkRadio', 'dark_radio', regions)


def prepare(source, output, prefix, regions):
    raw = source.read_bytes()
    digest = hashlib.sha256(raw).hexdigest()
    with wave.open(str(source)) as stream:
        assert stream.getnchannels() == 1 and stream.getsampwidth() == 2
        rate = stream.getframerate()
        samples = array.array('h', stream.readframes(stream.getnframes()))
    destinations = [output / f'{prefix}_{i:02}.wav' for i in range(1, len(regions) + 1)]
    if any(p.exists() for p in destinations):
        raise FileExistsError('Prepared clips already exist; refusing to overwrite them.')
    output.mkdir(parents=True, exist_ok=True)
    # One common gain preserves relative dynamics between fragments.
    gain = (10 ** (-6 / 20)) / (max(abs(x) for x in samples) / 32768)
    manifest = dict(source=str(source), sha256=digest, sample_rate=rate,
                    gain_db=20 * math.log10(gain), fragments=[])
    for destination, (start, end) in zip(destinations, regions):
        clip = samples[round(start * rate):round(end * rate)]
        fade = round(rate * 0.008)
        for i in range(len(clip)):
            envelope = min(1, i / fade, (len(clip) - 1 - i) / fade)
            clip[i] = round(clip[i] * gain * envelope)
        with wave.open(str(destination), 'wb') as stream:
            stream.setnchannels(1)
            stream.setsampwidth(2)
            stream.setframerate(rate)
            stream.writeframes(clip.tobytes())
        manifest['fragments'].append(dict(file=destination.name, start=start, end=end,
                                          duration=round(end-start, 3)))
    assert hashlib.sha256(source.read_bytes()).hexdigest() == digest
    (output / 'preparation.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    print(f'Prepared {len(regions)} mono fragments, {min(b-a for a,b in regions):.3f}'
          f'–{max(b-a for a,b in regions):.3f} s; source SHA-256 unchanged: {digest}')


if __name__ == '__main__':
    main()

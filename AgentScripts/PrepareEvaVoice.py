"""Prepare Eva's supplied recordings at quiet syllable boundaries; preserve originals."""
import argparse
import array
import hashlib
import json
import math
from pathlib import Path
import wave


# Seconds, selected from 5 ms RMS analysis. Keep the connected third take intact.
REGIONS = {
    '01': [(0.08, 0.60), (0.60, 1.15), (1.15, 1.96), (1.96, 2.965)],
    '02': [(0.05, 0.49), (0.49, 0.895), (0.895, 1.445), (1.445, 1.855), (1.855, 2.42)],
    '03': [(0.065, 2.05)],
    '05': [(0.07, 1.13), (1.13, 1.815)],
    '06': [(0.07, 0.435), (0.435, 0.83), (0.83, 1.14), (1.14, 1.60), (1.60, 1.975), (1.975, 2.79)],
    '08': [(0.06, 0.77)],
    '09': [(0.04, 0.425), (0.425, 0.785), (0.785, 1.065), (1.065, 1.515)],
    '11': [(0.06, 0.76), (0.76, 1.22), (1.22, 1.84)],
    '12': [(0.07, 1.025), (1.025, 1.88), (1.88, 2.335), (2.335, 2.925), (2.925, 3.305)],
}


def prepare(source):
    output = Path(__file__).resolve().parent.parent / 'Assets/Audio/Voices/EvaWarmLow'
    recordings = []
    for take, regions in REGIONS.items():
        path = source / f'Eva_Warm_Low_{take}.wav'
        digest = hashlib.sha256(path.read_bytes()).hexdigest()
        with wave.open(str(path), 'rb') as stream:
            assert stream.getnchannels() == 1 and stream.getsampwidth() == 2
            rate = stream.getframerate()
            samples = array.array('h', stream.readframes(stream.getnframes()))
        assert all(0 <= a < b <= len(samples) / rate for a, b in regions)
        recordings.append((path, digest, rate, samples, regions))
    destinations = [output / f'eva_warm_low_{take}_{i:02}.wav'
                    for take, regions in REGIONS.items() for i in range(1, len(regions) + 1)]
    if any(p.exists() for p in destinations) or (output / 'preparation.json').exists():
        raise FileExistsError('Prepared clips already exist; refusing to overwrite them.')
    output.mkdir(parents=True, exist_ok=True)
    gain = 10 ** (-6 / 20) / (max(abs(x) for _, _, _, s, _ in recordings for x in s) / 32768)
    manifest = dict(source_directory=str(source), gain_db=20 * math.log10(gain),
                    edge_fade_seconds=0.008, sources=[], fragments=[])
    for (take, _), (path, digest, rate, samples, regions) in zip(REGIONS.items(), recordings):
        manifest['sources'].append(dict(file=path.name, sha256=digest, sample_rate=rate))
        for index, (start, end) in enumerate(regions, 1):
            clip = samples[round(start * rate):round(end * rate)]
            fade = round(rate * 0.008)
            for i in range(len(clip)):
                clip[i] = round(clip[i] * gain * min(1, i / fade, (len(clip) - 1 - i) / fade))
            destination = output / f'eva_warm_low_{take}_{index:02}.wav'
            with wave.open(str(destination), 'wb') as stream:
                stream.setnchannels(1)
                stream.setsampwidth(2)
                stream.setframerate(rate)
                stream.writeframes(clip.tobytes())
            manifest['fragments'].append(dict(file=destination.name, source=path.name,
                                              start=start, end=end, duration=round(end-start, 3)))
        assert hashlib.sha256(path.read_bytes()).hexdigest() == digest
    (output / 'preparation.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    print(f"Prepared {len(destinations)} Eva fragments; all nine source SHA-256 hashes unchanged.")


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--source', type=Path, default=Path('C:/Users/moroz/Music/DREAMS/EvaSource'))
    prepare(parser.parse_args().source)

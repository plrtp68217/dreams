"""Make independent NPC footstep variants from the existing FastFood walking sounds."""
import array
import hashlib
import json
import math
from pathlib import Path
import wave


def main():
    root = Path(__file__).resolve().parent.parent
    sources = sorted((root / 'Assets/Audio/FastFood/Movement').glob('fastfood_sfx_player_walk_*.wav'))
    assert len(sources) == 3
    settings = [('Admin', .93, 3800, .12), ('FacelessMan', .82, 1800, .26)]
    paths = [root / f'Assets/Audio/FastFood/NpcFootsteps/{name}/fastfood_sfx_{name.lower()}_walk_{i:02}.wav'
             for name, *_ in settings for i in range(1, 4)]
    if any(p.exists() for p in paths):
        raise FileExistsError('Refusing to overwrite prepared NPC footsteps')
    manifest = dict(processing='Pitch resample, low-pass and low-frequency body, short edge fades; peak -8 dBFS', files=[])
    for name, ratio, cutoff, body in settings:
        prepared = []
        for source in sources:
            digest = hashlib.sha256(source.read_bytes()).hexdigest()
            with wave.open(str(source)) as stream:
                assert stream.getnchannels() == 1 and stream.getsampwidth() == 2
                rate = stream.getframerate()
                pcm = array.array('h', stream.readframes(stream.getnframes()))
            # Keep the impact and a short tail so every contact fits inside the walk cycle.
            count = min(len(pcm), round(rate * .25))
            alpha = 1 - math.exp(-2 * math.pi * cutoff / rate)
            bass_alpha = 1 - math.exp(-2 * math.pi * 220 / rate)
            filtered = bass = 0.0
            result = []
            for i in range(int((count - 1) / ratio)):
                pos = i * ratio
                lo = int(pos)
                x = pcm[lo] + (pcm[lo+1] - pcm[lo]) * (pos - lo)
                filtered += alpha * (x - filtered)
                bass += bass_alpha * (filtered - bass)
                result.append(filtered + body * bass)
            fade_in, fade_out = round(rate * .004), round(rate * .04)
            for i in range(len(result)):
                result[i] *= min(1, i / fade_in, (len(result) - 1 - i) / fade_out)
            prepared.append((source, digest, rate, result))
        # One gain per character keeps the original three variants' relative dynamics.
        gain = 32767 * 10**(-8/20) / max(abs(v) for *_, data in prepared for v in data)
        for i, (source, digest, rate, data) in enumerate(prepared, 1):
            target = root / f'Assets/Audio/FastFood/NpcFootsteps/{name}/fastfood_sfx_{name.lower()}_walk_{i:02}.wav'
            target.parent.mkdir(parents=True, exist_ok=True)
            result = array.array('h', (round(v * gain) for v in data))
            with wave.open(str(target), 'wb') as stream:
                stream.setnchannels(1)
                stream.setsampwidth(2)
                stream.setframerate(rate)
                stream.writeframes(result.tobytes())
            assert hashlib.sha256(source.read_bytes()).hexdigest() == digest
            manifest['files'].append(dict(source=source.relative_to(root).as_posix(), sha256=digest,
                output=target.relative_to(root).as_posix(), pitch_ratio=ratio, lowpass_hz=cutoff,
                low_body=body, gain_db=20*math.log10(gain), duration=len(result)/rate))
    (root / 'Assets/Audio/FastFood/NpcFootsteps/preparation.json').write_text(
        json.dumps(manifest, indent=2), encoding='utf-8')
    print('Prepared 3 Admin and 3 FacelessMan variants; original player clips unchanged.')


if __name__ == '__main__':
    main()

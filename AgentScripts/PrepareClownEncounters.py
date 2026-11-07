"""Import only authored idle/walk and prepare low-gap voice fragments; sources stay unchanged."""
import array
import hashlib
import json
import math
import sys
import wave
from pathlib import Path
from PIL import Image

sys.dont_write_bytecode = True
from PrepareDarkRadioVoice import prepare

SOURCE = Path('SourceAssets/Art/FastFood/animations/characters/clown')
OUTPUT = Path('Assets/Sprites/FastFoodService/Clown')
VOICE = Path('SourceAssets/Voice/clown_voice/02_Gruff_Stranger.wav')
VOICE_OUT = Path('Assets/Audio/Voices/ClownGruffStranger')


def voice_regions():
    with wave.open(str(VOICE)) as stream:
        assert stream.getnchannels() == 1 and stream.getsampwidth() == 2
        rate = stream.getframerate()
        samples = array.array('h', stream.readframes(stream.getnframes()))
    step = round(rate * .005)
    rms = [math.sqrt(sum(v*v for v in samples[i:i+step]) / len(samples[i:i+step])) / 32768
           for i in range(0, len(samples), step)]
    threshold = max(.006, max(rms) * .05)
    active = [i for i, energy in enumerate(rms) if energy >= threshold]
    groups = []
    start = previous = active[0]
    for i in active[1:]:
        if (i-previous)*.005 > .12:
            groups.append((max(0, start*.005-.02), min(len(samples)/rate, previous*.005+.04)))
            start = i
        previous = i
    groups.append((max(0,start*.005-.02), min(len(samples)/rate,previous*.005+.04)))
    regions = []
    for start, end in groups:
        while end-start > 1.0:
            left, right = round((start+.35)/.005), min(len(rms)-1, round(min(start+.85,end-.28)/.005))
            cut = min(range(left,right+1), key=lambda i:rms[i]) * .005
            regions.append((round(start,3),round(cut,3)))
            start = cut
        if end-start >= .25:
            regions.append((round(start,3),round(end,3)))
        elif regions and start-regions[-1][1] < .04 and end-regions[-1][0] < 1.2:
            regions[-1] = (regions[-1][0],round(end,3))
    return regions


def main():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    manifest = dict(source=SOURCE.as_posix(), frameSize=[185,185], fps=5, animations=[])
    for name in ['idle', 'walk']:
        path = SOURCE / f'clown_{name}.png'
        image = Image.open(path).convert('RGBA')
        assert image.size == (1480,185)
        for i in range(8):
            image.crop((i*185,0,(i+1)*185,185)).save(OUTPUT/f'{name}_{i:02}.png')
        manifest['animations'].append(dict(name=name,frames=8,sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
    manifest['reservedAnimation'] = 'clown_run.png is deliberately not imported or referenced; reserved for the final chase.'
    (OUTPUT/'source-manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    panel=Path('SourceAssets/Art/Subtitles/SubtitlesClownFastFood.png')
    (OUTPUT/'SubtitlesClownFastFood.png').write_bytes(panel.read_bytes())
    if not (VOICE_OUT/'preparation.json').exists():
        regions=voice_regions()
        prepare(VOICE,VOICE_OUT,'gruff_stranger',regions)
    print('Exported 8 idle + 8 walk frames; run excluded. Dialogue panel copied byte-for-byte.')


if __name__ == '__main__':
    main()

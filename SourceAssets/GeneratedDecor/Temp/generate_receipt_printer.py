import base64
import json
import os
from pathlib import Path

import requests
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT.parents[1]
PROMPT = '''One old thermal receipt printer, a small decorative counter prop for an abandoned fast-food restaurant in a moody 2D side-scrolling pixel-art game with a 320x180 native resolution. Strict native 32x32 pixel art, one-pixel grid. Front-facing orthographic elevation, a very narrow visible top surface, no isometric perspective. Compact squat rectangular dull charcoal and desaturated slate metal/plastic housing, one thin black horizontal paper slot, a short curling strip of aged cream receipt paper with just three tiny dark pixel dashes, one small faded burgundy button. Worn corners and two or three restrained scratches. Muted plum-brown darkest outlines, subtle warm cream upper-left edge highlights, two or three flat shadow tones per material. Match the supplied cashier background's small utilitarian appliances, low contrast, restrained detail and pixel density. Object about 26 pixels wide and 22 pixels high, centered with transparent padding. Single isolated complete object, transparent background, hard opaque pixels only. Use only the supplied twelve palette colors. No anti-aliasing, no gradients, no dithering, no glow, no cast shadow outside the silhouette, no scenery, no counter, no extra objects, no logo, no readable text.'''
PALETTE = ['#281122', '#3a1d25', '#482b33', '#1e2436', '#3e4555', '#484e5e', '#4d5464', '#716b5e', '#957d68', '#918e73', '#ece0c2', '#882c35']

def main():
    reference = PROJECT / 'Assets/Sprites/Backgrounds/FastFood/FastFoodTill/FastFoodTill - 1.png'
    payload = dict(description=PROMPT, image_size=dict(width=32, height=32), seed=5112026,
                   no_background=True, style_image=dict(image=dict(base64=base64.b64encode(reference.read_bytes()).decode()),
                   size=dict(width=320, height=180), usage_description='Style reference only: native pixel density, appliance shapes, dark outlines and flat shading; do not reproduce the room.'),
                   style_options=dict(color_palette=False, outline=True, detail=True, shading=True),
                   target_palette=dict(colors=PALETTE, max_colors=12, name='DREAMS cashier muted metal and cream'))
    (ROOT / 'Temp/request.json').write_text(json.dumps(payload, indent=2), encoding='utf-8')
    (ROOT / 'prompts.md').write_text('# Test: receipt printer\n\nProvider: PixelLab API, POST /v2/generate-image-v2.\n32x32 px; seed 5112026; no_background=true; max_colors=12.\nStyle reference: Assets/Sprites/Backgrounds/FastFood/FastFoodTill/FastFoodTill - 1.png (320x180).\nCopy outline, detail, shading; palette supplied separately.\n\n```text\n' + PROMPT + '\n```\n\nOne generation only. Remaining eleven items await approval.\n', encoding='utf-8')
    session = requests.Session()
    session.headers['Authorization'] = 'Bearer ' + os.environ['PIXELLAB_API_KEY']
    job_path = ROOT / 'Temp/job.json'
    if not job_path.exists():
        response = session.post('https://api.pixellab.ai/v2/generate-image-v2', json=payload, timeout=120)
        if response.status_code not in (200, 202):
            print('PixelLab HTTP', response.status_code)
            # Never log request headers or raw authentication error bodies.
            return
        job_path.write_text(json.dumps(response.json(), indent=2), encoding='utf-8')
    job = json.loads(job_path.read_text(encoding='utf-8'))
    job_id = job.get('background_job_id') or job.get('job_id') or job.get('id')
    if job_id:
        response = session.get('https://api.pixellab.ai/v2/background-jobs/' + job_id, timeout=60)
        print('Poll HTTP', response.status_code)
        if response.status_code != 200:
            return
        result = response.json()
        (ROOT / 'Temp/status.json').write_text(json.dumps(result, indent=2), encoding='utf-8')
        print('Status:', result.get('status'))
    else:
        result = job
    def find_image(value):
        if isinstance(value, dict):
            if 'base64' in value:
                return ('base64', value['base64'])
            for k, v in value.items():
                if isinstance(v, str) and v.startswith('https://') and (k in ('url', 'image_url') or '.png' in v):
                    return ('url', v)
            for v in value.values():
                found = find_image(v)
                if found:
                    return found
        elif isinstance(value, list):
            for v in value:
                found = find_image(v)
                if found:
                    return found
    found = find_image(result.get('last_response', result))
    if found:
        kind, data = found
        raw = base64.b64decode(data.split(',')[-1]) if kind == 'base64' else requests.get(data, timeout=60).content
        raw_path = ROOT / 'Temp/receipt_printer_raw.png'
        raw_path.write_bytes(raw)
        im = Image.open(raw_path).convert('RGBA')
        im.save(ROOT / 'Cashier/receipt_printer.png')
        colors = {p[:3] for p in im.get_flattened_data() if p[3]}
        print('Saved PNG:', im.size, 'visible colors:', len(colors), 'alpha:', sorted(set(im.getchannel('A').get_flattened_data())))

if __name__ == '__main__':
    main()

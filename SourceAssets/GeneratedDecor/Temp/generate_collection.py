"""Resume-safe PixelLab generation. Credentials only from environment; never logged."""
import base64
import concurrent.futures
import json
import os
import time
from pathlib import Path

import requests
from PIL import Image, ImageDraw
from generate_receipt_printer import PALETTE

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT.parents[1]
ITEMS = [
    ('paper_cups', 16, 24, 'One nested stack of five old disposable paper cups. Tapered aged cream paper body, thin faded burgundy horizontal stripe, five clearly stepped rims, no lid. Narrow vertical silhouette.'),
    ('napkin_holder', 24, 24, 'One old squat rectangular metal napkin dispenser. Desaturated slate metal sides, dark front rectangular opening showing folded cream paper napkins, slightly worn top edge. Compact counter appliance.'),
    ('straw_container', 16, 24, 'One small open rectangular burgundy countertop holder containing six thin cream drinking straws of slightly uneven heights. Readable individual straight one-pixel straws, one or two bent ends. No cups.'),
    ('service_phone', 32, 24, 'One old corded service telephone. Dark desaturated slate rectangular base with a small simple keypad, chunky receiver resting horizontally on top, a short curled cord attached to the right side. No display, no readable text.'),
    ('wall_clock', 24, 24, 'One old circular wall clock, seen perfectly straight on. Thin dark burgundy rim, aged cream face, sparse dark hour ticks, two clear dark clock hands at ten past ten. No numerals, no glass glare, no pendulum.'),
    ('paper_bag_box', 32, 24, 'One low open worn cardboard storage box containing a neat upright stack of folded flat kraft paper takeaway bags. Warm muted brown cardboard, darker folded edges and a few cream bag tops visible. No handles, no food, no lettering.'),
    ('receipt_paper_rolls', 24, 16, 'Three small thermal receipt paper rolls grouped tightly as one prop, two at the bottom and one slightly behind. Aged cream paper cylinders, visible dark small circular cardboard cores, very restrained warm shadow. No loose trailing sheets.'),
    ('coin_container', 24, 16, 'One shallow rectangular old metal change tray with five tiny dull cream and brown coins inside. Dark slate raised rim and dark interior. Coins are sparse readable two-to-three-pixel clusters, no sparkling gold, no cash register.'),
    ('cashier_closed_sign', 80, 32, 'One old horizontal rectangular countertop closed sign, perfectly front facing. Muted dark burgundy frame, aged cream inset panel. The only text is the exact Russian Cyrillic phrase "КАССА ЗАКРЫТА", on two centered lines, "КАССА" above "ЗАКРЫТА", in crisp dark simple block pixel letters. Spell each Cyrillic letter correctly. Large legible text with empty margins. A narrow dark base integrated below the panel, no other objects.'),
    ('old_calendar', 32, 40, 'One old small rectangular tear-off wall calendar. Faded burgundy header, two tiny dark binding rings, aged cream paper sheet, sparse dark 7-column calendar grid with tiny block date marks, lower right paper corner slightly curled. No readable words, no illustration, no wall.'),
    ('employee_instructions', 40, 48, 'One old single rectangular employee instruction notice pinned flat. Aged cream paper with dark plum border and two tiny burgundy pin heads at upper corners. One short burgundy header bar, three clearly separated numbered steps represented by small dark squares and short dark horizontal pixel lines, one tiny simple caution triangle near bottom. Abstract writing marks only, no readable words, no illustration, no frame, no wall.'),
]
COMMON = (' Strict native-resolution pixel art for DREAMS, a moody abandoned fast-food restaurant in a 320x180 2D side-scroller. '
          'Match the approved receipt printer and cashier background references: one-pixel grid, front-facing orthographic elevation with only a narrow top edge visible, no isometric view. '
          'Dark plum-brown one-pixel outlines, desaturated slate metal, aged cream paper, faded burgundy accents, subtle warm upper-left edge highlights. '
          'Only 2 or 3 flat tones per material, low contrast, sparse wear, chunky clean readable clusters. '
          'One isolated complete prop centered within the canvas with 1 or 2 transparent pixels of padding. '
          'Use only the supplied twelve colors. Fully transparent background, hard opaque pixels. '
          'No anti-aliasing, gradients, dithering, blur, glow, external cast shadow, scenery, characters, watermarks, logos or extra objects. ')

def reference(path, usage):
    image = Image.open(path)
    return dict(image=dict(base64=base64.b64encode(path.read_bytes()).decode()),
                size=dict(width=image.width, height=image.height), usage_description=usage)

def find_image(value):
    if isinstance(value, dict):
        if isinstance(value.get('base64'), str):
            return ('base64', value['base64'])
        for key, val in value.items():
            if isinstance(val, str) and val.startswith('https://') and (key in ('url', 'image_url') or '.png' in val):
                return ('url', val)
        for val in value.values():
            found = find_image(val)
            if found:
                return found
    if isinstance(value, list):
        for val in value:
            found = find_image(val)
            if found:
                return found

def generate(item):
    name, width, height, subject = item
    folder = ROOT / 'Temp' / name
    folder.mkdir(exist_ok=True)
    final = ROOT / 'Cashier' / (name + '.png')
    if final.exists():
        return name, 'already saved'
    prompt = subject + COMMON + f'Canvas is exactly {width} by {height} native pixels.'
    payload = dict(description=prompt, image_size=dict(width=width, height=height), seed=5112026 + ITEMS.index(item) + 1,
                   no_background=True, style_image=reference(ROOT / 'Cashier/receipt_printer.png', 'Approved style reference: match pixel density, outline, metal and paper rendering; create the requested different prop.'),
                   reference_images=[reference(PROJECT / 'Assets/Sprites/Backgrounds/FastFood/FastFoodTill/FastFoodTill - 1.png', 'Environment style only; do not draw this room.')],
                   style_options=dict(color_palette=False, outline=True, detail=True, shading=True),
                   target_palette=dict(colors=PALETTE, max_colors=12, name='DREAMS cashier muted metal and cream'))
    (folder / 'request.json').write_text(json.dumps(payload, indent=2), encoding='utf-8')
    session = requests.Session()
    session.headers['Authorization'] = 'Bearer ' + os.environ['PIXELLAB_API_KEY']
    job_path = folder / 'job.json'
    if not job_path.exists():
        # Mark before submission so an ambiguous timeout cannot silently cause another paid request.
        marker = folder / 'submitted.txt'
        if marker.exists():
            return name, 'submission unresolved; no automatic repeat'
        marker.write_text('One paid request submitted. Do not retry automatically.', encoding='utf-8')
        response = session.post('https://api.pixellab.ai/v2/generate-image-v2', json=payload, timeout=120)
        if response.status_code not in (200, 202):
            (folder / 'http_error.txt').write_text(str(response.status_code), encoding='utf-8')
            return name, 'HTTP ' + str(response.status_code)
        job_path.write_text(json.dumps(response.json(), indent=2), encoding='utf-8')
    job = json.loads(job_path.read_text(encoding='utf-8'))
    job_id = job.get('background_job_id') or job.get('job_id') or job.get('id')
    for attempt in range(90):
        result = job
        if job_id:
            response = session.get('https://api.pixellab.ai/v2/background-jobs/' + job_id, timeout=60)
            if response.status_code != 200:
                print(name, 'poll HTTP', response.status_code, flush=True)
                time.sleep(10)
                continue
            result = response.json()
            (folder / 'status.json').write_text(json.dumps(result, indent=2), encoding='utf-8')
        found = find_image(result.get('last_response', result))
        if found:
            kind, value = found
            if kind == 'base64':
                raw = base64.b64decode(value.split(',')[-1])
            else:
                download = requests.get(value, timeout=60)
                download.raise_for_status()
                raw = download.content
            raw_path = folder / 'raw.png'
            raw_path.write_bytes(raw)
            image = Image.open(raw_path).convert('RGBA')
            assert image.size == (width, height), 'Unexpected output dimensions'
            colors = {'#%02x%02x%02x' % pixel[:3] for pixel in image.get_flattened_data() if pixel[3]}
            alpha = sorted(set(image.getchannel('A').get_flattened_data()))
            assert alpha == [0, 255], 'Unexpected alpha'
            assert colors <= set(PALETTE), 'Unexpected palette colors'
            assert len(colors) <= 12
            image.save(final)
            validation = dict(size=list(image.size), visible_colors=len(colors), colors=sorted(colors), alpha=alpha, bbox=image.getbbox())
            (folder / 'validation.json').write_text(json.dumps(validation, indent=2), encoding='utf-8')
            return name, 'saved; ' + str(len(colors)) + ' colors'
        if result.get('status') == 'failed':
            return name, 'failed'
        if attempt % 6 == 0:
            print(name, result.get('status', 'waiting'), flush=True)
        time.sleep(10)
    return name, 'pending; resume to poll existing job'

def docs():
    path = ROOT / 'prompts.md'
    original = path.read_text(encoding='utf-8').split('\n# Approved collection')[0]
    original = original.replace('One generation only. Remaining eleven items await approval.', 'Test approved by user; remaining eleven items authorized on 2026-11-05.')
    text = original + '\n# Approved collection\n\nProvider: PixelLab POST /v2/generate-image-v2. One generation per item.\nTransparent background; twelve-color palette in palette.md; no post-generation scaling or palette changes.\nStyle image: approved 32x32 receipt printer. Environment reference: original 320x180 cashier background.\nstyle_options: color_palette=false, outline=true, detail=true, shading=true.\n\n'
    for index, (name, width, height, subject) in enumerate(ITEMS):
        text += f'## {name}.png\n\nSize: {width}x{height}; seed: {5112027 + index}; max_colors: 12.\n\n```text\n{subject + COMMON}Canvas is exactly {width} by {height} native pixels.\n```\n\n'
    path.write_text(text, encoding='utf-8')

def preview():
    files = [ROOT / 'Cashier/receipt_printer.png'] + [ROOT / 'Cashier' / (item[0] + '.png') for item in ITEMS]
    canvas = Image.new('RGB', (960, 720), '#281122')
    draw = ImageDraw.Draw(canvas)
    for index, path in enumerate(files):
        x, y = (index % 4) * 240, (index // 4) * 240
        for cy in range(y + 24, y + 216, 8):
            for cx in range(x + 8, x + 232, 8):
                draw.rectangle((cx, cy, cx + 7, cy + 7), fill=('#482b33' if (cx // 8 + cy // 8) % 2 else '#3a1d25'))
        if path.exists():
            im = Image.open(path).convert('RGBA')
            scale = min(4, 208 // im.width, 180 // im.height)
            scaled = im.resize((im.width * scale, im.height * scale), Image.Resampling.NEAREST)
            canvas.paste(scaled, (x + (240 - scaled.width) // 2, y + 24 + (192 - scaled.height) // 2), scaled)
            draw.text((x + 10, y + 8), path.stem, fill='#ece0c2')
            draw.text((x + 10, y + 223), f'{im.width}x{im.height} / {scale}x preview', fill='#ece0c2')
    canvas.save(ROOT / 'preview.png')

if __name__ == '__main__':
    docs()
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as executor:
        futures = [executor.submit(generate, item) for item in ITEMS]
        for future in concurrent.futures.as_completed(futures):
            try:
                print(*future.result(), flush=True)
            except Exception as error:
                print('Generation error:', type(error).__name__, flush=True)
    preview()

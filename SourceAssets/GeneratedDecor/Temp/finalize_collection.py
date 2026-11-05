import hashlib
import json
import shutil
from pathlib import Path

from PIL import Image
from generate_collection import ITEMS, PALETTE, preview

root = Path(__file__).resolve().parents[1]
selection_path = root / 'Temp/selections.json'
selections = json.loads(selection_path.read_text())
selections['employee_instructions'] = 5
selections['straw_container'] = 34
selections['wall_clock'] = 35
selection_path.write_text(json.dumps(selections, indent=2), encoding='utf-8')
for name, index in selections.items():
    shutil.copyfile(root / 'Temp' / name / ('candidate_%02d.png' % index), root / 'Cashier' / (name + '.png'))

expected = {'receipt_printer': (32, 32)}
expected.update({name: (width, height) for name, width, height, _ in ITEMS})
manifest = []
for name, size in expected.items():
    path = root / 'Cashier' / (name + '.png')
    image = Image.open(path)
    assert image.format == 'PNG' and image.mode == 'RGBA'
    assert image.size == size
    alpha = sorted(set(image.getchannel('A').get_flattened_data()))
    assert alpha == [0, 255]
    colors = {'#%02x%02x%02x' % pixel[:3] for pixel in image.get_flattened_data() if pixel[3]}
    assert 1 < len(colors) <= 12 and colors <= set(PALETTE)
    bbox = image.getbbox()
    assert bbox and image.getchannel('A').getextrema() == (0, 255)
    record = dict(file=path.name, size=list(size), bbox=bbox, opaque_colors=len(colors),
                  colors=sorted(colors), alpha_values=alpha,
                  selected_candidate=selections.get(name, 0),
                  sha256=hashlib.sha256(path.read_bytes()).hexdigest())
    manifest.append(record)
    if name != 'receipt_printer':
        (root / 'Temp' / name / 'validation.json').write_text(json.dumps(record, indent=2), encoding='utf-8')
    print(name, size, len(colors), 'colors; PASS')

(root / 'Temp/collection-validation.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
preview()

# A second contact sheet demonstrates the actual unscaled pixel density at the game's native resolution.
native = Image.new('RGBA', (320, 180), '#281122')
positions = [(8, 12), (52, 16), (80, 16), (116, 16), (148, 16), (192, 16),
             (236, 16), (276, 16), (8, 88), (48, 90), (156, 76), (216, 80)]
for (name, _), position in zip(expected.items(), positions):
    native.alpha_composite(Image.open(root / 'Cashier' / (name + '.png')).convert('RGBA'), position)
native.save(root / 'Temp/native_scale_320x180.png')

prompts_path = root / 'prompts.md'
text = prompts_path.read_text(encoding='utf-8').split('\n# Final selection and checks')[0]
text += '\n# Final selection and checks\n\nUser approved continuation on 2026-11-05. Eleven additional paid generation requests, one per new prop; no paid rerolls. PixelLab returned multiple candidates per request (16–64); all were extracted to Temp/<name>/candidate_XX.png. Rejected candidates remain there.\n\nApproved printer unchanged. The selected PNGs were copied from API candidates without drawing, scaling, recoloring or alpha modification.\n\n| File | Size | Opaque colors | Candidate |\n|---|---|---|---|\n'
for item in manifest:
    text += f"| {item['file']} | {item['size'][0]}x{item['size'][1]} | {item['opaque_colors']} | {item['selected_candidate']} |\n"
text += '\nAll twelve final PNGs: RGBA, expected dimensions, only the specified palette, at most twelve visible colors, binary alpha 0/255. Some sprites fit tightly against the canvas edge; no extra padding was added.\nVisual review: frontal pixel-art silhouettes, consistent metal/paper shading; no stray lower fragments in selected straw holder; a single connected clock silhouette without detached fragments. Closed sign reads «КАССА ЗАКРЫТА». Employee instructions use schematic writing marks; calendar uses a date grid.\n\npreview.png is the final twelve-item contact sheet with nearest-neighbor enlargement. Temp/native_scale_320x180.png shows all props at 1:1 native pixel size. Full validation and SHA-256 hashes: Temp/collection-validation.json. Unity files were not changed or imported; no commits created.\n'
prompts_path.write_text(text, encoding='utf-8')

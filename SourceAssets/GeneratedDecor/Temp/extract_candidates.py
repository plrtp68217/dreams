import base64
import io
import json
from pathlib import Path
from PIL import Image, ImageDraw

root = Path(__file__).resolve().parent
for folder in root.iterdir():
    if not folder.is_dir() or not (folder / 'status.json').exists():
        continue
    result = json.loads((folder / 'status.json').read_text())
    images = (result.get('last_response') or {}).get('images', [])
    if not images:
        continue
    sheet = Image.new('RGB', (1024, ((len(images) + 7) // 8) * 144), '#281122')
    draw = ImageDraw.Draw(sheet)
    for index, data in enumerate(images):
        im = Image.open(io.BytesIO(base64.b64decode(data['base64'].split(',')[-1]))).convert('RGBA')
        im.save(folder / ('candidate_%02d.png' % index))
        scale = min(4, 120 // im.width, 116 // im.height)
        scaled = im.resize((im.width * scale, im.height * scale), Image.Resampling.NEAREST)
        x, y = (index % 8) * 128, (index // 8) * 144
        sheet.paste(scaled, (x + (128 - scaled.width) // 2, y + 20 + (116 - scaled.height) // 2), scaled)
        draw.text((x + 4, y + 4), str(index), fill='#ece0c2')
    sheet.save(folder / 'candidates.png')
    print(folder.name, len(images), 'candidates extracted')

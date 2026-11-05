import base64
import io
import json
from pathlib import Path
from PIL import Image, ImageDraw

root = Path(__file__).resolve().parent
names = ['paper_cups', 'napkin_holder', 'straw_container']
out = Image.new('RGB', (768, 384), '#281122')
draw = ImageDraw.Draw(out)
for row, name in enumerate(names):
    imgs = json.loads((root / name / 'status.json').read_text())['last_response']['images']
    print(name, len(imgs), 'unique', len({i['base64'] for i in imgs}))
    for col, index in enumerate([0, 1, len(imgs) // 2, len(imgs) - 1]):
        im = Image.open(io.BytesIO(base64.b64decode(imgs[index]['base64'].split(',')[-1]))).convert('RGBA')
        im.save(root / name / ('candidate_%02d.png' % index))
        scaled = im.resize((im.width * 4, im.height * 4), Image.Resampling.NEAREST)
        out.paste(scaled, (col * 192 + 40, row * 128 + 20), scaled)
        draw.text((col * 192 + 4, row * 128 + 4), name + ' #' + str(index), fill='#ece0c2')
out.save(root / 'candidate_inspection.png')

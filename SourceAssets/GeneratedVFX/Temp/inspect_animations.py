import base64
import io
import json
from PIL import Image, ImageDraw
from generate_tests import ROOT, TESTS, write

for name,cfg in TESTS.items():
    folder=ROOT/'Temp'/name/'animation'
    result=json.loads((folder/'result.json').read_text(encoding='utf-8'))
    images=result['images']
    sheet=Image.new('RGB',(len(images)*144,200),'#281122')
    draw=ImageDraw.Draw(sheet)
    info=[]
    for index,data in enumerate(images):
        image=Image.open(io.BytesIO(base64.b64decode(data['base64'].split(',')[-1]))).convert('RGBA')
        image.save(folder/f'raw_frame_{index:03d}.png')
        scale=min(4,136//image.width,176//image.height)
        scaled=image.resize((image.width*scale,image.height*scale),Image.Resampling.NEAREST)
        x=index*144
        sheet.paste(scaled,(x+(144-scaled.width)//2,24+(176-scaled.height)//2),scaled)
        draw.text((x+4,4),str(index),fill='#ece0c2')
        colors={p[:3] for p in image.get_flattened_data() if p[3]}
        alpha=sorted(set(image.getchannel('A').get_flattened_data()))
        info.append(dict(index=index,size=image.size,bbox=image.getbbox(),colors=len(colors),alpha=alpha))
    sheet.save(folder/'raw_contact.png')
    write(folder/'raw_info.json',info)
    print(name,info)

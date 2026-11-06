"""Inspect/export the authored drink vending machine; source files are read only."""
import hashlib
import json
import struct
import zlib
from pathlib import Path
from PIL import Image, ImageChops, ImageFilter, ImageDraw

SOURCE = Path('SourceAssets/Art/FastFood/art/guests/01_faceless_man/03_drink/drink_vending.aseprite')
OUTPUT = Path('Assets/Sprites/FastFoodService/DrinkVending')
LOG = Path('Logs/DrinkVending')

def read(source=SOURCE):
    data = source.read_bytes()
    count, width, height, depth = struct.unpack_from('<4H', data, 6)
    assert depth == 32
    layers, frames, durations = [], [], []
    offset = 128
    for frame in range(count):
        size, magic, chunks, duration = struct.unpack_from('<IHHH', data, offset)
        cursor, cells = offset + 16, {}
        durations.append(duration)
        for _ in range(chunks):
            length, kind = struct.unpack_from('<IH', data, cursor)
            body = data[cursor + 6:cursor + length]
            if kind == 0x2004:
                flags, typ, child, w, h, blend, opacity = struct.unpack_from('<6HB', body)
                name_length = struct.unpack_from('<H', body, 16)[0]
                layers.append(dict(name=body[18:18+name_length].decode('utf8'), flags=flags,
                                   type=typ, child=child, blend=blend, opacity=opacity))
            elif kind == 0x2005:
                layer, x, y, opacity, cel_kind = struct.unpack_from('<HhhBH', body)
                if cel_kind in (0, 2):
                    w, h = struct.unpack_from('<HH', body, 16)
                    raw = body[20:] if cel_kind == 0 else zlib.decompress(body[20:])
                    cel = Image.frombytes('RGBA', (w, h), raw)
                elif cel_kind == 1:
                    link = struct.unpack_from('<H', body, 16)[0]
                    cel = frames[link][layer][2].copy()
                else:
                    raise ValueError(cel_kind)
                cells[layer] = (x, y, cel, opacity)
            cursor += length
        frames.append(cells)
        offset += size
    return data, width, height, layers, frames, durations

def main():
    data, width, height, layers, frames, durations = read()
    def compose(frame, indices):
        result = Image.new('RGBA', (width, height))
        for index in indices:
            if index in frames[frame]:
                x, y, cel, opacity = frames[frame][index]
                cel = cel.copy()
                alpha = opacity * layers[index]['opacity'] / 255
                if alpha != 255:
                    cel.putalpha(cel.getchannel('A').point(lambda a: round(a*alpha/255)))
                result.alpha_composite(cel, (x, y))
        return result
    LOG.mkdir(parents=True, exist_ok=True)
    print('Canvas', width, height, 'frames', len(frames), 'durations', durations)
    sheet = Image.new('RGB', (width*3, (height+24)*((len(layers)+2)//3)), '#23232b')
    draw = ImageDraw.Draw(sheet)
    for i, layer in enumerate(layers):
        nonempty = [(f,frames[f][i][:2],frames[f][i][2].size) for f in range(len(frames)) if i in frames[f] and frames[f][i][2].getbbox()]
        print(i, layer, 'cels', nonempty)
        f = next((f for f in range(len(frames)) if i in frames[f] and frames[f][i][2].getbbox()),0)
        image = compose(f,[i])
        sheet.paste(image,((i%3)*width,(i//3)*(height+24)+24),image)
        draw.text(((i%3)*width+3,(i//3)*(height+24)+4),f'{i}: {layer["name"]} frame {f}',fill='white')
    sheet.save(LOG/'layers.png')
    composite=compose(0,[i for i,l in enumerate(layers) if l['flags']&1])
    composite.save(LOG/'frame0.png')
    for i in [0,5,6]:
        compose(0,[i]).resize((1280,720),Image.Resampling.NEAREST).save(LOG/f'layer{i}.png')
    sheet=Image.new('RGB',(320*4,204*6),'#23232b')
    draw=ImageDraw.Draw(sheet)
    for f in range(len(frames)):
        im=compose(f,list(range(len(layers))))
        sheet.paste(im,((f%4)*320,(f//4)*204+24),im)
        draw.text(((f%4)*320+3,(f//4)*204+4),f'Frame {f}',fill='white')
    sheet.save(LOG/'animation.png')
    OUTPUT.mkdir(parents=True, exist_ok=True)
    bay = (235,85,310,150)
    shell = compose(0,[5])
    shell.paste((0,0,0,0),bay)
    compose(0,[0]).save(OUTPUT/'Background.png')
    shell.save(OUTPUT/'Machine.png')
    compose(0,[0]).crop(bay).save(OUTPUT/'EmptyBay.png')
    atlases = []
    def atlas(name, images, cols, cell, bounds):
        rows = (len(images)+cols-1)//cols
        result=Image.new('RGBA',(cols*cell[0],rows*cell[1]))
        for i,im in enumerate(images): result.paste(im,((i%cols)*cell[0],(i//cols)*cell[1]))
        result.save(OUTPUT/f'{name}.png')
        atlases.append(dict(name=name,count=len(images),columns=cols,cell=list(cell),bounds=list(bounds)))
    atlas('DrinkAnimation',[compose(f,[0,1,5,6]).crop(bay) for f in range(len(frames))],5,(75,65),bay)
    # Reuse the authored wave/texture at each height, shifting it down into the
    # original flask bounds. One atlas slice per consumed percent gives exact
    # 1% stops without interpolating/blurring the pixel art.
    syrup_bounds=[(88,27,121,89),(126,28,163,88),(162,28,200,88)]
    for name,index,bounds in zip(['RedLevels','AmberLevels','GreenLevels'],[4,3,2],syrup_bounds):
        originals=[compose(f,[index]) for f in range(12)]
        levels=[]
        for percent in range(101):
            target=bounds[1]+round((bounds[3]-bounds[1])*percent/100)
            candidates=[im for im in originals if im.getbbox() and im.getbbox()[1]<=target]
            im=candidates[-1]
            shifted=Image.new('RGBA',(width,height))
            if percent<100: shifted.alpha_composite(im,(0,target-im.getbbox()[1]))
            levels.append(shifted.crop(bounds))
        atlas(name,levels,11,(40,64),bounds)
    buttons=[]
    for i in range(7,14):
        im=compose(0,[i]); bounds=im.getbbox()
        name=f'Button_{i-7:02}'
        im.crop(bounds).save(OUTPUT/f'{name}.png')
        buttons.append(dict(name=name,layer=i,bounds=list(bounds)))
    # The last cel contains the complete drink; isolate its silhouette for
    # lifting it out of the bay, leaving no painted placeholder behind.
    glass=compose(0,[6]); mask=Image.new('L',(width,height))
    md=ImageDraw.Draw(mask)
    for y in range(height):
        xs=[x for x in range(width) if glass.getpixel((x,y))[3]>10]
        if xs: md.line((min(xs),y,max(xs),y),fill=255)
    ready=compose(21,[0,1,5,6]); ready.putalpha(mask)
    cup_bounds=mask.getbbox()
    ready.crop(cup_bounds).save(OUTPUT/'ReadyDrink.png')
    edge=ImageChops.subtract(mask.filter(ImageFilter.MaxFilter(3)),mask)
    outline=Image.new('RGBA',(width,height),(255,224,148,0)); outline.putalpha(edge)
    outline.save(OUTPUT/'DrinkOutline.png')
    (OUTPUT/'DrinkVending.aseprite.bytes').write_bytes(data)
    reference=SOURCE.with_suffix('.png').read_bytes()
    manifest=dict(source=SOURCE.as_posix(),sha256=hashlib.sha256(data).hexdigest(),
                  referenceSha256=hashlib.sha256(reference).hexdigest(),width=width,height=height,
                  layers=layers,durationsMs=durations,atlases=atlases,buttons=buttons,
                  bayBounds=list(bay),cupBounds=list(cup_bounds),
                  syrupLevelMethod='Authored wave cels shifted down and clipped at each integer consumed percent; 100 is empty',
                  textReference=SOURCE.with_suffix('.png').as_posix())
    (OUTPUT/'source-layers.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
    print('Exported vending machine, 22 authored drink frames, 3 x 101 syrup levels, 7 buttons; SHA256',manifest['sha256'])

if __name__ == '__main__':
    main()

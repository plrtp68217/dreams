"""Inspect and export the authored fridge. The source Aseprite is read only."""
import hashlib
import json
import sys
from pathlib import Path
from PIL import Image, ImageDraw, ImageChops, ImageFilter
sys.dont_write_bytecode=True
from PrepareDrinkVendingArt import read

SOURCE=Path('SourceAssets/Art/FastFood/art/guests/01_faceless_man/02_fries/fridge_keypad.aseprite')
OUTPUT=Path('Assets/Sprites/FastFoodService/FridgeCloseUp')
LOG=Path('Logs/FridgeCloseUp')

def main():
    data,width,height,layers,frames,durations=read(SOURCE)
    def compose(frame,indices):
        result=Image.new('RGBA',(width,height))
        for index in indices:
            if index in frames[frame]:
                x,y,cel,opacity=frames[frame][index]
                assert layers[index]['blend']==0
                cel=cel.copy()
                a=opacity*layers[index]['opacity']/255
                if a!=255:cel.putalpha(cel.getchannel('A').point(lambda v:round(v*a/255)))
                result.alpha_composite(cel,(x,y))
        return result
    LOG.mkdir(parents=True,exist_ok=True)
    sheet=Image.new('RGB',(width*3,(height+24)*((len(layers)+2)//3)),'#23232b');draw=ImageDraw.Draw(sheet)
    for i,layer in enumerate(layers):
        f=next((f for f in range(len(frames)) if i in frames[f] and frames[f][i][2].getbbox()),0)
        im=compose(f,[i]);x=(i%3)*width;y=(i//3)*(height+24)
        sheet.paste(im,(x,y+24),im);draw.text((x+3,y+4),f'{i}: {layer["name"]} frame {f}',fill='white')
        print(i,layer,'bounds',im.getbbox(),'nonemptyFrames',[f for f in range(len(frames)) if i in frames[f] and frames[f][i][2].getbbox()])
    sheet.save(LOG/'layers.png')
    active=[i for i,l in enumerate(layers) if l['flags']&1]
    composite=compose(0,active);composite.resize((width*4,height*4),Image.Resampling.NEAREST).save(LOG/'closed.png')
    anim=Image.new('RGB',(width*4,(height+24)*((len(frames)+3)//4)),'#23232b');draw=ImageDraw.Draw(anim)
    for f in range(len(frames)):
        im=compose(f,active);x=(f%4)*width;y=(f//4)*(height+24)
        anim.paste(im,(x,y+24),im);draw.text((x+3,y+4),f'Frame {f}, {durations[f]} ms',fill='white')
    anim.save(LOG/'animation.png')
    print('Canvas',width,height,'frames',len(frames),'durations',durations)
    assert (width,height,len(frames))==(294,193,17)
    OUTPUT.mkdir(parents=True,exist_ok=True)
    background=compose(0,[0,1]);background.save(OUTPUT/'Background.png')
    potato=compose(0,[2]);potato_bounds=potato.getbbox()
    potato.crop(potato_bounds).save(OUTPUT/'Fries.png')
    alpha=potato.getchannel('A');edge=ImageChops.subtract(alpha.filter(ImageFilter.MaxFilter(3)),alpha)
    outline=Image.new('RGBA',(width,height),(255,224,148,0));outline.putalpha(edge);outline.save(OUTPUT/'FriesOutline.png')
    for f in range(len(frames)):
        assert compose(f,[0,1]).tobytes()==background.tobytes()
        assert compose(f,[2]).tobytes()==potato.tobytes()
        compose(f,[14]).save(OUTPUT/f'Doors_{f:02}.png')
    buttons=[]
    for index in range(3,14):
        im=compose(0,[index]);bounds=im.getbbox();name='Key_'+layers[index]['name']
        im.crop(bounds).save(OUTPUT/f'{name}.png')
        buttons.append(dict(name=name,label=layers[index]['name'],layer=index,bounds=list(bounds)))
    (OUTPUT/'Fridge.aseprite.bytes').write_bytes(data)
    manifest=dict(source=SOURCE.as_posix(),sha256=hashlib.sha256(data).hexdigest(),width=width,height=height,
                  layers=layers,durationsMs=durations,buttons=buttons,potatoBounds=list(potato_bounds),
                  backgroundLayers=[0,1],potatoLayer=2,doorLayer=14,codeDisplayBounds=[183,41,244,56])
    (OUTPUT/'source-layers.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
    print('Exported 17 door frames, 11 authored keys and separate fries; SHA256',manifest['sha256'])

if __name__=='__main__':main()

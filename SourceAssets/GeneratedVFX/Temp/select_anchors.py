from pathlib import Path
from PIL import Image
from generate_tests import ROOT, TESTS, write

for name,index in {'steam_small':11,'stove_fire':1}.items():
    cfg=TESTS[name]
    source=ROOT/'Temp'/name/'anchor'/f'candidate_{index:02d}.png'
    im=Image.open(source).convert('RGBA')
    bbox=im.getbbox()
    bottom=bbox[3]-1
    xs=[x for x in range(im.width) if im.getpixel((x,bottom))[3]]
    base_x=round(sum(xs)/len(xs))
    target=(16,42 if name=='steam_small' else 20)
    shift=(target[0]-base_x,target[1]-bottom)
    assert bbox[0]+shift[0]>=0 and bbox[1]+shift[1]>=0 and bbox[2]+shift[0]<=im.width and bbox[3]+shift[1]<=im.height
    aligned=Image.new('RGBA',im.size)
    aligned.paste(im,shift)
    aligned.save(ROOT/'Temp'/name/'first_frame.png')
    write(ROOT/'Temp'/name/'anchor-selection.json',dict(candidate=index,integer_translation=shift,emission_anchor=target,scaling=False))
    print(name,'candidate',index,'native canvas',im.size,'anchor',target,'shift',shift)

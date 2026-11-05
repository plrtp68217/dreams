import sys
from PIL import Image
from generate_remaining import ROOT, EFFECTS, write

name=sys.argv[1]
index=int(sys.argv[2])
cfg=EFFECTS[name]
source=ROOT/'Temp'/name/'anchor'/f'candidate_{index:02d}.png'
image=Image.open(source).convert('RGBA')
b=image.getbbox()
assert b
dx,dy=0,0
if 'anchor' in cfg:
    bottom=b[3]-1
    xs=[x for x in range(image.width) if image.getpixel((x,bottom))[3]]
    dx=cfg['anchor'][0]-round(sum(xs)/len(xs))
    dy=cfg['anchor'][1]-bottom
elif name=='water_drip':
    dx=8-round((b[0]+b[2]-1)/2)
    dy=4-b[1]
elif name=='electric_sparks':
    dx=12-round((b[0]+b[2]-1)/2)
    dy=12-round((b[1]+b[3]-1)/2)
assert b[0]+dx>=0 and b[1]+dy>=0 and b[2]+dx<=image.width and b[3]+dy<=image.height,(name,b,dx,dy)
aligned=Image.new('RGBA',image.size)
aligned.paste(image,(dx,dy))
aligned.save(ROOT/'Temp'/name/'first_frame.png')
write(ROOT/'Temp'/name/'anchor-selection.json',dict(candidate=index,integer_translation=[dx,dy],original_bbox=b,aligned_bbox=aligned.getbbox(),scaling=False))
print(name,'selected',index,'shift',dx,dy)

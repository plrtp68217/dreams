from PIL import Image
from generate_remaining import ROOT, write

for name,count in [('floating_dust',3),('floating_dust_light',4)]:
    folder=ROOT/'Temp'/name
    sparse=Image.open(folder/'first_frame.png').convert('RGBA')
    field=Image.new('RGBA',sparse.size)
    # Reuse API-generated particle shapes, spread the sparse seed to a readable field.
    offsets=[(0,0),(17,9),(33,23),(8,39)][:count]
    for dx,dy in offsets:
        for y in range(sparse.height):
            for x in range(sparse.width):
                p=sparse.getpixel((x,y))
                if p[3]:
                    field.putpixel(((x+dx)%64,(y+dy)%64),p)
    field.save(folder/'particle_reference.png')
    matte=Image.new('RGBA',field.size,(40,17,34,255))
    matte.alpha_composite(field)
    matte.save(folder/'first_frame_matte.png')
    write(folder/'particle-reference-preparation.json',dict(source='first_frame.png',reused_seed_translations=offsets,
        reason='Preserve tiny API-generated particle shapes that automated background removal erases; animate the field through PixelLab with a temporary exact-color matte.',
        key_color='#281122',scaling=False))
    print(name,'seed pixels',sum(p[3]>0 for p in field.get_flattened_data()))

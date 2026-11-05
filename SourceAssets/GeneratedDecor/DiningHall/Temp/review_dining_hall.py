import hashlib
import json
import shutil
import sys
from pathlib import Path

from PIL import Image
from generate_dining_hall import ITEMS, PALETTE, ROOT, COLLECTION, REUSE, preview, cost_so_far, save_json
sys.path.insert(0,str(COLLECTION/'Temp'))
from analyze_components import components

def valid(path,size):
    im=Image.open(path).convert('RGBA')
    colors={'#%02x%02x%02x'%pixel[:3] for pixel in im.get_flattened_data() if pixel[3]}
    alpha=sorted(set(im.getchannel('A').get_flattened_data()))
    return im.size==size and alpha==[0,255] and 1<len(colors)<=12 and colors<=set(PALETTE)

def score(path):
    im=Image.open(path).convert('RGBA')
    groups=components(im)
    total=sum(map(len,groups))
    b=im.getbbox()
    edge=sum((b[0]==0,b[1]==0,b[2]==im.width,b[3]==im.height))
    return len(groups[0])/total*10-edge*.6-min(len(groups)-1,5)*.3

selection_path=ROOT/'Temp/selections.json'
selections=json.loads(selection_path.read_text()) if selection_path.exists() else {}
for name,w,h,_ in ITEMS:
    if name==REUSE:
        continue
    folder=ROOT/'Temp'/name
    paths=[p for p in folder.glob('candidate_*.png') if valid(p,(w,h))]
    if not paths:
        print(name,'pending or no valid candidates')
        continue
    if name not in selections:
        chosen=max(paths,key=score)
        selections[name]=int(chosen.stem.split('_')[-1])
    source=folder/f'candidate_{selections[name]:02d}.png'
    assert valid(source,(w,h))
    shutil.copyfile(source,ROOT/(name+'.png'))
    if name=='security_camera':
        # PixelLab omitted the requested red indicator in every returned variant.
        # Restore this one required native pixel; keep the untouched API candidates.
        camera=Image.open(ROOT/(name+'.png')).convert('RGBA')
        assert camera.getpixel((9,13))[3]==255
        camera.putpixel((9,13),(136,44,53,255))
        camera.save(ROOT/(name+'.png'))
    print(name,selections[name])
save_json(selection_path,selections)
preview()

if '--finalize' in sys.argv:
    records=[]
    for name,w,h,_ in ITEMS:
        path=ROOT/(name+'.png')
        assert valid(path,(w,h)),name
        im=Image.open(path).convert('RGBA')
        colors={'#%02x%02x%02x'%pixel[:3] for pixel in im.get_flattened_data() if pixel[3]}
        record=dict(file=path.name,size=[w,h],visible_colors=len(colors),colors=sorted(colors),alpha=[0,255],bbox=im.getbbox(),
                    selected_candidate=selections.get(name),reused=(name==REUSE),sha256=hashlib.sha256(path.read_bytes()).hexdigest())
        records.append(record)
    assert (ROOT/'napkin_dispenser.png').read_bytes()==(COLLECTION/'Cashier/napkin_holder.png').read_bytes()
    actual=cost_so_far()
    assert actual<=1.50,actual
    save_json(ROOT/'Temp/validation.json',dict(budget_usd=1.50,reported_cost_usd=actual,new_requests=11,reused_items=1,files=records))
    # Verify that the pre-existing approved cashier collection remains byte-identical.
    original=json.loads((COLLECTION/'Temp/collection-validation.json').read_text(encoding='utf-8'))
    for record in original:
        assert hashlib.sha256((COLLECTION/'Cashier'/record['file']).read_bytes()).hexdigest()==record['sha256'],record['file']
    path=ROOT/'prompts.md'
    text=path.read_text(encoding='utf-8').split('\n# Final results')[0]
    text+='\n# Final results\n\n11 new generation requests; 1 reused asset. No paid retries. API-reported cost: USD '+f'{actual:.3f}'+', within approved USD 1.50.\nAll returned variants retained in Temp/<item>/candidate_XX.png with a candidates.png sheet.\nSelected images copied at native dimensions without scaling or alpha modification. One technical correction: PixelLab omitted the requested red camera indicator in all candidates, so pixel (9,13) of security_camera.png was set to the existing palette color #882c35, no glow or added lighting. The untouched API source remains Temp/security_camera/candidate_00.png. No other pixels were redrawn or recolored. All final PNGs: expected native sizes, RGBA, binary alpha 0/255, only the approved palette, at most 12 visible colors.\n\n| File | Native size | Colors | Candidate |\n|---|---|---|---|\n'
    for record in records:
        text+=f"| {record['file']} | {record['size'][0]}x{record['size'][1]} | {record['visible_colors']} | {'reused' if record['reused'] else record['selected_candidate']} |\n"
    text+='\nContact sheet: preview.png (nearest-neighbor enlargement). Validation, hashes and reported cost: Temp/validation.json. Original cashier assets verified byte-identical. No Unity imports, scene changes or commits.\n'
    path.write_text(text,encoding='utf-8')
    print('PASS: twelve final PNGs; previous cashier collection unchanged; cost USD',round(actual,3))

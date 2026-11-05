"""DiningHall: approved limit USD 1.50, eleven requests, no paid retries."""
import base64
import concurrent.futures
import hashlib
import io
import json
import os
import shutil
import sys
import threading
import time
from pathlib import Path

import requests
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
COLLECTION = ROOT.parent
PROJECT = COLLECTION.parents[1]
sys.path.insert(0, str(COLLECTION / 'Temp'))
from generate_collection import reference, find_image, PALETTE

ITEMS = [
 ('abandoned_food_tray',40,24,'One abandoned dark burgundy rectangular food tray holding exactly a crumpled cream burger wrapper, an empty red paper cup and one dirty folded napkin. All three items are inside the tray; a single compact tabletop arrangement. Narrow top surface visible, low side elevation, no food left, no table.'),
 ('forgotten_paper_cup',16,24,'One lonely faded red disposable paper drink cup with a narrow cream rim and one visibly bent cream drinking straw. Empty battered cup, slightly dented tapered silhouette, a tiny muted cream stripe. Upright, no lid, no lettering, no puddle.'),
 ('napkin_dispenser',24,24,'One old compact slate metal tabletop napkin dispenser with folded cream napkins. Reuse the already approved identical cashier napkin holder; no new generation.'),
 ('forgotten_toy',16,16,'One tiny strange plastic toy from an old restaurant kids meal: a squat burgundy rabbit-like mascot with two uneven upright ears, a cream face, two tiny dark dot eyes and an unnervingly blank expression. Clearly a small molded plastic figurine with a flat base, no realistic fur, no packaging, no accessories.'),
 ('restaurant_trash_bin',24,32,'One small worn restaurant waste bin for the floor beside red booth seats. Squat rectangular desaturated charcoal-slate body with faded burgundy upper rim, a single dark recessed rectangular flap opening near the top, subtle worn corners and two sparse scratches. Closed intact silhouette, no loose rubbish, no lettering, no wheels.'),
 ('spilled_paper_cup',24,16,'One overturned red paper cup lying on its side with a single small irregular flat puddle of dark muted brown drink beside its mouth. Cream rim, short bent cream straw lying against the cup. The small puddle is opaque flat pixel clusters, not a shadow or reflection. No extra cups, no floor, no droplets far from the main object.'),
 ('withered_floor_plant',32,48,'One small floor plant in a worn muted brown rectangular terracotta pot. Sparse drooping broad leaves on three thin brown stems; several leaves are partly withered with curled brown tips. Use only dull taupe and muted brown for the dying leaves, no bright green, no flowers. Thin open gaps between leaves, readable silhouette, no ground or external shadow.'),
 ('window_sticker',24,24,'One old faded advertisement decal of a small burger on an aged cream rounded rectangular sticker. A clear tiny burger icon with cream bun, dull brown patty, faded burgundy filling. One lower corner peeling upward and a slightly chipped edge. Empty transparent exterior; the cream decal itself remains opaque. No words, no glass, no window frame.'),
 ('security_camera',32,24,'One old wall-mounted CCTV security camera pointing toward the lower left, a small rectangular cream and taupe housing with a dark slate front lens and a single dull red indicator pixel. Short attached dark metal wall bracket extends to the right. Indicator is a flat burgundy pixel, absolutely no emitted light or glow. One camera only, no wall, no wires outside the silhouette.'),
 ('childrens_drawing',40,32,'One small aged cream sheet of paper taped at its upper two corners. On the sheet is a primitive childish crayon drawing of a restaurant: simple red roof, two square windows, one door, and four stick human figures. Three figures are short and one is conspicuously twice as tall with elongated thin legs. Uneven one-pixel drawn lines, dark plum and faded red marks on cream paper. No realistic people, no scene outside the paper, no text.'),
 ('promotional_poster',32,48,'One old rectangular fast food advertising poster on opaque aged cream paper, scuffed edges, two tiny burgundy pin marks. A small simple appetizing burger illustration occupies only the upper third, built from cream bun, taupe patty and dark red filling in flat pixel clusters. The lower half is completely blank cream paper reserved for later text. A narrow faded burgundy border. No generated lettering, no writing marks in the blank area, no wall.'),
 ('damaged_window_blinds',64,48,'One standalone set of old horizontal window blinds. A narrow dark upper mounting rail and nine separated long thin dull cream and taupe horizontal slats, each a flat straight pixel strip. Three slats are visibly crooked or bent with stepped pixel angles and one has a small missing end. Two thin dark vertical support cords. Transparent gaps between slats and transparent exterior. No window, no wall, no forest, no sunlight or light rays, no bottom scene.'),
]
COMMON = (' Strict native-resolution pixel art for DREAMS, abandoned fast-food dining hall with dark red booth seats and night forest windows. '
 'Match the approved cashier sprites: native one-pixel grid, restrained detail, front-facing side-scroller elevation, no isometric perspective. '
 'Dark plum-brown one-pixel outlines, desaturated slate metal, aged cream paper, muted taupe and faded burgundy accents; only the supplied twelve colors. '
 'Two or three flat local material tones. No lighting effects or directional shine, no gradients, anti-aliasing, dithering, blur, glow or external cast shadows. '
 'One isolated complete prop centered with transparent padding, hard opaque pixels and fully transparent exterior. '
 'No scene, background, floor, extra props, watermark or logo. ')
REUSE='napkin_dispenser'
ASSET_REFS={'restaurant_trash_bin':'TrashBin','withered_floor_plant':'PottedPlant','security_camera':'SecurityCamera'}
LOCK=threading.Lock()
BUDGET=1.50

def save_json(path,data):
    path.write_text(json.dumps(data,indent=2,ensure_ascii=False),encoding='utf-8')

def cost_so_far():
    total=0.0
    for path in (ROOT/'Temp').glob('*/status.json'):
        result=json.loads(path.read_text(encoding='utf-8'))
        total+=float((result.get('last_response') or {}).get('usage_cost_usd') or 0)
    return total

def make_payload(item):
    name,w,h,subject=item
    refs=[reference(PROJECT/'Assets/Sprites/Backgrounds/FastFood/FastFoodHall/FastFoodHall - 5.png','Environment palette and mood only: red booths and dark window framing; do not reproduce the room.')]
    if name in ASSET_REFS:
        refs.append(reference(PROJECT/'Assets/Sprites/Backgrounds/FastFood/Props/HallDecor'/f'{ASSET_REFS[name]}.png','Object design reference only. Adapt to requested small native size and supplied palette. If reference is a sheet, draw only ONE object, never the grid.'))
    return dict(description=subject+COMMON+f'Canvas exactly {w}x{h} native pixels.',image_size=dict(width=w,height=h),seed=5112100+ITEMS.index(item),
        no_background=True,style_image=reference(COLLECTION/'Cashier/receipt_printer.png','Approved style reference only: match native pixel density, outlines and flat material shading, not the subject.'),
        reference_images=refs,style_options=dict(color_palette=False,outline=True,detail=True,shading=True),
        target_palette=dict(colors=PALETTE,max_colors=12,name='DREAMS approved cashier palette'))

def docs():
    lines=['# DiningHall — prompts and settings','',
      'Budget approved 2026-11-05: up to USD 1.50; eleven new PixelLab requests, no paid retries. One existing napkin holder reused with no charge.',
      'Previous observed charges: USD 0.08–0.115/request; estimate for this set about USD 0.9–1.4. Actual charges recorded from API responses.',
      'Endpoint: POST https://api.pixellab.ai/v2/generate-image-v2. no_background=true; max_colors=12; same palette as ../palette.md.',
      'Style: ../Cashier/receipt_printer.png. Environment reference: existing FastFoodHall - 5.png. Copy outline, detail, shading; color_palette=false because target_palette is explicit.',
      'No lighting, glow, gradients, antialiasing, dithering or animations. Final sprites retain native dimensions.','',
      '## Existing assets',
      '- napkin_dispenser.png: exact copy of ../Cashier/napkin_holder.png, no new request.',
      '- Existing TrashBin.png, PottedPlant.png and SecurityCamera.png were inspected. These use different colors or density; the camera image is a multi-camera sheet. They are design references for new versions at the approved palette and native sizes. Originals remain unchanged.','']
    for item in ITEMS:
        name,w,h,subject=item
        lines += [f'## {name}.png',f'Size: {w}x{h}; seed: {5112100+ITEMS.index(item)}; palette cap: 12.',
                  ('Reused; no API call.' if name==REUSE else 'New PixelLab generation; one request.'),
                  '```text',subject+COMMON+f'Canvas exactly {w}x{h} native pixels.','```','']
    (ROOT/'prompts.md').write_text('\n'.join(lines),encoding='utf-8')
    source=COLLECTION/'Cashier/napkin_holder.png'
    dest=ROOT/'napkin_dispenser.png'
    if not dest.exists():
        shutil.copyfile(source,dest)
    assert source.read_bytes()==dest.read_bytes()

def run(item):
    name,w,h,_=item
    if name==REUSE:
        return name+' reused'
    folder=ROOT/'Temp'/name
    folder.mkdir(exist_ok=True)
    if (folder/'extracted.json').exists():
        return name+' candidates already saved'
    payload=make_payload(item)
    save_json(folder/'request.json',payload)
    session=requests.Session()
    session.headers['Authorization']='Bearer '+os.environ['PIXELLAB_API_KEY']
    job_path=folder/'job.json'
    if not job_path.exists():
        with LOCK:
            if (folder/'submitted.txt').exists():
                return name+' unresolved submission; no automatic paid repeat'
            # Reserve USD 0.13 per outstanding request, above previously observed costs.
            outstanding=sum(1 for d in (ROOT/'Temp').iterdir() if d.is_dir() and (d/'submitted.txt').exists() and not (d/'status.json').exists())
            if cost_so_far()+0.13*(outstanding+1)>BUDGET:
                return name+' stopped by budget guard'
            (folder/'submitted.txt').write_text('One authorized paid request; never automatically resubmit.',encoding='utf-8')
        response=session.post('https://api.pixellab.ai/v2/generate-image-v2',json=payload,timeout=120)
        if response.status_code not in (200,202):
            (folder/'http_error.txt').write_text(str(response.status_code),encoding='utf-8')
            return name+' HTTP '+str(response.status_code)
        save_json(job_path,response.json())
    job=json.loads(job_path.read_text(encoding='utf-8'))
    job_id=job.get('background_job_id') or job.get('job_id') or job.get('id')
    for attempt in range(100):
        response=session.get('https://api.pixellab.ai/v2/background-jobs/'+job_id,timeout=60)
        if response.status_code!=200:
            time.sleep(10)
            continue
        result=response.json()
        with LOCK:
            save_json(folder/'status.json',result)
        last=result.get('last_response') or {}
        imgs=last.get('images') or []
        if imgs:
            saved=[]
            for index,data in enumerate(imgs):
                raw=base64.b64decode(data['base64'].split(',')[-1])
                im=Image.open(io.BytesIO(raw)).convert('RGBA')
                assert im.size==(w,h)
                path=folder/f'candidate_{index:02d}.png'
                im.save(path)
                saved.append(path.name)
            save_json(folder/'extracted.json',dict(candidates=saved,usage_cost_usd=last.get('usage_cost_usd')))
            make_sheet(folder,saved)
            return name+f' {len(saved)} candidates; USD '+str(last.get('usage_cost_usd'))
        if result.get('status')=='failed':
            return name+' failed; no paid retry'
        if attempt%6==0:
            print(name,result.get('status'),flush=True)
        time.sleep(10)
    return name+' still pending'

def make_sheet(folder,files):
    out=Image.new('RGB',(1024,((len(files)+7)//8)*144),'#281122')
    draw=ImageDraw.Draw(out)
    for index,name in enumerate(files):
        im=Image.open(folder/name).convert('RGBA')
        s=min(4,120//im.width,116//im.height)
        im=im.resize((im.width*s,im.height*s),Image.Resampling.NEAREST)
        x,y=index%8*128,index//8*144
        out.paste(im,(x+(128-im.width)//2,y+20+(116-im.height)//2),im)
        draw.text((x+4,y+4),str(index),fill='#ece0c2')
    out.save(folder/'candidates.png')

def preview():
    out=Image.new('RGB',(960,720),'#281122')
    draw=ImageDraw.Draw(out)
    for index,(name,w,h,_) in enumerate(ITEMS):
        x,y=index%4*240,index//4*240
        for cy in range(y+24,y+216,8):
            for cx in range(x+8,x+232,8):
                draw.rectangle((cx,cy,cx+7,cy+7),fill=('#482b33' if (cx//8+cy//8)%2 else '#3a1d25'))
        draw.text((x+8,y+8),name,fill='#ece0c2')
        path=ROOT/(name+'.png')
        if path.exists():
            im=Image.open(path).convert('RGBA')
            s=min(4,208//w,180//h)
            im=im.resize((w*s,h*s),Image.Resampling.NEAREST)
            out.paste(im,(x+(240-im.width)//2,y+24+(192-im.height)//2),im)
            draw.text((x+8,y+222),f'{w}x{h} / {s}x'+(' / reused' if name==REUSE else ''),fill='#ece0c2')
    out.save(ROOT/'preview.png')

if __name__=='__main__':
    docs()
    if '--generate' in sys.argv:
        with concurrent.futures.ThreadPoolExecutor(max_workers=2) as executor:
            futures=[executor.submit(run,item) for item in ITEMS]
            for future in concurrent.futures.as_completed(futures):
                try:
                    print(future.result(),flush=True)
                except Exception as error:
                    print('Generation error:',type(error).__name__,flush=True)
        print('Reported API cost USD',round(cost_so_far(),3),flush=True)
    preview()

"""Only two authorized VFX tests. Credentials from environment, no paid resubmission."""
import base64
import concurrent.futures
import io
import json
import os
import sys
import time
from pathlib import Path

import requests
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
PROJECT=ROOT.parents[1]
sys.path.insert(0,str(PROJECT/'SourceAssets/GeneratedDecor/Temp'))
from generate_collection import reference

TESTS={
 'steam_small':dict(size=(32,48),count=8,fps=8,palette=['#4d5464','#716b5e','#918e73','#ece0c2'],
   description='One small isolated plume of pale gray kitchen steam, a VFX sprite for a moody abandoned fast-food restaurant in a 320x180 pixel-art side-scroller. Native 32x48 pixels, one-pixel grid. Two small connected wisps curl upward from a narrow invisible fixed source at bottom center, x=16 y=42. Sparse clear soft-looking cloud shapes formed ONLY with hard flat pixel clusters and four muted colors, no outlined tube or smoke stack. Compact plume within x=7..25 and y=8..42, generous transparent margins. No container, appliance, ground, black outline, lighting, glow, blur, gradient, dithering or antialiasing. Fully transparent background. Only steam, not a character or object.',
   action='Seamless looping small steam VFX, fixed 32x48 canvas and static camera. Narrow emission source stays exactly at bottom center x16 y42. Two light wisps gently curl and rise, upper wisp disperses into transparency while new steam emerges at the same source. Smooth small one-pixel changes between adjacent frames. Keep plume scale, center and origin stable; no whole-sprite translation, no zoom, no sudden bursts or flickering noise. Last frame should lead smoothly back into the first with a similar plume distribution, not a fully vanished ending. Strict hard-edged native pixel clusters, four muted gray and cream tones, no black outlines, gradients, antialiasing, glow, drawn lighting or extra objects. Transparent exterior.'),
 'stove_fire':dict(size=(32,24),count=6,fps=10,palette=['#762527','#9c2020','#b66e3d','#e3984c','#d8bb7e','#fee a9a'.replace(' ',''),'#ece0c2'],
   description='One small isolated low yellow-orange gas stove flame effect for a moody abandoned fast-food restaurant in a 320x180 pixel-art side-scroller. Strict native 32x24 pixels, one-pixel grid. Three short connected flame tongues, warm muted burnt-orange outside and pale yellow core, flat two or three-tone pixel clusters. Base fixed horizontally around x=8..24 y=20, tallest tongue reaches about y=6, complete silhouette contained within x=5..27 y=3..21. No burner, stove, pan, fuel, smoke, sparks, floor, blue flame, black outline, outer halo or cast light. No glow, gradients, antialiasing, blur or dithering. Fully transparent background, effect only.',
   action='Seamless looping small yellow-orange gas flame VFX on a fixed 32x24 canvas. The short flame base stays locked at x8..24 y20. Three tongues alternately lean and curl only one or two pixels; their tips fluctuate gently while the base remains stable. Continuous controlled small combustion, no explosive growth, no whole-sprite translation, zoom, disappearance, smoke, sparks or other objects. Last frame must transition naturally back to the first. Strict native hard pixel clusters with flat muted burnt-orange, yellow and cream colors, no black outline, glow, light rays, gradients, antialiasing or background. Keep transparent exterior.')
}

def write(path,data):
    path.write_text(json.dumps(data,indent=2,ensure_ascii=False),encoding='utf-8')

def get_session():
    session=requests.Session()
    session.headers['Authorization']='Bearer '+os.environ['PIXELLAB_API_KEY']
    return session

def submit_and_poll(folder,payload,endpoint,ready):
    folder.mkdir(parents=True,exist_ok=True)
    write(folder/'request.json',payload)
    session=get_session()
    job_path=folder/'job.json'
    if not job_path.exists():
        marker=folder/'submitted.txt'
        if marker.exists():
            raise RuntimeError('Ambiguous previous submission; automatic paid repeat forbidden')
        marker.write_text('One authorized request. Do not submit again automatically.',encoding='utf-8')
        response=session.post('https://api.pixellab.ai/v2/'+endpoint,json=payload,timeout=150)
        if response.status_code not in (200,202):
            (folder/'http_error.txt').write_text(str(response.status_code),encoding='utf-8')
            print(folder.parent.name,endpoint,'HTTP',response.status_code,flush=True)
            return None
        write(job_path,response.json())
    job=json.loads(job_path.read_text(encoding='utf-8'))
    job_id=job.get('background_job_id') or job.get('job_id') or job.get('id')
    for attempt in range(120):
        if job_id:
            response=session.get('https://api.pixellab.ai/v2/background-jobs/'+job_id,timeout=60)
            if response.status_code!=200:
                time.sleep(10)
                continue
            result=response.json()
        else:
            result=job
        write(folder/'status.json',result)
        last=result.get('last_response') or result
        if ready(last):
            return last
        if result.get('status')=='failed':
            print(folder.parent.name,'failed',flush=True)
            return None
        if attempt%6==0:
            print(folder.parent.name,endpoint,result.get('status'),last.get('action',''),flush=True)
        time.sleep(10)
    return None

def make_anchor(name):
    cfg=TESTS[name]
    folder=ROOT/'Temp'/name/'anchor'
    w,h=cfg['size']
    payload=dict(description=cfg['description'],image_size=dict(width=w,height=h),seed=5112300+list(TESTS).index(name),no_background=True,
      reference_images=[reference(PROJECT/'SourceAssets/GeneratedDecor/Cashier/receipt_printer.png','Only pixel density, muted aesthetic and hard flat pixel rendering; do NOT draw any printer, object or outline.')],
      target_palette=dict(colors=cfg['palette'],max_colors=len(cfg['palette']),name='DREAMS '+name+' VFX'))
    result=submit_and_poll(folder,payload,'generate-image-v2',lambda last:bool(last.get('images')))
    if not result:
        return
    images=result['images']
    out=Image.new('RGB',(1024,((len(images)+7)//8)*160),'#281122')
    draw=ImageDraw.Draw(out)
    for index,data in enumerate(images):
        image=Image.open(io.BytesIO(base64.b64decode(data['base64'].split(',')[-1]))).convert('RGBA')
        assert image.size==cfg['size']
        image.save(folder/f'candidate_{index:02d}.png')
        scale=min(3,120//w,136//h)
        scaled=image.resize((w*scale,h*scale),Image.Resampling.NEAREST)
        x,y=index%8*128,index//8*160
        out.paste(scaled,(x+(128-scaled.width)//2,y+20+(136-scaled.height)//2),scaled)
        draw.text((x+4,y+4),str(index),fill='#ece0c2')
    out.save(folder/'candidates.png')
    print(name,'anchor candidates saved:',len(images),'cost USD',result.get('usage_cost_usd'),flush=True)

def animate(name):
    cfg=TESTS[name]
    source=ROOT/'Temp'/name/'first_frame.png'
    assert source.exists(),'Select anchor first'
    folder=ROOT/'Temp'/name/'animation'
    payload=dict(first_frame=dict(base64=base64.b64encode(source.read_bytes()).decode()),action=cfg['action'],
                 frame_count=cfg['count'],seed=5112400+list(TESTS).index(name),no_background=True,drift_threshold=0,enhance_prompt=False)
    result=submit_and_poll(folder,payload,'animate-with-text-v3',lambda last: bool(last.get('images') or last.get('frames')))
    if result:
        # Preserve the untouched full response. Export follows inspection of response shape.
        write(folder/'result.json',result)
        print(name,'animation ready; keys',list(result),'cost USD',result.get('usage_cost_usd'),flush=True)

def docs():
    for name,cfg in TESTS.items():
        target=ROOT/name
        target.mkdir(exist_ok=True)
        (target/'frames').mkdir(exist_ok=True)
        w,h=cfg['size']
        text=f'# {name}\n\nPixelLab; native canvas {w}x{h}; requested frames: {cfg["count"]}; recommended FPS: {cfg["fps"]}.\nAnchor: POST /v2/generate-image-v2. Animation: POST /v2/animate-with-text-v3. no_background=true; drift_threshold=0; enhance_prompt=false. Original animation request omitted last_frame; selected steam refinement is documented below. No duplicated padding frames.\n\n## Palette\n'+', '.join(cfg['palette'])+'\n\n## Anchor prompt\n```text\n'+cfg['description']+'\n```\n\n## Animation prompt\n```text\n'+cfg['action']+'\n```\n'
        if name=='steam_small':
            text+='\nIf alpha is binary, use global playback opacity about 0.25–0.40 in Unity. This is guidance only; no Unity changes are made.\n'
        (target/'prompt.md').write_text(text,encoding='utf-8')

if __name__=='__main__':
    docs()
    mode=sys.argv[1]
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as executor:
        futures=[executor.submit(make_anchor if mode=='anchors' else animate,name) for name in TESTS]
        for future in concurrent.futures.as_completed(futures):
            try:
                future.result()
            except Exception as error:
                print('VFX error:',type(error).__name__,str(error)[:100],flush=True)

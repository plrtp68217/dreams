"""Approved continuation: remaining ten VFX, resume existing jobs without resubmission."""
import base64
import concurrent.futures
import hashlib
import io
import json
import sys
from PIL import Image, ImageDraw
from generate_tests import ROOT, PROJECT, reference, submit_and_poll, write

GRAY=['#4d5464','#716b5e','#918e73','#ece0c2']
DARK=['#1e2436','#3e4555','#484e5e','#4d5464']
WARM=['#b66e3d','#e3984c','#d8bb7e','#feea9a','#ece0c2']
EFFECTS={
 'steam_large':dict(size=(48,64),count=10,fps=8,palette=GRAY,loop=True,opacity='0.25–0.40',anchor=(24,58),
  description='A broad medium-size isolated plume of pale kitchen steam, several rounded wisps curling upward from an invisible narrow bottom-center emission point. More voluminous than a single thin wisp, but sparse with transparent gaps. Keep the complete plume inside the canvas with generous margins. No pot, pipe or appliance.',
  action='Seamless looping broad kitchen steam. Lower emission stays fixed at x24 y58, while rounded upper wisps slowly curl upward and disperse and new steam joins from below. Keep fairly constant volume. Return to the supplied endpoint phase without abrupt collapse or reappearance.'),
 'fryer_smoke':dict(size=(32,48),count=8,fps=8,palette=DARK,loop=True,opacity='0.55–0.75',anchor=(16,42),
  description='An isolated dense dark-gray smoke plume from an unseen faulty fryer. Three compact rounded charcoal and desaturated slate smoke lobes rise from a narrow bottom-center source. Thick silhouette, two or three flat dark-gray tones, small transparent holes, complete contained plume. No fryer, fire, appliance or face.',
  action='Seamless looping heavy dark-gray fryer smoke. Source stays fixed at x16 y42. Dense rounded lobes roll upward slowly, a top lobe breaks apart while a new lower lobe replaces it. Keep overall scale and dark palette stable, no explosive growth, no sudden whole-plume translation.'),
 'water_drip':dict(size=(16,32),count=8,fps=12,palette=['#3e4555','#4d5464','#918e73','#ece0c2'],loop=False,opacity='1.0',
  description='One tiny pale desaturated water droplet forming at top center x8 y4, about three pixels wide with a small pointed top. Isolated water only, otherwise empty transparent 16x32 canvas with room below for the fall and splash. No faucet, pipe, floor, puddle or other object.',
  action='One water-drip cycle, not walking or morphing into an object. Small drop forms at x8 y4, elongates, releases, falls vertically down the fixed x8 axis and hits an invisible surface at y28, splitting into three tiny outward splash pixels that disappear. No camera movement or horizontal drift of the falling drop. Finish quiet with transparent space; playback may pause before repeating.'),
 'electric_sparks':dict(size=(24,24),count=6,fps=16,palette=['#e3984c','#feea9a','#ece0c2','#4d5464'],loop=False,opacity='1.0',
  description='One tiny isolated burst of electrical sparks centered at x12 y12. Four very short angular one-pixel white-cream and muted amber strokes with a tiny central spark, sparse and irregular. Full burst inside a 24x24 canvas with transparent margins. No equipment, cable, light halo or background.',
  action='One brief electrical spark burst at fixed center x12 y12. A tiny flash branches into three or four short angular pixel sparks, expands only a few pixels, fragments and extinguishes. No constant beam, star icon or object. End dark and allow an external pause between bursts. No glow or cast light.'),
 'floating_dust':dict(size=(64,64),count=12,fps=6,palette=['#4d5464','#716b5e','#918e73'],loop=True,opacity='0.20–0.35',
  description='A sparse isolated field of nine tiny dim gray and taupe dust motes distributed over a transparent 64x64 canvas. Each mote is exactly one or two native pixels, with generous empty space, no central cloud or container. Motes spread across the canvas rather than forming an icon.',
  action='Seamless looping sparse floating dust field. Nine tiny individual motes drift independently by at most one pixel between adjacent frames on slow short curved paths and return to the supplied cyclic arrangement. Keep the particle count and size stable; no common whole-field translation, sudden blinking, growing blobs or random noise.'),
 'screen_flicker':dict(size=(32,24),count=6,fps=12,palette=['#1e2436','#3e4555','#4d5464','#918e73','#ece0c2'],loop=True,opacity='0.6–1.0',
  description='Abstract content-only overlay for an old electronic display: a small flat dark-slate rectangular screen area centered inside 32x24, two thin broken desaturated gray horizontal scan bars and a few isolated pixel glitches. Sharp rectangular footprint, transparent outside. No monitor bezel, hardware, writing, numbers, glow or drawn lighting.',
  action='A short old-display flicker loop. Keep the rectangular screen footprint absolutely locked. A few thin horizontal scan bars briefly shift by one pixel, dim and reappear in a restrained uneven sequence. No readable text, no moving monitor object, no halo or cast light. Return naturally to the reference display pattern.'),
 'light_flicker':dict(size=(16,16),count=8,fps=12,palette=['#404040','#808080','#bfbfbf','#ffffff'],loop=True,opacity='Mask luminance, multiply; not a painted light sprite',mask=True,
  description='A technical pixel-art luminance mask, not a lamp or scene: one perfectly flat uniform solid medium-light gray 12x12 square centered on a transparent 16x16 canvas. Crisp square edges, no border, no texture, no icon, no light rays, no bulb and no gradient. Only the uniform square mask, four allowed neutral gray intensity levels.',
  action='Technical brightness-mask flicker. Keep the exact same centered square footprint and hard edge on every frame. Change only the uniform gray intensity inside the entire square between dark gray, mid gray and white in an irregular brief dim-bright sequence, returning to the starting level. No movement, no shape change, no texture, no drawn lamp, glow, spotlight or gradient.'),
 'window_fog':dict(size=(64,48),count=10,fps=6,palette=GRAY,loop=True,opacity='0.15–0.30',
  description='Two isolated thin low-contrast horizontal ribbons of night fog, a soft-looking effect made exclusively from hard flat muted-gray pixel clusters. Long irregular rounded horizontal wisps separated by transparent gaps, spread over a 64x48 canvas. No window frame, landscape, trees, ground, sky or lighting.',
  action='Seamless looping light fog outside a window. Two low horizontal fog ribbons gently curl and flow internally, drifting only one pixel at a time. Edges gradually break into transparent gaps and reform; preserve overall placement and scale. Return to the supplied endpoint phase without whole-canvas jumps, sudden flashing or a filled background.'),
 'falling_leaves':dict(size=(48,64),count=10,fps=8,palette=['#716b5e','#957d68','#882c35','#918e73'],loop=False,opacity='0.7–1.0',
  description='Four tiny separate dry autumn leaves spread over a transparent 48x64 canvas. Each leaf is a simple three-to-five-pixel curled shape in dull brown, taupe or faded burgundy. Sparse arrangement at different heights with lots of empty space. No branches, tree, window, ground, forest or background.',
  action='One continuous downward passage of four tiny dry leaves outside a window. Every leaf must descend through the canvas, about one or two pixels lower on each next frame, with slight independent sideways sway and tiny shape rotations. Do NOT orbit, rise, return to the original arrangement or reverse direction to force a loop. Preserve each leaf size and the static 48x64 camera canvas. Only individual leaves move; no global canvas drift. This is a short non-looping falling passage, not a repeating dance.'),
 'floating_dust_light':dict(size=(64,64),count=12,fps=6,palette=['#918e73','#d8bb7e','#ece0c2'],loop=True,opacity='0.30–0.50',
  description='A sparse isolated field of twelve tiny pale warm cream and muted amber dust motes, distributed across a fully transparent 64x64 canvas. Every mote is only one or two native pixels. Brighter than the dim dust variant but not glowing. No light cone, light ray, cloud, lamp, object, floor or background.',
  action='Seamless looping pale dust motes visible in an already lit room. Twelve tiny particles drift independently on very short slow paths, at most one pixel per frame, preserving count and pixel size. No synchronized whole-field movement or abrupt blinking. Return to the supplied cyclic arrangement. Absolutely no painted illumination or glow.'),
}
COMMON=(' Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. '
 'Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter. ')

def docs():
    # Snapshot approved test deliverables before any continuation work.
    snapshot=ROOT/'Temp/approved-test-hashes.json'
    if not snapshot.exists():
        files={str(p.relative_to(ROOT)):hashlib.sha256(p.read_bytes()).hexdigest() for name in ['steam_small','stove_fire'] for p in (ROOT/name).rglob('*') if p.is_file()}
        write(snapshot,files)
    for name,cfg in EFFECTS.items():
        folder=ROOT/name
        (folder/'frames').mkdir(parents=True,exist_ok=True)
        w,h=cfg['size']
        index=list(EFFECTS).index(name)
        text=f'# {name}\n\nPixelLab; {w}x{h} native pixels; {cfg["count"]} requested frames; recommended {cfg["fps"]} FPS.\nPalette: '+', '.join(cfg['palette'])+f'.\nAnchor seed {5112500+index}; animation seed {5112600+index}.\n'
        text+='Anchor: POST /v2/generate-image-v2. Animation: POST /v2/animate-with-text-v3; no_background=true; enhance_prompt=false. Cyclic effects use identical first/last boundary references; the initial input reference is excluded from output, no duplicate padding frames added.\n'
        text+='\n## Anchor prompt\n```text\n'+cfg['description']+COMMON+f'Canvas exactly {w}x{h} pixels.\n```\n\n## Motion prompt\n```text\n'+cfg['action']+COMMON+'\n```\n\nPlayback opacity recommendation: '+cfg['opacity']+'. Guidance only, no Unity edits.\n'
        if cfg.get('mask'):
            text+='\nThis is a capability test of generic image animation; PixelLab has no dedicated lamp-brightness endpoint. Accept only a stationary mask with useful brightness variation. Otherwise report unsupported and retain raw attempt in Temp.\n'
        path=folder/'prompt.md'
        if not path.exists():
            path.write_text(text,encoding='utf-8')

def make_anchor(name):
    cfg=EFFECTS[name]
    folder=ROOT/'Temp'/name/'anchor'
    if (folder/'candidates.png').exists():
        return name+' anchors already saved'
    w,h=cfg['size']
    refs=[] if cfg.get('mask') else [reference(ROOT/'Temp/steam_small/first_frame.png','Native pixel density and flat pixel rendering only. Create the requested different VFX, not this steam shape.')]
    payload=dict(description=cfg['description']+COMMON+f'Canvas exactly {w}x{h} pixels.',image_size=dict(width=w,height=h),
        seed=5112500+list(EFFECTS).index(name),no_background=True,target_palette=dict(colors=cfg['palette'],max_colors=len(cfg['palette']),name='DREAMS '+name+' VFX'))
    if refs:
        payload['reference_images']=refs
    result=submit_and_poll(folder,payload,'generate-image-v2',lambda last:bool(last.get('images')))
    if not result:
        return name+' anchor pending or failed'
    sheet=Image.new('RGB',(1024,((len(result['images'])+7)//8)*160),'#281122')
    draw=ImageDraw.Draw(sheet)
    for index,data in enumerate(result['images']):
        image=Image.open(io.BytesIO(base64.b64decode(data['base64'].split(',')[-1]))).convert('RGBA')
        assert image.size==(w,h)
        image.save(folder/f'candidate_{index:02d}.png')
        scale=min(3,120//w,136//h)
        scaled=image.resize((w*scale,h*scale),Image.Resampling.NEAREST)
        x,y=index%8*128,index//8*160
        sheet.paste(scaled,(x+(128-scaled.width)//2,y+20+(136-scaled.height)//2),scaled)
        draw.text((x+4,y+4),str(index),fill='#ece0c2')
    sheet.save(folder/'candidates.png')
    return name+f' {len(result["images"])} anchor variants ready'

def animate(name):
    cfg=EFFECTS[name]
    particle=name in ['floating_dust','floating_dust_light']
    source=ROOT/'Temp'/name/('first_frame_matte.png' if particle else 'first_frame.png')
    if not source.exists():
        return name+' select anchor first'
    folder=ROOT/'Temp'/name/('animation_matte' if particle else 'animation_fall' if name=='falling_leaves' else 'animation')
    if (folder/'result.json').exists():
        return name+' animation already saved'
    encoded=dict(base64=base64.b64encode(source.read_bytes()).decode())
    payload=dict(first_frame=encoded,action=cfg['action']+COMMON,frame_count=cfg['count'],seed=5112600+list(EFFECTS).index(name),no_background=True,enhance_prompt=False)
    if particle:
        payload['no_background']=False
        payload['action']=cfg['action']+' Keep the exact solid dark plum background #281122 absolutely flat and unchanged. Animate only the tiny contrasting motes. The background is a temporary technical chroma key and will be removed later; never remove the motes or paint any haze, object or illumination. Hard pixel art, no gradients, antialiasing or blur.'
    if cfg['loop']:
        payload['last_frame']=encoded
    if not cfg.get('mask'):
        payload['drift_threshold']=0
    else:
        # Global luminance variation is the signal here, not unwanted color drift.
        payload['drift_threshold']=1000
    result=submit_and_poll(folder,payload,'animate-with-text-v3',lambda last:bool(last.get('images')))
    if result:
        write(folder/'result.json',result)
        return name+f' animation ready: {len(result["images"])} images including reference'
    return name+' animation pending or failed'

if __name__=='__main__':
    docs()
    mode=sys.argv[1]
    names=sys.argv[2:] or list(EFFECTS)
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as executor:
        futures=[executor.submit(make_anchor if mode=='anchors' else animate,name) for name in names]
        for future in concurrent.futures.as_completed(futures):
            try:
                print(future.result(),flush=True)
            except Exception as error:
                print('VFX continuation error',type(error).__name__,str(error)[:100],flush=True)

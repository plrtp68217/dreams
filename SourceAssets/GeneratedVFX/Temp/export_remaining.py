import base64
import hashlib
import io
import json
import statistics
from pathlib import Path
from PIL import Image, ImageDraw
from generate_remaining import ROOT, EFFECTS, write
from analyze_components import components

def canonical(image):
    image=image.convert('RGBA')
    image.putdata([p if p[3] else (0,0,0,0) for p in image.get_flattened_data()])
    return image

def diff(a,b):
    return sum(p!=q for p,q in zip(a.get_flattened_data(),b.get_flattened_data()))

def export(name):
    cfg=EFFECTS[name]
    particle=name in ['floating_dust','floating_dust_light']
    donor='floating_dust_light' if name=='floating_dust' else name
    folder=ROOT/'Temp'/donor/('animation_matte' if particle else 'animation_fall' if name=='falling_leaves' else 'animation')
    if not (folder/'result.json').exists():
        return None
    result=json.loads((folder/'result.json').read_text(encoding='utf-8'))
    raw=[]
    for index,data in enumerate(result['images']):
        im=Image.open(io.BytesIO(base64.b64decode(data['base64'].split(',')[-1]))).convert('RGBA')
        im.save(folder/f'raw_frame_{index:03d}.png')
        raw.append(canonical(im))
    initial=canonical(Image.open(ROOT/'Temp'/donor/('first_frame_matte.png' if particle else 'first_frame.png')))
    assert raw[0].tobytes()==initial.tobytes()
    frames=raw[1:]
    assert len(frames)==cfg['count'],(name,len(frames))
    assert all(im.size==cfg['size'] for im in frames)
    notes=[]
    mask_curve=None
    leaf_offsets=[]
    if name=='falling_leaves':
        def center(group):
            return (sum(x for x,y in group)/len(group),sum(y for x,y in group)/len(group))
        references=sorted(components(initial),key=lambda g:center(g)[1])
        corrected=[]
        for index,im in enumerate(frames):
            groups=sorted(components(im),key=lambda g:center(g)[1])
            assert len(groups)==len(references)
            out=Image.new('RGBA',im.size)
            offsets=[]
            for group,ref in zip(groups,references):
                cx,cy=center(group)
                rx,ry=center(ref)
                tx=round(rx+(cx-rx)*.15)
                ty=round(ry)+(index+1)*8//len(frames)
                dx,dy=tx-round(cx),ty-round(cy)
                assert all(0<=x+dx<im.width and 0<=y+dy<im.height for x,y in group)
                for x,y in group:
                    out.putpixel((x+dx,y+dy),im.getpixel((x,y)))
                offsets.append([dx,dy])
            leaf_offsets.append(offsets)
            corrected.append(out)
        frames=corrected
        notes.append('Leaf artwork and changing poses come from the API. The generator still reversed some trajectories even without endpoint conditioning; individual connected leaf poses were translated by integer pixels onto a slow downward passage (8 px over 10 frames), with the API sideways motion damped to a small sway. No leaf pixels redrawn, scaled, blurred or interpolated; no frames duplicated. This is motion correction, not a seamless loop. Per-leaf offsets retained in validation.json.')
    if name=='floating_dust':
        retint={(145,142,115):(77,84,100),(216,187,126):(113,107,94),(236,224,194):(145,142,115)}
        dim=[]
        for im in frames:
            image=im.copy()
            image.putdata([retint.get(p[:3],p[:3])+(p[3],) for p in im.get_flattened_data()])
            dim.append(image)
        frames=dim
        notes.append('Dim dust is a color variant of the stable separately API-generated floating_dust_light motion: pale palette retinted to three approved dim gray tones. Its own generated motion faded particles out mid-cycle and is retained in Temp/floating_dust/animation_matte, not used. No particles, motion paths, timing phases or extra frames were drawn procedurally.')
    if not cfg.get('mask'):
        allowed=cfg['palette']+(['#281122'] if particle else [])
        rgb=[tuple(bytes.fromhex(color[1:])) for color in allowed]
        mapping={}
        quantized=0
        normalized=[]
        for im in frames:
            pixels=[]
            for pixel in im.get_flattened_data():
                r,g,b,a=pixel
                if not a:
                    pixels.append((0,0,0,0))
                    continue
                color=(r,g,b)
                if color not in mapping:
                    mapping[color]=min(rgb,key=lambda c:sum((x-y)**2 for x,y in zip(c,color)))
                target=mapping[color]
                quantized+=target!=color
                pixels.append((0,0,0,0) if particle and target==(40,17,34) else target+(255,))
            corrected=Image.new('RGBA',im.size)
            corrected.putdata(pixels)
            normalized.append(corrected)
        frames=normalized
        if quantized:
            notes.append(f'{quantized} opaque pixels mapped to the nearest approved palette color, no dithering, blur or spatial interpolation. Raw API frames retained in Temp.')
        if particle:
            notes.append('Particles animated on a temporary exact-color #281122 matte to prevent API background removal from erasing them. Matte keyed out after palette enforcement; final PNGs are transparent. Sparse API-generated seed shapes were distributed at integer offsets before animation; temporal frames are generated by PixelLab, not procedural motion.')
    registrations=[]
    if 'anchor' in cfg:
        anchored=[]
        for im in frames:
            b=im.getbbox()
            bottom=b[3]-1
            xs=[x for x in range(im.width) if im.getpixel((x,bottom))[3]]
            dx=cfg['anchor'][0]-round(sum(xs)/len(xs))
            dy=cfg['anchor'][1]-bottom
            assert b[0]+dx>=0 and b[1]+dy>=0 and b[2]+dx<=im.width and b[3]+dy<=im.height
            aligned=Image.new('RGBA',im.size)
            aligned.paste(im,(dx,dy))
            anchored.append(aligned)
            registrations.append([dx,dy])
        frames=anchored
        notes.append('Emission origin stabilized by integer-only translation of each generated frame to the requested fixed source. No scaling, blur, interpolation or redraw. Per-frame offsets recorded in validation.json; untouched originals remain in Temp.')
    if cfg.get('mask'):
        # Convert the API's temporal luminance into a technical constant-footprint alpha mask.
        # No artificial animation phases, duplication, gradients or spatial illumination.
        support=[(x,y) for y in range(initial.height) for x in range(initial.width) if initial.getpixel((x,y))[3]]
        curve=[]
        for im in frames:
            values=[]
            for x,y in support:
                r,g,b,a=im.getpixel((x,y))
                values.append((r*.2126+g*.7152+b*.0722)*a/255)
            curve.append(round(sum(values)/len(values)))
        if max(curve)-min(curve)<8:
            write(ROOT/'Temp'/name/'unsupported.json',dict(reason='No useful generated luminance variation',curve=curve))
            return dict(unsupported=True,reason='API animation did not supply useful lamp-mask intensity variation',curve=curve)
        frames=[]
        for level in curve:
            im=Image.new('RGBA',initial.size)
            for x,y in support:
                im.putpixel((x,y),(255,255,255,level))
            frames.append(canonical(im))
        mask_curve=curve
        notes.append('Technical light mask: API-generated temporal luminance averaged inside the input support, transferred to uniform alpha on that exact static support. No procedural timing curve or missing frames added. Original RGB animation retained in Temp. White RGB, alpha varies per frame; no spatial light painting or gradient.')
    palette=set()
    frame_info=[]
    for index,im in enumerate(frames):
        alpha=sorted(set(im.getchannel('A').get_flattened_data()))
        colors={'#%02x%02x%02x'%p[:3] for p in im.get_flattened_data() if p[3]}
        palette|=colors
        if not cfg.get('mask'):
            assert set(alpha)<={0,255},(name,index,alpha)
            assert colors<=set(cfg['palette']),(name,index,colors)
        pixels=[(x,y) for y in range(im.height) for x in range(im.width) if im.getpixel((x,y))[3]]
        info=dict(index=index,bbox=im.getbbox(),visible_pixels=len(pixels),alpha_values=alpha,sha256_pixels=hashlib.sha256(im.tobytes()).hexdigest())
        if pixels:
            bottom=max(y for x,y in pixels)
            xs=[x for x,y in pixels if y==bottom]
            info.update(bottom_y=bottom,bottom_x=sum(xs)/len(xs),centroid=[sum(x for x,y in pixels)/len(pixels),sum(y for x,y in pixels)/len(pixels)])
        im.save(ROOT/name/'frames'/f'frame_{index:03d}.png')
        frame_info.append(info)
    unique=len({f['sha256_pixels'] for f in frame_info})
    assert unique>1,(name,'static animation')
    w,h=cfg['size']
    sheet=Image.new('RGBA',(w*len(frames),h))
    for index,im in enumerate(frames):
        sheet.paste(im,(index*w,0))
    sheet.save(ROOT/name/'spritesheet.png')
    reopened=Image.open(ROOT/name/'spritesheet.png').convert('RGBA')
    assert all(reopened.crop((i*w,0,(i+1)*w,h)).tobytes()==im.tobytes() for i,im in enumerate(frames))
    transitions=[diff(frames[i],frames[(i+1)%len(frames)]) for i in range(len(frames))]
    ratio=transitions[-1]/statistics.mean(transitions[:-1]) if sum(transitions[:-1]) else 0
    nonempty=[f for f in frame_info if f['visible_pixels']]
    by=max(f['bottom_y'] for f in nonempty)-min(f['bottom_y'] for f in nonempty)
    bx=max(f['bottom_x'] for f in nonempty)-min(f['bottom_x'] for f in nonempty)
    report=dict(size=[w,h],count=len(frames),fps=cfg['fps'],loop=cfg['loop'],unique_frames=unique,palette=sorted(palette),
        seam_ratio=ratio,changes=transitions,bottom_y_range=by,bottom_x_range=bx,notes=notes,frames=frame_info,mask_luminance_curve=mask_curve,emitter_registration_offsets=registrations,leaf_registration_offsets=leaf_offsets)
    # Review contact sheet: four columns, so tiny details remain legible.
    scale=min(3,128//w,144//h)
    cellw,cellh=w*scale+16,h*scale+32
    contact=Image.new('RGB',(cellw*4,cellh*((len(frames)+3)//4)),'#281122')
    draw=ImageDraw.Draw(contact)
    for index,im in enumerate(frames):
        x,y=index%4*cellw,index//4*cellh
        for cy in range(y+24,y+24+h*scale,8):
            for cx in range(x,x+w*scale,8):
                draw.rectangle((cx,cy,cx+7,cy+7),fill=('#482b33' if (cx//8+cy//8)%2 else '#3a1d25'))
        scaled=im.resize((w*scale,h*scale),Image.Resampling.NEAREST)
        contact.paste(scaled,(x,y+24),scaled)
        draw.text((x+4,y+4),str(index),fill='#ece0c2')
    contact.save(ROOT/name/'contact.png')
    previews=[]
    preview_scale=min(3,144//w,144//h)
    for im in frames:
        preview=Image.new('RGB',(320,180),'#281122')
        draw=ImageDraw.Draw(preview)
        for cy in range(24,168,8):
            for cx in range(16,304,8):
                draw.rectangle((cx,cy,cx+7,cy+7),fill=('#482b33' if (cx//8+cy//8)%2 else '#3a1d25'))
        preview.paste(im,(64-w//2,160-h),im)
        scaled=im.resize((w*preview_scale,h*preview_scale),Image.Resampling.NEAREST)
        preview.paste(scaled,(216-scaled.width//2,160-scaled.height),scaled)
        draw.text((32,8),'1x native',fill='#ece0c2')
        draw.text((184,8),f'{preview_scale}x nearest',fill='#ece0c2')
        previews.append(preview.quantize(colors=64,method=Image.Quantize.MEDIANCUT,dither=Image.Dither.NONE))
    durations=[10*(round((i+1)*100/cfg['fps'])-round(i*100/cfg['fps'])) for i in range(len(previews))]
    if not cfg['loop']:
        durations[-1]+=800
    previews[0].save(ROOT/name/'preview.gif',save_all=True,append_images=previews[1:],duration=durations,loop=0,disposal=2,optimize=False)
    path=ROOT/name/'prompt.md'
    text=path.read_text(encoding='utf-8').split('\n## Export and quality review')[0]
    text+=f'\n## Export and quality review\n\nActual {len(frames)} frames ({unique} unique pixel states), {w}x{h}, {cfg["fps"]} FPS. Spritesheet {w*len(frames)}x{h}, one horizontal row, zero padding. Palette: '+', '.join(sorted(palette))+'.\n'
    text+='Input reference excluded from API response; no duplicate padding or interpolated frames were created. Generated image scale and canvas remain unchanged. Invisible RGB of transparent pixels normalized to zero. Initial anchor may have one fixed integer translation (recorded in Temp). Emission-origin registration, where applied, is recorded below.\n'
    text+='\n'.join(notes)+'\n'
    selected=json.loads((folder/'request.json').read_text(encoding='utf-8'))
    text+='\n### Selected API motion request\nSource folder: '+str(folder.relative_to(ROOT))+'. Seed: '+str(selected.get('seed'))+'. no_background='+str(selected.get('no_background'))+'. drift_threshold='+str(selected.get('drift_threshold'))+'.\n```text\n'+selected['action']+'\n```\n'
    if name=='falling_leaves':
        text+='\nThe first cyclic attempt made the leaves rise again to match its endpoint. It remains in Temp/falling_leaves/animation. Selected animation is deliberately a one-way downward passage, not a seamless loop. No direction reversal or duplicate frames were used to fake a cycle.\n'
    text+=f'\nLoop-boundary pixel change / mean internal change: {ratio:.2f}. Visible bottom-row range: {by} pixels; bottom-row horizontal center range: {bx:.2f} pixels. These are diagnostics, not a claim that moving dust/leaf edges should be static.\n'
    if not cfg['loop']:
        text+='\nOne-shot animation; an 800 ms pause is used only in GIF preview by holding the final state. PNG sequence has no additional duplicate pause frames. Recommended playback pause 0.5–1.5 s.\n'
    path.write_text(text,encoding='utf-8')
    write(ROOT/'Temp'/name/'validation.json',report)
    print(name,len(frames),'frames',unique,'unique','seam',round(ratio,2),'bottom',by,round(bx,2))
    return report

if __name__=='__main__':
    import sys
    names=sys.argv[1:] or list(EFFECTS)
    for name in names:
        try:
            export(name)
        except Exception as error:
            print(name,'EXPORT REVIEW REQUIRED',type(error).__name__,str(error)[:160])

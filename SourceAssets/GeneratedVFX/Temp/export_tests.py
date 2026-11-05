import base64
import hashlib
import io
import json
import math
import statistics
from pathlib import Path
from PIL import Image, ImageDraw
from generate_tests import ROOT, TESTS, write

def canonical(image):
    image=image.convert('RGBA')
    image.putdata([pixel if pixel[3] else (0,0,0,0) for pixel in image.get_flattened_data()])
    return image

def changed(a,b):
    return sum(x!=y for x,y in zip(a.get_flattened_data(),b.get_flattened_data()))

def details(image):
    pixels=[(x,y) for y in range(image.height) for x in range(image.width) if image.getpixel((x,y))[3]]
    bottom=max(y for x,y in pixels)
    base=[x for x,y in pixels if y==bottom]
    return dict(bbox=image.getbbox(),visible_pixels=len(pixels),centroid=[sum(x for x,y in pixels)/len(pixels),sum(y for x,y in pixels)/len(pixels)],
                bottom_y=bottom,bottom_x=sum(base)/len(base),sha256_pixels=hashlib.sha256(image.tobytes()).hexdigest())

all_reports={}
for name,cfg in TESTS.items():
    w,h=cfg['size']
    source=ROOT/'Temp'/name/('animation_loop' if name=='steam_small' else 'animation')
    result=json.loads((source/'result.json').read_text(encoding='utf-8'))
    images=[]
    for index,data in enumerate(result['images']):
        raw=Image.open(io.BytesIO(base64.b64decode(data['base64'].split(',')[-1]))).convert('RGBA')
        raw.save(source/f'raw_frame_{index:03d}.png')
        images.append(canonical(raw))
    assert len(images)==cfg['count']+1,(name,len(images))
    assert images[0].tobytes()==canonical(Image.open(ROOT/'Temp'/name/'first_frame.png')).tobytes()
    frames=images[1:]
    report=[]
    palette=set()
    for index,image in enumerate(frames):
        assert image.size==(w,h)
        alpha=sorted(set(image.getchannel('A').get_flattened_data()))
        assert alpha==[0,255]
        palette|={'#%02x%02x%02x'%pixel[:3] for pixel in image.get_flattened_data() if pixel[3]}
        path=ROOT/name/'frames'/f'frame_{index:03d}.png'
        image.save(path)
        report.append(details(image))
    assert palette<=set(cfg['palette'])
    assert len({entry['sha256_pixels'] for entry in report})==len(frames),'Duplicate output frames'
    sheet=Image.new('RGBA',(w*len(frames),h))
    for index,image in enumerate(frames):
        sheet.paste(image,(index*w,0))
    sheet.save(ROOT/name/'spritesheet.png')
    # Check exact sprite-sheet reconstruction, not just dimensions.
    reopened=Image.open(ROOT/name/'spritesheet.png').convert('RGBA')
    assert all(reopened.crop((i*w,0,(i+1)*w,h)).tobytes()==im.tobytes() for i,im in enumerate(frames))
    transitions=[changed(frames[i],frames[(i+1)%len(frames)]) for i in range(len(frames))]
    ratio=transitions[-1]/statistics.mean(transitions[:-1]) if sum(transitions[:-1]) else 0
    base_y_range=max(entry['bottom_y'] for entry in report)-min(entry['bottom_y'] for entry in report)
    base_x_range=max(entry['bottom_x'] for entry in report)-min(entry['bottom_x'] for entry in report)
    all_reports[name]=dict(size=[w,h],frame_count=len(frames),fps=cfg['fps'],palette=sorted(palette),alpha=[0,255],unique_frames=len(frames),
        sheet_size=[w*len(frames),h],source=str(source.relative_to(ROOT)),discarded_input_reference=True,
        pixel_change_per_transition=transitions,seam_change_ratio=ratio,emission_bottom_y_range=base_y_range,emission_bottom_x_range=base_x_range,frames=report)

    # Labeled filmstrip uses a checker only for review. PNG frames stay transparent.
    contact=Image.new('RGB',(len(frames)*(w*3+12),h*3+32),'#281122')
    draw=ImageDraw.Draw(contact)
    for index,image in enumerate(frames):
        x=index*(w*3+12)
        for cy in range(24,h*3+24,6):
            for cx in range(x,x+w*3,6):
                draw.rectangle((cx,cy,cx+5,cy+5),fill=('#482b33' if (cx//6+cy//6)%2 else '#3a1d25'))
        scaled=image.resize((w*3,h*3),Image.Resampling.NEAREST)
        contact.paste(scaled,(x,24),scaled)
        draw.text((x+4,4),str(index),fill='#ece0c2')
    contact.save(ROOT/name/'contact.png')
    previews=[]
    for frame in frames:
        preview=Image.new('RGB',(320,180),'#281122')
        draw=ImageDraw.Draw(preview)
        for cy in range(24,168,8):
            for cx in range(16,304,8):
                draw.rectangle((cx,cy,cx+7,cy+7),fill=('#482b33' if (cx//8+cy//8)%2 else '#3a1d25'))
        preview.paste(frame,(64-w//2,160-h),frame)
        scaled=frame.resize((w*3,h*3),Image.Resampling.NEAREST)
        preview.paste(scaled,(216-w*3//2,160-h*3),scaled)
        draw.text((32,8),'1x native',fill='#ece0c2')
        draw.text((184,8),'3x nearest',fill='#ece0c2')
        previews.append(preview.quantize(colors=32,method=Image.Quantize.MEDIANCUT,dither=Image.Dither.NONE))
    # GIF stores centiseconds: alternate 120/130 ms to represent 8 FPS accurately.
    durations=[10*(round((i+1)*100/cfg['fps'])-round(i*100/cfg['fps'])) for i in range(len(previews))]
    previews[0].save(ROOT/name/'preview.gif',save_all=True,append_images=previews[1:],duration=durations,loop=0,disposal=2,optimize=False)
    doc_path=ROOT/name/'prompt.md'
    text=doc_path.read_text(encoding='utf-8').split('\n## Export and verification')[0]
    text+='\n## Export and verification\n\n'+f'Actual: {len(frames)} unique frames, {w}x{h}, {len(palette)} visible colors, alpha 0/255; {cfg["fps"]} FPS. Spritesheet is one horizontal row, {w*len(frames)}x{h}, zero padding.\n'
    text+='API returned the input reference followed by the generated frames. Input reference discarded; no missing frames duplicated. No resizing, blur, recoloring, interpolation or per-frame positioning applied to generated PNGs. Only invisible RGB bytes of alpha-zero pixels normalized to zero. Initial reference was translated once on the native canvas to the fixed emission anchor; original candidates retained in Temp.\n'
    if name=='steam_small':
        request=json.loads((source/'request.json').read_text(encoding='utf-8'))
        text+='\n### Selected animation refinement\nThe first animation had a more visible loop boundary and remains in Temp/steam_small/animation. Selected second animation uses first_frame and last_frame as the same cyclic boundary reference (not duplicated padding in the export), seed 5112402. Actual action:\n```text\n'+request['action']+'\n```\nPlayback opacity recommendation: 0.25–0.40; no Unity settings were edited.\n'
    text+=f'\nMeasured closing transition / mean internal transition: {ratio:.2f}. Lowest visible row range: {base_y_range} px. Lowest-row X center range: {base_x_range:.2f} px. These measurements describe pixel changes and emission stability; they do not replace visual review.\n'
    doc_path.write_text(text,encoding='utf-8')
    print(name,'frames',len(frames),'palette',len(palette),'seam ratio',round(ratio,2),'base range',base_y_range,round(base_x_range,2))

write(ROOT/'Temp/validation.json',all_reports)
costs=[]
for path in (ROOT/'Temp').glob('*/*/status.json'):
    result=json.loads(path.read_text(encoding='utf-8'))
    last=result.get('last_response') or {}
    usage=last.get('usage') or {}
    amount=last.get('usage_cost_usd') or usage.get('cost_if_paid') or usage.get('usd') or 0
    costs.append(dict(stage=str(path.parent.relative_to(ROOT)),usd_estimate=amount,billing_usage=last.get('billing_usage')))
write(ROOT/'Temp/costs.json',dict(stages=costs,usd_equivalent_estimate=sum(item['usd_estimate'] for item in costs)))
text='# DREAMS — тестовые VFX\n\nСозданы только два теста. Остальные десять эффектов ожидают подтверждения.\n\n'
for name,cfg in TESTS.items():
    r=all_reports[name]
    text+=f'## {name}\n\n{cfg["size"][0]}×{cfg["size"][1]} px; {cfg["count"]} уникальных кадров; {cfg["fps"]} FPS; {len(r["palette"])} цветов.\n\n![Анимация]({name}/preview.gif)\n\n[PNG-кадры]({name}/frames/) · [Spritesheet]({name}/spritesheet.png) · [Промпт и параметры]({name}/prompt.md) · [Покадровая проверка]({name}/contact.png)\n\n'
text+='Превью GIF показывает исходный масштаб в окне 320×180 и увеличение 3× через Nearest Neighbor. Шахматный фон существует только в превью; экспортированные кадры и spritesheet имеют прозрачный фон. Пар экспортирован с альфой 0/255: рекомендуемая общая прозрачность при воспроизведении 0.25–0.40. Огонь — 1.0.\n\nРазмеры и число кадров совпадают с запросом; дублированием кадры не дополнялись. Проверены палитра, альфа, уникальность кадров, точное восстановление spritesheet и переход последнего кадра к первому. Полные измерения — Temp/validation.json. Исходные ответы и отклонённая первая анимация пара сохранены в Temp.\n\nAPI: [PixelLab animations](https://api.pixellab.ai/v2/docs). Все вызовы: два исходных спрайта и три анимации (включая уточнение пара). Денежный эквивалент по ответам API: около $'+f'{sum(item["usd_estimate"] for item in costs):.3f}'+'. API может списывать поколения тарифа вместо USD; исходные billing_usage сохранены в Temp/costs.json. Unity-проект, прежние ассеты и настройки не изменялись; импорт и коммиты не выполнялись.\n'
(ROOT/'preview.md').write_text(text,encoding='utf-8')

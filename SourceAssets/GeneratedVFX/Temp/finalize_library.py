import hashlib
import json
from pathlib import Path
from PIL import Image, ImageDraw
from generate_tests import ROOT, TESTS, write
from generate_remaining import EFFECTS

ALL=dict(TESTS)
ALL.update(EFFECTS)
order=['steam_small','steam_large','stove_fire','fryer_smoke','water_drip','electric_sparks','floating_dust','screen_flicker','light_flicker','window_fog','falling_leaves','floating_dust_light']
reports={}
test_reports=json.loads((ROOT/'Temp/validation.json').read_text(encoding='utf-8'))
for name in order:
    cfg=ALL[name]
    path=ROOT/name
    files=sorted((path/'frames').glob('frame_*.png'))
    if not files:
        unsupported=ROOT/'Temp'/name/'unsupported.json'
        assert cfg.get('mask') and unsupported.exists(),name+' missing frames'
        reports[name]=dict(unsupported=True,reason=json.loads(unsupported.read_text(encoding='utf-8'))['reason'])
        continue
    assert len(files)==cfg['count'],(name,len(files))
    w,h=cfg['size']
    sheet=Image.open(path/'spritesheet.png').convert('RGBA')
    assert sheet.size==(w*len(files),h)
    palette=set()
    alpha=set()
    hashes=[]
    for index,file in enumerate(files):
        im=Image.open(file).convert('RGBA')
        assert im.size==(w,h)
        assert sheet.crop((index*w,0,(index+1)*w,h)).tobytes()==im.tobytes()
        palette|={'#%02x%02x%02x'%pixel[:3] for pixel in im.get_flattened_data() if pixel[3]}
        alpha|=set(im.getchannel('A').get_flattened_data())
        hashes.append(hashlib.sha256(im.tobytes()).hexdigest())
    assert 0 in alpha and len(set(hashes))>1
    assert len(palette)<=12
    if not cfg.get('mask'):
        assert palette<=set(cfg['palette']) and alpha=={0,255}
    gif=Image.open(path/'preview.gif')
    # GIF may merge consecutive identical generated hold states; PNG timing remains authoritative.
    assert 1<gif.n_frames<=len(files)
    detail=test_reports[name] if name in TESTS else json.loads((ROOT/'Temp'/name/'validation.json').read_text(encoding='utf-8'))
    reports[name]=dict(size=[w,h],count=len(files),unique_frames=len(set(hashes)),fps=cfg['fps'],colors=sorted(palette),alpha=sorted(alpha),sheet_size=list(sheet.size),
        loop=cfg.get('loop',True),seam_ratio=detail.get('seam_ratio',detail.get('seam_change_ratio')),mask_derived=cfg.get('mask',False),
        frame_files=[dict(file=str(file.relative_to(ROOT)),sha256=hashlib.sha256(file.read_bytes()).hexdigest()) for file in files])

snapshot=json.loads((ROOT/'Temp/approved-test-hashes.json').read_text(encoding='utf-8'))
assert all(hashlib.sha256((ROOT/file).read_bytes()).hexdigest()==digest for file,digest in snapshot.items()),'Approved tests changed'
costs=[]
for path in (ROOT/'Temp').glob('*/*/status.json'):
    result=json.loads(path.read_text(encoding='utf-8'))
    last=result.get('last_response') or {}
    usage=last.get('usage') or {}
    amount=last.get('usage_cost_usd') or usage.get('cost_if_paid') or usage.get('usd') or 0
    costs.append(dict(stage=str(path.parent.relative_to(ROOT)),usd_equivalent=amount,billing_usage=last.get('billing_usage')))
total=sum(item['usd_equivalent'] for item in costs)
write(ROOT/'Temp/costs.json',dict(stages=costs,usd_equivalent_estimate=total))
write(ROOT/'Temp/library-validation.json',dict(effects=reports,approved_tests_unchanged=True,frame_count=sum(r.get('count',0) for r in reports.values()),usd_equivalent_estimate=total))

overview=Image.new('RGB',(960,720),'#281122')
draw=ImageDraw.Draw(overview)
for index,name in enumerate(order):
    x,y=index%4*240,index//4*240
    draw.text((x+8,y+8),name,fill='#ece0c2')
    for cy in range(y+24,y+216,8):
        for cx in range(x+8,x+232,8):
            draw.rectangle((cx,cy,cx+7,cy+7),fill=('#482b33' if (cx//8+cy//8)%2 else '#3a1d25'))
    r=reports[name]
    if r.get('unsupported'):
        draw.text((x+20,y+116),'Not supported by API',fill='#ece0c2')
        continue
    # First nonempty pose for a useful static overview of one-shot effects.
    files=sorted((ROOT/name/'frames').glob('*.png'))
    images=[Image.open(file).convert('RGBA') for file in files]
    im=next(im for im in images if im.getbbox())
    scale=min(3,208//im.width,180//im.height)
    scaled=im.resize((im.width*scale,im.height*scale),Image.Resampling.NEAREST)
    overview.paste(scaled,(x+(240-scaled.width)//2,y+24+(192-scaled.height)//2),scaled)
    draw.text((x+8,y+223),f'{r["size"][0]}x{r["size"][1]} / {r["count"]} frames / {r["fps"]} FPS',fill='#ece0c2')
overview.save(ROOT/'preview.png')
text='# DREAMS — библиотека VFX заброшенного фастфуда\n\nУтверждённые тесты steam_small и stove_fire сохранены побайтово. Новые эффекты добавлены после подтверждения пользователя.\n\n![Обзор](preview.png)\n\n| Эффект | Размер кадра | Кадры | FPS | Воспроизведение |\n|---|---|---|---|---|\n'
for name in order:
    r=reports[name]
    if r.get('unsupported'):
        text+=f'| {name} | — | — | — | API не дал полезную маску |\n'
    else:
        text+=f'| [{name}]({name}/prompt.md) | {r["size"][0]}×{r["size"][1]} | {r["count"]} | {r["fps"]} | '+('Цикл' if r['loop'] else 'Один проход вниз' if name=='falling_leaves' else 'Один раз, пауза 0.5–1.5 с')+' |\n'
text+='\nРазмеры и число кадров совпадают с запросом. Для light_flicker, где параметры не были заданы, выбран холст 16×16 и 8 кадров. В каждом spritesheet одна последовательная горизонтальная строка без отступов. Не создавались дублирующие или интерполированные кадры для заполнения последовательностей.\n\nPNG-кадры и spritesheet прозрачные. Все эффекты, кроме технической маски лампы, имеют бинарную альфу 0/255; общая прозрачность задаётся при будущем ручном использовании, рекомендуемые значения — в prompt.md. Маска лампы использует один белый RGB-цвет и однородную меняющуюся альфу на неподвижном контуре: яркость извлечена из сгенерированной API последовательности, отдельного режима ламп у PixelLab нет. Нет нарисованного света или пространственного градиента.\n\nПри генерации светлой пыли PixelLab удалил все частицы как фон. Исходные пустые варианты оставлены в Temp; стартовое расположение взято из тусклой пыли и переведено в светлую палитру. Движение светлой пыли сгенерировано отдельным запросом, её кадры не копировались из тусклой версии.\n\nGIF показывает исходный масштаб в окне 320×180 и увеличение Nearest Neighbor; шахматный фон существует только в превью. В GIF капли и искр добавлено время ожидания последнего состояния, без дополнительных PNG-кадров.\n\n'
text+='## Технические поправки и ограничения\n\n- Пыль анимирована API на временном одноцветном фоне #281122, который удалён по цвету при экспорте: автоматическое удаление фона стирало частицы. Разреженные формы частиц из API распределены по холсту до анимации.\n- Собственная анимация тусклой пыли исчезала в середине цикла. Итоговая тусклая версия — перевод стабильной светлой последовательности в серую палитру, поэтому два вида пыли имеют одинаковое движение. Неудачные попытки сохранены; за полностью пустые анимации API сообщил отсутствие списания.\n- Основание большого пара и дыма закреплено целочисленными сдвигами кадров.\n- Листья — один проход вниз, не бесшовный цикл. Вращения и формы получены из API; отдельные листья целочисленно перемещены по нисходящим траекториям, поскольку модель возвращала их вверх даже без конечного кадра. Ни один пиксель листа не перерисован и не интерполирован.\n- При отклонении цвета от палитры он приведён к ближайшему допустимому цвету без дизеринга.\n- Повторяющиеся состояния экрана, пустые окончания капли и искр пришли из API: это удержания, не добавленные копированием недостающие кадры. GIF может объединять такие состояния с сохранением их длительности; PNG-последовательность сохраняет заданное число кадров.\n\n'
for name in order:
    r=reports[name]
    text+='## '+name+'\n\n'
    if r.get('unsupported'):
        text+='Генератор не дал полезную кривую яркости; попытка сохранена в Temp. Возможная альтернатива — отдельная заданная вручную кривая яркости при воспроизведении; она здесь не подменяет результат API.\n\n'
        continue
    text+=f'![{name}]({name}/preview.gif)\n\n[Кадры]({name}/frames/) · [Spritesheet]({name}/spritesheet.png) · [Промпт и настройки]({name}/prompt.md) · [Все кадры]({name}/contact.png)\n\n'
text+='## Проверки\n\nПроверены размеры, число кадров, палитры, альфа, отсутствие полностью статичных последовательностей, точное восстановление кадров из spritesheet и временные параметры GIF. Для циклов измерено отличие последнего кадра от первого относительно внутренних переходов; для эмиттеров проверено положение основания. В исходных ответах и Temp сохранены все варианты, включая неудачные. Метрики и SHA-256 — Temp/library-validation.json.\n\nСуммарный денежный эквивалент по ответам API, включая исходные два теста и уточнение малого пара: около $'+f'{total:.3f}'+'. Оплата может идти поколениями тарифного плана; billing_usage сохранены в Temp/costs.json.\n\nUnity-сцены, GameObject, материалы, настройки и прежние ассеты не изменялись. Импорт и коммиты не выполнялись.\n'
(ROOT/'preview.md').write_text(text,encoding='utf-8')
print('Library complete:',len(reports),'effects,',sum(r.get('count',0) for r in reports.values()),'frames; USD equivalent',round(total,3))

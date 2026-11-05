"""Export the supplied layered fryer without altering the external Aseprite.

Run from the project root. PNGs retain the 320x180 source coordinates;
the foreground contains the authored oil drain and red gauge animation.
"""
import hashlib
import json
import struct
import zlib
from pathlib import Path
from PIL import Image, ImageChops, ImageFilter

SOURCE = Path(r'C:\Users\moroz\Desktop\DEV\UNITY\DREAMS-DATABASES\ArtDatabase_Merged\FastFood\art\guests\01_faceless_man\01_burger\Фритюр.aseprite')
OUTPUT = Path('Assets/Sprites/FastFoodService/FryerCloseUp')

def main():
    data = SOURCE.read_bytes()
    count, width, height, depth = struct.unpack_from('<4H', data, 6)
    if (count, width, height, depth) != (21, 320, 180, 32):
        raise ValueError('Expected the supplied 21-frame RGBA fryer')
    layers, frames, durations = [], [], []
    offset = 128
    for frame in range(count):
        size, magic, chunks, duration = struct.unpack_from('<IHHH', data, offset)
        cursor, cells = offset + 16, {}
        durations.append(duration)
        for _ in range(chunks):
            length, kind = struct.unpack_from('<IH', data, cursor)
            body = data[cursor + 6:cursor + length]
            if kind == 0x2004:
                name_length = struct.unpack_from('<H', body, 16)[0]
                layers.append(body[18:18 + name_length].decode('utf8'))
            elif kind == 0x2005:
                layer, x, y, opacity, cel_kind = struct.unpack_from('<HhhBH', body)
                if cel_kind in (0, 2):
                    w, h = struct.unpack_from('<HH', body, 16)
                    raw = body[20:] if cel_kind == 0 else zlib.decompress(body[20:])
                    image = Image.frombytes('RGBA', (w, h), raw)
                    if opacity != 255:
                        image.putalpha(image.getchannel('A').point(lambda a: a * opacity // 255))
                    cells[layer] = (x, y, image)
                elif cel_kind == 1:
                    link = struct.unpack_from('<H', body, 16)[0]
                    cells[layer] = (x, y, frames[link][layer][2])
                else:
                    raise ValueError('Unsupported cel type')
            cursor += length
        frames.append(cells)
        offset += size
    OUTPUT.mkdir(parents=True, exist_ok=True)
    def compose(frame, indices):
        image = Image.new('RGBA', (width, height))
        for index in indices:
            if index in frames[frame]:
                x, y, cel = frames[frame][index]
                image.alpha_composite(cel, (x, y))
        return image
    def outline(image, name):
        alpha = image.getchannel('A')
        edge = ImageChops.subtract(alpha.filter(ImageFilter.MaxFilter(3)), alpha)
        result = Image.new('RGBA', image.size, (255, 224, 148, 0))
        result.putalpha(edge)
        result.save(OUTPUT / name)
    compose(0, [0, 1, 2, 3, 6]).save(OUTPUT / 'Background.png')
    burger = compose(0, [7]); burger.save(OUTPUT / 'Burger.png')
    burger.crop(burger.getbbox()).save(OUTPUT / 'BurgerItem.png')
    outline(burger, 'BurgerOutline.png')
    switch = Image.new('RGBA', (width, height))
    switch.paste(compose(0, [3]).crop((198, 42, 219, 70)), (198, 42))
    switch.save(OUTPUT / 'SwitchHit.png'); outline(switch, 'SwitchOutline.png')
    switch.save(OUTPUT / 'Switch_00.png')
    compose(1, [5]).save(OUTPUT / 'Switch_01.png')
    compose(3, [5]).save(OUTPUT / 'Switch_02.png')
    compose(3, [5]).save(OUTPUT / 'SwitchOff.png')
    tumbler = compose(0, [4]).crop((277, 90, 293, 126))
    tumbler.save(OUTPUT / 'Tumbler.png'); outline(tumbler, 'TumblerOutline.png')
    for frame in range(count):
        # Layer 11 redraws the emerging burger with the authored oil rim.
        # Keep it throughout the drain; only the final, interactive state
        # omits this copy so picking up the separate burger leaves no remnant.
        foreground = [8, 9, 10, 11, 12, 13] if frame < count - 1 else [8, 9, 10, 12, 13]
        compose(frame, foreground).save(OUTPUT / f'Drain_{frame:02}.png')
    compose(0, [8, 9, 10, 12]).save(OUTPUT / 'Idle.png')
    (OUTPUT / 'Fryer.aseprite.bytes').write_bytes(data)
    manifest = dict(source=str(SOURCE), sha256=hashlib.sha256(data).hexdigest(),
                    width=width, height=height, layers=layers, durationsMs=durations,
                    tumblerCenter=[285, 108], extractionDistancePixels=28,
                    backgroundLayers=[0, 1, 2, 3, 6], foregroundLayers=[8, 9, 10, 11, 12, 13],
                    idleForegroundLayers=[8, 9, 10, 12], switchSourceFrames=[0, 1, 3],
                    burgerOilRimLayer=11, finalForegroundLayers=[8, 9, 10, 12, 13])
    (OUTPUT / 'source-layers.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2)+'\n', encoding='utf8')
    print('Exported layered fryer, 21 authored drain frames; SHA256', manifest['sha256'])

if __name__ == '__main__':
    main()

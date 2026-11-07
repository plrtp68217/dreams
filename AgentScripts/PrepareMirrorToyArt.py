"""Export authored mirror/toy layers without modifying the source Aseprite."""
import hashlib
import json
import sys
from pathlib import Path
from PIL import Image, ImageChops, ImageFilter

sys.dont_write_bytecode = True
from PrepareDrinkVendingArt import read

SOURCE = Path('SourceAssets/Art/FastFood/art/guests/01_faceless_man/04_toy/mirror_toy.aseprite')
OUTPUT = Path('Assets/Sprites/FastFoodService/MirrorToy')


def main():
    data, width, height, layers, frames, durations = read(SOURCE)
    assert (width, height, len(frames)) == (128, 128, 17)
    assert [l['name'] for l in layers] == ['Mirror', 'Sprite Sheet']
    assert all(l['blend'] == 0 and l['opacity'] == 255 for l in layers)

    def layer(frame, index):
        result = Image.new('RGBA', (width, height))
        x, y, cel, opacity = frames[frame][index]
        assert opacity == 255
        result.alpha_composite(cel, (x, y))
        return result

    mirror = layer(0, 0)
    original_toys = [layer(f, 1) for f in range(len(frames))]
    assert all(layer(f, 0).tobytes() == mirror.tobytes() for f in range(len(frames)))
    all_alpha = mirror.getchannel('A')
    # Keep the original common crop: changing the toy must not shift the mirror in the hall.
    for toy in original_toys:
        all_alpha = ImageChops.lighter(all_alpha, toy.getchannel('A'))
    x0, y0, x1, y1 = all_alpha.getbbox()
    world_bounds = (x0 - 3, y0 - 3, x1 + 3, y1 + 3)
    # Half-size toy, anchored at its feet on the mirror shelf. Nearest preserves hard pixel edges.
    toy_scale = .5
    foot_anchor = (42, 106)
    toys = []
    for toy in original_toys:
        reduced = toy.resize((width // 2, height // 2), Image.Resampling.NEAREST)
        image = Image.new('RGBA', (width, height))
        image.alpha_composite(reduced, (foot_anchor[0] // 2, foot_anchor[1] // 2))
        toys.append(image)
    toy_bounds = toys[-1].getbbox()
    OUTPUT.mkdir(parents=True, exist_ok=True)
    mirror.save(OUTPUT / 'Mirror.png')
    for f, toy in enumerate(toys):
        toy.save(OUTPUT / f'Toy_{f:02}.png')
    toys[-1].crop(toy_bounds).save(OUTPUT / 'ToyStanding.png')
    alpha = toys[-1].getchannel('A')
    outline = Image.new('RGBA', (width, height), (255, 224, 148, 0))
    outline.putalpha(ImageChops.subtract(alpha.filter(ImageFilter.MaxFilter(3)), alpha))
    outline.save(OUTPUT / 'ToyOutline.png')
    for name, toy in [('WorldSitting', toys[0]), ('WorldStanding', toys[-1]), ('WorldEmpty', None)]:
        composite = mirror.copy()
        if toy is not None:
            composite.alpha_composite(toy)
        composite.crop(world_bounds).save(OUTPUT / f'{name}.png')
    (OUTPUT / 'MirrorToy.aseprite.bytes').write_bytes(data)
    manifest = dict(source=SOURCE.as_posix(), sha256=hashlib.sha256(data).hexdigest(),
                    width=width, height=height, layers=layers, durationsMs=durations,
                    standingToyBounds=list(toy_bounds), worldBounds=list(world_bounds),
                    mirrorLayer=0, toyLayer=1, worldPadding=3,
                    toyScale=toy_scale, footAnchor=list(foot_anchor), resizeFilter='Nearest',
                    sizing='Toy layer only, scaled around shelf contact; mirror and original world crop unchanged')
    (OUTPUT / 'source-layers.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print('Exported mirror, 17 authored toy frames, standing pickup, outline and three world states.', manifest)


if __name__ == '__main__':
    main()

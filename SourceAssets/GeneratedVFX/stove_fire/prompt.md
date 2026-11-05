# stove_fire

PixelLab; native canvas 32x24; requested frames: 6; recommended FPS: 10.
Anchor: POST /v2/generate-image-v2. Animation: POST /v2/animate-with-text-v3. no_background=true; drift_threshold=0; enhance_prompt=false. Original animation request omitted last_frame; selected steam refinement is documented below. No duplicated padding frames.

## Palette
#762527, #9c2020, #b66e3d, #e3984c, #d8bb7e, #feea9a, #ece0c2

## Anchor prompt
```text
One small isolated low yellow-orange gas stove flame effect for a moody abandoned fast-food restaurant in a 320x180 pixel-art side-scroller. Strict native 32x24 pixels, one-pixel grid. Three short connected flame tongues, warm muted burnt-orange outside and pale yellow core, flat two or three-tone pixel clusters. Base fixed horizontally around x=8..24 y=20, tallest tongue reaches about y=6, complete silhouette contained within x=5..27 y=3..21. No burner, stove, pan, fuel, smoke, sparks, floor, blue flame, black outline, outer halo or cast light. No glow, gradients, antialiasing, blur or dithering. Fully transparent background, effect only.
```

## Animation prompt
```text
Seamless looping small yellow-orange gas flame VFX on a fixed 32x24 canvas. The short flame base stays locked at x8..24 y20. Three tongues alternately lean and curl only one or two pixels; their tips fluctuate gently while the base remains stable. Continuous controlled small combustion, no explosive growth, no whole-sprite translation, zoom, disappearance, smoke, sparks or other objects. Last frame must transition naturally back to the first. Strict native hard pixel clusters with flat muted burnt-orange, yellow and cream colors, no black outline, glow, light rays, gradients, antialiasing or background. Keep transparent exterior.
```

## Export and verification

Actual: 6 unique frames, 32x24, 5 visible colors, alpha 0/255; 10 FPS. Spritesheet is one horizontal row, 192x24, zero padding.
API returned the input reference followed by the generated frames. Input reference discarded; no missing frames duplicated. No resizing, blur, recoloring, interpolation or per-frame positioning applied to generated PNGs. Only invisible RGB bytes of alpha-zero pixels normalized to zero. Initial reference was translated once on the native canvas to the fixed emission anchor; original candidates retained in Temp.

Measured closing transition / mean internal transition: 1.03. Lowest visible row range: 0 px. Lowest-row X center range: 0.00 px. These measurements describe pixel changes and emission stability; they do not replace visual review.

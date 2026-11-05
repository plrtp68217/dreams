# light_flicker

PixelLab; 16x16 native pixels; 8 requested frames; recommended 12 FPS.
Palette: #404040, #808080, #bfbfbf, #ffffff.
Anchor seed 5112506; animation seed 5112606.
Anchor: POST /v2/generate-image-v2. Animation: POST /v2/animate-with-text-v3; no_background=true; enhance_prompt=false. Cyclic effects use identical first/last boundary references; the initial input reference is excluded from output, no duplicate padding frames added.

## Anchor prompt
```text
A technical pixel-art luminance mask, not a lamp or scene: one perfectly flat uniform solid medium-light gray 12x12 square centered on a transparent 16x16 canvas. Crisp square edges, no border, no texture, no icon, no light rays, no bulb and no gradient. Only the uniform square mask, four allowed neutral gray intensity levels. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter. Canvas exactly 16x16 pixels.
```

## Motion prompt
```text
Technical brightness-mask flicker. Keep the exact same centered square footprint and hard edge on every frame. Change only the uniform gray intensity inside the entire square between dark gray, mid gray and white in an irregular brief dim-bright sequence, returning to the starting level. No movement, no shape change, no texture, no drawn lamp, glow, spotlight or gradient. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter.
```

Playback opacity recommendation: Mask luminance, multiply; not a painted light sprite. Guidance only, no Unity edits.

This is a capability test of generic image animation; PixelLab has no dedicated lamp-brightness endpoint. Accept only a stationary mask with useful brightness variation. Otherwise report unsupported and retain raw attempt in Temp.

## Export and quality review

Actual 8 frames (8 unique pixel states), 16x16, 12 FPS. Spritesheet 128x16, one horizontal row, zero padding. Palette: #ffffff.
Input reference excluded from API response; no duplicate padding or interpolated frames were created. Generated image scale and canvas remain unchanged. Invisible RGB of transparent pixels normalized to zero. Initial anchor may have one fixed integer translation (recorded in Temp). Emission-origin registration, where applied, is recorded below.
Technical light mask: API-generated temporal luminance averaged inside the input support, transferred to uniform alpha on that exact static support. No procedural timing curve or missing frames added. Original RGB animation retained in Temp. White RGB, alpha varies per frame; no spatial light painting or gradient.

### Selected API motion request
Source folder: Temp\light_flicker\animation. Seed: 5112606. no_background=True. drift_threshold=1000.
```text
Technical brightness-mask flicker. Keep the exact same centered square footprint and hard edge on every frame. Change only the uniform gray intensity inside the entire square between dark gray, mid gray and white in an irregular brief dim-bright sequence, returning to the starting level. No movement, no shape change, no texture, no drawn lamp, glow, spotlight or gradient. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter.
```

Loop-boundary pixel change / mean internal change: 1.00. Visible bottom-row range: 0 pixels; bottom-row horizontal center range: 0.00 pixels. These are diagnostics, not a claim that moving dust/leaf edges should be static.

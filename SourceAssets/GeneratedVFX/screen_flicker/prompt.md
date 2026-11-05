# screen_flicker

PixelLab; 32x24 native pixels; 6 requested frames; recommended 12 FPS.
Palette: #1e2436, #3e4555, #4d5464, #918e73, #ece0c2.
Anchor seed 5112505; animation seed 5112605.
Anchor: POST /v2/generate-image-v2. Animation: POST /v2/animate-with-text-v3; no_background=true; enhance_prompt=false. Cyclic effects use identical first/last boundary references; the initial input reference is excluded from output, no duplicate padding frames added.

## Anchor prompt
```text
Abstract content-only overlay for an old electronic display: a small flat dark-slate rectangular screen area centered inside 32x24, two thin broken desaturated gray horizontal scan bars and a few isolated pixel glitches. Sharp rectangular footprint, transparent outside. No monitor bezel, hardware, writing, numbers, glow or drawn lighting. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter. Canvas exactly 32x24 pixels.
```

## Motion prompt
```text
A short old-display flicker loop. Keep the rectangular screen footprint absolutely locked. A few thin horizontal scan bars briefly shift by one pixel, dim and reappear in a restrained uneven sequence. No readable text, no moving monitor object, no halo or cast light. Return naturally to the reference display pattern. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter.
```

Playback opacity recommendation: 0.6–1.0. Guidance only, no Unity edits.

## Export and quality review

Actual 6 frames (3 unique pixel states), 32x24, 12 FPS. Spritesheet 192x24, one horizontal row, zero padding. Palette: #1e2436, #918e73.
Input reference excluded from API response; no duplicate padding or interpolated frames were created. Generated image scale and canvas remain unchanged. Invisible RGB of transparent pixels normalized to zero. Initial anchor may have one fixed integer translation (recorded in Temp). Emission-origin registration, where applied, is recorded below.


### Selected API motion request
Source folder: Temp\screen_flicker\animation. Seed: 5112605. no_background=True. drift_threshold=0.
```text
A short old-display flicker loop. Keep the rectangular screen footprint absolutely locked. A few thin horizontal scan bars briefly shift by one pixel, dim and reappear in a restrained uneven sequence. No readable text, no moving monitor object, no halo or cast light. Return naturally to the reference display pattern. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter.
```

Loop-boundary pixel change / mean internal change: 0.00. Visible bottom-row range: 0 pixels; bottom-row horizontal center range: 0.00 pixels. These are diagnostics, not a claim that moving dust/leaf edges should be static.

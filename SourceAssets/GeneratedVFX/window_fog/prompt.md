# window_fog

PixelLab; 64x48 native pixels; 10 requested frames; recommended 6 FPS.
Palette: #4d5464, #716b5e, #918e73, #ece0c2.
Anchor seed 5112507; animation seed 5112607.
Anchor: POST /v2/generate-image-v2. Animation: POST /v2/animate-with-text-v3; no_background=true; enhance_prompt=false. Cyclic effects use identical first/last boundary references; the initial input reference is excluded from output, no duplicate padding frames added.

## Anchor prompt
```text
Two isolated thin low-contrast horizontal ribbons of night fog, a soft-looking effect made exclusively from hard flat muted-gray pixel clusters. Long irregular rounded horizontal wisps separated by transparent gaps, spread over a 64x48 canvas. No window frame, landscape, trees, ground, sky or lighting. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter. Canvas exactly 64x48 pixels.
```

## Motion prompt
```text
Seamless looping light fog outside a window. Two low horizontal fog ribbons gently curl and flow internally, drifting only one pixel at a time. Edges gradually break into transparent gaps and reform; preserve overall placement and scale. Return to the supplied endpoint phase without whole-canvas jumps, sudden flashing or a filled background. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter.
```

Playback opacity recommendation: 0.15–0.30. Guidance only, no Unity edits.

## Export and quality review

Actual 10 frames (10 unique pixel states), 64x48, 6 FPS. Spritesheet 640x48, one horizontal row, zero padding. Palette: #4d5464, #716b5e, #918e73, #ece0c2.
Input reference excluded from API response; no duplicate padding or interpolated frames were created. Generated image scale and canvas remain unchanged. Invisible RGB of transparent pixels normalized to zero. Initial anchor may have one fixed integer translation (recorded in Temp). Emission-origin registration, where applied, is recorded below.


### Selected API motion request
Source folder: Temp\window_fog\animation. Seed: 5112607. no_background=True. drift_threshold=0.
```text
Seamless looping light fog outside a window. Two low horizontal fog ribbons gently curl and flow internally, drifting only one pixel at a time. Edges gradually break into transparent gaps and reform; preserve overall placement and scale. Return to the supplied endpoint phase without whole-canvas jumps, sudden flashing or a filled background. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter.
```

Loop-boundary pixel change / mean internal change: 0.91. Visible bottom-row range: 0 pixels; bottom-row horizontal center range: 15.66 pixels. These are diagnostics, not a claim that moving dust/leaf edges should be static.

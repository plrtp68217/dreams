# steam_large

PixelLab; 48x64 native pixels; 10 requested frames; recommended 8 FPS.
Palette: #4d5464, #716b5e, #918e73, #ece0c2.
Anchor seed 5112500; animation seed 5112600.
Anchor: POST /v2/generate-image-v2. Animation: POST /v2/animate-with-text-v3; no_background=true; enhance_prompt=false. Cyclic effects use identical first/last boundary references; the initial input reference is excluded from output, no duplicate padding frames added.

## Anchor prompt
```text
A broad medium-size isolated plume of pale kitchen steam, several rounded wisps curling upward from an invisible narrow bottom-center emission point. More voluminous than a single thin wisp, but sparse with transparent gaps. Keep the complete plume inside the canvas with generous margins. No pot, pipe or appliance. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter. Canvas exactly 48x64 pixels.
```

## Motion prompt
```text
Seamless looping broad kitchen steam. Lower emission stays fixed at x24 y58, while rounded upper wisps slowly curl upward and disperse and new steam joins from below. Keep fairly constant volume. Return to the supplied endpoint phase without abrupt collapse or reappearance. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter.
```

Playback opacity recommendation: 0.25–0.40. Guidance only, no Unity edits.

## Export and quality review

Actual 10 frames (10 unique pixel states), 48x64, 8 FPS. Spritesheet 480x64, one horizontal row, zero padding. Palette: #4d5464, #918e73, #ece0c2.
Input reference excluded from API response; no duplicate padding or interpolated frames were created. Generated image scale and canvas remain unchanged. Invisible RGB of transparent pixels normalized to zero. Initial anchor may have one fixed integer translation (recorded in Temp). Emission-origin registration, where applied, is recorded below.
Emission origin stabilized by integer-only translation of each generated frame to the requested fixed source. No scaling, blur, interpolation or redraw. Per-frame offsets recorded in validation.json; untouched originals remain in Temp.

### Selected API motion request
Source folder: Temp\steam_large\animation. Seed: 5112600. no_background=True. drift_threshold=0.
```text
Seamless looping broad kitchen steam. Lower emission stays fixed at x24 y58, while rounded upper wisps slowly curl upward and disperse and new steam joins from below. Keep fairly constant volume. Return to the supplied endpoint phase without abrupt collapse or reappearance. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter.
```

Loop-boundary pixel change / mean internal change: 1.07. Visible bottom-row range: 0 pixels; bottom-row horizontal center range: 0.00 pixels. These are diagnostics, not a claim that moving dust/leaf edges should be static.

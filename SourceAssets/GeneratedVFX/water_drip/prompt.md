# water_drip

PixelLab; 16x32 native pixels; 8 requested frames; recommended 12 FPS.
Palette: #3e4555, #4d5464, #918e73, #ece0c2.
Anchor seed 5112502; animation seed 5112602.
Anchor: POST /v2/generate-image-v2. Animation: POST /v2/animate-with-text-v3; no_background=true; enhance_prompt=false. Cyclic effects use identical first/last boundary references; the initial input reference is excluded from output, no duplicate padding frames added.

## Anchor prompt
```text
One tiny pale desaturated water droplet forming at top center x8 y4, about three pixels wide with a small pointed top. Isolated water only, otherwise empty transparent 16x32 canvas with room below for the fall and splash. No faucet, pipe, floor, puddle or other object. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter. Canvas exactly 16x32 pixels.
```

## Motion prompt
```text
One water-drip cycle, not walking or morphing into an object. Small drop forms at x8 y4, elongates, releases, falls vertically down the fixed x8 axis and hits an invisible surface at y28, splitting into three tiny outward splash pixels that disappear. No camera movement or horizontal drift of the falling drop. Finish quiet with transparent space; playback may pause before repeating. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter.
```

Playback opacity recommendation: 1.0. Guidance only, no Unity edits.

## Export and quality review

Actual 8 frames (7 unique pixel states), 16x32, 12 FPS. Spritesheet 128x32, one horizontal row, zero padding. Palette: #3e4555, #918e73, #ece0c2.
Input reference excluded from API response; no duplicate padding or interpolated frames were created. Generated image scale and canvas remain unchanged. Invisible RGB of transparent pixels normalized to zero. Initial anchor may have one fixed integer translation (recorded in Temp). Emission-origin registration, where applied, is recorded below.


### Selected API motion request
Source folder: Temp\water_drip\animation. Seed: 5112602. no_background=True. drift_threshold=0.
```text
One water-drip cycle, not walking or morphing into an object. Small drop forms at x8 y4, elongates, releases, falls vertically down the fixed x8 axis and hits an invisible surface at y28, splitting into three tiny outward splash pixels that disappear. No camera movement or horizontal drift of the falling drop. Finish quiet with transparent space; playback may pause before repeating. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter.
```

Loop-boundary pixel change / mean internal change: 1.25. Visible bottom-row range: 13 pixels; bottom-row horizontal center range: 1.50 pixels. These are diagnostics, not a claim that moving dust/leaf edges should be static.

One-shot animation; an 800 ms pause is used only in GIF preview by holding the final state. PNG sequence has no additional duplicate pause frames. Recommended playback pause 0.5–1.5 s.

# falling_leaves

PixelLab; 48x64 native pixels; 10 requested frames; recommended 8 FPS.
Palette: #716b5e, #957d68, #882c35, #918e73.
Anchor seed 5112508; animation seed 5112608.
Anchor: POST /v2/generate-image-v2. Animation: POST /v2/animate-with-text-v3; no_background=true; enhance_prompt=false. Cyclic effects use identical first/last boundary references; the initial input reference is excluded from output, no duplicate padding frames added.

## Anchor prompt
```text
Four tiny separate dry autumn leaves spread over a transparent 48x64 canvas. Each leaf is a simple three-to-five-pixel curled shape in dull brown, taupe or faded burgundy. Sparse arrangement at different heights with lots of empty space. No branches, tree, window, ground, forest or background. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter. Canvas exactly 48x64 pixels.
```

## Motion prompt
```text
Seamless looping small falling leaves outside a window. Four tiny leaves slowly descend on separate curved paths with a gentle one-pixel sideways sway and small pixel-shape rotations, retaining native size. Individual leaves may wrap at the canvas boundary at different times; never move the whole field together or change canvas placement. Match the cyclic arrangement naturally at the ending phase. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter.
```

Playback opacity recommendation: 0.7–1.0. Guidance only, no Unity edits.

## Export and quality review

Actual 10 frames (10 unique pixel states), 48x64, 8 FPS. Spritesheet 480x64, one horizontal row, zero padding. Palette: #716b5e, #957d68.
Input reference excluded from API response; no duplicate padding or interpolated frames were created. Generated image scale and canvas remain unchanged. Invisible RGB of transparent pixels normalized to zero. Initial anchor may have one fixed integer translation (recorded in Temp). Emission-origin registration, where applied, is recorded below.
Leaf artwork and changing poses come from the API. The generator still reversed some trajectories even without endpoint conditioning; individual connected leaf poses were translated by integer pixels onto a slow downward passage (8 px over 10 frames), with the API sideways motion damped to a small sway. No leaf pixels redrawn, scaled, blurred or interpolated; no frames duplicated. This is motion correction, not a seamless loop. Per-leaf offsets retained in validation.json.
25 opaque pixels mapped to the nearest approved palette color, no dithering, blur or spatial interpolation. Raw API frames retained in Temp.

### Selected API motion request
Source folder: Temp\falling_leaves\animation_fall. Seed: 5112608. no_background=True. drift_threshold=0.
```text
One continuous downward passage of four tiny dry leaves outside a window. Every leaf must descend through the canvas, about one or two pixels lower on each next frame, with slight independent sideways sway and tiny shape rotations. Do NOT orbit, rise, return to the original arrangement or reverse direction to force a loop. Preserve each leaf size and the static 48x64 camera canvas. Only individual leaves move; no global canvas drift. This is a short non-looping falling passage, not a repeating dance. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter.
```

The first cyclic attempt made the leaves rise again to match its endpoint. It remains in Temp/falling_leaves/animation. Selected animation is deliberately a one-way downward passage, not a seamless loop. No direction reversal or duplicate frames were used to fake a cycle.

Loop-boundary pixel change / mean internal change: 2.04. Visible bottom-row range: 9 pixels; bottom-row horizontal center range: 5.00 pixels. These are diagnostics, not a claim that moving dust/leaf edges should be static.

One-shot animation; an 800 ms pause is used only in GIF preview by holding the final state. PNG sequence has no additional duplicate pause frames. Recommended playback pause 0.5–1.5 s.

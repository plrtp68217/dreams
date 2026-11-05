# fryer_smoke

PixelLab; 32x48 native pixels; 8 requested frames; recommended 8 FPS.
Palette: #1e2436, #3e4555, #484e5e, #4d5464.
Anchor seed 5112501; animation seed 5112601.
Anchor: POST /v2/generate-image-v2. Animation: POST /v2/animate-with-text-v3; no_background=true; enhance_prompt=false. Cyclic effects use identical first/last boundary references; the initial input reference is excluded from output, no duplicate padding frames added.

## Anchor prompt
```text
An isolated dense dark-gray smoke plume from an unseen faulty fryer. Three compact rounded charcoal and desaturated slate smoke lobes rise from a narrow bottom-center source. Thick silhouette, two or three flat dark-gray tones, small transparent holes, complete contained plume. No fryer, fire, appliance or face. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter. Canvas exactly 32x48 pixels.
```

## Motion prompt
```text
Seamless looping heavy dark-gray fryer smoke. Source stays fixed at x16 y42. Dense rounded lobes roll upward slowly, a top lobe breaks apart while a new lower lobe replaces it. Keep overall scale and dark palette stable, no explosive growth, no sudden whole-plume translation. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter.
```

Playback opacity recommendation: 0.55–0.75. Guidance only, no Unity edits.

## Export and quality review

Actual 8 frames (8 unique pixel states), 32x48, 8 FPS. Spritesheet 256x48, one horizontal row, zero padding. Palette: #1e2436, #3e4555, #484e5e.
Input reference excluded from API response; no duplicate padding or interpolated frames were created. Generated image scale and canvas remain unchanged. Invisible RGB of transparent pixels normalized to zero. Initial anchor may have one fixed integer translation (recorded in Temp). Emission-origin registration, where applied, is recorded below.
Emission origin stabilized by integer-only translation of each generated frame to the requested fixed source. No scaling, blur, interpolation or redraw. Per-frame offsets recorded in validation.json; untouched originals remain in Temp.

### Selected API motion request
Source folder: Temp\fryer_smoke\animation. Seed: 5112601. no_background=True. drift_threshold=0.
```text
Seamless looping heavy dark-gray fryer smoke. Source stays fixed at x16 y42. Dense rounded lobes roll upward slowly, a top lobe breaks apart while a new lower lobe replaces it. Keep overall scale and dark palette stable, no explosive growth, no sudden whole-plume translation. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter.
```

Loop-boundary pixel change / mean internal change: 0.61. Visible bottom-row range: 0 pixels; bottom-row horizontal center range: 1.00 pixels. These are diagnostics, not a claim that moving dust/leaf edges should be static.

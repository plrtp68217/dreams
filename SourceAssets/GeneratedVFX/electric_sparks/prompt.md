# electric_sparks

PixelLab; 24x24 native pixels; 6 requested frames; recommended 16 FPS.
Palette: #e3984c, #feea9a, #ece0c2, #4d5464.
Anchor seed 5112503; animation seed 5112603.
Anchor: POST /v2/generate-image-v2. Animation: POST /v2/animate-with-text-v3; no_background=true; enhance_prompt=false. Cyclic effects use identical first/last boundary references; the initial input reference is excluded from output, no duplicate padding frames added.

## Anchor prompt
```text
One tiny isolated burst of electrical sparks centered at x12 y12. Four very short angular one-pixel white-cream and muted amber strokes with a tiny central spark, sparse and irregular. Full burst inside a 24x24 canvas with transparent margins. No equipment, cable, light halo or background. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter. Canvas exactly 24x24 pixels.
```

## Motion prompt
```text
One brief electrical spark burst at fixed center x12 y12. A tiny flash branches into three or four short angular pixel sparks, expands only a few pixels, fragments and extinguishes. No constant beam, star icon or object. End dark and allow an external pause between bursts. No glow or cast light. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter.
```

Playback opacity recommendation: 1.0. Guidance only, no Unity edits.

## Export and quality review

Actual 6 frames (4 unique pixel states), 24x24, 16 FPS. Spritesheet 144x24, one horizontal row, zero padding. Palette: #e3984c.
Input reference excluded from API response; no duplicate padding or interpolated frames were created. Generated image scale and canvas remain unchanged. Invisible RGB of transparent pixels normalized to zero. Initial anchor may have one fixed integer translation (recorded in Temp). Emission-origin registration, where applied, is recorded below.
1 opaque pixels mapped to the nearest approved palette color, no dithering, blur or spatial interpolation. Raw API frames retained in Temp.

### Selected API motion request
Source folder: Temp\electric_sparks\animation. Seed: 5112603. no_background=True. drift_threshold=0.
```text
One brief electrical spark burst at fixed center x12 y12. A tiny flash branches into three or four short angular pixel sparks, expands only a few pixels, fragments and extinguishes. No constant beam, star icon or object. End dark and allow an external pause between bursts. No glow or cast light. Strict native one-pixel-grid pixel art for DREAMS, a dark abandoned fast-food restaurant at 320x180 native resolution. Effect only, transparent background, hard opaque pixel clusters, limited supplied palette. No anti-aliasing, gradients, dithering, blur, glow, drawn background lighting, characters, text, furniture or other objects. Static camera and fixed canvas; no zoom or global position/scale jitter.
```

Loop-boundary pixel change / mean internal change: 2.32. Visible bottom-row range: 1 pixels; bottom-row horizontal center range: 0.17 pixels. These are diagnostics, not a claim that moving dust/leaf edges should be static.

One-shot animation; an 800 ms pause is used only in GIF preview by holding the final state. PNG sequence has no additional duplicate pause frames. Recommended playback pause 0.5–1.5 s.

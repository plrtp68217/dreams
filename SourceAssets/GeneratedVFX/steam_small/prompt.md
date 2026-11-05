# steam_small

PixelLab; native canvas 32x48; requested frames: 8; recommended FPS: 8.
Anchor: POST /v2/generate-image-v2. Animation: POST /v2/animate-with-text-v3. no_background=true; drift_threshold=0; enhance_prompt=false. Original animation request omitted last_frame; selected steam refinement is documented below. No duplicated padding frames.

## Palette
#4d5464, #716b5e, #918e73, #ece0c2

## Anchor prompt
```text
One small isolated plume of pale gray kitchen steam, a VFX sprite for a moody abandoned fast-food restaurant in a 320x180 pixel-art side-scroller. Native 32x48 pixels, one-pixel grid. Two small connected wisps curl upward from a narrow invisible fixed source at bottom center, x=16 y=42. Sparse clear soft-looking cloud shapes formed ONLY with hard flat pixel clusters and four muted colors, no outlined tube or smoke stack. Compact plume within x=7..25 and y=8..42, generous transparent margins. No container, appliance, ground, black outline, lighting, glow, blur, gradient, dithering or antialiasing. Fully transparent background. Only steam, not a character or object.
```

## Animation prompt
```text
Seamless looping small steam VFX, fixed 32x48 canvas and static camera. Narrow emission source stays exactly at bottom center x16 y42. Two light wisps gently curl and rise, upper wisp disperses into transparency while new steam emerges at the same source. Smooth small one-pixel changes between adjacent frames. Keep plume scale, center and origin stable; no whole-sprite translation, no zoom, no sudden bursts or flickering noise. Last frame should lead smoothly back into the first with a similar plume distribution, not a fully vanished ending. Strict hard-edged native pixel clusters, four muted gray and cream tones, no black outlines, gradients, antialiasing, glow, drawn lighting or extra objects. Transparent exterior.
```

If alpha is binary, use global playback opacity about 0.25–0.40 in Unity. This is guidance only; no Unity changes are made.

## Export and verification

Actual: 8 unique frames, 32x48, 4 visible colors, alpha 0/255; 8 FPS. Spritesheet is one horizontal row, 256x48, zero padding.
API returned the input reference followed by the generated frames. Input reference discarded; no missing frames duplicated. No resizing, blur, recoloring, interpolation or per-frame positioning applied to generated PNGs. Only invisible RGB bytes of alpha-zero pixels normalized to zero. Initial reference was translated once on the native canvas to the fixed emission anchor; original candidates retained in Temp.

### Selected animation refinement
The first animation had a more visible loop boundary and remains in Temp/steam_small/animation. Selected second animation uses first_frame and last_frame as the same cyclic boundary reference (not duplicated padding in the export), seed 5112402. Actual action:
```text
Looping kitchen steam VFX, static 32x48 pixel canvas. The same beginning and ending plume is an intentional cyclic boundary, not a still image: animate a gentle continuous upward curl through the intermediate frames and return smoothly to this phase. Keep the emission origin fixed at x16 y42 in EVERY frame. The lower wisp persists while one upper wisp slowly curls and rises then dissolves; new steam joins smoothly. Maintain fairly even visible steam volume, never an abrupt disappearance or sudden bright puff. Tiny one-pixel evolution between frames. End shape and palette must match the supplied reference. No whole-sprite drift, zoom, jitter, black outlines, gradients, glow, background, appliance or other object. Strict hard-edged muted gray and cream pixel clusters, transparent exterior.
```
Playback opacity recommendation: 0.25–0.40; no Unity settings were edited.

Measured closing transition / mean internal transition: 0.85. Lowest visible row range: 0 px. Lowest-row X center range: 0.00 px. These measurements describe pixel changes and emission stability; they do not replace visual review.

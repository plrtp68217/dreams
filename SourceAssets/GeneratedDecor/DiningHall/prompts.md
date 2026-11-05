# DiningHall — prompts and settings

Budget approved 2026-11-05: up to USD 1.50; eleven new PixelLab requests, no paid retries. One existing napkin holder reused with no charge.
Previous observed charges: USD 0.08–0.115/request; estimate for this set about USD 0.9–1.4. Actual charges recorded from API responses.
Endpoint: POST https://api.pixellab.ai/v2/generate-image-v2. no_background=true; max_colors=12; same palette as ../palette.md.
Style: ../Cashier/receipt_printer.png. Environment reference: existing FastFoodHall - 5.png. Copy outline, detail, shading; color_palette=false because target_palette is explicit.
No lighting, glow, gradients, antialiasing, dithering or animations. Final sprites retain native dimensions.

## Existing assets
- napkin_dispenser.png: exact copy of ../Cashier/napkin_holder.png, no new request.
- Existing TrashBin.png, PottedPlant.png and SecurityCamera.png were inspected. These use different colors or density; the camera image is a multi-camera sheet. They are design references for new versions at the approved palette and native sizes. Originals remain unchanged.

## abandoned_food_tray.png
Size: 40x24; seed: 5112100; palette cap: 12.
New PixelLab generation; one request.
```text
One abandoned dark burgundy rectangular food tray holding exactly a crumpled cream burger wrapper, an empty red paper cup and one dirty folded napkin. All three items are inside the tray; a single compact tabletop arrangement. Narrow top surface visible, low side elevation, no food left, no table. Strict native-resolution pixel art for DREAMS, abandoned fast-food dining hall with dark red booth seats and night forest windows. Match the approved cashier sprites: native one-pixel grid, restrained detail, front-facing side-scroller elevation, no isometric perspective. Dark plum-brown one-pixel outlines, desaturated slate metal, aged cream paper, muted taupe and faded burgundy accents; only the supplied twelve colors. Two or three flat local material tones. No lighting effects or directional shine, no gradients, anti-aliasing, dithering, blur, glow or external cast shadows. One isolated complete prop centered with transparent padding, hard opaque pixels and fully transparent exterior. No scene, background, floor, extra props, watermark or logo. Canvas exactly 40x24 native pixels.
```

## forgotten_paper_cup.png
Size: 16x24; seed: 5112101; palette cap: 12.
New PixelLab generation; one request.
```text
One lonely faded red disposable paper drink cup with a narrow cream rim and one visibly bent cream drinking straw. Empty battered cup, slightly dented tapered silhouette, a tiny muted cream stripe. Upright, no lid, no lettering, no puddle. Strict native-resolution pixel art for DREAMS, abandoned fast-food dining hall with dark red booth seats and night forest windows. Match the approved cashier sprites: native one-pixel grid, restrained detail, front-facing side-scroller elevation, no isometric perspective. Dark plum-brown one-pixel outlines, desaturated slate metal, aged cream paper, muted taupe and faded burgundy accents; only the supplied twelve colors. Two or three flat local material tones. No lighting effects or directional shine, no gradients, anti-aliasing, dithering, blur, glow or external cast shadows. One isolated complete prop centered with transparent padding, hard opaque pixels and fully transparent exterior. No scene, background, floor, extra props, watermark or logo. Canvas exactly 16x24 native pixels.
```

## napkin_dispenser.png
Size: 24x24; seed: 5112102; palette cap: 12.
Reused; no API call.
```text
One old compact slate metal tabletop napkin dispenser with folded cream napkins. Reuse the already approved identical cashier napkin holder; no new generation. Strict native-resolution pixel art for DREAMS, abandoned fast-food dining hall with dark red booth seats and night forest windows. Match the approved cashier sprites: native one-pixel grid, restrained detail, front-facing side-scroller elevation, no isometric perspective. Dark plum-brown one-pixel outlines, desaturated slate metal, aged cream paper, muted taupe and faded burgundy accents; only the supplied twelve colors. Two or three flat local material tones. No lighting effects or directional shine, no gradients, anti-aliasing, dithering, blur, glow or external cast shadows. One isolated complete prop centered with transparent padding, hard opaque pixels and fully transparent exterior. No scene, background, floor, extra props, watermark or logo. Canvas exactly 24x24 native pixels.
```

## forgotten_toy.png
Size: 16x16; seed: 5112103; palette cap: 12.
New PixelLab generation; one request.
```text
One tiny strange plastic toy from an old restaurant kids meal: a squat burgundy rabbit-like mascot with two uneven upright ears, a cream face, two tiny dark dot eyes and an unnervingly blank expression. Clearly a small molded plastic figurine with a flat base, no realistic fur, no packaging, no accessories. Strict native-resolution pixel art for DREAMS, abandoned fast-food dining hall with dark red booth seats and night forest windows. Match the approved cashier sprites: native one-pixel grid, restrained detail, front-facing side-scroller elevation, no isometric perspective. Dark plum-brown one-pixel outlines, desaturated slate metal, aged cream paper, muted taupe and faded burgundy accents; only the supplied twelve colors. Two or three flat local material tones. No lighting effects or directional shine, no gradients, anti-aliasing, dithering, blur, glow or external cast shadows. One isolated complete prop centered with transparent padding, hard opaque pixels and fully transparent exterior. No scene, background, floor, extra props, watermark or logo. Canvas exactly 16x16 native pixels.
```

## restaurant_trash_bin.png
Size: 24x32; seed: 5112104; palette cap: 12.
New PixelLab generation; one request.
```text
One small worn restaurant waste bin for the floor beside red booth seats. Squat rectangular desaturated charcoal-slate body with faded burgundy upper rim, a single dark recessed rectangular flap opening near the top, subtle worn corners and two sparse scratches. Closed intact silhouette, no loose rubbish, no lettering, no wheels. Strict native-resolution pixel art for DREAMS, abandoned fast-food dining hall with dark red booth seats and night forest windows. Match the approved cashier sprites: native one-pixel grid, restrained detail, front-facing side-scroller elevation, no isometric perspective. Dark plum-brown one-pixel outlines, desaturated slate metal, aged cream paper, muted taupe and faded burgundy accents; only the supplied twelve colors. Two or three flat local material tones. No lighting effects or directional shine, no gradients, anti-aliasing, dithering, blur, glow or external cast shadows. One isolated complete prop centered with transparent padding, hard opaque pixels and fully transparent exterior. No scene, background, floor, extra props, watermark or logo. Canvas exactly 24x32 native pixels.
```

## spilled_paper_cup.png
Size: 24x16; seed: 5112105; palette cap: 12.
New PixelLab generation; one request.
```text
One overturned red paper cup lying on its side with a single small irregular flat puddle of dark muted brown drink beside its mouth. Cream rim, short bent cream straw lying against the cup. The small puddle is opaque flat pixel clusters, not a shadow or reflection. No extra cups, no floor, no droplets far from the main object. Strict native-resolution pixel art for DREAMS, abandoned fast-food dining hall with dark red booth seats and night forest windows. Match the approved cashier sprites: native one-pixel grid, restrained detail, front-facing side-scroller elevation, no isometric perspective. Dark plum-brown one-pixel outlines, desaturated slate metal, aged cream paper, muted taupe and faded burgundy accents; only the supplied twelve colors. Two or three flat local material tones. No lighting effects or directional shine, no gradients, anti-aliasing, dithering, blur, glow or external cast shadows. One isolated complete prop centered with transparent padding, hard opaque pixels and fully transparent exterior. No scene, background, floor, extra props, watermark or logo. Canvas exactly 24x16 native pixels.
```

## withered_floor_plant.png
Size: 32x48; seed: 5112106; palette cap: 12.
New PixelLab generation; one request.
```text
One small floor plant in a worn muted brown rectangular terracotta pot. Sparse drooping broad leaves on three thin brown stems; several leaves are partly withered with curled brown tips. Use only dull taupe and muted brown for the dying leaves, no bright green, no flowers. Thin open gaps between leaves, readable silhouette, no ground or external shadow. Strict native-resolution pixel art for DREAMS, abandoned fast-food dining hall with dark red booth seats and night forest windows. Match the approved cashier sprites: native one-pixel grid, restrained detail, front-facing side-scroller elevation, no isometric perspective. Dark plum-brown one-pixel outlines, desaturated slate metal, aged cream paper, muted taupe and faded burgundy accents; only the supplied twelve colors. Two or three flat local material tones. No lighting effects or directional shine, no gradients, anti-aliasing, dithering, blur, glow or external cast shadows. One isolated complete prop centered with transparent padding, hard opaque pixels and fully transparent exterior. No scene, background, floor, extra props, watermark or logo. Canvas exactly 32x48 native pixels.
```

## window_sticker.png
Size: 24x24; seed: 5112107; palette cap: 12.
New PixelLab generation; one request.
```text
One old faded advertisement decal of a small burger on an aged cream rounded rectangular sticker. A clear tiny burger icon with cream bun, dull brown patty, faded burgundy filling. One lower corner peeling upward and a slightly chipped edge. Empty transparent exterior; the cream decal itself remains opaque. No words, no glass, no window frame. Strict native-resolution pixel art for DREAMS, abandoned fast-food dining hall with dark red booth seats and night forest windows. Match the approved cashier sprites: native one-pixel grid, restrained detail, front-facing side-scroller elevation, no isometric perspective. Dark plum-brown one-pixel outlines, desaturated slate metal, aged cream paper, muted taupe and faded burgundy accents; only the supplied twelve colors. Two or three flat local material tones. No lighting effects or directional shine, no gradients, anti-aliasing, dithering, blur, glow or external cast shadows. One isolated complete prop centered with transparent padding, hard opaque pixels and fully transparent exterior. No scene, background, floor, extra props, watermark or logo. Canvas exactly 24x24 native pixels.
```

## security_camera.png
Size: 32x24; seed: 5112108; palette cap: 12.
New PixelLab generation; one request.
```text
One old wall-mounted CCTV security camera pointing toward the lower left, a small rectangular cream and taupe housing with a dark slate front lens and a single dull red indicator pixel. Short attached dark metal wall bracket extends to the right. Indicator is a flat burgundy pixel, absolutely no emitted light or glow. One camera only, no wall, no wires outside the silhouette. Strict native-resolution pixel art for DREAMS, abandoned fast-food dining hall with dark red booth seats and night forest windows. Match the approved cashier sprites: native one-pixel grid, restrained detail, front-facing side-scroller elevation, no isometric perspective. Dark plum-brown one-pixel outlines, desaturated slate metal, aged cream paper, muted taupe and faded burgundy accents; only the supplied twelve colors. Two or three flat local material tones. No lighting effects or directional shine, no gradients, anti-aliasing, dithering, blur, glow or external cast shadows. One isolated complete prop centered with transparent padding, hard opaque pixels and fully transparent exterior. No scene, background, floor, extra props, watermark or logo. Canvas exactly 32x24 native pixels.
```

## childrens_drawing.png
Size: 40x32; seed: 5112109; palette cap: 12.
New PixelLab generation; one request.
```text
One small aged cream sheet of paper taped at its upper two corners. On the sheet is a primitive childish crayon drawing of a restaurant: simple red roof, two square windows, one door, and four stick human figures. Three figures are short and one is conspicuously twice as tall with elongated thin legs. Uneven one-pixel drawn lines, dark plum and faded red marks on cream paper. No realistic people, no scene outside the paper, no text. Strict native-resolution pixel art for DREAMS, abandoned fast-food dining hall with dark red booth seats and night forest windows. Match the approved cashier sprites: native one-pixel grid, restrained detail, front-facing side-scroller elevation, no isometric perspective. Dark plum-brown one-pixel outlines, desaturated slate metal, aged cream paper, muted taupe and faded burgundy accents; only the supplied twelve colors. Two or three flat local material tones. No lighting effects or directional shine, no gradients, anti-aliasing, dithering, blur, glow or external cast shadows. One isolated complete prop centered with transparent padding, hard opaque pixels and fully transparent exterior. No scene, background, floor, extra props, watermark or logo. Canvas exactly 40x32 native pixels.
```

## promotional_poster.png
Size: 32x48; seed: 5112110; palette cap: 12.
New PixelLab generation; one request.
```text
One old rectangular fast food advertising poster on opaque aged cream paper, scuffed edges, two tiny burgundy pin marks. A small simple appetizing burger illustration occupies only the upper third, built from cream bun, taupe patty and dark red filling in flat pixel clusters. The lower half is completely blank cream paper reserved for later text. A narrow faded burgundy border. No generated lettering, no writing marks in the blank area, no wall. Strict native-resolution pixel art for DREAMS, abandoned fast-food dining hall with dark red booth seats and night forest windows. Match the approved cashier sprites: native one-pixel grid, restrained detail, front-facing side-scroller elevation, no isometric perspective. Dark plum-brown one-pixel outlines, desaturated slate metal, aged cream paper, muted taupe and faded burgundy accents; only the supplied twelve colors. Two or three flat local material tones. No lighting effects or directional shine, no gradients, anti-aliasing, dithering, blur, glow or external cast shadows. One isolated complete prop centered with transparent padding, hard opaque pixels and fully transparent exterior. No scene, background, floor, extra props, watermark or logo. Canvas exactly 32x48 native pixels.
```

## damaged_window_blinds.png
Size: 64x48; seed: 5112111; palette cap: 12.
New PixelLab generation; one request.
```text
One standalone set of old horizontal window blinds. A narrow dark upper mounting rail and nine separated long thin dull cream and taupe horizontal slats, each a flat straight pixel strip. Three slats are visibly crooked or bent with stepped pixel angles and one has a small missing end. Two thin dark vertical support cords. Transparent gaps between slats and transparent exterior. No window, no wall, no forest, no sunlight or light rays, no bottom scene. Strict native-resolution pixel art for DREAMS, abandoned fast-food dining hall with dark red booth seats and night forest windows. Match the approved cashier sprites: native one-pixel grid, restrained detail, front-facing side-scroller elevation, no isometric perspective. Dark plum-brown one-pixel outlines, desaturated slate metal, aged cream paper, muted taupe and faded burgundy accents; only the supplied twelve colors. Two or three flat local material tones. No lighting effects or directional shine, no gradients, anti-aliasing, dithering, blur, glow or external cast shadows. One isolated complete prop centered with transparent padding, hard opaque pixels and fully transparent exterior. No scene, background, floor, extra props, watermark or logo. Canvas exactly 64x48 native pixels.
```

# Final results

11 new generation requests; 1 reused asset. No paid retries. API-reported cost: USD 0.915, within approved USD 1.50.
All returned variants retained in Temp/<item>/candidate_XX.png with a candidates.png sheet.
Selected images copied at native dimensions without scaling or alpha modification. One technical correction: PixelLab omitted the requested red camera indicator in all candidates, so pixel (9,13) of security_camera.png was set to the existing palette color #882c35, no glow or added lighting. The untouched API source remains Temp/security_camera/candidate_00.png. No other pixels were redrawn or recolored. All final PNGs: expected native sizes, RGBA, binary alpha 0/255, only the approved palette, at most 12 visible colors.

| File | Native size | Colors | Candidate |
|---|---|---|---|
| abandoned_food_tray.png | 40x24 | 8 | 52 |
| forgotten_paper_cup.png | 16x24 | 7 | 25 |
| napkin_dispenser.png | 24x24 | 7 | reused |
| forgotten_toy.png | 16x16 | 7 | 0 |
| restaurant_trash_bin.png | 24x32 | 8 | 56 |
| spilled_paper_cup.png | 24x16 | 8 | 43 |
| withered_floor_plant.png | 32x48 | 6 | 5 |
| window_sticker.png | 24x24 | 9 | 27 |
| security_camera.png | 32x24 | 9 | 0 |
| childrens_drawing.png | 40x32 | 7 | 44 |
| promotional_poster.png | 32x48 | 5 | 18 |
| damaged_window_blinds.png | 64x48 | 7 | 8 |

Contact sheet: preview.png (nearest-neighbor enlargement). Validation, hashes and reported cost: Temp/validation.json. Original cashier assets verified byte-identical. No Unity imports, scene changes or commits.

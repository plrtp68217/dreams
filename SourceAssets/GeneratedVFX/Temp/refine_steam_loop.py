import base64
from generate_tests import ROOT, TESTS, submit_and_poll, write

source=ROOT/'Temp/steam_small/first_frame.png'
encoded=dict(base64=base64.b64encode(source.read_bytes()).decode())
payload=dict(first_frame=encoded,last_frame=encoded,
    action='Looping kitchen steam VFX, static 32x48 pixel canvas. The same beginning and ending plume is an intentional cyclic boundary, not a still image: animate a gentle continuous upward curl through the intermediate frames and return smoothly to this phase. Keep the emission origin fixed at x16 y42 in EVERY frame. The lower wisp persists while one upper wisp slowly curls and rises then dissolves; new steam joins smoothly. Maintain fairly even visible steam volume, never an abrupt disappearance or sudden bright puff. Tiny one-pixel evolution between frames. End shape and palette must match the supplied reference. No whole-sprite drift, zoom, jitter, black outlines, gradients, glow, background, appliance or other object. Strict hard-edged muted gray and cream pixel clusters, transparent exterior.',
    frame_count=8,seed=5112402,no_background=True,drift_threshold=0,enhance_prompt=False)
result=submit_and_poll(ROOT/'Temp/steam_small/animation_loop',payload,'animate-with-text-v3',lambda last:bool(last.get('images')))
if result:
    write(ROOT/'Temp/steam_small/animation_loop/result.json',result)
    print('Steam loop refinement ready; image count',len(result['images']),flush=True)

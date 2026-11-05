"""PixelLab HTTP bridge. Credential lives only in this running process; never writes it.

Start interactively, enter an API key through getpass, then send one JSON command per
line: {"method":"GET","path":"/balance"}, or specify a JSON payload file and output
directory. Only generated artwork and non-secret response metadata are saved.
"""
import base64
import getpass
import io
import json
from pathlib import Path
import sys
import urllib.request
import urllib.error

BASE='https://api.pixellab.ai/v2'

def payload_images(value):
    if isinstance(value,dict):
        if '$image' in value:
            from PIL import Image
            im=Image.open(value['$image']).convert('RGBA')
            maximum=value.get('max',256)
            im.thumbnail((maximum,maximum),Image.Resampling.NEAREST)
            buffer=io.BytesIO();im.save(buffer,format='PNG')
            return {'type':'base64','base64':base64.b64encode(buffer.getvalue()).decode()}
        return {k:payload_images(v) for k,v in value.items()}
    if isinstance(value,list):return [payload_images(v) for v in value]
    return value

def save_images(value,out,prefix='image'):
    saved=[]
    if isinstance(value,dict):
        encoded=value.get('base64')
        if isinstance(encoded,str) and len(encoded)>100:
            data=base64.b64decode(encoded.split(',')[-1]);path=out/(prefix+'.png');path.write_bytes(data)
            saved.append(str(path));return saved
        for k,v in value.items():saved+=save_images(v,out,prefix+'-'+k)
    elif isinstance(value,list):
        for i,v in enumerate(value):saved+=save_images(v,out,prefix+'-'+str(i))
    return saved

def redact_images(value):
    if isinstance(value,dict):return {k:('[image omitted]' if k=='base64' else redact_images(v)) for k,v in value.items()}
    if isinstance(value,list):return [redact_images(v) for v in value]
    return value

def main():
    sys.stdout.reconfigure(encoding='utf-8')
    secret=getpass.getpass('PixelLab session credential: ')
    print('SESSION_READY',flush=True)
    try:
        for line in sys.stdin:
            try:
                command=json.loads(line)
                if command.get('close'):break
                path=command['path']
                if not path.startswith('/') or '://' in path:raise ValueError('Relative PixelLab API path required')
                payload=payload_images(json.loads(Path(command['payload']).read_text(encoding='utf-8'))) if command.get('payload') else command.get('body')
                request=urllib.request.Request(BASE+path,data=json.dumps(payload).encode() if payload is not None else None,
                    headers={'Authorization':'Bearer '+secret,'Content-Type':'application/json'},method=command.get('method','GET'))
                with urllib.request.urlopen(request,timeout=240) as response:result=json.load(response)
                files=[]
                if command.get('output'):
                    out=Path(command['output']);out.mkdir(parents=True,exist_ok=True)
                    files=save_images(result,out)
                    (out/'response.json').write_text(json.dumps(redact_images(result),ensure_ascii=False,indent=2),encoding='utf-8')
                print(json.dumps({'success':True,'result':redact_images(result),'files':files},ensure_ascii=False),flush=True)
            except urllib.error.HTTPError as e:
                print(json.dumps({'success':False,'status':e.code,'error':e.read().decode(errors='replace').replace(secret,'[redacted]')[:3000]}),flush=True)
            except Exception as e:
                print(json.dumps({'success':False,'error':str(e).replace(secret,'[redacted]')}),flush=True)
    finally:
        secret=None
        print('SESSION_CLOSED',flush=True)

if __name__=='__main__':main()

from pathlib import Path
from PIL import Image

def components(im):
    remaining = {(x,y) for y in range(im.height) for x in range(im.width) if im.getpixel((x,y))[3]}
    result = []
    while remaining:
        seed = remaining.pop()
        group = {seed}
        todo = [seed]
        while todo:
            x,y = todo.pop()
            for dx,dy in ((-1,-1),(0,-1),(1,-1),(-1,0),(1,0),(-1,1),(0,1),(1,1)):
                other = (x+dx,y+dy)
                if other in remaining:
                    remaining.remove(other)
                    group.add(other)
                    todo.append(other)
        result.append(group)
    return sorted(result,key=len,reverse=True)

if __name__ == '__main__':
    root = Path(__file__).resolve().parent
    for name in ['wall_clock','straw_container','service_phone','paper_bag_box','receipt_paper_rolls']:
        print(name)
        found=[]
        for path in (root/name).glob('candidate_*.png'):
            im=Image.open(path).convert('RGBA')
            parts=components(im)
            main=parts[0]
            b=(min(x for x,y in main),min(y for x,y in main),max(x for x,y in main)+1,max(y for x,y in main)+1)
            if b[0]>0 and b[1]>0 and b[2]<im.width and b[3]<im.height:
                found.append((path.stem,b,[len(part) for part in parts]))
        print(found[:24])

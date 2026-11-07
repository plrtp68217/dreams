"""Extract independent leaf poses without changing any source pixels or files."""
import hashlib
import json
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "SourceAssets/GeneratedVFX/falling_leaves"
OUTPUT = ROOT / "Assets/VFX/FallingLeaves"


def components(image):
    remaining = {(x, y) for y in range(image.height) for x in range(image.width)
                 if image.getpixel((x, y))[3]}
    groups = []
    while remaining:
        group = {remaining.pop()}
        pending = list(group)
        while pending:
            x, y = pending.pop()
            for dx in (-1, 0, 1):
                for dy in (-1, 0, 1):
                    neighbour = x + dx, y + dy
                    if neighbour in remaining:
                        remaining.remove(neighbour)
                        group.add(neighbour)
                        pending.append(neighbour)
        groups.append(group)
    return sorted(groups, key=lambda group: sum(y for x, y in group) / len(group))


def main():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    cell = 16
    atlas = Image.new("RGBA", (10 * cell, 3 * cell))
    manifest = {"source": SOURCE.relative_to(ROOT).as_posix(), "cell": cell,
                "columns": 10, "rows": 3, "fps": 8,
                "method": "8-connected leaves, ordered top to bottom; integer bounding-box centering removes baked translation; original RGBA pixels preserved",
                "frames": []}
    for frame_index, path in enumerate(sorted((SOURCE / "frames").glob("frame_*.png"))):
        image = Image.open(path).convert("RGBA")
        groups = components(image)
        assert len(groups) == 3, (path, len(groups))
        row = {"file": path.relative_to(ROOT).as_posix(),
               "sha256": hashlib.sha256(path.read_bytes()).hexdigest(), "leaves": []}
        for leaf_index, group in enumerate(groups):
            left, top = min(x for x, y in group), min(y for x, y in group)
            width = max(x for x, y in group) - left + 1
            height = max(y for x, y in group) - top + 1
            assert width <= cell - 2 and height <= cell - 2
            offset_x = (cell - width) // 2
            offset_y = (cell - height) // 2
            for x, y in group:
                atlas.putpixel((frame_index * cell + offset_x + x - left,
                                leaf_index * cell + offset_y + y - top), image.getpixel((x, y)))
            row["leaves"].append({"bounds": [left, top, width, height], "pixels": len(group)})
        manifest["frames"].append(row)
    atlas.save(OUTPUT / "LeafPoses.png")
    manifest["atlas_sha256"] = hashlib.sha256((OUTPUT / "LeafPoses.png").read_bytes()).hexdigest()
    (OUTPUT / "source-manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print("Extracted 30 independent poses into LeafPoses.png (160x48); source pixels preserved.")


if __name__ == "__main__":
    main()

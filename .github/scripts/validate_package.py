"""Validate package metadata, asset references, and the distribution layout."""

import json
from pathlib import Path
import re


REPO = Path(__file__).resolve().parents[2]
PACKAGE = REPO / "Packages/com.kamahir0.debug-ui"


def main():
    manifest = json.loads((PACKAGE / "package.json").read_text())
    assert manifest["name"] == PACKAGE.name
    assert re.fullmatch(r"\d+\.\d+\.\d+(?:-[\w.-]+)?", manifest["version"])
    assert manifest["unity"] == "6000.3"
    for name in ("com.unity.modules.uielements", "com.unity.modules.vectorgraphics"):
        assert manifest["dependencies"].get(name) == "1.0.0", f"Missing dependency: {name}"
    for key, filename in (("documentationUrl", "README.md"), ("changelogUrl", "CHANGELOG.md"),
                          ("licensesUrl", "LICENSE")):
        assert (PACKAGE / filename).is_file()
        expected = f"https://github.com/kamahir0-lilja/DebugUI/blob/main/Packages/{PACKAGE.name}/{filename}"
        assert manifest[key] == expected, f"Stale {key}"
    assert (REPO / "README.md").read_bytes() == (PACKAGE / "README.md").read_bytes()
    assert (REPO / "LICENSE").read_bytes() == (PACKAGE / "LICENSE").read_bytes()
    for sample in manifest.get("samples", []):
        assert sample["path"].startswith("Samples~/")
        assert (PACKAGE / sample["path"]).is_dir()

    guids = {}
    for meta in PACKAGE.rglob("*.meta"):
        target = meta.with_suffix("")
        assert target.exists(), f"Orphan meta: {meta}"
        match = re.search(r"^guid: ([0-9a-f]{32})$", meta.read_text(), re.MULTILINE)
        assert match, f"Invalid meta: {meta}"
        guid = match[1]
        assert guid not in guids, f"Duplicate GUID: {meta} and {guids.get(guid)}"
        guids[guid] = meta

    for asset in PACKAGE.rglob("*"):
        relative = asset.relative_to(PACKAGE)
        if asset.suffix == ".meta" or any(part.endswith("~") for part in relative.parts):
            continue
        assert Path(str(asset) + ".meta").is_file(), f"Missing meta: {asset}"
        if asset.suffix == ".asmdef":
            json.loads(asset.read_text())
        if asset.suffix not in (".asset", ".asmdef", ".uxml", ".uss", ".tss"):
            continue
        content = asset.read_text()
        for guid in re.findall(r"(?:guid[:=]\s*|GUID:)([0-9a-f]{32})", content):
            assert guid in guids or guid.startswith("0000000000000000"), f"External GUID in {asset}: {guid}"
        for matches in re.findall(r'url\("([^"\n]+)"\)|src="([^"\n]+)"', content):
            reference = next(value for value in matches if value)
            if "://" not in reference:
                assert (asset.parent / reference).is_file(), f"Missing asset in {asset}: {reference}"
    print("Package metadata, .meta files, GUIDs, and relative references are valid.")


if __name__ == "__main__":
    main()

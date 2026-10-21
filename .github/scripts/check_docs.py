"""Check documentation links, specification IDs, and record classifications."""

from collections import defaultdict
from pathlib import Path
import re
import sys
from urllib.parse import unquote, urlsplit


REPO = Path(__file__).resolve().parents[2]
REQUIREMENT = re.compile(r"\bDBG-[A-Z]+-\d{3}\b")
DEFINITION = re.compile(r"^### (DBG-[A-Z]+-\d{3})\b", re.MULTILINE)
LINK = re.compile(r"\[[^\]\n]*\]\(([^)\n]+)\)")


def prose(text):
    """Exclude code examples and template comments from structural checks."""
    text = re.sub(r"<!--.*?-->", "", text, flags=re.DOTALL)
    lines, fence = [], None
    for line in text.splitlines():
        match = re.match(r"^\s*(`{3,}|~{3,})", line)
        if match:
            marker = match[1]
            if fence is None:
                fence = marker
            elif marker[0] == fence[0] and len(marker) >= len(fence):
                fence = None
            continue
        if fence is None:
            lines.append(line)
    return "\n".join(lines)


def validate(root):
    files = {root / "AGENTS.md", root / "README.md", root / "Packages/com.kamahir0.debug-ui/README.md"}
    files.update((root / "docs").rglob("*.md"))
    files.update((root / ".agents/skills").rglob("SKILL.md"))
    errors, definitions, references = [], defaultdict(list), defaultdict(set)
    for path in sorted(files):
        if not path.is_file():
            errors.append(f"Missing entry point: {path.relative_to(root)}")
            continue
        name = path.relative_to(root).as_posix()
        text = prose(path.read_text())
        category = path.relative_to(root).parts[:2]
        if path.name not in {"README.md", "_template.md"}:
            expected_status = {
                ("docs", "knowledge"): "Reference",
                ("docs", "evidence"): "Historical",
                ("docs", "archive"): "Archived",
            }.get(category)
            if expected_status and not re.search(r"^Status: " + expected_status + r"\s*$", text, re.MULTILINE):
                errors.append(f"Missing or invalid document category status ({expected_status}): {name}")
            if category == ("docs", "knowledge") and not re.search(
                r"^Verification: (Current|Historical|Unknown)\s*$", text, re.MULTILINE
            ):
                errors.append(f"Missing or invalid knowledge verification state: {name}")
        for raw in LINK.findall(text):
            destination = raw.strip()
            if destination.startswith("<") and ">" in destination:
                destination = destination[1:destination.index(">")]
            else:
                destination = destination.split()[0]
            parts = urlsplit(destination)
            if parts.scheme or parts.netloc or not parts.path:
                continue
            target = (path.parent / unquote(parts.path)).resolve()
            if not target.exists():
                errors.append(f"Broken link in {name}: {destination}")
            elif re.fullmatch(r"dbg-[a-z]+-\d{3}", parts.fragment) and target.is_file():
                expected = parts.fragment.upper()
                if expected not in DEFINITION.findall(prose(target.read_text())):
                    errors.append(f"Broken requirement anchor in {name}: {destination}")
        for requirement in REQUIREMENT.findall(text):
            references[requirement].add(name)
        if path.parent == root / "docs/specs" and path.name not in {"README.md", "_template.md"}:
            status = re.search(r"^Status: (\w+)\s*$", text, re.MULTILINE)
            if status is None or status[1] not in {"Draft", "Proposed", "Approved", "Implemented"}:
                errors.append(f"Missing or invalid specification status: {name}")
            for requirement in DEFINITION.findall(text):
                definitions[requirement].append(name)
                if not re.search(r"^\|\s*" + re.escape(requirement) + r"\s*\|", text, re.MULTILINE):
                    errors.append(f"No acceptance row for {requirement}: {name}")
        if path.parent == root / "docs/spec-changes" and path.name not in {"README.md", "_template.md"}:
            status = re.search(r"^Status: (\w+)\s*$", text, re.MULTILINE)
            if status is None or status[1] not in {"Draft", "Proposed", "Approved", "Applied", "Rejected"}:
                errors.append(f"Missing or invalid change status: {name}")
    for requirement, owners in sorted(definitions.items()):
        if len(owners) > 1:
            errors.append(f"Duplicate requirement {requirement}: {', '.join(owners)}")
    for requirement, sources in sorted(references.items()):
        if requirement not in definitions:
            errors.append(f"Undefined requirement {requirement}: {', '.join(sorted(sources))}")
    return errors


def main():
    errors = validate(REPO)
    if errors:
        print("\n".join(errors), file=sys.stderr)
        return 1
    print("Documentation links, specification references, and document classifications are valid.")
    return 0


if __name__ == "__main__":
    sys.exit(main())

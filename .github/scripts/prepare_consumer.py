"""Create a fresh project that installs this checkout through a local Git URL."""

import argparse
import json
from pathlib import Path
import shutil
import subprocess


PACKAGE_NAME = "com.kamahir0.debug-ui"
REPO = Path(__file__).resolve().parents[2]


def git(*args):
    return subprocess.check_output(["git", *map(str, args)], text=True).strip()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--unity-version", required=True)
    parser.add_argument("--snapshot-url", help="Git URL visible to Unity when the project runs in a container")
    args = parser.parse_args()
    output = args.output.resolve()
    output.mkdir(parents=True, exist_ok=False)
    source = output / "source"
    package_path = Path("Packages") / PACKAGE_NAME
    shutil.copytree(REPO / package_path, source / package_path)
    git("init", "--quiet", "--initial-branch=main", source)
    git("-C", source, "add", "Packages")
    git("-C", source, "-c", "user.name=Package verification", "-c",
        "user.email=package-verification@example.invalid", "commit", "--quiet", "-m", "Package snapshot")
    revision = git("-C", source, "rev-parse", "HEAD")
    snapshot = output / "package.git"
    git("clone", "--quiet", "--bare", source, snapshot)
    shutil.rmtree(source)

    project = output / "Consumer"
    (project / "Packages").mkdir(parents=True)
    (project / "ProjectSettings").mkdir()
    shutil.copytree(REPO / package_path / "Samples~/BasicDebugMenu", project / "Assets/Development")
    manifest = {
        "dependencies": {
            PACKAGE_NAME: f"{args.snapshot_url or snapshot.as_uri()}?path=/{package_path.as_posix()}#{revision}",
            "com.unity.test-framework": "1.4.6",
        },
        "testables": [PACKAGE_NAME],
    }
    (project / "Packages/manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
    (project / "ProjectSettings/ProjectVersion.txt").write_text(f"m_EditorVersion: {args.unity_version}\n")
    print(project)


if __name__ == "__main__":
    main()

#!/usr/bin/env python3
"""Define Numos package families, dependencies, projects, and version sources."""

from __future__ import annotations

import argparse
from pathlib import Path
import re


PACKAGE_DEPENDENCIES = {
    "Nod3r.Collections": (),
    "Nod3r.Types": ("Nod3r.Collections",),
    "Nod3r.Solver": ("Nod3r.Types",),
    "Nod3r.API": ("Nod3r.Solver",),
    "Nod3r.Examples": ("Nod3r.API",),
}
PACKAGE_VERSION_FILES = {
    "Nod3r.Collections": Path("src/Nod3r.Solver/Version.props"),
    "Nod3r.Types": Path("src/Nod3r.Solver/Version.props"),
    "Nod3r.Solver": Path("src/Nod3r.Solver/Version.props"),
    "Nod3r.API": Path("src/Nod3r.Solver/Version.props"),
    "Nod3r.Examples": Path("src/Nod3r.Solver/Version.props"),
}
PACKAGE_FAMILIES = {
    "solver": (
    "Nod3r.Collections",
    "Nod3r.Types",
    "Nod3r.Solver",
    "Nod3r.API",
    "Nod3r.Examples",
    ),
}

def package_ids(family: str) -> tuple[str, ...]:
    if family == "all":
        return tuple(PACKAGE_DEPENDENCIES)
    return PACKAGE_FAMILIES[family]


def project_file(package_id: str) -> Path:
    return Path("src") / package_id / f"{package_id}.csproj"


def package_version(root: Path, package_id: str) -> str:
    relative_path = PACKAGE_VERSION_FILES[package_id]
    text = (root / relative_path).read_text(encoding="utf-8-sig")
    match = re.search(r"<Version>([^<]+)</Version>", text)
    if match is None:
        raise ValueError(f"missing Version in {relative_path}")
    return match.group(1)


def package_file_name(root: Path, package_id: str, suffix: str = "nupkg") -> str:
    normalized_version = package_version(root, package_id).partition("+")[0]
    return f"{package_id}.{normalized_version}.{suffix}"


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("family", choices=("all", *PACKAGE_FAMILIES))
    args = parser.parse_args()
    for package_id in package_ids(args.family):
        print(package_id)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

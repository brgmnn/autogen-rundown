#!/usr/bin/env python3
"""Bump Thunderstore dependency pins in manifest.json and thunderstore.toml.

Queries the Thunderstore API for the latest version of every dependency pinned in
thunderstore.toml's [package.dependencies] table and rewrites both files in place.
Always moves to the latest published version -- Thunderstore authors don't reliably
follow semver, so there is no version range logic here.

Usage:
    ./update-dependencies.py [--check] [--summary-file PATH]

    --check           Report available updates without writing any files.
    --summary-file    Write a markdown summary (used as the PR body in CI).

When $GITHUB_OUTPUT is set, `has_updates`, `count` and `title` are written to it.

Exit codes:
    0  Success (with or without updates found).
    1  manifest.json and thunderstore.toml disagree, or every API lookup failed.
"""

import argparse
import json
import os
import re
import sys
import time
import urllib.error
import urllib.request

API_URL = "https://thunderstore.io/api/experimental/package/{namespace}/{name}/"
PACKAGE_URL = "https://thunderstore.io/package/{namespace}/{name}/"

# Thunderstore rejects the default urllib user agent with a 403.
USER_AGENT = "autogen-rundown-dep-updater"

RETRIES = 3
RETRY_BACKOFF = 2

ROOT = os.path.dirname(os.path.abspath(__file__))
MANIFEST = os.path.join(ROOT, "manifest.json")
TOML = os.path.join(ROOT, "thunderstore.toml")


def read_toml_dependencies(text):
    """Return [(full_name, version)] from the [package.dependencies] table."""
    match = re.search(
        r"^\[package\.dependencies\][^\n]*\n(.*?)(?=^\[|\Z)", text, re.M | re.S
    )

    if match is None:
        sys.exit("error: thunderstore.toml has no [package.dependencies] table")

    return re.findall(r'^\s*([\w\-]+)\s*=\s*"([^"]+)"', match.group(1), re.M)


def read_manifest_dependencies(text):
    """Return {full_name: version} from the manifest's dependencies array."""
    deps = {}

    for entry in json.loads(text)["dependencies"]:
        full_name, _, version = entry.rpartition("-")
        deps[full_name] = version

    return deps


def check_files_agree(toml_deps, manifest_deps):
    """Both files pin the same versions, or we bail out for a human to fix."""
    errors = []

    for full_name, version in toml_deps:
        if full_name not in manifest_deps:
            errors.append(f"  {full_name}: missing from manifest.json")
        elif manifest_deps[full_name] != version:
            errors.append(
                f"  {full_name}: thunderstore.toml has {version}, "
                f"manifest.json has {manifest_deps[full_name]}"
            )

    toml_names = {full_name for full_name, _ in toml_deps}

    for full_name in manifest_deps:
        if full_name not in toml_names:
            errors.append(f"  {full_name}: missing from thunderstore.toml")

    if errors:
        sys.exit(
            "error: manifest.json and thunderstore.toml disagree:\n"
            + "\n".join(sorted(errors))
        )


def fetch_package(namespace, name):
    """Fetch package metadata from Thunderstore, retrying on transient failures."""
    url = API_URL.format(namespace=namespace, name=name)
    request = urllib.request.Request(
        url, headers={"User-Agent": USER_AGENT, "Accept": "application/json"}
    )
    last_error = None

    for attempt in range(RETRIES):
        try:
            with urllib.request.urlopen(request, timeout=30) as response:
                return json.load(response)
        except (urllib.error.URLError, TimeoutError, json.JSONDecodeError) as error:
            last_error = error

            if attempt < RETRIES - 1:
                time.sleep(RETRY_BACKOFF * (attempt + 1))

    raise RuntimeError(f"{url}: {last_error}")


def check_updates(toml_deps):
    """Return (updates, failures) for every pinned dependency."""
    updates = []
    failures = []

    for full_name, current in toml_deps:
        namespace, _, name = full_name.partition("-")

        try:
            package = fetch_package(namespace, name)
        except RuntimeError as error:
            print(f"::warning::Could not check {full_name}: {error}")
            failures.append(full_name)
            continue

        latest = package["latest"]["version_number"]

        if package.get("is_deprecated"):
            print(f"::warning::{full_name} is deprecated on Thunderstore")

        status = "->" if latest != current else "  "
        print(f"  {full_name:45} {current:10} {status} {latest}")

        if latest != current:
            updates.append(
                {
                    "full_name": full_name,
                    "namespace": namespace,
                    "name": name,
                    "from": current,
                    "to": latest,
                }
            )

    return updates, failures


def apply_updates(updates):
    """Rewrite both files in place, changing only the pinned version strings."""
    with open(TOML, encoding="utf-8") as file:
        toml = file.read()

    with open(MANIFEST, encoding="utf-8") as file:
        manifest = file.read()

    for update in updates:
        full_name = update["full_name"]

        toml, count = re.subn(
            rf'^(\s*{re.escape(full_name)}\s*=\s*)"{re.escape(update["from"])}"',
            rf'\g<1>"{update["to"]}"',
            toml,
            count=1,
            flags=re.M,
        )

        if count != 1:
            sys.exit(f"error: could not update {full_name} in thunderstore.toml")

        old_entry = f'"{full_name}-{update["from"]}"'

        if manifest.count(old_entry) != 1:
            sys.exit(f"error: could not update {full_name} in manifest.json")

        manifest = manifest.replace(old_entry, f'"{full_name}-{update["to"]}"', 1)

    # Parses after rewriting, so we never commit a broken manifest.
    json.loads(manifest)

    with open(TOML, "w", encoding="utf-8") as file:
        file.write(toml)

    with open(MANIFEST, "w", encoding="utf-8") as file:
        file.write(manifest)


def build_title(updates):
    if len(updates) == 1:
        update = updates[0]
        return f"Bump {update['full_name']} from {update['from']} to {update['to']}"

    return f"Bump {len(updates)} Thunderstore dependencies"


def build_summary(updates, failures):
    lines = [
        "Daily Thunderstore dependency check. "
        "Each mod is bumped to its latest published version.",
        "",
        "| Package | From | To |",
        "| --- | --- | --- |",
    ]

    for update in updates:
        url = PACKAGE_URL.format(namespace=update["namespace"], name=update["name"])
        lines.append(
            f"| [{update['full_name']}]({url}) | `{update['from']}` | `{update['to']}` |"
        )

    if failures:
        lines += [
            "",
            "> [!WARNING]",
            "> Could not check the following packages: "
            + ", ".join(f"`{name}`" for name in failures),
        ]

    return "\n".join(lines) + "\n"


def write_outputs(**outputs):
    path = os.environ.get("GITHUB_OUTPUT")

    if not path:
        return

    with open(path, "a", encoding="utf-8") as file:
        for key, value in outputs.items():
            file.write(f"{key}={value}\n")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--check", action="store_true", help="report updates without writing files"
    )
    parser.add_argument("--summary-file", help="write a markdown summary to this path")
    args = parser.parse_args()

    with open(TOML, encoding="utf-8") as file:
        toml_deps = read_toml_dependencies(file.read())

    with open(MANIFEST, encoding="utf-8") as file:
        manifest_deps = read_manifest_dependencies(file.read())

    check_files_agree(toml_deps, manifest_deps)

    print(f":: Checking {len(toml_deps)} Thunderstore dependencies")
    updates, failures = check_updates(toml_deps)

    if len(failures) == len(toml_deps):
        sys.exit("error: every Thunderstore lookup failed")

    if not updates:
        print(":: All dependencies are up to date")
        write_outputs(has_updates="false", count=0)
        return

    title = build_title(updates)

    if args.check:
        print(f":: {len(updates)} update(s) available, no files written")
    else:
        apply_updates(updates)
        print(f":: Applied {len(updates)} update(s) to manifest.json, thunderstore.toml")

    if args.summary_file:
        with open(args.summary_file, "w", encoding="utf-8") as file:
            file.write(build_summary(updates, failures))

    write_outputs(has_updates="true", count=len(updates), title=title)


if __name__ == "__main__":
    main()

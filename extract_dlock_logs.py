#!/usr/bin/env python3
"""Extract DLock Decipherer log file text content from TextDataBlock JSON."""

import json
import os
import re

CS_FILE = "AutogenRundown/src/DataBlocks/Logs/DLockDecipherer.cs"
JSON_FILE = "GameData/GameData_TextDataBlock_bin.json"
OUTPUT_DIR = "docs/dlock-decipherer-logs"

# Parse CS file for LogFile entries
with open(CS_FILE) as f:
    cs_text = f.read()

# Match each LogFile entry: field name, FileName, PersistentId
pattern = re.compile(
    r'public static readonly LogFile (\w+)\s*=\s*new LogFile\s*\{[^}]*'
    r'FileName\s*=\s*"([^"]+)"[^}]*'
    r'PersistentId\s*=\s*(\d+)u',
    re.DOTALL
)

entries = pattern.findall(cs_text)
print(f"Found {len(entries)} LogFile entries")

# Parse JSON and build persistentID -> English lookup
with open(JSON_FILE) as f:
    data = json.load(f)

text_lookup = {}
for block in data["Blocks"]:
    pid = block.get("persistentID")
    if pid is not None:
        text_lookup[pid] = block.get("English", "")

print(f"Loaded {len(text_lookup)} text blocks")

# Process each entry
written = 0
missing = 0

for field_name, file_name, persistent_id_str in entries:
    persistent_id = int(persistent_id_str)

    # Extract rundown number from field name prefix (e.g. R7B1_Z92 -> 7)
    rundown_match = re.match(r'R(\d+)', field_name)
    if not rundown_match:
        print(f"  WARNING: Could not extract rundown from {field_name}, skipping")
        continue
    rundown_num = rundown_match.group(1)

    # Build output filename: everything before _Z is the prefix
    # R7B1_Z92 -> R7B1, R6D4_Z412_1 -> R6D4
    prefix_match = re.match(r'(R\d+\w+?)_Z\d+', field_name)
    if prefix_match:
        prefix = prefix_match.group(1)
    else:
        # Handle entries like R6D1 (no _Z suffix)
        prefix = field_name

    out_filename = f"{prefix}_{file_name}.txt"
    out_dir = os.path.join(OUTPUT_DIR, f"rundown-{rundown_num}")
    out_path = os.path.join(out_dir, out_filename)

    # Look up text
    text = text_lookup.get(persistent_id)
    if text is None:
        print(f"  WARNING: PersistentId {persistent_id} not found for {field_name}")
        missing += 1
        continue

    # Post-process: replace literal \r\n with real newlines, literal \r with nothing
    text = text.replace("\\r\\n", "\n").replace("\\r", "")

    os.makedirs(out_dir, exist_ok=True)
    with open(out_path, "w") as f:
        f.write(text)

    written += 1

print(f"\nWrote {written} files, {missing} missing text entries")

#!/usr/bin/env python3
"""Add or remove a single word in the Tier-A/B/C/D/E uplink word lists.

The word's length determines which list it touches:
  4 letters -> FourLetterWords  (10 per line)
  5 letters -> FiveLetterWords  (9  per line)
  6 letters -> SixLetterWords   (8  per line)
  7 letters -> SevenLetterWords (7  per line)

The list is re-sorted alphabetically and rewritten with the per-line count
above. Indent is 8 spaces (matches existing style).

Usage:
    ./manage_terminal_words.py add <word>
    ./manage_terminal_words.py remove <word>

Examples:
    ./manage_terminal_words.py add bovine
    ./manage_terminal_words.py remove crappy
"""
import argparse
import re
import sys
from pathlib import Path

FILE = Path(__file__).parent / "AutogenRundown" / "src" / "Patches" / "TerminalUplink.cs"

# length -> (array name, words per line)
LISTS = {
    4: ("FourLetterWords", 10),
    5: ("FiveLetterWords", 9),
    6: ("SixLetterWords", 8),
    7: ("SevenLetterWords", 7),
}

INDENT = " " * 8  # 8 spaces


def load_array(content: str, array_name: str):
    """Return (match, header, body, footer) for the named string[] array."""
    pattern = re.compile(
        rf'(public static string\[\] {array_name} => new\[\]\s*\{{)(.*?)(\}};)',
        re.DOTALL,
    )
    m = pattern.search(content)
    if not m:
        sys.exit(f"Could not locate {array_name} in {FILE}")
    return m


def format_body(words, per_line: int) -> str:
    """Format a sorted word list back into the source-file body."""
    lines = []
    for i in range(0, len(words), per_line):
        chunk = words[i : i + per_line]
        parts = [f'"{w}"' for w in chunk]
        lines.append(INDENT + ", ".join(parts) + ",")
    if lines:
        lines[-1] = lines[-1].rstrip(",")
    return "\n" + "\n".join(lines) + "\n    "


def main() -> None:
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("action", choices=["add", "remove"])
    p.add_argument("word")
    args = p.parse_args()

    word = args.word.strip().lower()
    if not word.isalpha():
        sys.exit(f"Word must be alphabetic (got: {args.word!r})")
    if len(word) not in LISTS:
        sys.exit(f"Word length must be 4-7 (got {len(word)} letters: {word!r})")

    array_name, per_line = LISTS[len(word)]
    content = FILE.read_text()
    m = load_array(content, array_name)
    body = m.group(2)
    words = re.findall(r'"([a-z]+)"', body)

    # Dedupe (collapse any existing duplicates) and sort.
    unique = sorted(set(words))
    dupes_removed = len(words) - len(unique)

    if args.action == "add":
        if word in unique:
            print(f"'{word}' is already in {array_name} (count: {len(unique)}); nothing to add.")
            return
        unique.append(word)
        unique.sort()
        verb = "Added"
    else:  # remove
        if word not in unique:
            print(f"'{word}' is not in {array_name} (count: {len(unique)}); nothing to remove.")
            return
        unique.remove(word)
        verb = "Removed"

    new_body = format_body(unique, per_line)
    new_content = content[: m.start(2)] + new_body + content[m.end(2) :]
    FILE.write_text(new_content)

    msg = f"{verb} '{word}' in {array_name}. New count: {len(unique)}."
    if dupes_removed:
        msg += f" (Also collapsed {dupes_removed} pre-existing duplicate{'s' if dupes_removed > 1 else ''}.)"
    print(msg)


if __name__ == "__main__":
    main()

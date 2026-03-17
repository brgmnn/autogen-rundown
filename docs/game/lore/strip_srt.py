# import re, sys

# with open(sys.argv[1]) as f:
#     content = f.read()

# content = re.sub(r'^\d+\s*$', '', content, flags=re.MULTILINE)
# content = re.sub(r'\d{2}:\d{2}:\d{2},\d{3} --> \d{2}:\d{2}:\d{2},\d{3}', '', content)
# content = re.sub(r'<[^>]+>', '', content)   # strip HTML tags
# content = re.sub(r'\n{2,}', '\n', content)  # collapse blank lines → single newline

# print(content.strip())

import re, json, sys
from datetime import time

def srt_time_to_seconds(ts):
    h, m, s_ms = ts.split(':')
    s, ms = s_ms.split(',')
    return int(h) * 3600 + int(m) * 60 + int(s) + int(ms) / 1000

def parse_srt(path):
    with open(path) as f:
        content = f.read()
    blocks = re.split(r'\n\s*\n', content.strip())
    cues = []
    for block in blocks:
        lines = block.splitlines()
        ts_line = next((l for l in lines if '-->' in l), None)
        if not ts_line:
            continue
        start_str = ts_line.split('-->')[0].strip()
        start_sec = srt_time_to_seconds(start_str)
        text_lines = [
            l for l in lines
            if not re.match(r'^\d+$', l.strip())
            and '-->' not in l
            and l.strip()
        ]
        text = re.sub(r'<[^>]+>', '', ' '.join(text_lines)).strip()
        if text:
            cues.append((start_sec, text))
    return cues

def parse_chapters(info_json_path):
    with open(info_json_path) as f:
        data = json.load(f)
    chapters = data.get('chapters', [])
    if not chapters:
        print("No chapters found in video metadata.", file=sys.stderr)
        sys.exit(1)
    return chapters  # each has start_time, end_time, title

def assign_chapter(start_sec, chapters):
    for ch in reversed(chapters):
        if start_sec >= ch['start_time']:
            return ch['title']
    return chapters[0]['title']

srt_path = sys.argv[1]        # e.g. video.en.srt
info_path = sys.argv[2]       # e.g. video.info.json

cues = parse_srt(srt_path)
chapters = parse_chapters(info_path)

# Group cues by chapter
from collections import defaultdict, OrderedDict
grouped = OrderedDict((ch['title'], []) for ch in chapters)

for start_sec, text in cues:
    ch_title = assign_chapter(start_sec, chapters)
    grouped[ch_title].append(text)

# Output
for title, lines in grouped.items():
    if lines:
        print(f"\n## {title}\n")
        print('\n'.join(lines))

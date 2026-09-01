#!/bin/bash

# Thunderstore rejects CHANGELOG.md files over 100 KB, so only the most recent
# releases are included. Override with CHANGELOG_MAX_ENTRIES.
max_entries="${CHANGELOG_MAX_ENTRIES:-80}"

# Fetch one extra release so we can tell whether anything was cut off.
releases=$(gh release list --exclude-drafts --json name,publishedAt,tagName --limit "$((max_entries + 1))")

# Generate the header
cat <<-EOF > CHANGELOG.md
# Changelog

See more at https://github.com/brgmnn/autogen-rundown

EOF

# Iterate over the tags and generate their respective change logs
echo "$releases" | jq -c ".[:$max_entries][]" | while read -r release; do
  name=$(echo "$release" | jq -r '.name')
  publishedAt=$(echo "$release" | jq -r '.publishedAt')
  releasedAt=$(date -d "$publishedAt" +"%B %d, %Y")
  tag=$(echo "$release" | jq -r '.tagName')

  if [ "$publishedAt" = "0001-01-01T00:00:00Z" ]; then
    releasedAt=$(date +"%B %d, %Y")
  fi

  echo "-> $name ($tag)"

    cat <<-EOF >> CHANGELOG.md

## [$name](https://github.com/brgmnn/autogen-rundown/releases/tag/$tag) — $releasedAt

$(gh release view $tag --json body -q '.body' | tr -d '\r')

EOF
done

# Point readers at GitHub when older releases were cut off
if [ "$(echo "$releases" | jq 'length')" -gt "$max_entries" ]; then
  cat <<-EOF >> CHANGELOG.md

---

Older releases are listed on the [GitHub releases page](https://github.com/brgmnn/autogen-rundown/releases).
EOF
fi

size=$(wc -c < CHANGELOG.md | tr -d ' ')
if [ "$size" -gt 100000 ]; then
  echo "CHANGELOG.md is ${size} bytes, over Thunderstore's 100 KB limit. Lower CHANGELOG_MAX_ENTRIES." >&2
  exit 1
fi

#!/bin/bash

releases=$(gh release list --json name,publishedAt,tagName --limit 10000)

# Generate the header
cat <<-EOF > CHANGELOG.md
# Changelog

See more at https://github.com/brgmnn/autogen-rundown

EOF

# Iterate over the tags and generate their respective change logs
echo "$releases" | jq -c '.[]' | while read -r release; do
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

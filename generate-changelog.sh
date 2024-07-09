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
  tag=$(echo "$release" | jq -r '.tagName')

  echo "-> $name ($tag)"

    cat <<-EOF >> CHANGELOG.md

## [$name](https://github.com/brgmnn/autogen-rundown/releases/tag/$tag) — $(date -d "2024-06-26T06:12:44Z" +"%B %d, %Y")

$(gh release view $tag --json body -q '.body')

EOF
done

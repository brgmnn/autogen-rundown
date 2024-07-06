#!/bin/bash

tags=$(gh release list --json tagName | jq -c '.[].tagName' | tr -d '"')

# Generate the header
cat <<-EOF > CHANGELOG.md
# Changelog

See more at https://github.com/brgmnn/autogen-rundown

EOF

# Iterate over the tags and generate their respective change logs
for tag in $tags; do
  echo "-> $tag"

  release=$(gh release view $tag --json name,publishedAt)
  name=$(echo $release | jq -r '.name')
  publishedAt=$(echo $release | jq -r '.publishedAt')

  cat <<-EOF >> CHANGELOG.md

## $name

$(gh release view $tag --json body -q '.body')

EOF

done

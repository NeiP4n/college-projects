#!/bin/bash
# Populate all C# course lessons with diverse content blocks
BASE="http://localhost:5003/api/builder/lessons"

post() {
  local lessonId=$1
  local type=$2
  local content=$3
  curl -s -X POST "$BASE/$lessonId/blocks" \
    -H "Content-Type: application/json" \
    -d "{\"type\":\"$type\",\"content\":$(echo "$content" | python3 -c 'import json,sys; print(json.dumps(sys.stdin.read()))')}" > /dev/null
}
echo "done"

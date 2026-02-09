#!/bin/bash

user_count=150
api_status="online"

# Use jq to construct the JSON object
json_output=$(jq -n \
  --arg count "$user_count" \
  --arg status "$api_status" \
  '{ "user_count": ($count | tonumber), "api_status": $status }')

echo "$json_output"

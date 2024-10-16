#!/bin/sh

# Get the value of the environment variable API_URL
api_url=$(printenv API_URL)

# Check if API_URL is set
if [ -z "$api_url" ]; then
    echo "API_URL environment variable is not set."
    exit 1
fi

# Find files and replace occurrences of IOT_IS_BACKEND_API_URL with API_URL
echo "Replacing IOT_IS_BACKEND_API_URL with $api_url in JS and CSS files..."

find /usr/share/nginx/html -type f \( -name '*.js' -o -name '*.css' \) -exec sed -i "s|IOT_IS_BACKEND_API_URL|${api_url}|g" '{}' +

echo "Replacement completed."
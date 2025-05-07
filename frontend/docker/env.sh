#!/bin/sh

# Get the value of the environment variable API_URL
api_url=$(printenv API_URL)

# Check if API_URL is set
if [ -z "$api_url" ]; then
    echo "API_URL environment variable is not set."
    exit 1
fi

# Get the value of the environment variable IOT_ONLY, defaulting to "false" if not set
iot_only=$(printenv IOT_ONLY)
if [ -z "$iot_only" ]; then
    iot_only="false"
fi

# Replace IOT_IS_BACKEND_API_URL with the value of API_URL in JS and CSS files
echo "Replacing IOT_IS_BACKEND_API_URL with $api_url in JS and CSS files..."
find /usr/share/nginx/html -type f \( -name '*.js' -o -name '*.css' \) -exec sed -i "s|IOT_IS_BACKEND_API_URL|${api_url}|g" '{}' +

# Replace IOT_IS_IOT_ONLY with the value of IOT_ONLY in JS and CSS files
echo "Replacing IOT_IS_IOT_ONLY with $iot_only in JS and CSS files..."
find /usr/share/nginx/html -type f \( -name '*.js' -o -name '*.css' \) -exec sed -i "s|IOT_IS_IOT_ONLY|${iot_only}|g" '{}' +

echo "Replacement completed."

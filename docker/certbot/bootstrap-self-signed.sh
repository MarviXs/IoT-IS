#!/bin/sh
set -eu

apk add --no-cache openssl >/dev/null

mkdir -p /emqx-certs

if [ -s /emqx-certs/server.pem ] && [ -s /emqx-certs/server.key ]; then
  exit 0
fi

openssl req \
  -x509 \
  -nodes \
  -newkey rsa:2048 \
  -days 1 \
  -subj "/CN=${MQTT_DOMAIN}" \
  -keyout /emqx-certs/server.key \
  -out /emqx-certs/server.pem

chmod 600 /emqx-certs/server.key
chmod 644 /emqx-certs/server.pem

# Also populate EMQX's default TLS filenames so any built-in listener that
# still references them does not crash before Let's Encrypt is issued.
cp /emqx-certs/server.pem /emqx-certs/cert.pem
cp /emqx-certs/server.key /emqx-certs/key.pem
cp /emqx-certs/server.pem /emqx-certs/cacert.pem

chmod 600 /emqx-certs/key.pem
chmod 644 /emqx-certs/cert.pem /emqx-certs/cacert.pem

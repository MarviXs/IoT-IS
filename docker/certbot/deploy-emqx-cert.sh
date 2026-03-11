#!/bin/sh
set -eu

lineage_name="${MQTT_DOMAIN}"
if [ -n "${RENEWED_LINEAGE:-}" ]; then
  lineage_name="${RENEWED_LINEAGE##*/}"
fi

live_dir="/etc/letsencrypt/live/${lineage_name}"
cert_src="${live_dir}/fullchain.pem"
key_src="${live_dir}/privkey.pem"

if [ ! -f "$cert_src" ] || [ ! -f "$key_src" ]; then
  echo "Missing renewed certificate files in ${live_dir}" >&2
  exit 1
fi

mkdir -p /emqx-certs

cp "$cert_src" /emqx-certs/server.pem.tmp
cp "$key_src" /emqx-certs/server.key.tmp

chmod 644 /emqx-certs/server.pem.tmp
chmod 644 /emqx-certs/server.key.tmp

mv /emqx-certs/server.pem.tmp /emqx-certs/server.pem
mv /emqx-certs/server.key.tmp /emqx-certs/server.key

# Keep EMQX default filenames in sync for compatibility with listeners that
# rely on the stock cert paths.
cp /emqx-certs/server.pem /emqx-certs/cert.pem.tmp
cp /emqx-certs/server.key /emqx-certs/key.pem.tmp
cp /emqx-certs/server.pem /emqx-certs/cacert.pem.tmp

chmod 644 /emqx-certs/cert.pem.tmp /emqx-certs/cacert.pem.tmp
chmod 644 /emqx-certs/key.pem.tmp

mv /emqx-certs/cert.pem.tmp /emqx-certs/cert.pem
mv /emqx-certs/key.pem.tmp /emqx-certs/key.pem
mv /emqx-certs/cacert.pem.tmp /emqx-certs/cacert.pem

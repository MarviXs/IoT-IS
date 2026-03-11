#!/bin/sh
set -eu

if [ -z "${MQTT_DOMAIN:-}" ]; then
  echo "MQTT_DOMAIN is required" >&2
  exit 1
fi

if [ -z "${LETSENCRYPT_EMAIL:-}" ]; then
  echo "LETSENCRYPT_EMAIL is required" >&2
  exit 1
fi

staging_args=""
if [ "${CERTBOT_STAGING:-0}" = "1" ]; then
  staging_args="--staging"
fi

mkdir -p /var/www/certbot /emqx-certs

issue_certificate() {
  certbot certonly \
    --non-interactive \
    --agree-tos \
    --email "${LETSENCRYPT_EMAIL}" \
    --webroot \
    --webroot-path /var/www/certbot \
    --cert-name "${MQTT_DOMAIN}" \
    --domain "${MQTT_DOMAIN}" \
    --key-type ecdsa \
    ${staging_args} \
    --deploy-hook "/bin/sh /opt/certbot/deploy-emqx-cert.sh"
}

renew_certificates() {
  certbot renew \
    --non-interactive \
    --webroot \
    --webroot-path /var/www/certbot \
    ${staging_args} \
    --deploy-hook "/bin/sh /opt/certbot/deploy-emqx-cert.sh"
}

if [ ! -e "/etc/letsencrypt/renewal/${MQTT_DOMAIN}.conf" ]; then
  until issue_certificate; do
    echo "Initial certificate request failed for ${MQTT_DOMAIN}. Retrying in 5 minutes..." >&2
    sleep 300
  done
else
  /bin/sh /opt/certbot/deploy-emqx-cert.sh || true
fi

while true; do
  renew_certificates || true
  sleep 12h
done

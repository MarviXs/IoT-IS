## MQTT benchmark

This benchmark mirrors the HTTP flow:

- logs in through the REST API
- creates one device template
- creates the requested number of MQTT devices
- connects each VU to EMQX with the assigned device access token
- publishes one FlatBuffer `DataPointFbs` message per sensor on `devices/{deviceAccessToken}/data`
- deletes the created devices and template in teardown

## Configuration

Edit the constants at the top of `script.js`:

- `BASE_URL`
- `MQTT_HOST`
- `MQTT_PORT`
- `USERNAME`
- `PASSWORD`
- `DEVICE_COUNT`
- `SENSORS_PER_DEVICE`
- `VUS`
- `DURATION`

## Run

The script imports `k6/x/mqtt`, so recent k6 versions should resolve the official MQTT extension automatically:

```bash
k6 run benchmark/mqtt/script.js
```

## Fallback

If automatic resolution fails in your environment, build a custom binary from the `benchmark/mqtt` directory:

```bash
docker run --rm -it \
  -u "$(id -u):$(id -g)" \
  -v "${PWD}:/xk6" \
  grafana/xk6 build v0.49.0 \
  --with github.com/pmalhaire/xk6-mqtt
```

Then run from the repo root:

```bash
benchmark/mqtt/k6 run benchmark/mqtt/script.js
```

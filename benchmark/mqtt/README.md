## MQTT benchmark

This benchmark mirrors the HTTP flow:

- logs in through the REST API
- creates one device template
- creates the requested number of MQTT devices
- connects each VU to EMQX like the ESP device: `client_id` and `username` are the device access token, with a persistent session
- enqueues FlatBuffer `DataPointFbs` publishes on `devices/{deviceAccessToken}/data` with `publishAsync()`, so QoS 1/2 acknowledgements do not pace every send call
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
- `DURATION`
- `MQTT_QOS`
- `MQTT_RETAIN`
- `ENQUEUE_BATCH_SIZE` - number of datapoints a VU tries to enqueue per event-loop tick
- `ENQUEUE_INTERVAL_MS` - delay between enqueue ticks
- `MAX_PENDING_PUBLISHES` - per-VU backpressure limit for unresolved async publishes
- `DRAIN_TIMEOUT_MS` - time to wait for queued async publishes before disconnecting

The legacy `mqtt_datapoints_sent` metric now means "accepted into the benchmark-side async publish queue". Use `mqtt_datapoints_acked` to see how many publishes the MQTT client reported as completed.

`mqtt_publish_latency` measures the time from calling `publishAsync()` until the MQTT extension reports the publish as completed. The script includes a `p(95)<1000` threshold for this metric, matching the HTTP benchmark's p95 threshold style.

For QoS 2, keep `MAX_PENDING_PUBLISHES` modest. QoS 2 requires a longer handshake than QoS 0/1, and a very large pending queue can make the broker close the TCP connection.

Example QoS 1 run:

```bash
benchmark/mqtt/k6 run benchmark/mqtt/script.js
```

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

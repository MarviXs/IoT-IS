## HTTP benchmark

This k6 script:

- logs in with a user account
- creates one device template
- creates the requested number of HTTP devices
- assigns VUs to devices deterministically
- sends datapoints to `POST /api/devices/{deviceAccessToken}/data`
- deletes the created devices and template in teardown

## Configuration

The script reads these values directly from the constants at the top of `script.js`:

- `BASE_URL`
- `USERNAME`
- `PASSWORD`
- `DEVICE_COUNT`
- `SENSORS_PER_DEVICE`
- `VUS`
- `DURATION`

## Run

```bash
k6 run benchmark/http/script.js
```

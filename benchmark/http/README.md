## HTTP benchmark

This k6 script:

- logs in with a user account
- creates one device template
- creates the requested number of HTTP devices
- optionally creates one enabled nested scene per device
- assigns VUs to devices deterministically
- sends datapoints to `POST /api/devices/{deviceAccessToken}/data`
- deletes the created scenes, devices, and template in teardown

## Configuration

The script reads these values directly from the constants at the top of `script.js`:

- `BASE_URL`
- `USERNAME`
- `PASSWORD`
- `DEVICE_COUNT`
- `SENSORS_PER_DEVICE`
- `CREATE_SCENES_WITH_DEVICES` - when `true`, setup creates one enabled scene per device with a nested JSON-logic condition referencing that device's sensors
- `CREATE_SCENE_NOTIFICATIONS` - when `true`, each generated scene includes a zero-cooldown `NOTIFICATION` action; requires `CREATE_SCENES_WITH_DEVICES=true`, and every benchmark datapoint request targets at least one always-true scene so it creates a notification
- `DISTRIBUTE_SCENE_DEVICES_BETWEEN_SCENES` - when `true`, each generated scene references a rotating pair of devices; setup seeds latest datapoints first so cross-device scene checks hit Redis and still trigger
- `VUS`
- `DURATION`

## Run

```bash
k6 run benchmark/http/script.js
```

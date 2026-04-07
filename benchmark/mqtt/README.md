## Build binary
docker run --rm -it -u "$(id -u):$(id -g)" -v "${PWD}:/xk6" grafana/xk6 build v0.49.0 --with github.com/pmalhaire/xk6-mqtt

## Run
./k6 run script.js --vus 50 --duration 10s
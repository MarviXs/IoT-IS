/*

This is a k6 test script that imports the xk6-mqtt and
tests Mqtt with a 100 messages per connection.

*/

import { check } from "k6";
import protobuf from "./protobuf/protobuf.js";

const mqtt = require("k6/x/mqtt");

const accessToken = "eoBEBzyoTLoy38bHULeTFWaj";

const rnd_count = 2000;

// create random number to create a new topic at each run
let rnd = Math.random() * rnd_count;

// conection timeout (ms)
let connectTimeout = 2000;

// publish timeout (ms)
let publishTimeout = 100;

// connection close timeout (ms)
let closeTimeout = 2000;

// Mqtt topic one per VU
const k6Topic = `devices/${accessToken}/data`;
// Connect IDs one connection per VU
const k6PubId = `k6-pub-${rnd}-${__VU}`;

// number of message pusblished and receives at each iteration
const messageCount = 1;

const host = "localhost";
const port = "1883";

// create publisher client
let publisher = new mqtt.Client(
  // The list of URL of  MQTT server to connect to
  [host + ":" + port],
  // A username to authenticate to the MQTT server
  accessToken,
  // Password to match username
  "",
  // clean session setting
  false,
  // Client id for reader
  k6PubId,
  // timeout in ms
  connectTimeout
);
let err;

try {
  publisher.connect();
} catch (error) {
  err = error;
}

if (err != undefined) {
  console.error("publish connect error:", err);
  // you may want to use fail here if you want only to test successfull connection only
  // fail("fatal could not connect to broker for publish")
}

const payload = 'CgNjbzIRAAAAAAAAOUA='

export default function () {

  check(publisher, {
    "is publisher connected": (publisher) => publisher.isConnected(),
  });

  for (let i = 0; i < messageCount; i++) {
    // publish count messages
    let err_publish;
    try {
      publisher.publish(
        // topic to be used
        k6Topic,
        // The QoS of messages
        1,
        // Message to be sent
        payload,
        // retain policy on message
        false,
        // timeout in ms
        publishTimeout
      );
    } catch (error) {
      err_publish = error;
    }
    check(err_publish, {
      "is sent": (err) => err == undefined,
    });
  }
}

export function teardown() {
  // closing both connections at VU close
  publisher.close(closeTimeout);
}

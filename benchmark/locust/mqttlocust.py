from locust import User, task, between
import paho.mqtt.client as mqtt
import time
import flatbuffers
from configparser import ConfigParser

# Load configuration (ensure your config.ini is in the correct location)
config = ConfigParser()
config.read('config.ini')

# Sample tokens (adjust as needed)
device_access_token = config.get('locust', 'device_access_token', fallback='default_device')
sensor_access_token = config.get('locust', 'sensor_access_token', fallback='default_sensor')

class BackendUserMQTT(User):
    wait_time = between(1, 5)

    def on_message(self, client, userdata, message):
        print("Message received:", message.payload.decode("utf-8"))

    def on_connect(self, client, userdata, flags, rc):
        print("Connection returned result: " + mqtt.connack_string(rc))

    @task
    def post_device_data_mqtt(self):
        from schema import DataPoint

        # Start timer for measuring request time.
        start_time = time.time()

        # Configure the MQTT connection details.
        broker = config.get('locust', 'hostMQTT', fallback='localhost')
        port = config.getint('locust', 'portMQTT', fallback=1883)
        topic = f"devices/{device_access_token}/data"

        # Create and configure the MQTT client.
        client = mqtt.Client()
        username = config.get('locust', 'mqttUsername', fallback=None)
        password = config.get('locust', 'mqttPassword', fallback=None)
        if username:
            client.username_pw_set(username, password)

        client.on_connect = self.on_connect
        client.on_message = self.on_message

        try:
            client.connect(broker, port, keepalive=60)
            client.loop_start()

            # Build the Flatbuffers DataPoint message.
            # builder = flatbuffers.Builder(1024)
            # tag = builder.CreateString(sensor_access_token)
            # DataPoint.DataPointStart(builder)
            # DataPoint.DataPointAddTag(builder, tag)
            # DataPoint.DataPointAddValue(builder, 0.0)
            # DataPoint.DataPointAddTs(builder, 0)  # Optional field
            # dp = DataPoint.DataPointEnd(builder)
            # builder.Finish(dp)
            # buf = builder.Output()

            # # Publish the Flatbuffers payload to the MQTT topic.
            # client.publish(topic, payload=buf)

        finally:
            # Allow time for the message to be processed.
            time.sleep(4)
            client.loop_stop()
            client.disconnect()

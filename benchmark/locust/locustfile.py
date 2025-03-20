import configparser
from locust import HttpUser, task, between
import flatbuffers
import paho.mqtt.client as mqtt

# Read configuration from config.ini
config = configparser.ConfigParser()
config.read('config.ini')

class BackendUser(HttpUser):
    # Set the host from the configuration file.
    host = config.get('locust', 'hostHTTP', fallback='http://localhost:9001')
    # Wait time between tasks (in seconds)
    wait_time = between(1, 5)
    device_access_token = config.get('locust', 'accessToken', fallback="dummyToken")
    sensor_access_token = config.get('locust', 'accessTokenSensor', fallback="dummyTag")

    @task
    def post_device_data(self):
        # Replace 'dummytoken' with your actual device access token as needed.

        url = f"/devices/{device_access_token}/data"
        # Define the JSON payload to be sent in the request body.
        payload = [
            {
                "tag": sensor_access_token,
                "value": 0,
                "timeStamp": 0
            }
        ]
        # Sends a POST request to the endpoint with the JSON payload.
        self.client.post(url, json=payload)

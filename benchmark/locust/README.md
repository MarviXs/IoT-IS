### 1. Install Locust

Install Locust using pip:

```bash
pip install locust paho-mqtt
```

### 2. Install the Flatbuffers Compiler
```bash
sudo apt-get update
sudo apt-get install flatbuffers-compiler
```
### 3. Generate Python Code from Flatbuffers Schema
Assuming you have a file named datapoint.fbs, generate the Python module:

```bash
flatc --python datapoint.fbs
```
> This will create the necessary Python files (such as DataPoint.py or within a namespace folder) for your project.

### 4. Start locust
```bash
locust
```
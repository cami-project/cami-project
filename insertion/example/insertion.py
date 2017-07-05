import requests

data = {
	"measurement_type": "weight",
	"unit_type": "kg",
	"timestamp": 0,
	"user": "/api/v1/user/14/",
	"device": "/api/v1/device/1/",
	"value_info": "{'systolic': 115, 'diastolic': 70}"
}

response = requests.post(
    "http://localhost:8010/api/v1/insertion/measurements/",
    json=data
)

print response.status_code
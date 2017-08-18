import requests

HOST_IP = "141.85.241.224"

data = {
	"measurement_type": "weight",
	"unit_type": "kg",
	"timestamp": 0,
	"user": "/api/v1/user/14/",
	"device": "/api/v1/device/1/",
	"value_info": "{'systolic': 115, 'diastolic': 70}",
	"gateway_id": "9f508c55-dfd2-4ea6-8228-af991c123f17"
}

response = requests.post(
    "http://" + HOST_IP + ":8010/api/v1/insertion/measurements/",
    json=data
)

print response.status_code
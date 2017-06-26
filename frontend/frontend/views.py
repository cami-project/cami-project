from django.http import HttpResponse
from frontend.push_notifications import notifications

def test_notification(request):
    return HttpResponse(str(notifications.send_message(
    	[{
    		'registration_id': request.GET.get('id', ''),
    		'type': "APNS",
		}],
    	request.GET.get('message', '')
	)))

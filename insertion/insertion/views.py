from django.http import HttpResponse

def insert_measurement(request):
    return HttpResponse(status = 201)


def insert_event(request):
    return HttpResponse(status = 201)


# Create your views here.

import tasks

from django.http import HttpResponse, HttpRequest, HttpResponseServerError


def sync_activities(request):
    return HttpResponse(tasks.sync_activities(), content_type="text/html")

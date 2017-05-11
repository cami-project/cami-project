# Create your views here.

import activities

from django.http import HttpResponse, HttpRequest, HttpResponseServerError


def test(request):
    return HttpResponse(activities.sync_for_user(None), content_type="text/html")

# Create your views here.

import activities

from django.http import HttpResponse, HttpRequest, HttpResponseServerError

from .models import User


def sync_activities(request):
    return HttpResponse(activities.sync_for_user(User.objects.get(username="camidemo")), content_type="text/html")

# Create your views here.

import json, logging, pprint
import tasks

from django.http import HttpResponse, HttpRequest, HttpResponseServerError

def test(request):
    return HttpResponse(tasks.test_model(), content_type="text/html")

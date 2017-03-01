from django.contrib import admin
from models import UserAccount, Activity, InterfaceService

admin.site.register([UserAccount, Activity, InterfaceService])
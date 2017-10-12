#!/usr/bin/env bash

USER="cami"
PASS="CamiAdmin4321"
MAIL="proiect.cami@gmail.com"
script="
from django.contrib.auth.models import User

username = '$USER'
password = '$PASS'
email = '$MAIL'

if User.objects.filter(username=username).count()==0:
    User.objects.create_superuser(username, email, password)
    print('Superuser created.')
else:
    print('Superuser creation skipped.')
"
echo "$script" | python manage.py shell
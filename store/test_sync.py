from django.test import TestCase
from store.tasks import sync_locally, sync_remote
from store.models import UserAccount, Activity, InterfaceService
import store.constants

import datetime, pytz

class SyncRemoteTestCase(TestCase):
    def setUp(self):
        ## create UserAccount object
        user = UserAccount.objects.create(first_name = "Nancy",
                                          last_name = "Ann",
                                          email = "nancy.ann@email.com")

        cal_service = InterfaceService.objects.create(
            name = store.constants.CALENDAR_BACKEND_SERVICE,
            service_url = store.constants.CALENDAR_BACKEND_URI,
            connection_info = {
                "api_credentials_file": "cami-calendar-quickstart.json",

                "calendar_id": "g822rl14osjc75mf88lovci6kg@group.calendar.google.com"
            },
            user = user
        )

        start = datetime.datetime.now(tz=pytz.timezone("Europe/Bucharest"))
        end = start + datetime.timedelta(hours = 1)
        name = "Renew Car Insurrance"

        Activity.objects.create(user = user,
                                 name = name,
                                 starts_at = start, ends_at = end,
                                 activity_source = store.constants.ActivitySource.SELF,
                                 activity_type = store.constants.ActivityType.PERSONAL)


    def test_remote_backup_and_sync(self):
        user = UserAccount.objects.get(email = "nancy.ann@email.com")
        activity = Activity.objects.get(name = "Renew Car Insurrance")

        res = sync_remote.delay(user.uuid, activity, insert=True)

        # assert task successful
        self.assertTrue(res.successful())

        # test that activity has gotten its backend_id and that the is_synced flag is set
        activity = Activity.objects.get(name = "Renew Car Insurrance")

        self.assertTrue(activity.backend_id is not None)
        self.assertEquals(activity.is_synced, True)

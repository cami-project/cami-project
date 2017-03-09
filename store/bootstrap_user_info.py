import os, sys
import django

import logging
logger = logging.getLogger("store.bootstrap_user_info")

BASE_DIR    = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
STORE_DIR   = os.path.join(BASE_DIR, "store")

## WITHINGS ACCESS INFO
WITHINGS_ACCESS_CONFIG = {
    "userid": 11115034,
    "consumer_key": "5b1f8cbeb36cffe108fd8fdd666c51cb5d6eee9f2e2940983958b836451",
    "consumer_secret": "2e75dfb7f1088f398b4cfc5ebed6d5909c48918ee637417e3b0de001b3b",
    "oauth_token": "59dd58ccbd19bfbd8b3522ce50d31c4cb6e530742d22234f4cb4bee11673084",
    "oauth_token_secret": "cf31bc8e405d96b975b8014d93c722830bd55f44b437f27c7e6d5964b3"
}

## GOOGLE FIT ACCESS INFO
GOOGLE_FIT_ACCESS_CONFIG = {
    "google_fit_client_id" : '701996606933-17j7km8f8ce8vohhdcnur453cbn44aau.apps.googleusercontent.com',
    "google_fit_client_secret": 'K-lZ7t49-Gvhtz2P-RTqBhAQ',
    "google_fit_refresh_token": '1/eAhtNXxq65LeyzTr4aju27wCPLDAipXdrEd8ovgO8CY',

    "google_fit_hr_test_datastream_name" : "CAMI Heart Rate Test",
    "google_fit_hr_datastream_id" : 'raw:com.google.heart_rate.bpm:com.ryansteckler.perfectcinch:',
    "google_fit_steps_test_datastream_name" : 'CAMI Steps Test'
}

## OPENTELE ACCESS INFO
OPENTELE_ACCESS_CONFIG = {
    "opentele_url_base" : "http://opentele.aliviate.dk:4288/opentele-citizen-server",
    "opentele_user" : 'nancyann',
    "opentele_password" : 'abcd1234'
}

## LINKWATCH ACCESS INFO
LINKWATCH_ACCESS_CONFIG = {
    "linkwatch_url_base" : "https://linkwatchrestservicetest.azurewebsites.net/",
    "linkwatch_user" : 'CNetDemo',
    "linkwatch_password" : 'password'
}


def run_bootstrap():
    from store.models import User, EndUserProfile, CaregiverProfile, Device, DeviceUsage, ExternalMonitoringService

    """ ==== CREATE USER ACCOUNTS - CARETAKER + CAREGIVER ==== """
    cami_enduser, created = User.objects.get_or_create(username = "camidemo",
                                                       email="cami.demo@cami.com",
                                                       password = "CamiDemo123$",
                                                       first_name = "Cami",
                                                       last_name = "EndUser")
    if created:
        logger.info("[store.bootstrap_user_info] User %s created." % str(cami_enduser))
    else:
        logger.info("[store.bootstrap_user_info] User %s already exists." % str(cami_enduser))


    cami_caregiver, created = User.objects.get_or_create(username = "camicare",
                                                email="cami.care@cami.com",
                                                password="CamiCare123$",
                                                first_name="Cami",
                                                last_name="Caregiver")
    if created:
        logger.info("[store.bootstrap_user_info] User %s created." % str(cami_caregiver))
    else:
        logger.info("[store.bootstrap_user_info] User %s already exists." % str(cami_caregiver))


    # create the associated user profiles
    cami_enduser_profile, created = EndUserProfile.objects.update_or_create(user = cami_enduser,
                                       account_role="end_user",
                                       age = 65,
                                       height = 180)
    if created:
        logger.info("[store.bootstrap_user_info] Profile for user %s CREATED." % str(cami_enduser))
    else:
        logger.info("[store.bootstrap_user_info] Profile for user %s UPDATED." % str(cami_enduser))



    cami_caregiver_profile, created = CaregiverProfile.objects.update_or_create(user = cami_caregiver,
                                              account_role = "caregiver",
                                              caretaker = cami_enduser)
    if created:
        logger.info("[store.bootstrap_user_info] Profile for user %s CREATED." % str(cami_caregiver))
    else:
        logger.info("[store.bootstrap_user_info] Profile for user %s UPDATED." % str(cami_caregiver))




    """ ==== CREATE USER DEVICES - WITHINGS Watch, LG Smartwatch ==== """
    ## define weight scale
    weight_scale, created = Device.objects.get_or_create(device_type = "weight",
                          manufacturer = "Withings",
                          model = "WS 30",
                          serial_number = "00:24:e4:24:6f:30")
    if created:
        logger.info("[store.bootstrap_user_info] Device %s CREATED." % str(weight_scale))
    else:
        logger.info("[store.bootstrap_user_info] Device %s EXISTS." % str(weight_scale))


    weight_usage, created = DeviceUsage.objects.update_or_create(user = cami_enduser,
                               device = weight_scale,
                               access_info = WITHINGS_ACCESS_CONFIG)
    if created:
        logger.info("[store.bootstrap_user_info] Device usage for device %s and user %s CREATED."
                    % (str(weight_scale), str(cami_enduser)))
    else:
        logger.info("[store.bootstrap_user_info] Device usage for device %s and user %s UPDATED." %
                    (str(weight_scale), str(cami_enduser)))


    ## define LG smart watch
    lg_watch, created = Device.objects.get_or_create(device_type = "pulse",
                            manufacturer = "LG",
                            model = "Urbane",
                            serial_number = "1234")
    if created:
        logger.info("[store.bootstrap_user_info] Device %s CREATED." % str(lg_watch))
    else:
        logger.info("[store.bootstrap_user_info] Device %s EXISTS." % str(lg_watch))


    pulse_usage, created = DeviceUsage.objects.update_or_create(user = cami_enduser,
                               device = lg_watch,
                               access_info = GOOGLE_FIT_ACCESS_CONFIG)
    if created:
        logger.info("[store.bootstrap_user_info] Device usage for device %s and user %s CREATED."
                    % (str(lg_watch), str(cami_enduser)))
    else:
        logger.info("[store.bootstrap_user_info] Device usage for device %s and user %s UPDATED." %
                    (str(lg_watch), str(cami_enduser)))



    """ ==== CREATE EXTERNAL MONITORING SERVICE INSTANCE - OpenTele and LinkWatch accouns ==== """
    opentele, created = ExternalMonitoringService.objects.update_or_create(user = cami_enduser,
                                         name = "OpenTele",
                                         service_url = "http://opentele.aliviate.dk:4388/opentele-citizen-server/",
                                         access_info = OPENTELE_ACCESS_CONFIG)
    if created:
        logger.info("[store.bootstrap_user_info] %s CREATED." % str(opentele))
    else:
        logger.info("[store.bootstrap_user_info] %s UPDATED." % str(opentele))



    linkwatch, created = ExternalMonitoringService.objects.update_or_create(user=cami_enduser,
                                         name="LinkWatch",
                                         service_url="https://linkwatchrestservicetest.azurewebsites.net/",
                                         access_info=LINKWATCH_ACCESS_CONFIG)
    if created:
        logger.info("[store.bootstrap_user_info] %s CREATED." % str(linkwatch))
    else:
        logger.info("[store.bootstrap_user_info] %s UPDATED." % str(linkwatch))




if __name__ == "__main__":
    sys.path.extend([STORE_DIR])
    os.environ.setdefault("DJANGO_SETTINGS_MODULE", "store.settings")
    django.setup()

    run_bootstrap()
import sys
import os

from django.conf import settings

# Add google_fit folder to python path
# TODO: find a cleaner way to do this
sys.path.append(os.path.join(os.path.dirname(__file__), '..', '..', '..', 'google_fit'))
from GoogleFitHeartRate import GoogleFitHeartRate

def get_heart_rate_data(time_from, time_to):
    # Create GoogleFitHeartRate object
    google_fit = GoogleFitHeartRate(
        settings.GOOGLE_FIT_CLIENT_ID,
        settings.GOOGLE_FIT_CLIENT_SECRET,
        settings.GOOGLE_FIT_REFRESH_TOKEN
    )

    # Get data from Cinch
    heart_rate_data = google_fit.get_data(
        "raw:com.google.heart_rate.bpm:com.ryansteckler.perfectcinch:", 
        time_from,
        time_to
    )

    # Get data from CAMI Test
    datastream_id = google_fit.get_datastream_id_by_name("CAMI Heart Rate Test")
    if datastream_id:
        heart_rate_data = heart_rate_data + google_fit.get_data(
            datastream_id, 
            time_from,
            time_to
        )

    return heart_rate_data
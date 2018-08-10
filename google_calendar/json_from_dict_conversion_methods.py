import json
from datetime import datetime

# difficulty types
EASY, MEDIUM, HARD = "EASY", "MEDIUM", "HARD"


def create_time_dict(time_name, hour, minutes):
    return time_name, {"hour": hour, "minutes": minutes}


def create_period_dict(period_name, hour, minutes, day_index):
    return {period_name: {"time": {"hour": hour, "minutes": minutes},
                          "weekDay": {"dayIndex": day_index}, "periodIndex": 0}}


def create_excluded_time_periods_penalty_dict():
    # TODO
    pass


def create_relative_activity_penalty_dict():
    # TODO
    pass


def create_permitted_intervals_list(activity_data):
    # TODO
    # process activity data
    hour = None
    minutes = None
    min_start = create_time_dict("minStart", hour, minutes)
    max_end = create_time_dict("maxEnd", hour, minutes)
    time_interval_dict = {"TimeInterval": []}
    pass


def create_activity_domain_dict(code, description=""):
    activity_domain_dict = {"code": code}

    if description:
        activity_domain_dict["description"] = description

    return activity_domain_dict


def create_activity_category_dict(code, domain=None):
    activity_category_dict = {"code": code}

    if domain:
        activity_category_dict["domain"] = domain

    return activity_category_dict


def create_activity_type_dict(id, code, duration, instances_per_day=0, instances_per_week=0,
                              calories=0, description="", activity_category=None, difficulty=None, imposed_period=None,
                              permitted_intervals=None):
    """
    Stores all the information about an activity.
    :param id: serialization id
    :param code: name of the activity
    :param duration: duration of the activity
    :param instances_per_day: instances of the activity in a day, repeating every day
    :param instances_per_week: instances of the activity in a week
    :param calories: how many calories does it consume
    :param description: additional info about the activity
    :param activity_category: the activity is part of the given category
    :param difficulty: how difficult the activity is
    :param imposed_period: activity must be planned on this period
    :param permitted_intervals: activity should be planned in these intervals
    :return: a dictionary with all the given information about the activity
    """
    # activity code must be set nevertheless, even if it can miss from dict
    activity_type_dict = {"id": id, "code": code, "duration": duration, "calories": calories,
                          "instancesPerDay": instances_per_day, "instancesPerWeek": instances_per_week}

    if imposed_period:
        activity_type_dict["imposedPeriod"] = imposed_period
    if difficulty:
        activity_type_dict["difficulty"] = difficulty
    if activity_category:
        activity_type_dict["activityCategory"] = activity_category
    if permitted_intervals:
        activity_type_dict["permittedIntervals"] = permitted_intervals
    if description:
        activity_type_dict["description"] = description

    return activity_type_dict


def create_activity_dict(activity_type_dict, uuid, activity_sub_class_type="NormalActivity", offset=0, postpone=None,
                         immovable=False, wanted_to_be_planned=False,
                         on_dropdown=False):
    activity_dict = {"@class": activity_sub_class_type, "activityType": activity_type_dict, "immovable": immovable,
                     "wantedToBePlanned": wanted_to_be_planned, "onDropdown": on_dropdown, "uuid": uuid}

    if postpone:
        activity_dict["postpone"] = postpone
    if activity_sub_class_type == "NormalRelativeActivity":
        activity_dict["offset"] = offset
        activity_dict["assigned"] = False
    return activity_dict


def create_new_activity_dict(id, activity=None, excluded_time_periods_penalty=None, relative_activity_penalty=None):
    new_activity_dict = {"id": id}

    if activity:
        new_activity_dict["activity"] = activity
    if excluded_time_periods_penalty:
        new_activity_dict["excludedTimePeriodsPenalty"] = excluded_time_periods_penalty
    if relative_activity_penalty:
        new_activity_dict["relativeActivityPenalty"] = relative_activity_penalty

    return new_activity_dict


def create_new_activities_dict(new_activities_list):
    """
    Create a dict with all the new activities.
    :param new_activities_list: list of new activities that are going to be added
    :return: dictionary with all the new activities
    """
    new_activities_dict = {"NewActivities": {"newActivities": [
        {"NewActivity": new_activities_list if len(new_activities_list) > 1 else new_activities_list[0]}]}}
    return new_activities_dict


def create_deleted_activity_dict(id, name, uuid):
    deleted_activity_dict = {"id": id, "name": name, "uuid": uuid}
    return deleted_activity_dict


def create_deleted_activities_dict(deleted_activities_list):
    deleted_activities_dict = {
        "DeletedActivities": {"deletedActivities": [{"DeletedActivity": deleted_activities_list if len(
            deleted_activities_list) > 1 else deleted_activities_list[0]}]}}
    return deleted_activities_dict


def get_datetime_from_timestamp(timestamp):
    return datetime.fromtimestamp(timestamp / 1000.0)


def generate_json_from_dict(my_dict):
    json_str = json.dumps(my_dict, separators=(',', ':'))
    return json_str


def main():
    # print(create_activity_period_dict(12, 30, 5))
    # print(create_activity_dict(create_activity_period_dict(12, 30, 5)))
    d = create_new_activities_dict(
        [create_new_activity_dict(0, create_activity_dict(create_activity_type_dict(id=0, code="Yoga", duration=30,
                                                                                    activity_category=create_activity_category_dict(
                                                                                        code="Indoor physical exercises",
                                                                                        domain=create_activity_domain_dict(
                                                                                            code="Health Related Activities"))),
                                                          uuid="28204e9fcf45459baaa9aa348ded622a")),
         create_new_activity_dict(1, create_activity_dict(create_activity_type_dict(id=1, code="FootBall", duration=30,
                                                                                    activity_category=create_activity_category_dict(
                                                                                        code="Outdoor physical exercises",
                                                                                        domain=create_activity_domain_dict(
                                                                                            code="Leisure Activities"))),
                                                          uuid="6b05c3ff6cd1449489daae351d73a079"))
         ])
    print generate_json_from_dict(d)
    print get_datetime_from_timestamp(1531832400000)
    print generate_json_from_dict(create_deleted_activities_dict(
        [create_deleted_activity_dict(0, "Yoga", "f323c5807f8748e1b057d1b56de70f0a")]))


if __name__ == '__main__':
    main()

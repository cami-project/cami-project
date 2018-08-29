# difficulty types
EASY, MEDIUM, HARD = "EASY", "MEDIUM", "HARD"

# relativity types
BEFORE, AFTER = "BEFORE", "AFTER"


def create_time_dict(time_name, hour, minutes):
    return time_name, {"hour": hour, "minutes": minutes}


def create_period_dict(hour, minutes, day_index):
    return {"time": {"hour": hour, "minutes": minutes},
            "weekDay": {"dayIndex": day_index}, "periodIndex": 0}


def create_period_intervals_dict(excluded_periods):
    # TODO
    excluded_periods_dict = {"PeriodInterval": []}

    for period in excluded_periods:
        # period is a tuple of 2 tuples of size 3
        start_hour, start_minutes, start_day = period[0]
        end_hour, end_minutes, end_day = period[1]
        period_interval_dict = {"startPeriod": create_period_dict(start_hour, start_minutes, start_day),
                                "endPeriod": create_period_dict(end_hour, end_minutes, end_day)}
        excluded_periods_dict["PeriodInterval"].append(period_interval_dict)

    if len(excluded_periods_dict["PeriodInterval"]) == 1:
        excluded_periods_dict["PeriodInterval"] = excluded_periods_dict["PeriodInterval"][0]

    return [excluded_periods_dict]


def create_excluded_time_periods_penalty_dict(activity_type, excluded_periods):
    # TODO
    excluded_time_periods_dict = {"activityType": activity_type,
                                  "excludedActivityPeriods": create_period_intervals_dict(excluded_periods)}
    return excluded_time_periods_dict


def create_relative_activity_penalty_dict(normal_activity_type="", relative_activity_type="", category="",
                                          relative_type=""):
    # TODO
    relative_activity_penalty_dict = {}

    if normal_activity_type:
        relative_activity_penalty_dict["normalActivityType"] = normal_activity_type
    if relative_activity_type:
        relative_activity_penalty_dict["relativeActivityType"] = relative_activity_type
    if category:
        relative_activity_penalty_dict["category"] = category
    if relative_type:
        relative_activity_penalty_dict["relativeType"] = relative_type

    return relative_activity_penalty_dict


def create_permitted_intervals_list(activity_data):
    # TODO
    time_interval_list = {"TimeInterval": []}

    # process activity data
    for item in activity_data:
        # item is a tuple of size 4
        start_hour, start_minutes, end_hour, end_minutes = item
        min_start, min_start_dict = create_time_dict("minStart", start_hour, start_minutes)
        max_end, max_end_dict = create_time_dict("maxEnd", end_hour, end_minutes)
        time_interval_dict = {min_start: min_start_dict, max_end: max_end_dict}
        time_interval_list["TimeInterval"].append(time_interval_dict)

    if len(time_interval_list["TimeInterval"]) == 1:
        time_interval_list["TimeInterval"] = time_interval_list["TimeInterval"][0]

    return time_interval_list


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


def create_activity_type_dict(code, duration, instances_per_day=0, instances_per_week=0,
                              calories=0, description="", activity_category=None, difficulty=None, imposed_period=None,
                              permitted_intervals=None):
    """
    Stores all the information about an activity.
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
    activity_type_dict = {"code": code, "duration": duration, "calories": calories,
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


def create_new_activity_dict(activity=None, excluded_activity_periods=None, relative_activity_penalty=None):
    new_activity_dict = {}

    if activity:
        new_activity_dict["activity"] = activity
    if activity and excluded_activity_periods:  # now, it is dependent on the activity existence
        new_activity_dict["excludedTimePeriodsPenalty"] = create_excluded_time_periods_penalty_dict(
            activity["activityType"], excluded_activity_periods)
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


def create_deleted_activity_dict(name, uuid):
    deleted_activity_dict = {"name": name, "uuid": uuid}
    return deleted_activity_dict


def create_deleted_activities_dict(deleted_activities_list):
    deleted_activities_dict = {
        "DeletedActivities": {"deletedActivities": [{"DeletedActivity": deleted_activities_list if len(
            deleted_activities_list) > 1 else deleted_activities_list[0]}]}}
    return deleted_activities_dict


def main():
    # testing purposes
    d = create_new_activities_dict(
        new_activities_list=
        [create_new_activity_dict(activity=create_activity_dict(
            activity_type_dict=create_activity_type_dict(
                code="Yoga", duration=30,
                activity_category=create_activity_category_dict(
                    code="Indoor physical exercises",
                    domain=create_activity_domain_dict(
                        code="Health Related Activities"))),
            uuid="28204e9fcf45459baaa9aa348ded622a")),
            create_new_activity_dict(
                activity=create_activity_dict(
                    activity_type_dict=create_activity_type_dict(
                        code="Football", duration=30,
                        activity_category=create_activity_category_dict(
                            code="Outdoor physical exercises",
                            domain=create_activity_domain_dict(
                                code="Leisure Activities"))),
                    uuid="6b05c3ff6cd1449489daae351d73a079"))
        ])

    print d
    print create_deleted_activities_dict(
        [create_deleted_activity_dict(name="Yoga", uuid="f323c5807f8748e1b057d1b56de70f0a")])
    print create_permitted_intervals_list(activity_data=[(1, 2, 3, 4), (1, 2, 3, 4)])
    print create_excluded_time_periods_penalty_dict(activity_type="Yoga",
                                                    excluded_periods=[((1, 2, 3), (4, 5, 6)), ((1, 2, 3), (4, 5, 6))])
    print create_relative_activity_penalty_dict(normal_activity_type="Yoga", category="Meal", relative_type=BEFORE)


if __name__ == '__main__':
    main()

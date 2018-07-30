package org.aimas.cami.scheduler.CAMIScheduler.utils;

import org.aimas.cami.scheduler.CAMIScheduler.domain.ActivityPeriod;
import org.aimas.cami.scheduler.CAMIScheduler.domain.Time;

/**
 * Shift a period(to left or to right).
 * 
 * @author Bogdan
 *
 */
public class AdjustActivityPeriod {

	/**
	 * Shift a period.
	 * 
	 * @param period
	 *            {@link ActivityPeriod}
	 * @param offsetTime
	 *            {@link Integer}
	 * @return the shifted period.
	 */
	public static ActivityPeriod getAdjustedPeriod(ActivityPeriod period, int offsetTime) {
		Time time = period.getTime();

		int timeMinutes = time.getMinutes();

		// convert "offsetTime" minutes to Time format
		int hours = offsetTime / 60;
		int minutes = offsetTime % 60;

		// adjust the time
		if (timeMinutes + minutes >= 60) {
			hours++;
			minutes = (timeMinutes + minutes) % 60;
		} else if (timeMinutes + minutes < 0) {
			hours--;
			minutes = 60 + (timeMinutes + minutes);
		} else if (timeMinutes + minutes == 0) {
			minutes = 0;
		} else {
			minutes += timeMinutes;
		}

		return new ActivityPeriod(new Time(time.getHour() + hours, minutes), period.getWeekDay());
	}

	/**
	 * Shift a time.
	 * 
	 * @param time
	 *            {@link Time}
	 * @param offsetTime
	 *            {@link Integer}
	 * @return the shifted time.
	 */
	public static Time getAdjustedTime(Time time, int offsetTime) {

		int timeMinutes = time.getMinutes();

		// convert "offsetTime" minutes to Time format
		int hours = offsetTime / 60;
		int minutes = offsetTime % 60;

		// adjust the time
		if (timeMinutes + minutes >= 60) {
			hours++;
			minutes = (timeMinutes + minutes) % 60;
		} else if (timeMinutes + minutes < 0) {
			hours--;
			minutes = 60 + (timeMinutes + minutes);
		} else if (timeMinutes + minutes == 0) {
			minutes = 0;
		} else {
			minutes += timeMinutes;
		}

		return new Time(time.getHour() + hours, minutes);
	}

	public static ActivityPeriod setRelativeActivityPeriod(ActivityPeriod period, int offsetTime, int activityDuration,
			int relativeActivityDuration) {

		ActivityPeriod activityEndPeriod = null;
		Time time;

		if (offsetTime > 0)
			activityEndPeriod = getAdjustedPeriod(period, activityDuration);
		else if (offsetTime < 0) {
			offsetTime -= relativeActivityDuration;
		}

		if (activityEndPeriod == null)
			time = period.getTime();
		else
			time = activityEndPeriod.getTime();

		int timeMinutes = time.getMinutes();

		// convert "offsetTime" minutes to Time format
		int hours = offsetTime / 60;
		int minutes = offsetTime % 60;

		// adjust the time
		if (timeMinutes + minutes >= 60) {
			hours++;
			minutes = (timeMinutes + minutes) % 60;
		} else if (timeMinutes + minutes < 0) {
			hours--;
			minutes = 60 + (timeMinutes + minutes);
		} else if (timeMinutes + minutes == 0) {
			minutes = 0;
		} else {
			minutes += timeMinutes;
		}

		return new ActivityPeriod(new Time(time.getHour() + hours, minutes), period.getWeekDay());
	}

	/*
	 * public static ActivityPeriod
	 * SetActivityPeriodInPermittedTimeslot(ActivityPeriod activityPeriod, Timeslot
	 * permittedTimeslot, int duration) { Time[] timeslotInterval = new Time[2];
	 * 
	 * if (activityPeriod == null || activityPeriod.getTimeslot() == null)
	 * timeslotInterval[0] = permittedTimeslot.getTimeslot()[0]; else
	 * timeslotInterval[0] = activityPeriod.getTimeslot().getTimeslot()[1];
	 * 
	 * int hours = duration / 60; int minutes = duration % 60;
	 * 
	 * if (timeslotInterval[0].getMinutes() + minutes >= 60) { hours++; minutes =
	 * (timeslotInterval[0].getMinutes() + minutes) % 60; }
	 * 
	 * timeslotInterval[1] = new Time(timeslotInterval[0].getHour() + hours,
	 * minutes);
	 * 
	 * Timeslot timeslot = new Timeslot(timeslotInterval);
	 * activityPeriod.setTimeslot(timeslot);
	 * 
	 * return activityPeriod; }
	 */

}

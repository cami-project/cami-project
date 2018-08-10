package org.aimas.cami.scheduler.CAMIScheduler.utils;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.Reader;
import java.io.Writer;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Set;
import java.util.UUID;

import org.aimas.cami.scheduler.CAMIScheduler.domain.Activity;
import org.aimas.cami.scheduler.CAMIScheduler.domain.ActivityPeriod;
import org.aimas.cami.scheduler.CAMIScheduler.domain.ActivitySchedule;
import org.aimas.cami.scheduler.CAMIScheduler.domain.PeriodInterval;
import org.aimas.cami.scheduler.CAMIScheduler.domain.RelativeType;
import org.aimas.cami.scheduler.CAMIScheduler.domain.ScoreParametrization;
import org.aimas.cami.scheduler.CAMIScheduler.domain.Time;
import org.aimas.cami.scheduler.CAMIScheduler.domain.TimeInterval;
import org.drools.core.spi.KnowledgeHelper;

import com.thoughtworks.xstream.XStream;
import com.thoughtworks.xstream.XStreamException;
import com.thoughtworks.xstream.io.json.JettisonMappedXmlDriver;

/**
 * 
 * @author Bogdan
 *
 */
public class Utility {

	public static void main(String[] args) {
		System.out.println(fullOverlap(new Time(19, 05), new Time(19, 35), new Time(19, 04), new Time(19, 34)));
		Date date = new Date();
		System.out.println(date.getDay());

		Calendar calendar = Calendar.getInstance();
		calendar.setTimeInMillis(1531832400000L);

		System.out.println(calendar.getTime());
	}

	public static void help(final KnowledgeHelper drools, final String message) {
		System.out.println(message);
		System.out.println("rule triggered: " + drools.getRule().getName() + "\n");
	}

	public static void helper(final KnowledgeHelper drools) {
		System.out.println("\nrule triggered: " + drools.getRule().getName());
	}

	/**
	 * Distance between two {@link Time} values.
	 */
	public static Integer getNumberOfMinutesInInterval(Time left, Time right) {

		return (right.getHour() - left.getHour()) * 60 + right.getMinutes() - left.getMinutes();
	}

	/**
	 * Distance between two {@link ActivityPeriod} values.
	 */
	public static Integer getNumberOfMinutesInPeriodInterval(int dayIndexLeft, int dayIndexRight, Time left,
			Time right) {

		return ((dayIndexRight - dayIndexLeft) * 24 + (right.getHour() - left.getHour())) * 60 + right.getMinutes()
				- left.getMinutes();
	}

	public static Set<Character> stringToCharacterSet(String s) {
		Set<Character> set = new LinkedHashSet<>();
		for (char c : s.toCharArray()) {
			set.add(c);
		}
		return set;
	}

	/**
	 * Check if the start of activity A is <= the end of activity B
	 * 
	 * @param startA
	 *            start of activity A
	 * @param endB
	 *            end of activity B
	 * @return true if start A is before end B, else false.
	 */
	public static boolean before(Time startA, Time endB) {
		if (startA.getHour() < endB.getHour())
			return true;
		else if (startA.getHour() == endB.getHour())
			if (startA.getMinutes() <= endB.getMinutes())
				return true;
		return false;
	}

	/**
	 * Check if the start of activity B is <= the end of activity A
	 * 
	 * @param startB
	 *            start of activity B
	 * @param endA
	 *            end of activity A
	 * @return true if end A is after start B, else false.
	 */
	public static boolean after(Time startB, Time endA) {
		if (startB.getHour() < endA.getHour())
			return true;
		else if (startB.getHour() == endA.getHour())
			if (startB.getMinutes() <= endA.getMinutes())
				return true;
		return false;
	}

	/**
	 * like {@link #before}, but strict less
	 */
	public static boolean exclusiveBefore(Time timeA, Time timeB) {
		if (timeA.getHour() < timeB.getHour())
			return true;
		else if (timeA.getHour() == timeB.getHour())
			if (timeA.getMinutes() < timeB.getMinutes())
				return true;
		return false;
	}

	/**
	 * like {@link #after}, but strict less
	 */
	public static boolean exclusiveAfter(Time timeA, Time timeB) {
		if (timeA.getHour() < timeB.getHour())
			return true;
		else if (timeA.getHour() == timeB.getHour())
			if (timeA.getMinutes() < timeB.getMinutes())
				return true;
		return false;
	}

	/**
	 * Check if an activity's period fully overlap an interval.
	 * 
	 * @param startActivity
	 * @param endActivity
	 * @param startPermittedInterval
	 * @param endPermittedInterval
	 * @return true, if the activity and the permitted interval fully overlap.
	 */
	public static boolean fullOverlap(Time startActivity, Time endActivity, Time startPermittedInterval,
			Time endPermittedInterval) {

		if (before(startPermittedInterval, startActivity) && before(endActivity, endPermittedInterval))
			return true;

		return false;

	}

	/**
	 * Get all the free periods from the current schedule.
	 * 
	 * @param activitySchedule
	 * @param activityEntity
	 * @return List<ActivityPeriod> activityPeriodList
	 */
	public static List<ActivityPeriod> getFreePeriods(ActivitySchedule activitySchedule, Activity activityEntity) {
		List<ActivityPeriod> activityPeriodList = new ArrayList<>();
		boolean overlapFound = false;

		for (ActivityPeriod activityPeriod : activitySchedule.getActivityPeriodList()) {
			overlapFound = false;
			ActivityPeriod activityEndPeriod = AdjustActivityPeriod.getAdjustedPeriod(activityPeriod,
					activityEntity.getActivityDuration());

			if (activityPeriod.getPeriodHour() >= activitySchedule.getScoreParametrization().getEarlyHour()) {

				overlapFound = findOverlap(activitySchedule, activityPeriod, activityEndPeriod);

				if (!overlapFound) {
					activityPeriodList.add(activityPeriod);
				}
			}
		}
		return activityPeriodList;
	}

	/**
	 * * Get all the free periods from the current schedule that are in the
	 * specified interval.
	 */
	public static List<ActivityPeriod> getFreePeriodsInInterval(ActivitySchedule activitySchedule,
			Activity activityEntity, TimeInterval timeInterval, int dayIndex) {

		List<ActivityPeriod> activityPeriodList = new ArrayList<>();
		boolean overlapFound = false;

		for (ActivityPeriod activityPeriod : activitySchedule.getActivityPeriodList()) {

			overlapFound = false;
			ActivityPeriod activityEndPeriod = AdjustActivityPeriod.getAdjustedPeriod(activityPeriod,
					activityEntity.getActivityDuration());

			if (activityPeriod.getWeekDayIndex() == dayIndex
					&& activityPeriod.getPeriodHour() >= activitySchedule.getScoreParametrization().getEarlyHour()
					&& fullOverlap(activityPeriod.getTime(), activityEndPeriod.getTime(), timeInterval.getMinStart(),
							timeInterval.getMaxEnd())) {

				overlapFound = findOverlap(activitySchedule, activityPeriod, activityEndPeriod);

				if (!overlapFound) {
					activityPeriodList.add(activityPeriod);
				}
			}
		}
		return activityPeriodList;
	}

	public static List<ActivityPeriod> getFreePeriodsLaterThisWeek(ActivitySchedule activitySchedule,
			Activity activityEntity, int dayIndex) {

		List<ActivityPeriod> activityPeriodList = new ArrayList<>();

		for (ActivityPeriod activityPeriod : activitySchedule.getActivityPeriodList()) {

			if (activityPeriod.getWeekDayIndex() > dayIndex
					&& activityPeriod.getPeriodHour() >= activitySchedule.getScoreParametrization().getEarlyHour()) {

				ActivityPeriod activityEndPeriod = AdjustActivityPeriod.getAdjustedPeriod(activityPeriod,
						activityEntity.getActivityDuration());

				if (!findOverlap(activitySchedule, activityPeriod, activityEndPeriod)) {
					activityPeriodList.add(activityPeriod);
				}
			}
		}
		return activityPeriodList;
	}

	/**
	 * Get the next free period after activityPeriod.
	 * 
	 * @param activitySchedule
	 * @param relativeActivityEntity
	 * @param activityPeriod
	 * @param increment
	 * @return {@link ActivityPeriod}
	 */
	public static ActivityPeriod getRelativeActivityPeriod(ActivitySchedule activitySchedule,
			Activity relativeActivityEntity, ActivityPeriod activityPeriod, int increment) {

		while (true) {

			ActivityPeriod activityEndPeriod = AdjustActivityPeriod.getAdjustedPeriod(activityPeriod,
					relativeActivityEntity.getActivityDuration());

			if (!findOverlap(activitySchedule, activityPeriod, activityEndPeriod)) {
				return activityPeriod;
			}

			activityPeriod = AdjustActivityPeriod.getAdjustedPeriod(activityPeriod, increment);

		}

	}

	/**
	 * Checks if the specified time interval (activityPeriod, activityEndPeriod)
	 * is free.
	 * 
	 */
	private static boolean findOverlap(ActivitySchedule activitySchedule, ActivityPeriod activityPeriod,
			ActivityPeriod activityEndPeriod) {

		for (Activity activity : activitySchedule.getActivityList()) {
			if (activity.getActivityPeriod() != null
					&& activityPeriod.getWeekDayIndex() == activity.getActivityPeriodWeekday().getDayIndex()) {

				if (Utility.before(activityPeriod.getTime(), activity.getActivityEndPeriod().getTime())
						&& Utility.after(activity.getActivityPeriodTime(), activityEndPeriod.getTime())) {
					return true;

				}
			}
		}

		return false;
	}

	/**
	 * Checks if the activityPeriod is in an excluded period interval.
	 * 
	 * @param activityPeriod
	 * @param excludedPeriodInterval
	 * @param activityDuration
	 * @param sameStartDay
	 * @param sameEndDay
	 * @return
	 */
	public static Boolean checkTimeslots(ActivityPeriod activityPeriod, PeriodInterval excludedPeriodInterval,
			int activityDuration, boolean sameStartDay, boolean sameEndDay) {

		ActivityPeriod activityEndPeriod = AdjustActivityPeriod.getAdjustedPeriod(activityPeriod, activityDuration);
		Time activityStartTime = activityPeriod.getTime();
		Time activityEndTime = activityEndPeriod.getTime();
		Time excludedStartTime = excludedPeriodInterval.getStartPeriod().getTime();
		Time excludedEndTime = excludedPeriodInterval.getEndPeriod().getTime();

		if (sameStartDay && sameEndDay) {

			if (before(activityStartTime, excludedEndTime) && after(excludedStartTime, activityEndTime))
				return true;

		} else if (sameStartDay) {

			if (before(activityStartTime, new Time(24, 0)) && after(excludedStartTime, activityEndTime))
				return true;

		} else if (sameEndDay) {

			if (before(activityStartTime, excludedEndTime) && after(new Time(0, 0), activityEndTime))
				return true;

		}

		return false;
	}

	/**
	 * Get {@link ScoreParametrization} from inputFile.
	 * 
	 * @param activitySchedule
	 * @param inputFile
	 * @return
	 */
	public static ScoreParametrization getScoreParametrization(ActivitySchedule activitySchedule, File inputFile) {
		XStream xStream = new XStream(new JettisonMappedXmlDriver());
		xStream.alias("ScoreParametrization", ScoreParametrization.class);
		xStream.setMode(XStream.NO_REFERENCES);
		xStream.autodetectAnnotations(true);

		try (Reader reader = new InputStreamReader(new FileInputStream(inputFile), "UTF-8")) {
			ScoreParametrization scoreParametrization = (ScoreParametrization) xStream.fromXML(reader);
			return scoreParametrization;
		} catch (XStreamException | IOException e) {
			throw new IllegalArgumentException("Failed reading inputSolutionFile (" + inputFile + ").", e);
		}
	}

	public static void setScoreParametrization(ActivitySchedule activitySchedule, File outputFile) {
		XStream xStream = new XStream(new JettisonMappedXmlDriver());
		xStream.alias("ScoreParametrization", ScoreParametrization.class);
		xStream.setMode(XStream.NO_REFERENCES);
		xStream.autodetectAnnotations(true);

		try (Writer writer = new OutputStreamWriter(new FileOutputStream(outputFile), "UTF-8")) {
			xStream.toXML(activitySchedule.getScoreParametrization(), writer);
		} catch (IOException e) {
			throw new IllegalArgumentException("Failed writing outputSolutionFile (" + outputFile + ").", e);
		}
	}

	/**
	 * Get the offset sign for a relative (activity) type.
	 * 
	 * @return 1 if the relative activity is AFTER, else -1.
	 */
	public static int getRelativeTypeSign(RelativeType relativeType) {
		return relativeType.equals(RelativeType.AFTER) ? 1 : (-1);
	}

	/**
	 * Check for activity periods equivalence.
	 * 
	 * @param activity1
	 * @param activity2
	 * @return false if the activities have the same defined period.
	 */
	public static boolean compareActivityPeriods(Activity activity1, Activity activity2) {
		ActivityPeriod activity1Period = activity1.getActivityPeriod();
		ActivityPeriod activity2Period = activity2.getActivityPeriod();

		if (activity1Period.getWeekDayIndex() != activity2Period.getWeekDayIndex()) {
			return true;
		} else {
			if (activity1Period.getPeriodHour() != activity2Period.getPeriodHour())
				return true;
			else {
				if (activity1Period.getPeriodMinutes() != activity2Period.getPeriodMinutes())
					return true;
			}
		}

		return false;
	}

	public static String getCategory(String particularCategory) {

		if (particularCategory.equals("Indoor physical exercises")
				|| particularCategory.equals("Outdoor physical exercises"))
			return "Exercise";
		else if (particularCategory.equals("Imposed/Suggested Health measurements")
				|| particularCategory.equals("Medication intake"))
			return "Medication";
		else
			return "Personal";

	}

	public static String getActivityCategory(Activity activity) {

		if (activity.getActivityCategory() == null)
			return "Personal";

		if (activity.getActivityCategory().getCode().equals("Indoor physical exercises")
				|| activity.getActivityCategory().getCode().equals("Outdoor physical exercises"))
			return "Exercise";
		else if (activity.getActivityCategory().getCode().equals("Imposed/Suggested Health measurements")
				|| activity.getActivityCategory().getCode().equals("Medication intake"))
			return "Medication";
		else
			return "Personal";

	}

	public static String generateRandomUuid() {
		return UUID.randomUUID().toString().replace("-", "");
	}

	public static List<ActivityPeriod> determineValueRange(ActivitySchedule activitySchedule, Activity activity) {

		if (activity.getActivityType().getPermittedIntervals() != null) {

			List<ActivityPeriod> restrictedPeriodDomainRange = new ArrayList<>();

			for (TimeInterval permittedInterval : activity.getActivityType().getPermittedIntervals()) {

				int offset = 1;

				TimeInterval relaxedPermittedInterval = new TimeInterval(
						new Time(permittedInterval.getMinStart().getHour() - offset,
								permittedInterval.getMinStart().getMinutes()),
						new Time(permittedInterval.getMaxEnd().getHour() + offset,
								permittedInterval.getMaxEnd().getMinutes()));

				for (ActivityPeriod period : activitySchedule.getActivityPeriodList()) {
					if (Utility.fullOverlap(period.getTime(), period.getTime(), relaxedPermittedInterval.getMinStart(),
							relaxedPermittedInterval.getMaxEnd()))
						restrictedPeriodDomainRange.add(period);
				}
			}

			return restrictedPeriodDomainRange;

		} else {
			// return the whole range
			return activitySchedule.getActivityPeriodList();
		}
	}

	public static long convertActivityPeriodToTimestamp(ActivityPeriod activityPeriod) {
		int dayOfMonth;
		Date actualDate = new Date();

		// Date has assigned value 0 for Sunday, whilst, in WeekDays enum, Sunday
		// has value 6 assigned
		if (actualDate.getDay() == 0)
			dayOfMonth = actualDate.getDate() + ((activityPeriod.getWeekDayIndex() + 1) - (actualDate.getDay() + 7));
		else
			dayOfMonth = actualDate.getDate() + ((activityPeriod.getWeekDayIndex() + 1) - actualDate.getDay());

		Date activityDate = new Date(actualDate.getYear(), actualDate.getMonth(), dayOfMonth,
				activityPeriod.getPeriodHour(), activityPeriod.getPeriodMinutes());

		return activityDate.getTime() / 1000L;
	}

}
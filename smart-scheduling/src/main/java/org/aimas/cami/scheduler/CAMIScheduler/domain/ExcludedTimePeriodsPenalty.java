package org.aimas.cami.scheduler.CAMIScheduler.domain;

import java.util.List;

import org.aimas.cami.scheduler.CAMIScheduler.utils.AbstractPersistable;

import com.thoughtworks.xstream.annotations.XStreamAlias;

/**
 * An activity should not be planned on specified intervals.
 * 
 * @author Bogdan
 *
 */
@XStreamAlias("ExcludedTimePeriodsPenalty")
public class ExcludedTimePeriodsPenalty extends AbstractPersistable {

	private ActivityType activityType;
	private List<PeriodInterval> excludedActivityPeriods;

	public ActivityType getActivityType() {
		return activityType;
	}

	public void setActivityType(ActivityType activityType) {
		this.activityType = activityType;
	}

	public List<PeriodInterval> getExcludedActivityPeriods() {
		return excludedActivityPeriods;
	}

	public void setExcludedActivityPeriods(List<PeriodInterval> excludedActivityPeriods) {
		this.excludedActivityPeriods = excludedActivityPeriods;
	}

	@Override
	public String toString() {
		return "ExcludedTimePeriodsPenalty [activityType=" + activityType + ", excludedActivityPeriods="
				+ excludedActivityPeriods + "]";
	}

}

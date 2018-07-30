package org.aimas.cami.scheduler.CAMIScheduler.domain.solver;

import org.aimas.cami.scheduler.CAMIScheduler.domain.Activity;
import org.aimas.cami.scheduler.CAMIScheduler.domain.ActivitySchedule;
import org.aimas.cami.scheduler.CAMIScheduler.domain.ExcludedTimePeriodsPenalty;
import org.aimas.cami.scheduler.CAMIScheduler.domain.RelativeActivityPenalty;
import org.apache.commons.lang3.builder.CompareToBuilder;
import org.optaplanner.core.impl.heuristic.selector.common.decorator.SelectionSorterWeightFactory;

public class ActivityDifficultyWeightFactory implements SelectionSorterWeightFactory<ActivitySchedule, Activity> {

	@Override
	public ActivityDifficultyWeight createSorterWeight(ActivitySchedule solution, Activity activity) {
		int priority = 0;

		if (activity.getActivityCategory().getCode().equals("Meal"))
			priority++;

		if (activity.getActivityType().getPermittedIntervals() != null)
			priority++;

		for (ExcludedTimePeriodsPenalty etp : solution.getExcludedTimePeriodsList()) {
			if (etp.getActivityType().getCode().equals(activity.getActivityTypeCode()))
				priority++;
		}

		for (RelativeActivityPenalty rap : solution.getRelativeActivityPenaltyList()) {
			if ((rap.getNormalActivityType() != null
					&& rap.getNormalActivityType().equals(activity.getActivityTypeCode()))
					|| (rap.getCategory() != null && rap.getCategory().equals(activity.getActivityCategory().getCode())))
				priority++;
		}

		return new ActivityDifficultyWeight(activity, priority);
	}

	public static class ActivityDifficultyWeight implements Comparable<ActivityDifficultyWeight> {

		private final Activity activity;
		private final int priority;

		public ActivityDifficultyWeight(Activity activity, int priority) {
			this.activity = activity;
			this.priority = priority;
		}

		@Override
		public int compareTo(ActivityDifficultyWeight other) {
			return new CompareToBuilder().append(other.priority, priority)
					.append(other.activity.getId(), activity.getId()).toComparison();
		}

	}

}

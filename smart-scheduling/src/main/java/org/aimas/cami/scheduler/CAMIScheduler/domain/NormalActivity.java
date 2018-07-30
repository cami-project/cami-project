package org.aimas.cami.scheduler.CAMIScheduler.domain;

import java.util.List;
import java.util.Map;

import org.aimas.cami.scheduler.CAMIScheduler.domain.solver.ActivityDifficultyWeightFactory;
import org.aimas.cami.scheduler.CAMIScheduler.domain.solver.ActivityDifficultyWeightFactory.ActivityDifficultyWeight;
import org.aimas.cami.scheduler.CAMIScheduler.domain.solver.TimeWeightFactory;
import org.aimas.cami.scheduler.CAMIScheduler.solver.move.MovableActivitySelectionFilter;
import org.aimas.cami.scheduler.CAMIScheduler.solver.move.ReinitializeActivityFilter;
import org.optaplanner.core.api.domain.entity.PlanningEntity;
import org.optaplanner.core.api.domain.valuerange.ValueRangeProvider;
import org.optaplanner.core.api.domain.variable.PlanningVariable;

import com.thoughtworks.xstream.annotations.XStreamAlias;

/**
 * An activity to be planned.
 * 
 * @author Bogdan
 *
 */
@PlanningEntity(movableEntitySelectionFilter = MovableActivitySelectionFilter.class, difficultyWeightFactoryClass = ActivityDifficultyWeightFactory.class)
@XStreamAlias("NormalActivity")
public class NormalActivity extends Activity {

	// planning variable, changes during planning
	protected ActivityPeriod activityPeriod;

	// map that keeps count of the assigned (relative) activities to this
	// activity
	Map<String, Long> assignedToRelativeActivityMap;

	// custom value range for the planning variable
	List<ActivityPeriod> periodDomainRangeList;

	@Override
	@PlanningVariable(valueRangeProviderRefs = {
			"activityPeriodRange" }, strengthWeightFactoryClass = TimeWeightFactory.class) // , nullable = true,
																							// reinitializeVariableEntityFilter
																							// =
																							// ReinitializeActivityFilter.class)
	public ActivityPeriod getActivityPeriod() {
		return activityPeriod;
	}

	public void setActivityPeriod(ActivityPeriod activityPeriod) {
		this.activityPeriod = activityPeriod;
	}

	@ValueRangeProvider(id = "activityPeriodRange")
	public List<ActivityPeriod> getPeriodDomainRangeList() {
		return periodDomainRangeList;
	}

	public void setPeriodDomainRangeList(List<ActivityPeriod> periodDomainRangeList) {
		this.periodDomainRangeList = periodDomainRangeList;
	}

	public Map<String, Long> getAssignedToRelativeActivityMap() {
		return assignedToRelativeActivityMap;
	}

	public void setAssignedToRelativeActivityMap(Map<String, Long> assignedRelativeActivityList) {
		this.assignedToRelativeActivityMap = assignedRelativeActivityList;
	}

	@Override
	public String toString() {
		return "Activity [activityType=" + getActivityType() + ", activityPeriod=" + activityPeriod + "]";
	}

	@Override
	public String getLabel() {
		return getActivityTypeCode();
	}

}

package org.aimas.cami.scheduler.CAMIScheduler.domain;

import org.aimas.cami.scheduler.CAMIScheduler.domain.solver.RelativeActivityPeriodUpdateListener;
import org.aimas.cami.scheduler.CAMIScheduler.domain.solver.TimeWeightFactory;
import org.aimas.cami.scheduler.CAMIScheduler.solver.move.MovableActivitySelectionFilter;
import org.aimas.cami.scheduler.CAMIScheduler.utils.AdjustActivityPeriod;
import org.optaplanner.core.api.domain.entity.PlanningEntity;
import org.optaplanner.core.api.domain.variable.CustomShadowVariable;
import org.optaplanner.core.api.domain.variable.PlanningVariable;
import org.optaplanner.core.api.domain.variable.PlanningVariableReference;

import com.thoughtworks.xstream.annotations.XStreamAlias;

/**
 * An activity that is planned depending on other's activity period.
 * 
 * @author Bogdan
 *
 */
@PlanningEntity
@XStreamAlias("NormalRelativeActivity")
public class NormalRelativeActivity extends Activity {

	// shadow variable (it depends on a planning variable)
	// if the planning variable changes, then the shadow variable adjusts
	// according to it
	private ActivityPeriod activityPeriod;

	private int offset; // offset (in minutes) - distance between an activity and an activity relative to it
	private boolean assigned;

	@Override
	@CustomShadowVariable(variableListenerClass = RelativeActivityPeriodUpdateListener.class, sources = {
			@PlanningVariableReference(entityClass = NormalActivity.class, variableName = "activityPeriod") })
	public ActivityPeriod getActivityPeriod() {
		return activityPeriod;
	}

	public void setActivityPeriod(ActivityPeriod activityPeriod) {
		this.activityPeriod = activityPeriod;
	}

	public int getOffset() {
		return offset;
	}

	public void setOffset(int offset) {
		this.offset = offset;
	}

	public boolean isAssigned() {
		return assigned;
	}

	public void setAssigned(boolean assigned) {
		this.assigned = assigned;
	}

	@Override
	public String toString() {
		return "RelativeActivity [activityType=" + getActivityType() + ", activityPeriod=" + activityPeriod
				+ ", offset=" + offset + "]";
	}

	@Override
	public String getLabel() {
		return getActivityTypeCode();
	}

}

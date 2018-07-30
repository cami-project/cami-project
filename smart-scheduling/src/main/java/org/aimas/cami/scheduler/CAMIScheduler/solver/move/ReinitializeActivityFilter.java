package org.aimas.cami.scheduler.CAMIScheduler.solver.move;

import org.aimas.cami.scheduler.CAMIScheduler.domain.Activity;
import org.aimas.cami.scheduler.CAMIScheduler.domain.ActivitySchedule;
import org.optaplanner.core.impl.heuristic.selector.common.decorator.SelectionFilter;
import org.optaplanner.core.impl.score.director.ScoreDirector;

/**
 * Tell the solver to initialize an uninitialized activity(planning variable is
 * null).
 * 
 * @author Bogdan
 *
 */
public class ReinitializeActivityFilter implements SelectionFilter<ActivitySchedule, Activity> {

	@Override
	public boolean accept(ScoreDirector<ActivitySchedule> scoreDirector, Activity selection) {

		if (selection.isWantedToBePlanned()) {
			// System.out.println(selection.getActivityTypeCode());
			return true;
		}
		return false;
	}

}

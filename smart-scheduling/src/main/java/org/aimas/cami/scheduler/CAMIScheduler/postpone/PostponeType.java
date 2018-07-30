package org.aimas.cami.scheduler.CAMIScheduler.postpone;

import org.aimas.cami.scheduler.CAMIScheduler.solver.solver.Labeled;

/**
 * The type of the postpone.
 * 
 * @author Bogdan
 *
 */
public enum PostponeType implements Labeled {
	POSTPONE_15MIN, POSTPONE_30MIN, POSTPONE_1HOUR, POSTPONE_LATER_THIS_DAY, POSTPONE_LATER_THIS_WEEK;

	@Override
	public String getLabel() {
		return this.toString();
	}
}

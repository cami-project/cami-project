package org.aimas.cami.scheduler.CAMIScheduler.domain;

import org.aimas.cami.scheduler.CAMIScheduler.solver.solver.Labeled;

/**
 * Type of relativity that an activity has on other activity.
 * 
 * @author Bogdan
 *
 */
public enum RelativeType implements Labeled {
	BEFORE, AFTER;

	@Override
	public String getLabel() {
		return this.toString();
	}
}

package org.aimas.cami.scheduler.CAMIScheduler.domain.solver;

import java.util.Random;

import org.aimas.cami.scheduler.CAMIScheduler.domain.Activity;
import org.aimas.cami.scheduler.CAMIScheduler.domain.ActivityPeriod;
import org.aimas.cami.scheduler.CAMIScheduler.domain.ActivitySchedule;
import org.aimas.cami.scheduler.CAMIScheduler.domain.NormalActivity;
import org.aimas.cami.scheduler.CAMIScheduler.domain.NormalRelativeActivity;
import org.aimas.cami.scheduler.CAMIScheduler.domain.RelativeActivityPenalty;
import org.aimas.cami.scheduler.CAMIScheduler.domain.RelativeType;
import org.aimas.cami.scheduler.CAMIScheduler.utils.AdjustActivityPeriod;
import org.aimas.cami.scheduler.CAMIScheduler.utils.Utility;
import org.optaplanner.core.impl.domain.variable.listener.VariableListener;
import org.optaplanner.core.impl.score.director.ScoreDirector;

/**
 * To update a shadow variable, the planner uses a VariableListener. It assures
 * that every relative activity is set to a specified activity. The
 * randomization is made by setting <selectionOrder>SHUFFLED</selectionOrder> in
 * solver config file.
 * 
 * @author Bogdan
 *
 */
public class RelativeActivityPeriodUpdateListener implements VariableListener<NormalActivity> {

	protected void updatePeriod(ScoreDirector scoreDirector, NormalActivity activityEntity) {

		// the solution
		ActivitySchedule activitySchedule = (ActivitySchedule) scoreDirector.getWorkingSolution();

		// iterate through RelativeActivityPenalty list, and pick a
		// NormalRelativeActivity that can be assigned to this activityEntity.
		for (RelativeActivityPenalty rap : activitySchedule.getRelativeActivityPenaltyList()) {

			// if it is the right activityEntity
			boolean rightEntity = activityEntity.getActivityTypeCode().equals(rap.getNormalActivityType());

			if (activityEntity.getActivityCategory() != null
					&& activityEntity.getActivityCategory().getCode().equals(rap.getCategory()))
				rightEntity = true;

			if (rightEntity) {

				// pick a NormalRelativeActivity that can be assigned to this
				// activityEntity
				// using a map
				for (Activity activity : activitySchedule.getActivityList()) {
					if (activity instanceof NormalRelativeActivity) {
						NormalRelativeActivity relativeActivity = (NormalRelativeActivity) activity;

						if (relativeActivity.getActivityTypeCode().equals(rap.getRelativeActivityType())) {// &&
																											// relativeActivity.isWantedToBePlanned())
																											// {

							if (!activityEntity.getAssignedToRelativeActivityMap()
									.containsKey(relativeActivity.getActivityTypeCode())
									&& !relativeActivity.isAssigned()) {

								if (rap.getCategory() != null) {

									// use an assignation probability for category assignation
									Random rand = new Random();
									float p = rand.nextFloat();

									if (p < 0.001f) {
										assignRelativeActivity(scoreDirector, activityEntity, relativeActivity);

									}
								} else {
									assignRelativeActivity(scoreDirector, activityEntity, relativeActivity);
								}

							}

							// and set it's period
							// scoreDirector is used to notify the solver about
							// the changes
							if (relativeActivity != null && activityEntity.getActivityPeriod() != null) {

								if (activityEntity.getAssignedToRelativeActivityMap()
										.containsKey(relativeActivity.getActivityTypeCode())
										&& activityEntity.getAssignedToRelativeActivityMap()
												.get(relativeActivity.getActivityTypeCode())
												.equals(relativeActivity.getId())) {

									if (rap.getRelativeType().equals(RelativeType.BEFORE)) {

										ActivityPeriod period = Utility.getRelativeActivityPeriod(activitySchedule,
												relativeActivity,
												AdjustActivityPeriod.getAdjustedPeriod(
														activityEntity.getActivityPeriod(),
														Math.abs(relativeActivity.getOffset()) * (-1)
																- relativeActivity.getActivityType().getDuration()),
												-5);

										scoreDirector.beforeVariableChanged(relativeActivity, "activityPeriod");
										relativeActivity.setActivityPeriod(period);
										scoreDirector.afterVariableChanged(relativeActivity, "activityPeriod");

									} else {

										ActivityPeriod period = Utility.getRelativeActivityPeriod(activitySchedule,
												relativeActivity,
												AdjustActivityPeriod.getAdjustedPeriod(
														activityEntity.getActivityEndPeriod(),
														relativeActivity.getOffset()),
												5);

										scoreDirector.beforeVariableChanged(relativeActivity, "activityPeriod");
										relativeActivity.setActivityPeriod(period);
										scoreDirector.afterVariableChanged(relativeActivity, "activityPeriod");

									}
								}
							}
						}
					}
				}
			}
		}
	}

	private void assignRelativeActivity(ScoreDirector scoreDirector, NormalActivity activityEntity,
			NormalRelativeActivity relativeActivity) {

		// scoreDirector is used to notify the
		// solver about the changes
		scoreDirector.beforeProblemPropertyChanged(activityEntity);
		activityEntity.getAssignedToRelativeActivityMap().put(relativeActivity.getActivityTypeCode(),
				relativeActivity.getId());
		scoreDirector.afterProblemPropertyChanged(activityEntity);

		scoreDirector.beforeProblemPropertyChanged(relativeActivity);
		relativeActivity.setAssigned(true);
		scoreDirector.afterProblemPropertyChanged(relativeActivity);
	}

	@Override
	public void beforeEntityAdded(ScoreDirector scoreDirector, NormalActivity entity) {

	}

	@Override
	public void afterEntityAdded(ScoreDirector scoreDirector, NormalActivity entity) {

	}

	@Override
	public void beforeVariableChanged(ScoreDirector scoreDirector, NormalActivity entity) {
		updatePeriod(scoreDirector, entity);

	}

	@Override
	public void afterVariableChanged(ScoreDirector scoreDirector, NormalActivity entity) {
		updatePeriod(scoreDirector, entity);

	}

	@Override
	public void beforeEntityRemoved(ScoreDirector scoreDirector, NormalActivity entity) {

	}

	@Override
	public void afterEntityRemoved(ScoreDirector scoreDirector, NormalActivity entity) {

	}

}

package org.aimas.cami.scheduler.CAMIScheduler.utils;

import java.io.Reader;
import java.util.ArrayList;
import java.util.List;

import org.aimas.cami.scheduler.CAMIScheduler.domain.Activity;
import org.aimas.cami.scheduler.CAMIScheduler.domain.ActivitySchedule;
import org.aimas.cami.scheduler.CAMIScheduler.domain.ActivityType;
import org.aimas.cami.scheduler.CAMIScheduler.domain.ExcludedTimePeriodsPenalty;
import org.aimas.cami.scheduler.CAMIScheduler.domain.NormalActivity;
import org.aimas.cami.scheduler.CAMIScheduler.domain.NormalRelativeActivity;
import org.aimas.cami.scheduler.CAMIScheduler.domain.RelativeActivityPenalty;
import org.aimas.cami.scheduler.CAMIScheduler.marshal.ChangedActivity;
import org.aimas.cami.scheduler.CAMIScheduler.marshal.DeletedActivities;
import org.aimas.cami.scheduler.CAMIScheduler.marshal.DeletedActivity;
import org.aimas.cami.scheduler.CAMIScheduler.marshal.NewActivities;
import org.aimas.cami.scheduler.CAMIScheduler.marshal.NewActivity;
import org.aimas.cami.scheduler.CAMIScheduler.solver.solver.ProblemSolver;
import org.optaplanner.core.impl.score.director.ScoreDirector;

import com.thoughtworks.xstream.XStream;

public class SolutionUtils<Solution_> {

	public void addNewActivityFromXML(SolutionBusiness<Solution_> solutionBusiness, Object xmlActivity,
			ProblemSolver problemSolver) {

		NewActivities newActivitiesDAO = null;

		// get the NewActivities instance from the xml object
		XStream xStream = new XStream();
		xStream.alias("NewActivities", NewActivities.class);
		xStream.setMode(XStream.ID_REFERENCES);
		xStream.autodetectAnnotations(true);

		if (xmlActivity instanceof String) {
			newActivitiesDAO = (NewActivities) xStream.fromXML((String) xmlActivity);
		} else {
			newActivitiesDAO = (NewActivities) xStream.fromXML((Reader) xmlActivity);
		}

		List<NewActivity> newActivitiesList = newActivitiesDAO.getNewActivities();

		// add the activities one by one
		for (NewActivity newActivityDAO : newActivitiesList) {

			Activity newActivity = newActivityDAO.getActivity();
			ExcludedTimePeriodsPenalty excludedTimePeriodsPenalty = newActivityDAO.getExcludedTimePeriodsPenalty();
			RelativeActivityPenalty relativeActivityPenalty = newActivityDAO.getRelativeActivityPenalty();

			ActivitySchedule activitySchedule = (ActivitySchedule) solutionBusiness.getSolution();

			if (newActivity instanceof NormalActivity) {

				solutionBusiness.doProblemFactChange(scoreDirector -> {

					List<Activity> activityList = new ArrayList<>(activitySchedule.getActivityList());
					activitySchedule.setActivityList(activityList);

					List<ActivityType> activityTypeList = new ArrayList<>(activitySchedule.getActivityTypeList());
					activitySchedule.setActivityTypeList(activityTypeList);

					// get activity instances
					int instances = 1;
					if (newActivity.getActivityType().getInstancesPerDay() != 0)
						instances = newActivity.getActivityType().getInstancesPerDay() * 7;
					else if (newActivity.getActivityType().getInstancesPerWeek() != 0)
						instances = newActivity.getActivityType().getInstancesPerWeek();

					// set activity type id
					newActivity.getActivityType().setId(activityTypeList.isEmpty() ? 0L
							: activityTypeList.get(activityTypeList.size() - 1).getId() + 1);

					// create instances of this type of activity
					for (int i = 0; i < instances; i++) {
						NormalActivity activity = new NormalActivity();

						activity.setActivityType(newActivity.getActivityType());
						activity.setImmovable(newActivity.isImmovable());
						activity.setPeriodDomainRangeList(Utility.determineValueRange(activitySchedule, activity));
						activity.setId(
								activityList.isEmpty() ? 0L : activityList.get(activityList.size() - 1).getId() + 1);
						activity.setUuid(Utility.generateRandomUuid());

						scoreDirector.beforeEntityAdded(activity);
						activityList.add(activity);
						scoreDirector.afterEntityAdded(activity);

						// if its period is imposed
						if (activity.getActivityType().getImposedPeriod() != null) {
							scoreDirector.beforeVariableChanged(activity, "activityPeriod");
							activity.setActivityPeriod(activity.getActivityType().getImposedPeriod());
							scoreDirector.afterVariableChanged(activity, "activityPeriod");

							activity.setImmovable(true);
						}
					}

					scoreDirector.beforeProblemFactAdded(newActivity.getActivityType());
					activityTypeList.add(newActivity.getActivityType());
					scoreDirector.afterProblemFactAdded(newActivity.getActivityType());

					scoreDirector.triggerVariableListeners();

				});
			} else if (newActivity instanceof NormalRelativeActivity) {

				if (relativeActivityPenalty != null) {
					solutionBusiness.doProblemFactChange(scoreDirector -> {
						List<Activity> activityList = new ArrayList<>(activitySchedule.getActivityList());
						activitySchedule.setActivityList(activityList);

						List<ActivityType> activityTypeList = new ArrayList<>(activitySchedule.getActivityTypeList());
						activitySchedule.setActivityTypeList(activityTypeList);

						List<RelativeActivityPenalty> relativeActivityPenaltyList = new ArrayList<>(
								activitySchedule.getRelativeActivityPenaltyList());
						activitySchedule.setRelativeActivityPenaltyList(relativeActivityPenaltyList);

						newActivity.getActivityType().setId(activityTypeList.isEmpty() ? 0L
								: activityTypeList.get(activityTypeList.size() - 1).getId() + 1);

						// if the activity is relative to a specific activity
						if (relativeActivityPenalty.getNormalActivityType() != null) {

							int instances = 1;

							// get activity instances(activity to which
							// newActivity
							// is relative)
							for (ActivityType activityType : activityTypeList) {
								if (activityType.getCode().equals(relativeActivityPenalty.getNormalActivityType())) {

									if (activityType.getInstancesPerDay() != 0) {
										instances = activityType.getInstancesPerDay() * 7;
										newActivity.getActivityType().setInstancesPerDay(instances / 7);
									} else if (activityType.getInstancesPerWeek() != 0) {
										instances = activityType.getInstancesPerWeek();
										newActivity.getActivityType().setInstancesPerWeek(instances);
									}

									break;
								}
							}

							// create instances of this type of activity
							for (int i = 0; i < instances; i++) {
								NormalRelativeActivity relativeActivity = new NormalRelativeActivity();
								relativeActivity.setActivityType(newActivity.getActivityType());
								relativeActivity.setOffset(((NormalRelativeActivity) newActivity).getOffset()
										* Utility.getRelativeTypeSign(relativeActivityPenalty.getRelativeType()));
								relativeActivity.setImmovable(newActivity.isImmovable());
								relativeActivity.setId(activityList.isEmpty() ? 0L
										: activityList.get(activityList.size() - 1).getId() + 1);
								relativeActivity.setUuid(Utility.generateRandomUuid());

								scoreDirector.beforeEntityAdded(relativeActivity);
								activityList.add(relativeActivity);
								scoreDirector.afterEntityAdded(relativeActivity);
							}

						} else if (relativeActivityPenalty.getCategory() != null) {
							// else if this activity is relative to a category
							// of activities

							int instances = 1;

							// get the imposed instances
							if (newActivity.getActivityType().getInstancesPerDay() != 0) {
								instances = newActivity.getActivityType().getInstancesPerDay() * 7;
							} else if (newActivity.getActivityType().getInstancesPerWeek() != 0) {
								instances = newActivity.getActivityType().getInstancesPerWeek();
							}

							// create instances of this type of activity
							for (int i = 0; i < instances; i++) {
								NormalRelativeActivity relativeActivity = new NormalRelativeActivity();
								relativeActivity.setActivityType(newActivity.getActivityType());
								relativeActivity.setOffset(((NormalRelativeActivity) newActivity).getOffset()
										* Utility.getRelativeTypeSign(relativeActivityPenalty.getRelativeType()));
								relativeActivity.setImmovable(newActivity.isImmovable());
								relativeActivity.setId(activityList.isEmpty() ? 0L
										: activityList.get(activityList.size() - 1).getId() + 1);

								scoreDirector.beforeEntityAdded(relativeActivity);
								activityList.add(relativeActivity);
								scoreDirector.afterEntityAdded(relativeActivity);
							}

						}

						// add relativeActivityPenalty fact to the solution
						relativeActivityPenalty.setId(relativeActivityPenaltyList.isEmpty() ? 0L
								: relativeActivityPenaltyList.get(relativeActivityPenaltyList.size() - 1).getId() + 1);

						scoreDirector.beforeProblemFactAdded(relativeActivityPenalty);
						relativeActivityPenaltyList.add(relativeActivityPenalty);
						scoreDirector.afterProblemFactAdded(relativeActivityPenalty);

						scoreDirector.beforeProblemFactAdded(newActivity.getActivityType());
						activityTypeList.add(newActivity.getActivityType());
						scoreDirector.afterProblemFactAdded(newActivity.getActivityType());

						// *****trigger the listener*****
						// so the relative activity new created has its period
						// set
						if (relativeActivityPenalty.getNormalActivityType() != null) {

							triggerListener(solutionBusiness, activityList,
									relativeActivityPenalty.getNormalActivityType(), null);

						} else if ((relativeActivityPenalty.getCategory() != null)) {

							triggerListener(solutionBusiness, activityList, null,
									relativeActivityPenalty.getCategory());

						}

						scoreDirector.triggerVariableListeners();
					});
				}

			}

			// add excludedTimePeriodsPenalty to the solution
			if (excludedTimePeriodsPenalty != null) {

				List<ExcludedTimePeriodsPenalty> excludedTimePeriodsPenaltyList = new ArrayList<>(
						activitySchedule.getExcludedTimePeriodsList());
				activitySchedule.setExcludedTimePeriodsList(excludedTimePeriodsPenaltyList);

				solutionBusiness.doProblemFactChange(scoreDirector -> {
					excludedTimePeriodsPenalty.setId(excludedTimePeriodsPenaltyList.isEmpty() ? 0L
							: excludedTimePeriodsPenaltyList.get(excludedTimePeriodsPenaltyList.size() - 1).getId()
									+ 1);

					scoreDirector.beforeProblemFactAdded(excludedTimePeriodsPenalty);
					excludedTimePeriodsPenaltyList.add(excludedTimePeriodsPenalty);
					scoreDirector.afterProblemFactAdded(excludedTimePeriodsPenalty);

					scoreDirector.triggerVariableListeners();
				});

			}

		}

		problemSolver.startSolveAction();
	}

	public void deleteActivityFromSchedule(SolutionBusiness<Solution_> solutionBusiness, Object xmlActivity) {

		ActivitySchedule activitySchedule = (ActivitySchedule) solutionBusiness.getSolution();

		// shallow clone the activityList
		List<Activity> activityList = new ArrayList<>(activitySchedule.getActivityList());
		activitySchedule.setActivityList(activityList);

		List<Activity> deletedActivities = new ArrayList<>();

		// get the DeletedActivities instance from xml object
		DeletedActivities deletedActivitiesDAO = null;

		XStream xStream = new XStream();
		xStream.alias("DeletedActivities", DeletedActivities.class);
		xStream.setMode(XStream.ID_REFERENCES);
		xStream.autodetectAnnotations(true);

		if (xmlActivity instanceof String) {
			deletedActivitiesDAO = (DeletedActivities) xStream.fromXML((String) xmlActivity);
		} else {
			deletedActivitiesDAO = (DeletedActivities) xStream.fromXML((Reader) xmlActivity);
		}

		List<DeletedActivity> deletedActivitiesList = deletedActivitiesDAO.getDeletedActivities();

		// search for the activities that have to be deleted
		for (DeletedActivity deletedActivityDAO : deletedActivitiesList) {

			for (Activity activity : activitySchedule.getActivityList()) {
				if (activity.getActivityTypeCode().equals(deletedActivityDAO.getName())
						&& activity.getUuid().equals(deletedActivityDAO.getUuid()))

					deletedActivities.add(activity);

			}
		}

		// delete the activities one by one
		for (Activity activity : deletedActivities) {
			solutionBusiness.doProblemFactChange(scoreDirector -> {
				deleteActivity(scoreDirector, activity, activityList);
			});
		}
	}

	public void deleteActivity(ScoreDirector<Solution_> scoreDirector, Activity activity, List<Activity> activityList) {

		Activity activityEntity = scoreDirector.lookUpWorkingObject(activity);

		// it has been already deleted
		if (activityEntity == null) {
			return;
		}

		scoreDirector.beforeEntityRemoved(activityEntity);
		activityList.remove(activityEntity);
		scoreDirector.afterEntityRemoved(activityEntity);

		scoreDirector.triggerVariableListeners();

	}

	/**
	 * Find all activities with their name equals to normalActivityType and
	 * reset their period
	 * 
	 */
	public void triggerListener(SolutionBusiness<Solution_> solutionBusiness, List<Activity> activityList,
			String normalActivityType, String category) {

		solutionBusiness.doProblemFactChange(scoreDirector -> {
			if (normalActivityType != null) {

				for (Activity activity : activityList) {
					if (activity instanceof NormalActivity) {
						if (((NormalActivity) activity).getActivityTypeCode().equals(normalActivityType)) {

							scoreDirector.beforeVariableChanged(activity, "activityPeriod");
							((NormalActivity) activity).setActivityPeriod(activity.getActivityPeriod());
							scoreDirector.afterVariableChanged(activity, "activityPeriod");
						}
					}
				}

			} else if ((category != null)) {

				for (Activity activity : activityList) {
					if (activity instanceof NormalActivity) {
						if (((NormalActivity) activity).getActivityCategory().getCode() != null
								&& ((NormalActivity) activity).getActivityCategory().getCode().equals(category)) {

							scoreDirector.beforeVariableChanged(activity, "activityPeriod");
							((NormalActivity) activity).setActivityPeriod(activity.getActivityPeriod());
							scoreDirector.afterVariableChanged(activity, "activityPeriod");
						}
					}
				}

			}

			scoreDirector.triggerVariableListeners();
		});
	}

	public List<ChangedActivity> getChangedActivites(List<Activity> beforeAddActivityList,
			List<Activity> afterAddActivityList) {

		List<ChangedActivity> changedActivities = new ArrayList<>();

		for (Activity activity1 : beforeAddActivityList) {
			for (Activity activity2 : afterAddActivityList) {

				if (activity1.getActivityTypeCode() == activity2.getActivityTypeCode()
						&& activity1.getId() == activity2.getId()) {

					if (Utility.compareActivityPeriods(activity1, activity2)) {
						changedActivities.add(new ChangedActivity(activity1.getActivityTypeCode(),
								Utility.convertActivityPeriodToTimestamp(activity1.getActivityPeriod()),
								Utility.convertActivityPeriodToTimestamp(activity2.getActivityPeriod()),
								activity1.getActivityDuration()));
						break;
					}
				}
			}
		}

		return changedActivities;
	}

}
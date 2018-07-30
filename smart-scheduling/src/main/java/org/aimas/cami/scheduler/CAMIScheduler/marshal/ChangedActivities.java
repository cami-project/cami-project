package org.aimas.cami.scheduler.CAMIScheduler.marshal;

import java.util.List;

public class ChangedActivities {

	private int numberOfChangedActivities;
	private List<ChangedActivity> changedActivities;

	public ChangedActivities(int numberOfChangedActivities, List<ChangedActivity> changedActivities, int id) {
		super();
		this.numberOfChangedActivities = numberOfChangedActivities;
		this.changedActivities = changedActivities;
	}

	public int getNumberOfChangedActivities() {
		return numberOfChangedActivities;
	}

	public void setNumberOfChangedActivities(int numberOfChangedActivities) {
		this.numberOfChangedActivities = numberOfChangedActivities;
	}

	public List<ChangedActivity> getChangedActivities() {
		return changedActivities;
	}

	public void setChangedActivities(List<ChangedActivity> changedActivities) {
		this.changedActivities = changedActivities;
	}

}

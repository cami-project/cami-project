package org.aimas.cami.scheduler.CAMIScheduler.marshal;

import java.util.List;

import org.aimas.cami.scheduler.CAMIScheduler.utils.AbstractPersistable;

import com.thoughtworks.xstream.annotations.XStreamAlias;

/**
 * Delete a set of activities from the schedule.
 * 
 * @author Bogdan
 *
 */
@XStreamAlias("DeletedActivities")
public class DeletedActivities extends AbstractPersistable {
	private List<DeletedActivity> deletedActivities;

	public DeletedActivities(List<DeletedActivity> deletedActivities) {
		super();
		this.deletedActivities = deletedActivities;
	}

	public List<DeletedActivity> getDeletedActivities() {
		return deletedActivities;
	}

	public void setDeletedActivities(List<DeletedActivity> deletedActivities) {
		this.deletedActivities = deletedActivities;
	}

}

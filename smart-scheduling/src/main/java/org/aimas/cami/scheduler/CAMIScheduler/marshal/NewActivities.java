package org.aimas.cami.scheduler.CAMIScheduler.marshal;

import java.util.List;

import org.aimas.cami.scheduler.CAMIScheduler.utils.AbstractPersistable;

import com.thoughtworks.xstream.annotations.XStreamAlias;

/**
 * Add many activities to the schedule.
 * 
 * @author Bogdan
 *
 */
@XStreamAlias("NewActivities")
public class NewActivities extends AbstractPersistable {
	private List<NewActivity> newActivities;

	public NewActivities(List<NewActivity> newActivities) {
		super();
		this.newActivities = newActivities;
	}

	public List<NewActivity> getNewActivities() {
		return newActivities;
	}

	public void setNewActivities(List<NewActivity> newActivities) {
		this.newActivities = newActivities;
	}

}

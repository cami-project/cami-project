package org.aimas.cami.scheduler.CAMIScheduler.marshal;

import org.aimas.cami.scheduler.CAMIScheduler.utils.AbstractPersistable;

import com.thoughtworks.xstream.annotations.XStreamAlias;

/**
 * Represents an activity from the schedule that has to be deleted.
 * 
 * @author Bogdan
 *
 */
@XStreamAlias("DeletedActivity")
public class DeletedActivity extends AbstractPersistable {
	private String name;
	private String uuid;

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public String getUuid() {
		return uuid;
	}

	public void setUuid(String uuid) {
		this.uuid = uuid;
	}

}

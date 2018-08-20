package org.aimas.cami.scheduler.CAMIScheduler.marshal;

import java.util.concurrent.atomic.AtomicInteger;

public class ActivityProperties {

	private static final AtomicInteger COUNTER = new AtomicInteger();

	private final int id;
	private String uuid;
	private String activity;
	private long activityPeriod;
	private int activityDurationInMinutes;

	public ActivityProperties(String uuid, String activity, long activityPeriod, int activityDurationInMinutes) {
		super();
		this.id = COUNTER.getAndIncrement();
		this.uuid = uuid;
		this.activity = activity;
		this.activityPeriod = activityPeriod;
		this.activityDurationInMinutes = activityDurationInMinutes;
	}

	public ActivityProperties() {
		this.id = COUNTER.getAndIncrement();
	}

	public String getUuid() {
		return uuid;
	}

	public void setUuid(String uuid) {
		this.uuid = uuid;
	}

	public String getActivity() {
		return activity;
	}

	public void setActivity(String activity) {
		this.activity = activity;
	}

	public long getActivityPeriod() {
		return activityPeriod;
	}

	public void setActivityPeriod(long activityPeriod) {
		this.activityPeriod = activityPeriod;
	}

	public int getActivityDurationInMinutes() {
		return activityDurationInMinutes;
	}

	public void setActivityDurationInMinutes(int activityDurationInMinutes) {
		this.activityDurationInMinutes = activityDurationInMinutes;
	}

	public int getId() {
		return id;
	}

}

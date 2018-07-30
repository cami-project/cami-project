package org.aimas.cami.scheduler.CAMIScheduler.domain;

import org.aimas.cami.scheduler.CAMIScheduler.utils.AbstractPersistable;

import com.thoughtworks.xstream.annotations.XStreamAlias;

/**
 * An interval of time e.g. [12:00, 13:00]
 * 
 * @author Bogdan
 *
 */
@XStreamAlias("TimeInterval")
public class TimeInterval extends AbstractPersistable {

	private Time minStart, maxEnd;

	public TimeInterval() {

	}

	public TimeInterval(Time minStart, Time maxEnd) {
		super();
		this.minStart = minStart;
		this.maxEnd = maxEnd;
	}

	public Time getMinStart() {
		return minStart;
	}

	public void setMinStart(Time minStart) {
		this.minStart = minStart;
	}

	public Time getMaxEnd() {
		return maxEnd;
	}

	public void setMaxEnd(Time maxEnd) {
		this.maxEnd = maxEnd;
	}

	@Override
	public String toString() {
		return "TimeInterval [minStart=" + minStart + ", maxEnd=" + maxEnd + "]";
	}

}

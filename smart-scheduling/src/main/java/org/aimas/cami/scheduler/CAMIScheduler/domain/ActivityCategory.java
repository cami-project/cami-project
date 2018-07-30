package org.aimas.cami.scheduler.CAMIScheduler.domain;

import org.aimas.cami.scheduler.CAMIScheduler.solver.solver.Labeled;
import org.aimas.cami.scheduler.CAMIScheduler.utils.AbstractPersistable;
import com.thoughtworks.xstream.annotations.XStreamAlias;

/**
 * Category of activities. One or many categories are part of a domain.
 * 
 * @author Bogdan
 *
 */
@XStreamAlias("ActivityCategory")
public class ActivityCategory extends AbstractPersistable implements Labeled {

	private String code; // category's name

	/*
	 * used when need to compare the domain when are overlapped activities(e.g.
	 * leisure vs health, can overlap)
	 */
	private ActivityDomain domain;

	public ActivityCategory(String code, ActivityDomain domain, long id) {
		super();
		this.code = code;
		this.domain = domain;
		this.id = id;
	}

	public ActivityCategory(String code, ActivityDomain domain) {
		super();
		this.code = code;
		this.domain = domain;
	}

	public String getCode() {
		return code;
	}

	public void setCode(String code) {
		this.code = code;
	}

	public ActivityDomain getDomain() {
		return domain;
	}

	public void setDomain(ActivityDomain domain) {
		this.domain = domain;
	}

	@Override
	public String getLabel() {
		return code;
	}

}

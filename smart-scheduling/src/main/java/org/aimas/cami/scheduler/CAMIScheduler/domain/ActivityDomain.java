package org.aimas.cami.scheduler.CAMIScheduler.domain;

import java.util.List;

import org.aimas.cami.scheduler.CAMIScheduler.utils.AbstractPersistable;
import com.thoughtworks.xstream.annotations.XStreamAlias;

/**
 * A domain contains a list of categories.
 * 
 * @author Bogdan
 *
 */
@XStreamAlias("ActivityDomain")
public class ActivityDomain extends AbstractPersistable {

	private String code; // domain's name
	private String description;

	public ActivityDomain(String code) {
		super();
		this.code = code;
	}

	public String getCode() {
		return code;
	}

	public void setCode(String code) {
		this.code = code;
	}

	public String getDescription() {
		return description;
	}

	public void setDescription(String description) {
		this.description = description;
	}

	@Override
	public String toString() {
		return "ActivityCategory [code=" + code + ", description=" + description + "]";
	}

}

package org.aimas.cami.scheduler.CAMIScheduler.domain;

import java.util.List;

import org.aimas.cami.scheduler.CAMIScheduler.utils.AbstractPersistable;

import com.thoughtworks.xstream.annotations.XStreamAlias;

/**
 * Which activity is relative to which. An activity can be relative to a
 * specified activity or to an activity that is part of a category.
 * 
 * @author Bogdan
 *
 */
@XStreamAlias("RelativeActivityPenalty")
public class RelativeActivityPenalty extends AbstractPersistable {

	private RelativeType relativeType;
	// activity names
	private String relativeActivityType;
	private String normalActivityType;
	// activity category
	private String category;

	public RelativeType getRelativeType() {
		return relativeType;
	}

	public void setRelativeType(RelativeType relativeType) {
		this.relativeType = relativeType;
	}

	public String getRelativeActivityType() {
		return relativeActivityType;
	}

	public void setRelativeActivityType(String relativeActivityType) {
		this.relativeActivityType = relativeActivityType;
	}

	public String getNormalActivityType() {
		return normalActivityType;
	}

	public void setNormalActivityType(String normalActivityType) {
		this.normalActivityType = normalActivityType;
	}

	public String getCategory() {
		return category;
	}

	public void setCategory(String category) {
		this.category = category;
	}

	@Override
	public String toString() {
		return "RelativeActivityPenalty [relativeType=" + relativeType + ", relativeActivityType="
				+ relativeActivityType + ", staticActivityType=" + normalActivityType + "]";
	}

}

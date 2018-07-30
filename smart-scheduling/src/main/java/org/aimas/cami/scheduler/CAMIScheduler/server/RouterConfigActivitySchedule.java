package org.aimas.cami.scheduler.CAMIScheduler.server;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

import org.aimas.cami.scheduler.CAMIScheduler.app.CAMITaskSchedulerApp;
import org.aimas.cami.scheduler.CAMIScheduler.domain.Activity;
import org.aimas.cami.scheduler.CAMIScheduler.marshal.ActivityProperties;
import org.aimas.cami.scheduler.CAMIScheduler.utils.Utility;

import io.vertx.core.http.HttpServerResponse;
import io.vertx.core.json.Json;
import io.vertx.ext.web.RoutingContext;

public class RouterConfigActivitySchedule extends RouterConfigImplementation {

	private CAMITaskSchedulerApp camiTaskSchedulerApp;

	public RouterConfigActivitySchedule(CAMITaskSchedulerApp camiTaskSchedulerApp) {
		super();
		this.camiTaskSchedulerApp = camiTaskSchedulerApp;
	}

	public void getActivitySchedule(RoutingContext routingContext) {
		HttpServerResponse response = routingContext.response();

		System.out.println("Handling \"getActivitySchedule\"!");

		if (camiTaskSchedulerApp.getSolutionBusiness().getSolvedFileList().isEmpty()) {
			System.out.println("There is no solution generated, yet.");
			return;
		}

		File solvedSchedule = camiTaskSchedulerApp.getSolutionBusiness().getSolvedFileList().get(0);
		camiTaskSchedulerApp.getProblemSolver().openSolution(solvedSchedule);

		List<Activity> activityList = camiTaskSchedulerApp.getSolutionBusiness().getSolution().getActivityList();

		List<ActivityProperties> activityPropertiesList = new ArrayList<>();

		for (Activity activity : activityList) {
			activityPropertiesList.add(new ActivityProperties(activity.getActivityTypeCode(),
					Utility.convertActivityPeriodToTimestamp(activity.getActivityPeriod()),
					activity.getActivityDuration()));
		}

		// send the response(modified activities) back to client
		response.putHeader("content-type", "application/json; charset=utf-8")
				.end(Json.encodePrettily(activityPropertiesList));
	}
}

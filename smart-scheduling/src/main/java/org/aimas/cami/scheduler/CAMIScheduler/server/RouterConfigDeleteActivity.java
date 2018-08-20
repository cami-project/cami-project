package org.aimas.cami.scheduler.CAMIScheduler.server;

import java.io.File;

import org.aimas.cami.scheduler.CAMIScheduler.app.CAMITaskSchedulerApp;
import org.aimas.cami.scheduler.CAMIScheduler.utils.SolutionUtils;

import io.vertx.core.http.HttpServerResponse;
import io.vertx.core.json.Json;
import io.vertx.ext.web.RoutingContext;

public class RouterConfigDeleteActivity extends RouterConfigImplementation {

	private CAMITaskSchedulerApp camiTaskSchedulerApp;
	private SolutionUtils solutionUtils;

	public RouterConfigDeleteActivity(CAMITaskSchedulerApp camiTaskSchedulerApp) {
		super();
		this.camiTaskSchedulerApp = camiTaskSchedulerApp;
		solutionUtils = new SolutionUtils<>();
	}

	public void deleteActivity(RoutingContext routingContext) {
		HttpServerResponse response = routingContext.response();

		// get the activity that has to be deleted
		String jsonActivity = routingContext.getBodyAsString();

		System.out.println("Handling \"deleteActivity\"!");
		
		if (camiTaskSchedulerApp.getSolutionBusiness().getSolvedFileList().isEmpty()) {
			System.out.println("There is no solution generated, yet.");
			return;
		}

		File solvedSchedule = camiTaskSchedulerApp.getSolutionBusiness().getSolvedFileList().get(0);

		// if there is no working solution in memory, get the last changes from file
		if (camiTaskSchedulerApp.getSolutionBusiness().getSolution() == null)
			camiTaskSchedulerApp.getProblemSolver().openSolution(solvedSchedule);

		solutionUtils.deleteActivityFromSchedule(camiTaskSchedulerApp.getSolutionBusiness(), jsonActivity);

		// save changed solution to file
		if (solvedSchedule != null)
			camiTaskSchedulerApp.getSolutionBusiness().saveSolution(solvedSchedule);

		// send empty response back to client
		response.end();
	}

}

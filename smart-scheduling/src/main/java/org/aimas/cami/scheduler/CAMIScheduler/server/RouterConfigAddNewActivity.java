package org.aimas.cami.scheduler.CAMIScheduler.server;

import java.io.File;
import java.util.List;

import org.aimas.cami.scheduler.CAMIScheduler.app.CAMITaskSchedulerApp;
import org.aimas.cami.scheduler.CAMIScheduler.domain.Activity;
import org.aimas.cami.scheduler.CAMIScheduler.marshal.ChangedActivities;
import org.aimas.cami.scheduler.CAMIScheduler.marshal.ChangedActivity;
import org.aimas.cami.scheduler.CAMIScheduler.utils.SolutionUtils;

import io.vertx.core.Vertx;
import io.vertx.core.http.HttpServerResponse;
import io.vertx.core.json.Json;
import io.vertx.ext.web.RoutingContext;

public class RouterConfigAddNewActivity extends RouterConfigImplementation {

	private CAMITaskSchedulerApp camiTaskSchedulerApp;
	private Vertx vertx;
	private SolutionUtils solutionUtils;

	public RouterConfigAddNewActivity(Vertx vertx, CAMITaskSchedulerApp camiTaskSchedulerApp) {
		super();
		this.vertx = vertx;
		this.camiTaskSchedulerApp = camiTaskSchedulerApp;
		solutionUtils = new SolutionUtils<>();
	}

	public void putNewActivity(RoutingContext routingContext) {

		HttpServerResponse response = routingContext.response();

		// handle adding a new activity to the activitySchedule

		// get the activity from client
		String xmlActivity = routingContext.getBodyAsString();

		// System.out.println("Got from client:\n" + xmlActivity);

		System.out.println("Handling \"putNewActivity\"!");

		if (camiTaskSchedulerApp.getSolutionBusiness().getSolvedFileList().isEmpty()) {
			System.out.println("There is no solution generated, yet.");
			return;
		}

		// get first solution
		File solvedSchedule = camiTaskSchedulerApp.getSolutionBusiness().getSolvedFileList().get(0);
		camiTaskSchedulerApp.getProblemSolver().openSolution(solvedSchedule);

		List<Activity> beforeAddActivityList = camiTaskSchedulerApp.getSolutionBusiness().getSolution()
				.getActivityList();

		vertx.executeBlocking(future -> {

			// add the new activity to the schedule
			solutionUtils.addNewActivityFromXML(camiTaskSchedulerApp.getSolutionBusiness(), xmlActivity,
					camiTaskSchedulerApp.getProblemSolver());

			// wait on solving
			synchronized (camiTaskSchedulerApp.getProblemSolver()) {
				try {
					camiTaskSchedulerApp.getProblemSolver().wait();
				} catch (InterruptedException e) {
					// happens if someone interrupts your thread
				}
			}

			// get the new solution
			List<Activity> afterAddActivityList = camiTaskSchedulerApp.getSolutionBusiness().getSolution()
					.getActivityList();

			// get the modified activities(activity names)
			List<ChangedActivity> changedActivities = solutionUtils.getChangedActivites(beforeAddActivityList,
					afterAddActivityList);

			System.out
					.println("How many activities have modified? Answer: " + changedActivities.size() + " activities.");

			// send the response(modified activities) back to client
			response.putHeader("content-type", "application/json; charset=utf-8")
					.end(Json.encodePrettily(new ChangedActivities(changedActivities.size(), changedActivities, 0)));

			// save changed solution
			camiTaskSchedulerApp.getSolutionBusiness().saveSolution(solvedSchedule);
		}, handler -> {
			System.out.println("The result is: " + handler.result());
		});

	}
}

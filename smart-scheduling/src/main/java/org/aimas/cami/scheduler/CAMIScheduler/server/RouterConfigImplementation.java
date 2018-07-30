package org.aimas.cami.scheduler.CAMIScheduler.server;

import org.aimas.cami.scheduler.CAMIScheduler.app.CAMITaskSchedulerApp;

import io.vertx.core.Vertx;
import io.vertx.ext.web.Router;

public class RouterConfigImplementation extends RouterConfig {

	@Override
	public Router createRoutes(Vertx vertx, CAMITaskSchedulerApp camiTaskSchedulerApp) {
		Router router = this.createRouter(vertx);
		RouterConfigAddNewActivity addNewActivityRoute = new RouterConfigAddNewActivity(vertx, camiTaskSchedulerApp);
		// add a 'add a new activity' route to the router
		router.post(RoutePaths.API_ROUTE + RoutePaths.NEW_ACTIVITY_ROUTE).handler(addNewActivityRoute::putNewActivity);

		RouterConfigActivitySchedule activityScheduleRoute = new RouterConfigActivitySchedule(camiTaskSchedulerApp);
		// add a "get activity schedule" route to the router
		router.get(RoutePaths.API_ROUTE + RoutePaths.ACTIVITY_SCHEDULE_ROUTE)
				.handler(activityScheduleRoute::getActivitySchedule);

		RouterConfigDeleteActivity deleteActivityRoute = new RouterConfigDeleteActivity(camiTaskSchedulerApp);
		// add a "delete activity" route to the router
		router.delete(RoutePaths.API_ROUTE + RoutePaths.DELETE_ACTIVITY_ROUTE)
				.handler(deleteActivityRoute::deleteActivity);

		return router;
	}

}

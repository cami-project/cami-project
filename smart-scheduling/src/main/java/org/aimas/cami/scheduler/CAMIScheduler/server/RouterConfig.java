package org.aimas.cami.scheduler.CAMIScheduler.server;

import org.aimas.cami.scheduler.CAMIScheduler.app.CAMITaskSchedulerApp;

import io.vertx.core.Vertx;
import io.vertx.ext.web.Router;
import io.vertx.ext.web.handler.BodyHandler;

public abstract class RouterConfig {

	public abstract Router createRoutes(Vertx vertx, CAMITaskSchedulerApp camiTaskSchedulerApp);

	protected Router createRouter(Vertx vertx) {
		Router router = Router.router(vertx);
		router.route().handler(BodyHandler.create());

		return router;
	}

}

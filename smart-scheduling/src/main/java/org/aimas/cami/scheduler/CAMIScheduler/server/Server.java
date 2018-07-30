package org.aimas.cami.scheduler.CAMIScheduler.server;

import org.aimas.cami.scheduler.CAMIScheduler.app.CAMITaskSchedulerApp;

import io.vertx.core.AbstractVerticle;
import io.vertx.ext.web.Router;

public class Server extends AbstractVerticle {

	private Router router;
	private static int SERVER_PORT;
	private static String SERVER_HOST;

	// Convenience method so you can run it in your IDE
	public static void main(String[] args) {
		String host = args[0];
		String port = args[1];

		System.out.println("Host: " + host + "\nPort: " + port);
		SERVER_HOST = host;
		SERVER_PORT = Integer.parseInt(port);

		Runner.runExample(Server.class);
	}

	@Override
	public void start() {

		// init the app
		CAMITaskSchedulerApp camiTaskSchedulerApp = new CAMITaskSchedulerApp();

		// create a new empty router
		RouterConfig routerConfig = new RouterConfigImplementation();
		router = routerConfig.createRoutes(vertx, camiTaskSchedulerApp);

		vertx.createHttpServer().requestHandler(router::accept).listen(SERVER_PORT, SERVER_HOST, res -> {
			if (res.succeeded()) {
				System.out.println("Server is now listening!");

				vertx.executeBlocking(future -> {

					CAMITaskSchedulerApp.runApp(camiTaskSchedulerApp);

				}, handler -> {
					System.out.println("The result is: " + handler.result());
				});

			} else {
				System.out.println("Failed to bind!");
			}
		});
	}

}

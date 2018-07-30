package org.aimas.cami.scheduler.CAMIScheduler.notification.client;

import org.aimas.cami.scheduler.CAMIScheduler.domain.Activity;
import org.aimas.cami.scheduler.CAMIScheduler.server.Runner;

import io.vertx.core.AbstractVerticle;
import io.vertx.core.Handler;
import io.vertx.core.buffer.Buffer;
import io.vertx.core.http.HttpClient;
import io.vertx.core.http.HttpClientOptions;

public class Client extends AbstractVerticle {

	private final static String SERVER_HOST = "cami.vitaminsoftware.com";
	public static Activity activity;

	public static void runClient(Activity activity) {
		Client.activity = activity;
		Runner.runExample(Client.class);
	}

	private String getType(Activity activity) {

		if (activity.getActivityCategory() == null)
			return "appointment";

		if (activity.getActivityCategory().getCode().equals("Indoor physical exercises")
				|| activity.getActivityCategory().getCode().equals("Outdoor physical exercises"))
			return "exercise";
		else if (activity.getActivityCategory().getCode().equals("Imposed/Suggested Health measurements")
				|| activity.getActivityCategory().getCode().equals("Medication intake"))
			return "medication";
		else
			return "appointment";

	}

	@Override
	public void start() throws Exception {

		HttpClientOptions options = new HttpClientOptions().setDefaultHost(SERVER_HOST);
		HttpClient client = vertx.createHttpClient(options);

		// send a post request to the endpoint
		postActivityNotification(client, activity);

	}

	private void postActivityNotification(HttpClient client, Activity activity) {

		// System.out.println("What activity? " + activity);

		String journalPayload = "{\"description\": \"Do not forget your scheduled activity relating to: "
				+ activity.getActivityTypeCode() + "!\"," + " \"message\": \"New Activity Reminder\","
				+ " \"severity\": \"none\"," + " \"timestamp\": \"" + System.currentTimeMillis() / 1000L + "\","
				+ " \"type\": \"" + getType(activity) + "\"," + " \"user\": \"/api/v1/user/2/\"" + "}";

		String pushNotification = "{\"user_id\": 2,"
				+ " \"message\": \"Do not forget your scheduled activity relating to: " + activity.getActivityTypeCode()
				+ "!\"" + "}";

		// System.out.println(journalPayload);
		// System.out.println(pushNotification);

		client.post(8008, SERVER_HOST, "/api" + "/v1" + "/journal_entries/", response -> {

			System.out.println("Received response with status code " + response.statusCode());
			System.out.println("Received response with status message " + response.statusMessage());

			response.bodyHandler(new Handler<Buffer>() {

				@Override
				public void handle(Buffer event) {
					System.out.println("Got the event: " + event);
				}
			});
		}).putHeader("content-type", "application/json").end(journalPayload);

		client.post(8010, SERVER_HOST, "/api" + "/v1" + "/insertion" + "/push_notifications/", response -> {

			System.out.println("Received response with status code " + response.statusCode());
			System.out.println("Received response with status message " + response.statusMessage());

			response.bodyHandler(new Handler<Buffer>() {

				@Override
				public void handle(Buffer event) {
					System.out.println("Got the event: " + event);
				}
			});
		}).putHeader("content-type", "application/json").end(pushNotification);

	}

}

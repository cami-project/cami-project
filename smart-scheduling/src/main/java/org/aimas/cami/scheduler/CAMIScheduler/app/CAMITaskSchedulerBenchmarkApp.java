package org.aimas.cami.scheduler.CAMIScheduler.app;

import org.aimas.cami.scheduler.CAMIScheduler.utils.CommonBenchmarkApp;

/**
 * Main benchmark class application.
 * 
 * @author Bogdan
 *
 */
public class CAMITaskSchedulerBenchmarkApp extends CommonBenchmarkApp {

	public static void main(String[] args) {
		new CAMITaskSchedulerBenchmarkApp().buildAndBenchmark(args);
	}

	public CAMITaskSchedulerBenchmarkApp() {
		super(new ArgOption("default", "solver/benchmark/CAMITaskSchedulerBenchmarkConfig.xml"),
				new ArgOption("template", "solver/benchmark/CAMITaskSchedulerBenchmarkTemplateConfig.xml.ftl", true));
	}

}

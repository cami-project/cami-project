/*
 * Copyright 2010 Red Hat, Inc. and/or its affiliates.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package org.aimas.cami.scheduler.CAMIScheduler.solver.solver;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.io.File;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ExecutionException;

import javax.swing.SwingWorker;
import javax.swing.Timer;

import org.aimas.cami.scheduler.CAMIScheduler.domain.Activity;
import org.aimas.cami.scheduler.CAMIScheduler.domain.ActivityPeriod;
import org.aimas.cami.scheduler.CAMIScheduler.domain.ActivitySchedule;
import org.aimas.cami.scheduler.CAMIScheduler.domain.NormalActivity;
import org.aimas.cami.scheduler.CAMIScheduler.domain.ScoreParametrization;
import org.aimas.cami.scheduler.CAMIScheduler.domain.Time;
import org.aimas.cami.scheduler.CAMIScheduler.domain.TimeInterval;
import org.aimas.cami.scheduler.CAMIScheduler.notification.client.Client;
import org.aimas.cami.scheduler.CAMIScheduler.utils.SolutionBusiness;
import org.aimas.cami.scheduler.CAMIScheduler.utils.Utility;
import org.optaplanner.core.api.domain.solution.PlanningSolution;
import org.optaplanner.core.api.score.Score;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * @param <Solution_>
 *            the solution type, the class with the {@link PlanningSolution}
 *            annotation
 */
public class ProblemSolver<Solution_> {

	protected final transient Logger logger = LoggerFactory.getLogger(getClass());

	private final SolutionBusiness<Solution_> solutionBusiness;
	private boolean solvingState;
	private Timer timer;

	public ProblemSolver(SolutionBusiness<Solution_> solutionBusiness) {
		this.solutionBusiness = solutionBusiness;

		registerListeners();
	}

	private void createTimer() {
		timer = new Timer(60000, new TimerActionListener());
		timer.start();
	}

	/**
	 * Event notification using a time listener.
	 */
	class TimerActionListener implements ActionListener {

		public void actionPerformed(ActionEvent e) {

			// if the app isn't solving
			if (!solvingState) {

				ActivitySchedule solution = (ActivitySchedule) solutionBusiness.getSolution();

				if (solution != null) {
					// notify the user if there is a nearby activity by 15
					// minutes
					for (Activity activity : solution.getActivityList()) {
						if (activity.getActivityPeriod() != null) {
							if ((LocalDateTime.now().getDayOfWeek().getValue() - 1) == activity
									.getActivityPeriodWeekday().getDayIndex()) {
								if (Utility.getNumberOfMinutesInInterval(
										new Time(LocalDateTime.now().getHour(), LocalDateTime.now().getMinute()),
										activity.getActivityPeriodTime()) == 15) {

									// send notification to device
									Client.runClient(activity);
								}
							}
						}
					}
				}
			}

		}

	}

	private void registerListeners() {
		solutionBusiness.registerForBestSolutionChanges(this);
	}

	public void bestSolutionChanged() {
		// do nothing
	}

	public void init() {
		createTimer();
	}

	/**
	 * Get the {@link ScoreParametrization} from file and update it in solution.
	 */
	private void setScoreParametrization() {
		solutionBusiness.doProblemFactChange(scoreDirector -> {

			ActivitySchedule activitySchedule = (ActivitySchedule) solutionBusiness.getSolution();
			ScoreParametrization solutionScoreParametrization = activitySchedule.getScoreParametrization();
			ScoreParametrization workingScoreParametrization = scoreDirector
					.lookUpWorkingObject(solutionScoreParametrization);

			ScoreParametrization scoreParametrization = Utility.getScoreParametrization(
					(ActivitySchedule) solutionBusiness.getSolution(),
					new File(new File(solutionBusiness.getUnsolvedDataDir().getParentFile(), ""),
							"Score parametrization" + ".xml"));

			scoreParametrization.setId(0L);

			if (workingScoreParametrization != null) {
				scoreDirector.beforeProblemFactRemoved(workingScoreParametrization);
				activitySchedule.setScoreParametrization(null);
				scoreDirector.afterProblemFactRemoved(workingScoreParametrization);
			}

			scoreDirector.beforeProblemFactAdded(scoreParametrization);
			activitySchedule.setScoreParametrization(scoreParametrization);
			scoreDirector.afterProblemFactAdded(scoreParametrization);

			scoreDirector.triggerVariableListeners();

		});
	}

	/**
	 * Start a new solving(when reschedule)
	 */
	public void startSolveAction() {
		if (!solvingState) {
			setSolvingState(true);
			Solution_ planningProblem = solutionBusiness.getSolution();
			new SolveWorker(planningProblem).execute();
		}
	}

	protected class SolveWorker extends SwingWorker<Solution_, Void> {

		protected final Solution_ planningProblem;

		public SolveWorker(Solution_ planningProblem) {
			this.planningProblem = planningProblem;
		}

		@Override
		protected Solution_ doInBackground() throws Exception {
			return solutionBusiness.solve(planningProblem);
		}

		@Override
		protected void done() {
			try {
				Solution_ bestSolution = get();
				solutionBusiness.setSolution(bestSolution);
			} catch (InterruptedException e) {
				Thread.currentThread().interrupt();
				throw new IllegalStateException("Solving was interrupted.", e);
			} catch (ExecutionException e) {
				throw new IllegalStateException("Solving failed.", e.getCause());
			} finally {

				// notify the thread that is waiting that the solving has ended
				synchronized (ProblemSolver.this) {
					ProblemSolver.this.notify();
				}

				setSolvingState(false);
				resetScreen();
			}
		}

	}

	public void openSolution(File schedule) {
		solutionBusiness.openSolution(schedule);
		setSolutionLoaded();

		resetValueRange();
		setScoreParametrization();
	}

	/**
	 * When a new schedule is opened from file, reset all the activity domain
	 * value ranges. In real time rescheduling, activities after the current
	 * time will have a more restricted value range, and activities before the
	 * current time will be immovable.
	 */
	protected void resetValueRange() {

		ActivitySchedule activitySchedule = (ActivitySchedule) solutionBusiness.getSolution();

		solutionBusiness.doProblemFactChange(scoreDirector -> {
			for (Activity activity : activitySchedule.getActivityList()) {
				if (activity instanceof NormalActivity) {
					Activity workingActivity = scoreDirector.lookUpWorkingObject(activity);

					List<ActivityPeriod> periodDomainRange = Utility.determineValueRange(activitySchedule, workingActivity);

					scoreDirector.beforeProblemPropertyChanged(workingActivity);
					((NormalActivity) workingActivity).setPeriodDomainRangeList(periodDomainRange);
					scoreDirector.afterProblemPropertyChanged(workingActivity);
				}
			}
		});
	}

	private void setSolutionLoaded() {
		setSolvingState(false);
		resetScreen();
	}

	private void setSolvingState(boolean solving) {

		solvingState = solving;

		if (!solving) {
			setScoreParametrization();
		}

	}

	public void resetScreen() {
		setScoreParametrization();
	}

	public void refreshScoreField(Score score) {
		// do nothing
	}

}

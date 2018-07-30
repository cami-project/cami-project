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

package org.aimas.cami.scheduler.CAMIScheduler.utils;

import org.aimas.cami.scheduler.CAMIScheduler.solver.solver.ProblemSolver;
import org.optaplanner.core.api.domain.solution.PlanningSolution;
import org.optaplanner.core.api.solver.Solver;
import org.optaplanner.core.api.solver.SolverFactory;
import org.optaplanner.swing.impl.SwingUncaughtExceptionHandler;
import org.optaplanner.swing.impl.SwingUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import ch.qos.logback.classic.LoggerContext;

/**
 * @param <Solution_>
 *            the solution type, the class with the {@link PlanningSolution}
 *            annotation
 */
public abstract class CommonApp<Solution_> extends LoggingMain {

	protected static final Logger logger = LoggerFactory.getLogger(CommonApp.class);

	/**
	 * Some examples are not compatible with every native LookAndFeel. For example,
	 * NurseRosteringPanel is incompatible with Mac.
	 */
	public static void prepareSwingEnvironment() {
		LoggerContext loggerContext = (LoggerContext) LoggerFactory.getILoggerFactory();
		loggerContext.stop();
	}

	protected final String name;
	protected final String description;
	protected final String solverConfig;
	protected final String iconResource;

	protected ProblemSolver<Solution_> problemSolver;
	protected SolutionBusiness<Solution_> solutionBusiness;

	protected CommonApp(String name, String description, String solverConfig, String iconResource) {
		this.name = name;
		this.description = description;
		this.solverConfig = solverConfig;
		this.iconResource = iconResource;
	}

	public String getName() {
		return name;
	}

	public String getDescription() {
		return description;
	}

	public String getIconResource() {
		return iconResource;
	}

	public void init() {
		solutionBusiness = createSolutionBusiness();
		problemSolver = new ProblemSolver<>(solutionBusiness);
		problemSolver.init();
	}

	public SolutionBusiness<Solution_> createSolutionBusiness() {
		SolutionBusiness<Solution_> solutionBusiness = new SolutionBusiness<>(this);
		solutionBusiness.setSolutionDao(createSolutionDao());
		solutionBusiness.updateDataDirs();
		solutionBusiness.setSolver(createSolver());
		return solutionBusiness;
	}

	protected Solver<Solution_> createSolver() {
		SolverFactory<Solution_> solverFactory = SolverFactory.createFromXmlResource(solverConfig);
		return solverFactory.buildSolver();
	}

	protected abstract SolutionDao createSolutionDao();

	public SolutionBusiness<Solution_> getSolutionBusiness() {
		return solutionBusiness;
	}

	/**
	 * Just for testing. To see what happens in GUI as a result from a client request.
	 */
	public ProblemSolver<Solution_> getProblemSolver() {
		return problemSolver;
	}

}

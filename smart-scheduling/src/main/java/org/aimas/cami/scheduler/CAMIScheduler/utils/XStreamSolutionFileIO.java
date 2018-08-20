package org.aimas.cami.scheduler.CAMIScheduler.utils;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.Reader;
import java.io.Writer;

import org.optaplanner.core.api.domain.solution.PlanningSolution;
import org.optaplanner.persistence.common.api.domain.solution.SolutionFileIO;

import com.thoughtworks.xstream.XStream;
import com.thoughtworks.xstream.XStreamException;
import com.thoughtworks.xstream.io.json.JettisonMappedXmlDriver;
import com.thoughtworks.xstream.security.AnyTypePermission;

/**
 * Security warning: only use this class with JSON files from a trusted source,
 * because {@link XStream} is configured to allow all permissions, which can be
 * exploited if the JSON comes from an untrusted source.
 * 
 * @param <Solution_> the solution type, the class with the
 *        {@link PlanningSolution} annotation
 */
public class XStreamSolutionFileIO<Solution_> implements SolutionFileIO<Solution_> {

	protected XStream xStream;

	public XStreamSolutionFileIO(Class... xStreamAnnotatedClasses) {
		xStream = new XStream(new JettisonMappedXmlDriver());
		xStream.setMode(XStream.NO_REFERENCES);
		xStream.processAnnotations(xStreamAnnotatedClasses);
		XStream.setupDefaultSecurity(xStream);
		// Presume the JSON file comes from a trusted source so it works out of the box.
		// See class javadoc.
		xStream.addPermission(new AnyTypePermission());
	}

	public XStream getXStream() {
		return xStream;
	}

	@Override
	public String getInputFileExtension() {
		return "json";
	}

	@Override
	public Solution_ read(File inputSolutionFile) {
		// xStream.fromXml(InputStream) does not use UTF-8
		try (Reader reader = new InputStreamReader(new FileInputStream(inputSolutionFile), "UTF-8")) {
			Solution_ solution = (Solution_) xStream.fromXML(reader);
			reader.close();
			return solution;
		} catch (XStreamException | IOException e) {
			throw new IllegalArgumentException("Failed reading inputSolutionFile (" + inputSolutionFile + ").", e);
		}
	}

	@Override
	public void write(Solution_ solution, File outputSolutionFile) {
		try (Writer writer = new OutputStreamWriter(new FileOutputStream(outputSolutionFile), "UTF-8")) {
			xStream.toXML(solution, writer);
			writer.close();
		} catch (IOException e) {
			throw new IllegalArgumentException("Failed writing outputSolutionFile (" + outputSolutionFile + ").", e);
		}
	}

}

using PredictionLogic.Prediction.BenchmarksAndReaders;
using PredictionLogic.Prediction.BenchmarksAndReaders.Stanford;
using PredictionLogic.SimulationStatistics;
using PredictionLogic.Prediction;

namespace PredictionLogic.Simulation {
	public static class Simulator {
		public static BenchmarkStatisticsResult Call(PredictorInfo predictorInfo, BenchmarkInfo benchmarkInfo, SimulationOptions simulationParameters, ApplicationOptions applicationOptions) {
			if (simulationParameters == null) {
				simulationParameters = SimulationOptions.defaultOptions;
			}

			string folder = applicationOptions.TracePathMain;
			ITraceReader traceReader = new StanfordReader();
			BenchmarkStatisticsResult currentResult = null;

			// obtain predictor
			IPredictor predictor = predictorInfo.getPredictor();
			if (predictor == null) {
				return new BenchmarkStatisticsResult(benchmarkInfo.benchmarkName + " - Probleme de implementare la predictor.");
			}

			// open the trace file
			if (!traceReader.openTrace(folder, benchmarkInfo.benchmarkName)) {
				return new BenchmarkStatisticsResult(benchmarkInfo.benchmarkName + " - Fisierul tra nu a fost gasit. Verificati calea sau daca fisierul se afla in cale.");
			}

			// create new entry
			currentResult = new BenchmarkStatisticsResult(benchmarkInfo.benchmarkName);
			currentResult.NumberOfCorrectPredictions = 0;
			currentResult.NumberOfIncorrectPredictions = 0;

			while (true) {
				// get the next branch
				IBranch branch = traceReader.getNextBranch();

				// null means trace end
				if (branch == null) {
					break;
				}

				BranchInfo currentBranchInfo = branch.getBranchInfo();
				// predict
				bool prediction = predictor.predictBranch(currentBranchInfo);

				if (branch.taken() == prediction) {
					currentResult.NumberOfCorrectPredictions++;
				}
				else {
					currentResult.NumberOfIncorrectPredictions++;
				}

				// update the predictor
				predictor.update(branch);
			}

			traceReader.closeTrace();

			currentResult.NumberOfBranches = currentResult.NumberOfCorrectPredictions + currentResult.NumberOfIncorrectPredictions;
			currentResult.Accuracy = (double)currentResult.NumberOfCorrectPredictions / currentResult.NumberOfBranches;

			// free the predictor instance for future reusability
			predictorInfo.freePredictor(predictor);

			return currentResult;
		}
	}
}

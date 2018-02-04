using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using PredictionLogic.Prediction.BenchmarksAndReaders;
using PredictionLogic.Prediction.BenchmarksAndReaders.Stanford;
using PredictionLogic.Prediction.BenchmarksAndReaders.SPEC2000;
using PredictionLogic.Prediction.BenchmarksAndReaders.CBP2;
using PredictionLogic.SimulationStatistics;
using PredictionLogic.Prediction;

namespace PredictionLogic.Simulation
{
    public static class Simulator
    {
        public static BenchmarkStatisticsResult simulate(PredictorInfo predictorInfo, BenchmarkInfo benchmarkInfo, SimulationOptions simulationParameters, ApplicationOptions applicationOptions)
        {
            if (simulationParameters == null)
            {
                simulationParameters = SimulationOptions.defaultOptions;
            }

            string folder = "";
            ITraceReader traceReader = null;
            BenchmarkStatisticsResult currentResult = null;

            switch (benchmarkInfo.benchmarkType)
            {
                case BenchmarkType.Stanford:
                    folder = applicationOptions.TracePathStanford;
                    traceReader = new StanfordReader();
                    break;
                case BenchmarkType.SPEC2000:
                    folder = applicationOptions.TracePathSpec2000;
                    traceReader = new Spec2000Reader();
                    break;
                case BenchmarkType.CBP2:
                    folder = applicationOptions.TracePathCBP2;
                    traceReader = new CBP2Reader();
                    break;
            }

            // obtain predictor
            IPredictor predictor = predictorInfo.getPredictor();
            if (predictor == null)
            {
                return new BenchmarkStatisticsResult(benchmarkInfo.benchmarkName + " - Could not be performed: The predictor could not be instantiated on the client.");
            }

            // open the trace file
            if (!traceReader.openTrace(folder, benchmarkInfo.benchmarkName))
            {
                return new BenchmarkStatisticsResult(benchmarkInfo.benchmarkName + " - Could not be performed: The trace file could not be opened on the client.");
            }

            // create new entry
            currentResult = new BenchmarkStatisticsResult(benchmarkInfo.benchmarkName);
            currentResult.NumberOfCorrectPredictions = 0;
            currentResult.NumberOfIncorrectPredictions = 0;


            // check if we should skip N branches when counting the performance (to avoid accounting for the predictor warmup)
            int branchIndex = 0;
            int numberOfBranchesToSkip = simulationParameters.NumberOfBranchesToSkip;
            while (true)
            {
                // get the next branch
                IBranch branch = traceReader.getNextBranch();

                // null means trace end
                if (branch == null)
                {
                    break;
                }

                BranchInfo currentJumpInfo = branch.getBranchInfo();
                if (!simulationParameters.ConditionalOnly || ((branch.getBranchInfo().branchFlags & BranchInfo.BR_CONDITIONAL) > 0))
                {
                    // predict
                    bool prediction = predictor.predictBranch(branch.getBranchInfo());

                    if (branchIndex >= numberOfBranchesToSkip)
                    {
                        if (branch.taken() == prediction)
                        {
                            currentResult.NumberOfCorrectPredictions++;
                        }
                        else
                        {
                            currentResult.NumberOfIncorrectPredictions++;
                        }
                    }

                    // update the predictor
                    predictor.update(branch);

                    branchIndex++;
                }
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

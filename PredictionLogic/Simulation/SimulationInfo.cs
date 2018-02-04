using System;
using System.Collections.Generic;
using System.Text;
using PredictionLogic.Prediction.BenchmarksAndReaders;
using PredictionLogic.SimulationStatistics;
using PredictionLogic.Prediction;

namespace PredictionLogic.Simulation
{
    [Serializable]
    public class SimulationInfo
    {
        public readonly PredictorInfo predictorInfo;
        public readonly BenchmarkInfo benchmarkInfo;

        public SimulationInfo(PredictorInfo predictorInfo, BenchmarkInfo benchmarkInfo)
        {
            this.predictorInfo = predictorInfo;
            this.benchmarkInfo = benchmarkInfo;
        }

        public SimulationInfo(PredictorInfo predictorInfo, string benchmarkName, BenchmarkType benchmarkType)
        {
            this.predictorInfo = predictorInfo;
            this.benchmarkInfo = new BenchmarkInfo(benchmarkName, benchmarkType);
        }

        public BenchmarkStatisticsResult simulate(SimulationOptions simulationOptions, ApplicationOptions applicationOptions)
        {
            return Simulator.simulate(predictorInfo, benchmarkInfo, simulationOptions, applicationOptions);
        }
    }
}

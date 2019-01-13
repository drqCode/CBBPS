using System;
using System.Threading;
using PredictionLogic.SimulationStatistics;
using PredictionLogic.Simulation;
using GAgSimulator.SimulationResultStructures;

namespace GAgSimulator {
	public partial class WindowMain {
		private Thread[] threads;
		private int localThreadNumber = 1;
		private int threadSlots = Environment.ProcessorCount;

		private void startWork() {
			SimulationOptionsCurrentExecution = SimulationOptionsForBinding.GetCopy();
			session = new SimulationSession(SimulationOptionsCurrentExecution);

			MainChart.StartDrawData(simulationResultsDictionary);

			threads = null;

			threads = new Thread[threadSlots];
			for (int i = 0; i < threads.Length; i++) {
				threads[i] = new Thread(this.performWork);
				threads[i].Name = "Local Thread " + localThreadNumber++;
				threads[i].Start();
			}


			btnSimulate.IsEnabled = false;
			btnAbort.IsEnabled = true;
		}

		public void proxyTaskRequestReceived(object sender, EventArgs e) {
			SimulationInfo currentSimulation;
			lock (simulationQueue) {
				if (simulationQueue.Count > 0) {
					currentSimulation = simulationQueue.Dequeue();
				}
				else {
					return;
				}
			}

		}


		private void performWork() {
			try {
				SimulationInfo currentSimulation;
				while (true) {
					// acquire data
					lock (simulationQueue) {
						if (simulationQueue.Count > 0) {
							currentSimulation = simulationQueue.Dequeue();
						}
						else {
							return;
						}
					}

					// big work
					BenchmarkStatisticsResult result = currentSimulation.simulate(this.SimulationOptionsCurrentExecution, this.AppOptions);

					// results
					lock (simulationResultsDictionary) {
						simulationResultsDictionary.setResultUsingDispatcher(this.Dispatcher, currentSimulation.predictorInfo, currentSimulation.benchmarkInfo, result);
					}
				}
			}
			catch (ThreadAbortException) {
				Thread.ResetAbort();
				return;
			}
		}

		private void abortAllWork() {
			if (threads != null) {
				foreach (Thread t in threads) {
					t.Abort();
					t.Join();
				}
			}

			btnAbort.IsEnabled = false;
			btnSimulate.IsEnabled = true;
		}

		private void simulationResultsDictionaryFilled(object sender, EventArgs e) {
			this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(this.workFinished));
		}

		private void workFinished() {
			btnAbort.IsEnabled = false;
			btnSimulate.IsEnabled = true;
		}

		private void beginInvokePrintResults(object toPrint) {
			this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(
				delegate () {
					displayedResults.Add(toPrint);
				}
			));
		}

		private void beginInvokeBenchmarkStatisticsCollectionAdd(BenchmarkStatisticsCollection benchmarkStatisticsCollection, BenchmarkStatisticsResult benchmarkStatisticsResult) {
			this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(
				delegate () {
					benchmarkStatisticsCollection.addSorted(benchmarkStatisticsResult);
				}
			));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using PredictionLogic.Prediction.PredictorPropertyTypes;

namespace PredictionLogic.Prediction {
	public class GAg : IPredictor {
		private const int historyLengthMinimum = 1;
		private const int historyLengthMaximum = 17;
		private const int historyLengthDefault = 8;

		private const int branchAddressLowBitsMinimum = 1;
		private const int branchAddressLowBitsMaximum = 12;
		private const int branchAddressLowBitsDefault = 8;

		private const int counterBitsMinimum = 1;
		private const int counterBitsMaximum = 5;
		private const int counterBitsDefault = 3;

		public static readonly List<PredictorPropertyBase> Properties = new List<PredictorPropertyBase>();

		static GAg() {
			Properties.Add(new PredictorInt32Property("historyLength", "History length", historyLengthDefault, historyLengthMinimum, historyLengthMaximum));
			Properties.Add(new PredictorInt32Property("branchAddressLowBits", "Branch address low bits", branchAddressLowBitsDefault, branchAddressLowBitsMinimum, branchAddressLowBitsMaximum));
			Properties.Add(new PredictorInt32Property("counterBits", "Counter bits", counterBitsDefault, counterBitsMinimum, counterBitsMaximum));
		}

		struct PatternHistoryTableEntry {
			public byte tag;
			public sbyte counter;
		};

		int patternHistoryTableSize;
		PatternHistoryTableEntry[] patternHistoryTable;

		int counterBits;
		byte counterMax;

		int history;
		int historyLength;
		int historyMask;
		int branchAddressLowBits;
		int highBranchAddressMask;
		int lowBranchAddressMask;

		int patternHistoryTableIndex;
		byte tag;
		bool prediction;

		public GAg(int historyLength, int branchAddressLowBits, int counterBits) {
			this.historyLength = historyLength;
			historyMask = PowerOf2(historyLength) - 1;

			this.branchAddressLowBits = branchAddressLowBits;
			this.counterBits = counterBits;

			patternHistoryTableSize = PowerOf2(branchAddressLowBits + historyLength);
			patternHistoryTable = new PatternHistoryTableEntry[patternHistoryTableSize];

			counterMax = (byte)(PowerOf2(counterBits - 1) - 1);

			lowBranchAddressMask = PowerOf2(branchAddressLowBits) - 1;
			// tag 8 bits - counter bits
			highBranchAddressMask = PowerOf2(branchAddressLowBits + 8 - counterBits) - 1;
		}

		#region IPredictor Members

		public bool predictBranch(BranchInfo jumpInfo) {
			patternHistoryTableIndex = (int)(((jumpInfo.address & lowBranchAddressMask) << historyLength) + (history & historyMask));
			tag = (byte)((jumpInfo.address & highBranchAddressMask) >> branchAddressLowBits);
			if (patternHistoryTable[patternHistoryTableIndex].tag == tag) {
				prediction = (patternHistoryTable[patternHistoryTableIndex].counter >= 0);
			}
			else {
				prediction = true; // predict default taken
			}
			return prediction;
		}

		public void update(IBranch branch) {
			if (patternHistoryTable[patternHistoryTableIndex].tag == tag) {
				if (branch.taken()) {
					if (patternHistoryTable[patternHistoryTableIndex].counter < counterMax) {
						patternHistoryTable[patternHistoryTableIndex].counter++;
					}
				}
				else {
					if (patternHistoryTable[patternHistoryTableIndex].counter >= -counterMax) {
						patternHistoryTable[patternHistoryTableIndex].counter--;
					}
				}
			}
			else {
				patternHistoryTable[patternHistoryTableIndex].tag = tag;
				if (branch.taken()) {
					patternHistoryTable[patternHistoryTableIndex].counter = 1;
				}
				else {
					patternHistoryTable[patternHistoryTableIndex].counter = -1;
				}
			}

			history <<= 1;
			if (branch.taken()) {
				history++;
			}
		}

		public void reset() {
			history = 0;
			for (int i = 0; i < patternHistoryTableSize; i++) {
				patternHistoryTable[i].tag = 0;
				patternHistoryTable[i].counter = 0;
			}
		}

		private static int PowerOf2(int power) {
			return 1 << power;
		}

		#endregion
	}
}

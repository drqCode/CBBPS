using System;
using System.Collections.Generic;
using System.Text;
using PredictionLogic.Prediction.PredictorPropertyTypes;

namespace PredictionLogic.Prediction.Predictors
{
    // this predictor does not implement dynamic history length fitting as described in the original paper
    public class OGEHL : IPredictor
    {
        private const int numberOfTablesMinimum = 2;
        private const int numberOfTablesMaximum = 20;
        private const int numberOfTablesDefault = 12;

        private const int tableLengthBitsMinimum = 2;
        private const int tableLengthBitsMaximum = 24;
        private const int tableLengthBitsDefault = 16;

        private const int counterBitsMinimum = 2;
        private const int counterBitsMaximum = 16;
        private const int counterBitsDefault = 8;

        private const int historyLengthMinimum = 1;
        private const int historyLengthMaximum = 1048576;
        private const int historyLengthDefault = 301;

        private const int initialThresholdMinimum = 0;
        private const int initialThresholdMaximum = 1024;
        private const int initialThresholdDefault = 14;

        private const int thresholdAdaptBitsMinimum = 0;
        private const int thresholdAdaptBitsMaximum = 16;
        private const int thresholdAdaptBitsDefault = 3;

        public static readonly string ToolTipInfo = "Optimised GEometric History Length Branch Predictor";
        public static readonly List<PredictorPropertyBase> Properties = new List<PredictorPropertyBase>();

        static OGEHL()
        {
            Properties.Add(new PredictorInt32Property("numberOfTables", "Number of tables", numberOfTablesDefault, numberOfTablesMinimum, numberOfTablesMaximum));
            Properties.Add(new PredictorInt32Property("tableLengthBits", "Table length bits", tableLengthBitsDefault, tableLengthBitsMinimum, tableLengthBitsMaximum));
            Properties.Add(new PredictorInt32Property("counterBits", "Counter bits", counterBitsDefault, counterBitsMinimum, counterBitsMaximum));
            Properties.Add(new PredictorInt32Property("historyLength", "History length", historyLengthDefault, historyLengthMinimum, historyLengthMaximum));
            Properties.Add(new PredictorInt32Property("initialThreshold", "Initial threshold", initialThresholdDefault, initialThresholdMinimum, initialThresholdMaximum));
            Properties.Add(new PredictorInt32Property("thresholdAdaptBits", "Threshold adapt bits", thresholdAdaptBitsDefault, thresholdAdaptBitsMinimum, thresholdAdaptBitsMaximum));
        }

        private static int[] predefinedTableLengthValues = { 0, 2, 4, 9, 12, 18, 31, 54, 114, 145, 266, 301 };

        int numberOfTables;
        int tableLength;
        int tableLengthBits;

        short[,] weights;
        int[] tableHash;

        int counterBits;
        int counterMax;
        int counterMin;

        bool adaptThresold;
        int thresholdAdaptCount;
        int thresholdAdaptCountMax;
        int thresholdAdaptCountMin;

        int initialThreshold;
        int threshold;

        int sum;
        bool prediction;

        int globalHistoryLength;
        int[] globalHistory;

        int[] intermediateHash;
        int[] branchAddressBits;

        public OGEHL(int numberOfTables, int tableLengthBits, int counterBits, int historyLength, int initialThreshold, int thresholdAdaptBits)
        {
            this.numberOfTables = numberOfTables;

            this.tableLengthBits = tableLengthBits;
            this.tableLength = 1 << tableLengthBits;

            this.counterBits = counterBits;
            counterMax = (1 << counterBits - 1) - 1;
            counterMin = -counterMax - 1;

            weights = new short[numberOfTables, tableLength];
            tableHash = new int[numberOfTables];

            globalHistoryLength = historyLength;
            globalHistory = new int[globalHistoryLength];

            intermediateHash = new int[tableLengthBits];
            branchAddressBits = new int[32];

            this.initialThreshold = initialThreshold;
            this.threshold = initialThreshold;

            thresholdAdaptCount = 0;
            if (thresholdAdaptBits != 0)
            {
                thresholdAdaptCountMax = (1 << (thresholdAdaptBits - 1)) - 1;
                thresholdAdaptCountMin = -thresholdAdaptCountMax - 1;
                adaptThresold = true;
            }
            else
            {
                thresholdAdaptCountMax = 0;
                adaptThresold = false;
            }
        }

        #region IPredictor Members

        private int getLengthForTable(int tableIndex)
        {
            if (tableIndex <= 11)
            {
                return predefinedTableLengthValues[tableIndex];
            }
            return 1 << (tableIndex - 3);
        }

        private void convertBranchAddressToBits(uint branchAddress)
        {
            for (int i = 0; i < tableLengthBits; i++)
            {
                branchAddressBits[i] = (int)((branchAddress >> i) & 1);
            }
        }

        public bool predictBranch(BranchInfo branch)
        {
            convertBranchAddressToBits(branch.address);

            sum = 0;
            for (int tableIndex = 0; tableIndex < numberOfTables; tableIndex++)
            {
                // fill hash with the last branch address bits
                for (int i = 0; i < tableLengthBits; i++)
                {
                    intermediateHash[tableLengthBits - i - 1] = branchAddressBits[i]; // n - i - 1 to prevent colisions with short histories
                }

                int historyLengthForTable = getLengthForTable(tableIndex);
                if (historyLengthForTable > globalHistoryLength)
                {
                    historyLengthForTable = globalHistoryLength;
                }

                // using xor for hashing (any hashing function works)
                // simulation wastes a lot of time in this loop, but in hardware implementations this will all work in parallel
                for (int historyIndex = 0; historyIndex < historyLengthForTable; historyIndex++)
                {
                    intermediateHash[historyIndex % tableLengthBits] ^= globalHistory[historyIndex];
                }

                tableHash[tableIndex] = 0;
                for (int i = 0; i < tableLengthBits; i++)
                {
                    tableHash[tableIndex] = (tableHash[tableIndex] << 1) + intermediateHash[i];
                }
                sum += weights[tableIndex, tableHash[tableIndex]];
            }

            prediction = sum >= 0;
            return prediction;
        }

        public void update(IBranch branch)
        {
            if ((branch.taken() != prediction) || (Math.Abs(sum) <= threshold))
            {
                for (int i = 0; i < numberOfTables; i++)
                {
                    if (branch.taken())
                    {
                        if (weights[i, tableHash[i]] < counterMax)
                        {
                            weights[i, tableHash[i]]++;
                        }
                    }
                    else
                    {
                        if (weights[i, tableHash[i]] > counterMin)
                        {
                            weights[i, tableHash[i]]--;
                        }
                    }
                }
            }

            if (adaptThresold)
            {
                if (branch.taken() != prediction)
                {
                    thresholdAdaptCount++;
                    if (thresholdAdaptCount == thresholdAdaptCountMax)
                    {
                        thresholdAdaptCount = 0;
                        threshold++;
                    }
                }
                else
                {
                    if (Math.Abs(sum) <= threshold)
                    {
                        thresholdAdaptCount--;
                        if (thresholdAdaptCount == thresholdAdaptCountMin)
                        {
                            thresholdAdaptCount = 0;
                            threshold--;
                        }
                    }
                }
            }

            // shift global history
            for (int i = globalHistoryLength - 1; i > 0; i--)
            {
                globalHistory[i] = globalHistory[i - 1];
            }

            if (branch.taken())
            {
                globalHistory[0] = 1;
            }
            else
            {
                globalHistory[0] = 0;
            }
        }

        public void reset()
        {
            for (int i = 0; i < numberOfTables; i++)
            {
                for (int j = 0; j < tableLength; j++)
                {
                    weights[i, j] = 0;
                }
            }

            for (int i = 0; i < globalHistoryLength; i++)
            {
                globalHistory[i] = 0;
            }

            thresholdAdaptCount = 0;
            threshold = initialThreshold;
        }

        #endregion
    }
}

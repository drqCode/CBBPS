using System;
using System.Collections.Generic;
using System.Text;
using PredictionLogic.Prediction.PredictorPropertyTypes;

namespace PredictionLogic.Prediction.Predictors
{
    public class PAp : IPredictor
    {
        private const int historyLengthMinimum = 1;
        private const int historyLengthMaximum = 16;
        private const int historyLengthDefault = 8;

        private const int branchAddressLowBitsMinimum = 1;
        private const int branchAddressLowBitsMaximum = 12;
        private const int branchAddressLowBitsDefault = 8;

        private const int counterBitsMinimum = 1;
        private const int counterBitsMaximum = 5;
        private const int counterBitsDefault = 3;

        public static readonly List<PredictorPropertyBase> Properties = new List<PredictorPropertyBase>();

        static PAp()
        {
            Properties.Add(new PredictorInt32Property("historyLength", "History length", historyLengthDefault, historyLengthMinimum, historyLengthMaximum));
            Properties.Add(new PredictorInt32Property("branchAddressLowBits", "Branch address low bits", branchAddressLowBitsDefault, branchAddressLowBitsMinimum, branchAddressLowBitsMaximum));
            Properties.Add(new PredictorInt32Property("counterBits", "Counter bits", counterBitsDefault, counterBitsMinimum, counterBitsMaximum));
        }

        struct LocalPatternHistoryTableEntry
        {
            public byte tag;
            public ushort history;
            public sbyte[] globalPatternHistoryTable;
        };

        LocalPatternHistoryTableEntry[] localPatternHistoryTable;
        int globalPatternHistoryTableSize;
        int localPatternHistoryTableSize;
        int globalPatternHistoryTableIndex;
        int localPatternHistoryTableIndex;

        int historyLength;
        int historyMask;
        int branchAddressLowBits;
        int lowBranchAddressMask;

        int counterBits;
        byte counterMax;
        bool prediction;

        public PAp(int historyLength, int branchAddressLowBits, int counterBits)
        {
            this.historyLength = historyLength;
            historyMask = (1 << historyLength) - 1;

            this.branchAddressLowBits = branchAddressLowBits;
            this.counterBits = counterBits;

            localPatternHistoryTableSize = 1 << branchAddressLowBits;
            globalPatternHistoryTableSize = 1 << historyLength;

            localPatternHistoryTable = new LocalPatternHistoryTableEntry[localPatternHistoryTableSize];

            for (int i = 0; i < localPatternHistoryTableSize; i++)
            {
                localPatternHistoryTable[i].globalPatternHistoryTable = new sbyte[globalPatternHistoryTableSize];
            }

            counterMax = (byte)((1 << counterBits - 1) - 1);
            lowBranchAddressMask = (1 << branchAddressLowBits) - 1;
        }

        #region IPredictor Members

        public bool predictBranch(BranchInfo branch)
        {
            localPatternHistoryTableIndex = (int)(branch.address & lowBranchAddressMask);
            globalPatternHistoryTableIndex = (int)(localPatternHistoryTable[localPatternHistoryTableIndex].history & historyMask);

            prediction = (localPatternHistoryTable[localPatternHistoryTableIndex].globalPatternHistoryTable[globalPatternHistoryTableIndex] >= 0);
            return prediction;
        }

        public void update(IBranch branch)
        {
            if (branch.taken())
            {
                if (localPatternHistoryTable[localPatternHistoryTableIndex].globalPatternHistoryTable[globalPatternHistoryTableIndex] < counterMax)
                {
                    localPatternHistoryTable[localPatternHistoryTableIndex].globalPatternHistoryTable[globalPatternHistoryTableIndex]++;
                }
            }
            else
            {
                if (localPatternHistoryTable[localPatternHistoryTableIndex].globalPatternHistoryTable[globalPatternHistoryTableIndex] >= -counterMax)
                {
                    localPatternHistoryTable[localPatternHistoryTableIndex].globalPatternHistoryTable[globalPatternHistoryTableIndex]--;
                }
            }

            localPatternHistoryTable[localPatternHistoryTableIndex].history <<= 1;
            if (branch.taken())
            {
                localPatternHistoryTable[localPatternHistoryTableIndex].history++;
            }
        }

        public void reset()
        {
            for (int i = 0; i < localPatternHistoryTableSize; i++)
            {
                localPatternHistoryTable[i].history = 0;
                for (int j = 0; j < globalPatternHistoryTableSize; j++)
                {
                    localPatternHistoryTable[i].globalPatternHistoryTable[j] = 0;
                }
            }
        }

        #endregion
    }
}

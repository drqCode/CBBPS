using System;
using System.Collections.Generic;
using System.Text;
using PredictionLogic.Prediction.PredictorPropertyTypes;

namespace PredictionLogic.Prediction.Predictors
{
    public class PAg : IPredictor
    {
        private const int historyLengthMinimum = 1;
        private const int historyLengthMaximum = 28;
        private const int historyLengthDefault = 8;

        private const int branchAddressLowBitsMinimum = 1;
        private const int branchAddressLowBitsMaximum = 12;
        private const int branchAddressLowBitsDefault = 8;

        private const int counterBitsMinimum = 1;
        private const int counterBitsMaximum = 5;
        private const int counterBitsDefault = 2;

        public static readonly List<PredictorPropertyBase> Properties = new List<PredictorPropertyBase>();

        static PAg()
        {
            Properties.Add(new PredictorInt32Property("historyLength", "History length", historyLengthDefault, historyLengthMinimum, historyLengthMaximum));
            Properties.Add(new PredictorInt32Property("branchAddressLowBits", "Branch address low bits", branchAddressLowBitsDefault, branchAddressLowBitsMinimum, branchAddressLowBitsMaximum));
            Properties.Add(new PredictorInt32Property("counterBits", "Counter bits", counterBitsDefault, counterBitsMinimum, counterBitsMaximum));
        }

        sbyte[] globalPatternHistoryTable;
        int[] localPatternHistoryTable;
        int globalPatternHistoryTableSize;
        int localPatternHistoryTableSize;        
        int globalPatternHistoryTableIndex;
        int localPatternHistoryTableIndex;

        int historyLength;
        int branchAddressLowBits;
        int counterBits;
        byte counterMax;

        int historyMask;
        int lowBranchAddressMask;

        bool prediction;

        public PAg(int historyLength, int branchAddressLowBits, int counterBits)
        {
            this.historyLength = historyLength;
            this.branchAddressLowBits = branchAddressLowBits;
            this.counterBits = counterBits;

            historyMask = (1 << historyLength) - 1;

            localPatternHistoryTableSize = 1 << branchAddressLowBits;
            globalPatternHistoryTableSize = 1 << historyLength;

            localPatternHistoryTable = new int[localPatternHistoryTableSize];
            globalPatternHistoryTable = new sbyte[globalPatternHistoryTableSize];

            counterMax = (byte)((1 << counterBits - 1) - 1);

            lowBranchAddressMask = (1 << branchAddressLowBits) - 1;
        }

        #region IPredictor Members

        public bool predictBranch(BranchInfo branch)
        {
            localPatternHistoryTableIndex = (int)(branch.address & lowBranchAddressMask);
            globalPatternHistoryTableIndex = (int)(localPatternHistoryTable[localPatternHistoryTableIndex] & historyMask);
            prediction = (globalPatternHistoryTable[globalPatternHistoryTableIndex] >= 0);
            return prediction;
        }

        public void update(IBranch branch)
        {
            if (branch.taken())
            {
                if (globalPatternHistoryTable[globalPatternHistoryTableIndex] < counterMax)
                {
                    globalPatternHistoryTable[globalPatternHistoryTableIndex]++;
                }
            }
            else
            {
                if (globalPatternHistoryTable[globalPatternHistoryTableIndex] >= -counterMax)
                {
                    globalPatternHistoryTable[globalPatternHistoryTableIndex]--;
                }
            }
            localPatternHistoryTable[localPatternHistoryTableIndex] <<= 1;
            if (branch.taken())
            {
                localPatternHistoryTable[localPatternHistoryTableIndex]++;
            }
        }

        public void reset()
        {
            for (int i = 0; i < localPatternHistoryTableSize; i++)
            {
                localPatternHistoryTable[i] = 0;
            }
            for (int i = 0; i < globalPatternHistoryTableSize; i++)
            {
                globalPatternHistoryTable[i] = 0;
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using PredictionLogic.Prediction.PredictorPropertyTypes;

namespace PredictionLogic.Prediction.Predictors
{
    public class LGShare : IPredictor
    {
        private const int localHistoryLengthMinimum = 1;
        private const int localHistoryLengthMaximum = 12;
        private const int localHistoryLengthDefault = 8;

        private const int globalHistoryLengthMinimum = 1;
        private const int globalHistoryLengthMaximum = 16;
        private const int globalHistoryLengthDefault = 12;

        private const int counterBitsMinimum = 1;
        private const int counterBitsMaximum = 5;
        private const int counterBitsDefault = 3;

        public static readonly List<PredictorPropertyBase> Properties = new List<PredictorPropertyBase>();

        static LGShare()
        {
            Properties.Add(new PredictorInt32Property("localHistoryLength", "Local history length", localHistoryLengthDefault, localHistoryLengthMinimum, localHistoryLengthMaximum));
            Properties.Add(new PredictorInt32Property("globalHistoryLength", "Global history length", globalHistoryLengthDefault, globalHistoryLengthMinimum, globalHistoryLengthMaximum));
            Properties.Add(new PredictorInt32Property("counterBits", "Counter bits", counterBitsDefault, counterBitsMinimum, counterBitsMaximum));
        }

        struct PatternHistoryTableEntry
        {
            public byte tag;
            public sbyte counter;
        };

        int patternHistoryTableSize;
        PatternHistoryTableEntry[] patternHistoryTable;

        int counterBits;
        byte counterDefault;
        byte counterMax;

        int history;
        int globalHistoryLength;
        int globalHistoryMask;
        int highBranchAddressMask;
        int lowBranchAddressMask;

        int localHistoryLength;
        int localHistoryMask;
        short[] localHistory;

        int localHistoryIndex;
        int patternHistoryTableIndex;

        byte tag;
        bool prediction;

        public LGShare(int localHistoryLength, int globalHistoryLength, int counterBits)
        {
            this.localHistoryLength = localHistoryLength;
            this.globalHistoryLength = globalHistoryLength;

            localHistoryMask = (1 << localHistoryLength) - 1;
            globalHistoryMask = (1 << globalHistoryLength) - 1;

            this.counterBits = counterBits;

            patternHistoryTableSize = (1 << (localHistoryLength + globalHistoryLength));
            patternHistoryTable = new PatternHistoryTableEntry[patternHistoryTableSize];
            localHistory = new short[patternHistoryTableSize];

            counterDefault = (byte)(1 << (counterBits - 1) - 1);
            counterMax = (byte)((1 << counterBits) - 1);

            lowBranchAddressMask = (1 << (localHistoryLength + globalHistoryLength)) - 1;
            // tag 8 bits - counter bits
            highBranchAddressMask = (1 << (localHistoryLength + globalHistoryLength + 8 - counterBits)) - 1;
        }

        #region IPredictor Members

        public bool predictBranch(BranchInfo branch)
        {
            localHistoryIndex = (int)(branch.address & lowBranchAddressMask);
            patternHistoryTableIndex = (int)(((history & globalHistoryMask) << localHistoryLength) + (localHistory[localHistoryIndex] & localHistoryMask));
            patternHistoryTableIndex ^= localHistoryIndex;
            tag = (byte)((branch.address & highBranchAddressMask) >> (localHistoryLength + globalHistoryLength));

            if (patternHistoryTable[patternHistoryTableIndex].tag == tag)
            {
                prediction = (patternHistoryTable[patternHistoryTableIndex].counter >= 0);
            }
            else
            {
                prediction = true; // predict default taken
            }
            return prediction;
        }

        public void update(IBranch branch)
        {
            if (patternHistoryTable[patternHistoryTableIndex].tag == tag)
            {
                if (branch.taken())
                {
                    if (patternHistoryTable[patternHistoryTableIndex].counter < counterMax)
                    {
                        patternHistoryTable[patternHistoryTableIndex].counter++;
                    }
                }
                else
                {
                    if (patternHistoryTable[patternHistoryTableIndex].counter >= -counterMax)
                    {
                        patternHistoryTable[patternHistoryTableIndex].counter--;
                    }
                }
            }
            else
            {
                patternHistoryTable[patternHistoryTableIndex].tag = tag;
                if (branch.taken())
                {
                    patternHistoryTable[patternHistoryTableIndex].counter = 1;
                }
                else
                {
                    patternHistoryTable[patternHistoryTableIndex].counter = -1;
                }
            }

            history <<= 1;
            localHistory[localHistoryIndex] <<= 1;
            if (branch.taken())
            {
                history++;
                localHistory[localHistoryIndex]++;
            }
        }

        public void reset()
        {
            history = 0;
            for (int i = 0; i < patternHistoryTableSize; i++)
            {
                patternHistoryTable[i].tag = 0;
                patternHistoryTable[i].counter = 0;
                localHistory[i] = 0;
            }
        }

        #endregion
    }
}
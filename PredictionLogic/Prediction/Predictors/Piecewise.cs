using System;
using System.Collections.Generic;
using System.Text;
using PredictionLogic.Prediction.PredictorPropertyTypes;

namespace PredictionLogic.Prediction.Predictors
{
    public class Piecewise : IPredictor
    {
        private const int numberOfPerceptronsMinimum = 1;
        private const int numberOfPerceptronsMaximum = 10000;
        private const int numberOfPerceptronsDefault = 100;

        private const int counterBitsMinimum = 2;
        private const int counterBitsMaximum = 16;
        private const int counterBitsDefault = 8;

        private const int historyLengthMinimum = 1;
        private const int historyLengthMaximum = 1024;
        private const int historyLengthDefault = 32;

        private const int branchAddressBitsMinimum = 1;
        private const int branchAddressBitsMaximum = 30;
        private const int branchAddressBitsDefault = 5;

        private const int updateThresholdMinimum = 0;
        private const int updateThresholdMaximum = 1024;
        private const int updateThresholdDefault = 14;

        public static readonly List<PredictorPropertyBase> Properties = new List<PredictorPropertyBase>();

        static Piecewise()
        {
            Properties.Add(new PredictorInt32Property("numberOfPerceptrons", "Number of perceptrons", numberOfPerceptronsDefault, numberOfPerceptronsMinimum, numberOfPerceptronsMaximum));
            Properties.Add(new PredictorInt32Property("counterBits", "Counter bits", counterBitsDefault, counterBitsMinimum, counterBitsMaximum));
            Properties.Add(new PredictorInt32Property("historyLength", "History length", historyLengthDefault, historyLengthMinimum, historyLengthMaximum));
            Properties.Add(new PredictorInt32Property("branchAddressBits", "Branch address bits", branchAddressBitsDefault, branchAddressBitsMinimum, branchAddressBitsMaximum));
            Properties.Add(new PredictorInt32Property("updateThreshold", "Update threshold", updateThresholdDefault, updateThresholdMinimum, updateThresholdMaximum));
        }

        int numberOfPerceptrons;
        int counterBits;
        int updateThreshold;
        int branchAddressBits;
        
        int historyLength;
        bool[] globalHistory;

        int counterMax;
        int maximumBranchAddress;
        int branchAddressMask;

        int weightsIndex;
        short[, ,] weights;
        int[] branchPaths;

        int sum;
        bool prediction;

        public Piecewise(int numberOfPerceptrons, int counterBits, int historyLength, int branchAddressBits, int updateThreshold)
        {
            this.numberOfPerceptrons = numberOfPerceptrons;
            this.counterBits = counterBits;
            this.historyLength = historyLength;
            this.branchAddressBits = branchAddressBits;
            this.updateThreshold = updateThreshold;

            globalHistory = new bool[historyLength];
            branchPaths = new int[historyLength];
            maximumBranchAddress = (1 << branchAddressBits);
            weights = new short[numberOfPerceptrons, maximumBranchAddress + 1, historyLength + 1];

            counterMax = (1 << (counterBits - 1)) - 1;
            branchAddressMask = (1 << branchAddressBits) - 1;
            
        }

        #region IPredictor Members

        public bool predictBranch(BranchInfo branch)
        {
            weightsIndex = (int)(branch.address % numberOfPerceptrons);
            sum = weights[weightsIndex, maximumBranchAddress, historyLength]; // initialize with bias
            for (int i = 0; i < historyLength; i++)
            {
                if (globalHistory[i])
                {
                    sum += weights[weightsIndex, branchPaths[i], i];
                }
                else
                {
                    sum -= weights[weightsIndex, branchPaths[i], i];
                }
            }

            prediction = (sum >= 0);
            return prediction;
        }

        public void update(IBranch branch)
        {
            if ((branch.taken() != prediction) || (Math.Abs(sum) < updateThreshold))
            {
                // bias update
                if (branch.taken())
                {
                    if (weights[weightsIndex, maximumBranchAddress, historyLength] < counterMax)
                    {
                        weights[weightsIndex, maximumBranchAddress, historyLength]++;
                    }
                }
                else
                {
                    if (weights[weightsIndex, maximumBranchAddress, historyLength] > -counterMax - 1)
                    {
                        weights[weightsIndex, maximumBranchAddress, historyLength]--;
                    }
                }

                // weights update
                for (int i = 0; i < historyLength; i++)
                {
                    if (globalHistory[i] == branch.taken())
                    {
                        if (weights[weightsIndex, branchPaths[i], i] < counterMax)
                        {
                            weights[weightsIndex, branchPaths[i], i]++;
                        }
                    }
                    else
                    {
                        if (weights[weightsIndex, branchPaths[i], i] > -counterMax - 1)
                        {
                            weights[weightsIndex, branchPaths[i], i]--;
                        }
                    }
                }
            }

            for (int i = 0; i < historyLength - 1; i++)
            {
                globalHistory[i] = globalHistory[i + 1];
                branchPaths[i] = branchPaths[i + 1];
            }
            globalHistory[historyLength - 1] = branch.taken();
            branchPaths[historyLength - 1] = (int)((branch.getBranchInfo().address) & branchAddressMask);
        }

        public void reset()
        {
            for (int i = 0; i < numberOfPerceptrons; i++)
            {
                for (int j = 0; j < maximumBranchAddress + 1; j++)
                {
                    for (int k = 0; k < historyLength + 1; k++)
                    {
                        weights[i, j, k] = 0;
                    }
                }
            }

            for (int i = 0; i < historyLength; i++)
            {
                globalHistory[i] = false;
                branchPaths[i] = 0;
            }
        }

        #endregion
    }
}

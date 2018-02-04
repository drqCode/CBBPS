using System;
using System.Collections.Generic;
using System.Text;
using PredictionLogic.Prediction.PredictorPropertyTypes;

namespace PredictionLogic.Prediction.Predictors
{
    public class Perceptron : IPredictor
    {
        private const int numberOfPerceptronsMinimum = 1;
        private const int numberOfPerceptronsMaximum = 1048576;
        private const int numberOfPerceptronsDefault = 16536;

        private const int counterBitsMinimum = 2;
        private const int counterBitsMaximum = 16;
        private const int counterBitsDefault = 8;

        private const int historyLengthMinimum = 1;
        private const int historyLengthMaximum = 1024;
        private const int historyLengthDefault = 32;

        private const int updateThresholdMinimum = 0;
        private const int updateThresholdMaximum = 1024;
        private const int updateThresholdDefault = 14;

        public static readonly List<PredictorPropertyBase> Properties = new List<PredictorPropertyBase>();

        static Perceptron()
        {
            Properties.Add(new PredictorInt32Property("numberOfPerceptrons", "Number of perceptrons", numberOfPerceptronsDefault, numberOfPerceptronsMinimum, numberOfPerceptronsMaximum));
            Properties.Add(new PredictorInt32Property("counterBits", "Counter bits", counterBitsDefault, counterBitsMinimum, counterBitsMaximum));
            Properties.Add(new PredictorInt32Property("historyLength", "History length", historyLengthDefault, historyLengthMinimum, historyLengthMaximum));
            Properties.Add(new PredictorInt32Property("updateThreshold", "Update threshold", updateThresholdDefault, updateThresholdMinimum, updateThresholdMaximum));
        }

        int numberOfPerceptrons;
        int counterBits;
        int counterMax;

        int updateThreshold;

        int historyLength;
        bool[] globalHistory;

        int weightsIndex;
        short[,] weights;
        int sum;

        bool prediction;

        public Perceptron(int numberOfPerceptrons, int counterBits, int historyLength, int updateThreshold)
        {
            this.numberOfPerceptrons = numberOfPerceptrons;
            this.counterBits = counterBits;
            this.historyLength = historyLength;
            this.updateThreshold = updateThreshold;

            globalHistory = new bool[historyLength];
            weights = new short[numberOfPerceptrons, historyLength + 1];

            counterMax = (1 << (counterBits - 1)) - 1;
        }

        #region IPredictor Members

        public bool predictBranch(BranchInfo branch)
        {
            weightsIndex = (int)(branch.address % numberOfPerceptrons);

            sum = weights[weightsIndex, historyLength]; // initialize with bias
            for (int i = 0; i < historyLength; i++)
            {
                if (globalHistory[i])
                {
                    sum += weights[weightsIndex, i];
                }
                else
                {
                    sum -= weights[weightsIndex, i];
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
                    if (weights[weightsIndex, historyLength] < counterMax - 1)
                    {
                        weights[weightsIndex, historyLength]++;
                    }
                }
                else
                {
                    if (weights[weightsIndex, historyLength] > -counterMax)
                    {
                        weights[weightsIndex, historyLength]--;
                    }
                }

                // weights update
                for (int i = 0; i < historyLength; i++)
                {
                    if (globalHistory[i] == branch.taken())
                    {
                        if (weights[weightsIndex, i] < counterMax - 1)
                        {
                            weights[weightsIndex, i]++;
                        }
                    }
                    else
                    {
                        if (weights[weightsIndex, i] > -counterMax)
                        {
                            weights[weightsIndex, i]--;
                        }
                    }
                }
            }

            for (int i = 0; i < historyLength - 1; i++)
            {
                globalHistory[i] = globalHistory[i + 1];
            }
            globalHistory[historyLength - 1] = branch.taken();
        }

        public void reset()
        {
            for (int i = 0; i < numberOfPerceptrons; i++)
            {
                for (int j = 0; j < historyLength + 1; j++)
                {
                    weights[i, j] = 0;
                }
            }
            for (int i = 0; i < historyLength; i++)
            {
                globalHistory[i] = false;
            }
        }

        #endregion
    }
}

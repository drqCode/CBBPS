using System;
using System.Collections.Generic;
using System.Text;
using PredictionLogic.Prediction.PredictorPropertyTypes;

namespace PredictionLogic.Prediction.Predictors
{
    /* Inspired implementation from:
     * fpbnp.java found at: http://hpca23.cse.tamu.edu/taco/public-bp/fpbnp.java
     *
     * Copyright (c) 2004 Rutgers, The State University of New Jersey
     *
     * Daniel A. Jiménez
     *
     * Permission is hereby granted, free of charge, to any person
     * obtaining a copy of this software and associated documentation
     * files (the "Software"), to deal in the Software without
     * restriction, including without limitation the rights to use, copy,
     * modify, merge, publish, distribute, sublicense, and/or sell copies
     * of the Software, and to permit persons to whom the Software is
     * furnished to do so, subject to the following conditions:
     *
     * The above copyright notice and this permission notice shall be
     * included in all copies or substantial portions of the Software.
     *
     * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
     * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
     * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
     * NONINFRINGEMENT.  IN NO EVENT SHALL RUTGERS, THE STATE UNIVERSITY
     * OF NEW JERSEY BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
     * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
     * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
     * DEALINGS IN THE SOFTWARE.
     *
     */

    public class FPBNP : IPredictor
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

        public static readonly string ToolTipInfo = "Fast Path-Based Neural Branch Predictor";
        public static readonly List<PredictorPropertyBase> Properties = new List<PredictorPropertyBase>();

        static FPBNP()
        {
            Properties.Add(new PredictorInt32Property("numberOfPerceptrons", "Number of perceptrons", numberOfPerceptronsDefault, numberOfPerceptronsMinimum, numberOfPerceptronsMaximum));
            Properties.Add(new PredictorInt32Property("counterBits", "Counter bits", counterBitsDefault, counterBitsMinimum, counterBitsMaximum));
            Properties.Add(new PredictorInt32Property("historyLength", "History length", historyLengthDefault, historyLengthMinimum, historyLengthMaximum));
        }

        int numberOfPerceptrons;
        int historyLength;
        int counterBits; // number of bits per weight
        int threshold;
        int maximumWeightValue;
        int minimumWeightValue;
        short[,] weights;
        bool[] globalHistory;
        bool[] knownGlobalHistory;
        int[] shiftVector;
        int[] knownShiftVector;
        int[] knownBranchIndexHistory;
        int[] branchIndexHistory;

        bool prediction;
        public int outputSum;
        public int branchIndex;

        public int[] branchIndexHistoryCopy;
        public bool[] globalHistoryCopy;

        public FPBNP(int numberOfPerceptrons, int counterBits, int historyLength)
        {
            this.numberOfPerceptrons = numberOfPerceptrons;
            this.historyLength = historyLength;
            this.counterBits = counterBits;

            knownGlobalHistory = new bool[historyLength + 1];
            globalHistory = new bool[historyLength + 1];
            weights = new short[numberOfPerceptrons, historyLength + 1];
            shiftVector = new int[historyLength + 1];
            knownShiftVector = new int[historyLength + 1];

            branchIndexHistoryCopy = new int[historyLength + 1];
            globalHistoryCopy = new bool[historyLength + 1];

            knownBranchIndexHistory = new int[historyLength + 1];
            branchIndexHistory = new int[historyLength + 1];

            // set learning threshold as a function of history length
            threshold = (int)(2.14 * (historyLength + 1) + 20.58);

            maximumWeightValue = (1 << (counterBits - 1)) - 1;
            minimumWeightValue = -(maximumWeightValue + 1);
        }

        void shiftIntegerIntoFirstPosition(int[] vector, int x)
        {
            for (int i = historyLength; i >= 1; i--)
            {
                vector[i] = vector[i - 1];
            }
            vector[0] = x;
        }

        void shiftBooleanIntoSecondPosition(bool[] vector, bool x)
        {
            for (int i = historyLength; i >= 2; i--)
            {
                vector[i] = vector[i - 1];
            }
            vector[1] = x;
        }

        short saturatingIncrement(short weight)
        {
            if (weight < maximumWeightValue)
            {
                weight++;
            }
            return weight;
        }

        short saturatingDecrement(short weight)
        {
            if (weight > minimumWeightValue)
            {
                weight--;
            }
            return weight;
        }

        #region IPredictor Members

        public bool predictBranch(BranchInfo branch)
        {
            // hash the pc to produce an index
            branchIndex = (int)(branch.address % numberOfPerceptrons);

            // shift this index into the array of previous addresses so we know where to update
            shiftIntegerIntoFirstPosition(branchIndexHistory, branchIndex);

            for (int i = 0; i <= historyLength; i++)
            {
                branchIndexHistoryCopy[i] = branchIndexHistory[i];
                globalHistoryCopy[i] = globalHistory[i];
            }

            // add bias to current speculative running total
            outputSum = weights[branchIndex, 0] + shiftVector[historyLength];

            prediction = outputSum >= 0;

            // speculative computation for next prediction
            // there are no loop-carried dependences; this loop can proceed in parallel
            for (int i = 1; i <= historyLength; i++)
            {
                int shiftVectorIndex = historyLength - i;
                if (prediction)
                {
                    shiftVector[shiftVectorIndex + 1] = shiftVector[shiftVectorIndex] + weights[branchIndex, i];
                }
                else
                {
                    shiftVector[shiftVectorIndex + 1] = shiftVector[shiftVectorIndex] - weights[branchIndex, i];
                }
            }

            // start the running total for the branch h branches into the future
            shiftVector[0] = 0;

            // update speculative history
            shiftBooleanIntoSecondPosition(globalHistory, prediction);

            return prediction;
        }

        public void update(IBranch branch)
        {
            bool outcome = branch.taken();

            // these results are only useful for recovering from a misprediction; can be done in parallel
            for (int j = 1; j <= historyLength; j++)
            {
                int k = historyLength - j;

                if (outcome)
                {
                    knownShiftVector[k + 1] = knownShiftVector[k] + weights[branchIndex, j];
                }
                else
                {
                    knownShiftVector[k + 1] = knownShiftVector[k] - weights[branchIndex, j];
                }
            }
            knownShiftVector[0] = 0;

            shiftBooleanIntoSecondPosition(knownGlobalHistory, outcome);
            shiftIntegerIntoFirstPosition(knownBranchIndexHistory, branchIndex);

            // recover from misprediction, if any
            if (outcome != prediction)
            {
                for (int j = 0; j <= historyLength; j++)
                {
                    shiftVector[j] = knownShiftVector[j];
                    globalHistory[j] = knownGlobalHistory[j];
                    branchIndexHistory[j] = knownBranchIndexHistory[j];
                }
            }

            // perceptron learning rule
            if ((outcome != prediction) || (Math.Abs(outputSum) < threshold))
            {
                if (outcome)
                {
                    weights[branchIndex, 0] = saturatingIncrement(weights[branchIndex, 0]);
                }
                else
                {
                    weights[branchIndex, 0] = saturatingDecrement(weights[branchIndex, 0]);
                }

                for (int j = 1; j <= historyLength; j++)
                {
                    int k = branchIndexHistoryCopy[j];
                    if (outcome == globalHistoryCopy[j])
                    {
                        weights[k, j] = saturatingIncrement(weights[k, j]);
                    }
                    else
                    {
                        weights[k, j] = saturatingDecrement(weights[k, j]);
                    }
                }
            }
        }

        public void reset()
        {
            for (int i = 0; i < numberOfPerceptrons; i++)
            {
                for (int j = 0; j <= historyLength; j++)
                {
                    weights[i, j] = 0;
                }
            }

            for (int j = 0; j <= historyLength; j++)
            {
                shiftVector[j] = 0;
                knownShiftVector[j] = 0;
                knownGlobalHistory[j] = false;
                globalHistory[j] = false;

                knownBranchIndexHistory[j] = 0;
                branchIndexHistory[j] = 0;
            }
        }

        #endregion
    }
}
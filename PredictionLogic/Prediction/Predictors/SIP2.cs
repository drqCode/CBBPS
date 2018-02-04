using System;
using System.Collections.Generic;
using System.Text;
using PredictionLogic.Prediction.PredictorPropertyTypes;

namespace PredictionLogic.Prediction.Predictors
{
    public class SIP2 : IPredictor
    {
        private const int inertiaMinimum = 1;
        private const int inertiaMaximum = 100;
        private const int inertiaDefault = 5;

        private const uint numberOfBranchLocationsMinimum = 1;
        private const uint numberOfBranchLocationsMaximum = 1048576;
        private const uint numberOfBranchLocationsDefault = 16536;

        public static readonly string ToolTipInfo = "Simple Inertial Predictor 2.0";
        public static readonly List<PredictorPropertyBase> Properties = new List<PredictorPropertyBase>();

        static SIP2()
        {
            Properties.Add(new PredictorUInt32Property("numberOfBranchLocations", "Number of branch locations", numberOfBranchLocationsDefault, numberOfBranchLocationsMinimum, numberOfBranchLocationsMaximum));
            Properties.Add(new PredictorInt32Property("inertia", "Inertia", inertiaDefault, inertiaMinimum, inertiaMaximum));
        }

        uint numberOfBranchLocations;
        uint branchIndex;
        int[] states;
        int inertia;

        public SIP2(uint branchLocations, int inertia)
        {
            this.numberOfBranchLocations = branchLocations;
            this.inertia = inertia;
            states = new int[branchLocations];

            reset();
        }

        #region IPredictor Members

        public bool predictBranch(BranchInfo branch)
        {
            branchIndex = branch.address % numberOfBranchLocations;
            if (states[branchIndex] >= 0)
            {
                return true;
            }
            return false;
        }

        public void update(IBranch branch)
        {
            if (branch.taken())
            {
                if (states[branchIndex] < inertia - 1)
                {
                    states[branchIndex]++;
                }
            }
            else
            {
                if (states[branchIndex] > -inertia)
                {
                    states[branchIndex]--;
                }
            }
        }

        public void reset()
        {
            for (int i = 0; i < numberOfBranchLocations; i++)
            {
                states[i] = 0;
            }
        }

        #endregion
    }
}

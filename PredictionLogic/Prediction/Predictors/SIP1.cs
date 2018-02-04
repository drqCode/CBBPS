using System;
using System.Collections.Generic;
using System.Text;
using PredictionLogic.Prediction.PredictorPropertyTypes;

namespace PredictionLogic.Prediction.Predictors
{
    public class SIP1 : IPredictor
    {
        private const int inertiaMinimum = 1;
        private const int inertiaMaximum = 100;
        private const int inertiaDefault = 5;

        public static readonly string ToolTipInfo = "Simple Inertial Predictor 1.0";
        public static readonly List<PredictorPropertyBase> Properties = new List<PredictorPropertyBase>();

        static SIP1()
        {
            Properties.Add(new PredictorInt32Property("inertia", "Inertia", inertiaDefault, inertiaMinimum, inertiaMaximum));
        }

        int state;
        int inertia;

        public SIP1(int inertia)
        {
            this.inertia = inertia;
            reset();
        }

        #region IPredictor Members

        public bool predictBranch(BranchInfo branch)
        {
            if (state >= 0)
            {
                return true;
            }
            return false;
        }

        public void update(IBranch branch)
        {
            if (branch.taken())
            {
                if (state < inertia - 1)
                {
                    state++;
                }
            }
            else
            {
                if (state > -inertia)
                {
                    state--;
                }
            }
        }

        public void reset()
        {
            state = 0;
        }

        #endregion
    }
}

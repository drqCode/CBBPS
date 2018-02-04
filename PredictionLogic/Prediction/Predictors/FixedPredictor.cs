using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PredictionLogic.Prediction.PredictorPropertyTypes;

namespace PredictionLogic.Prediction.Predictors
{
    public class FixedPredictor : IPredictor
    {
        public static readonly string ToolTipInfo = "Fixed Predictor - always predicts Taken or Not Taken, respectively";
        public static readonly List<PredictorPropertyBase> Properties = new List<PredictorPropertyBase>();

        static FixedPredictor()
        {
            Properties.Add(new PredictorBoolProperty("predictTaken", "Always predict:", "Taken", "Not Taken", true));
        }
        
        private bool predictTaken;

        public FixedPredictor(bool predictTaken)
        {
            this.predictTaken = predictTaken;
        }
                
        public bool predictBranch(BranchInfo branch)
        {
            return this.predictTaken;
        }

        public void update(IBranch branch)
        {            
        }

        public void reset()
        {            
        }        
    }
}

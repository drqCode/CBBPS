using System;
using System.Collections.Generic;
using System.Text;

namespace PredictionLogic.Prediction
{
    public interface IBranch
    {
        BranchInfo getBranchInfo();
        bool taken();
        uint getTargetAddress();
    }
}

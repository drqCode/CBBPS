using System;
using System.Collections.Generic;
using System.Text;

namespace PredictionLogic.Prediction.BenchmarksAndReaders.Stanford
{
    public class StanfordBranch : IBranch
    {
        public BranchInfo branchInfo;
        public bool branchTaken;
        public uint targetAddress;

        public StanfordBranch()
        {
        }

        #region IBranch Members

        public BranchInfo getBranchInfo()
        {
            return branchInfo;
        }

        public bool taken()
        {
            return branchTaken;
        }

        #endregion
    }
}

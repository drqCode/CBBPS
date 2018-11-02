using System;
using System.Collections.Generic;
using System.Text;

namespace PredictionLogic.Prediction.BenchmarksAndReaders.CBP2
{
    public class CBP2Branch : IBranch
    {
        public BranchInfo branchInfo;
        public bool branchTaken;
        public uint targetAddress;

        public CBP2Branch()
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

        public uint getTargetAddress()
        {
            return targetAddress;
        }

        #endregion
    }
}

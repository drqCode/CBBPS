using System;
using System.Collections.Generic;
using System.Text;

namespace PredictionLogic.Prediction.BenchmarksAndReaders.SPEC2000
{
    public class Spec2000Branch : IBranch
    {
        public BranchInfo branchInfo;
        public bool branchTaken;

        public Spec2000Branch()
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
            // we have no other information here
            return branchInfo.address;
        }

        #endregion
    }
}

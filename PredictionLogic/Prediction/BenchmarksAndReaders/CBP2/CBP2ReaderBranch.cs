using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PredictionLogic.Prediction.BenchmarksAndReaders.CBP2
{
    class CBP2ReaderBranch
    {
        public bool taken;
        public int code;
        public uint address;        
        public uint targetAddress;
        public long lruTime;

        public CBP2ReaderBranch()
        {
            code = 0;
            address = 0;
            targetAddress = 0;
            taken = false;
            lruTime = 0;
        }

        public void clear()
        {
            code = 0;
            address = 0;
            targetAddress = 0;
            taken = false;
            lruTime = 0;
        }

        public CBP2ReaderBranch(int code, uint address, uint targetAddress, bool taken)
        {
            this.code = code;
            this.address = address;
            this.targetAddress = targetAddress;
            this.taken = taken;
        }

        public bool equal(CBP2ReaderBranch other, bool ignoreTarget)
        {
            return (other.code == code) && (other.taken == taken) && (other.address == address) && ((ignoreTarget) || (other.targetAddress == targetAddress));
        }
    }
}

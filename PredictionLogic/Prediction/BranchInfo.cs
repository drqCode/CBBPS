using System;
using System.Collections.Generic;
using System.Text;

namespace PredictionLogic.Prediction
{
    public struct BranchInfo
    {
        public const uint OP_JO = 0;
        public const uint OP_JNO = 1;
        public const uint OP_JC = 2;
        public const uint OP_JNC = 3;
        public const uint OP_JZ = 4;
        public const uint OP_JNZ = 5;
        public const uint OP_JBE = 6;
        public const uint OP_JA = 7;
        public const uint OP_JS = 8;
        public const uint OP_JNS = 9;
        public const uint OP_JP = 10;
        public const uint OP_JNP = 11;
        public const uint OP_JL = 12;
        public const uint OP_JGE = 13;
        public const uint OP_JLE = 14;
        public const uint OP_JG = 15;

        //// Note: JECXZ/JCXZ are aliased to the opcode for JO because the traces
        //// only have 4 bits for the opcode.  However, JECXZ/JCXZ occur *very*
        //// infrequently in the traces, literally less than 0.00002% of the time
        //// in the traces.

        public const uint BR_CONDITIONAL = 1;
        public const uint BR_INDIRECT = 2;
        public const uint BR_CALL = 4;
        public const uint BR_RETURN = 8;

        public uint address; 	// branch address
        public uint opcode;	// opcode for conditional branch
        public uint branchFlags;	// OR of some BR_ flags
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.BZip2;

namespace PredictionLogic.Prediction.BenchmarksAndReaders.CBP2
{
    /// <summary>
    /// Code adapted from the official sources of Championship of Branch Prediction 2
    /// </summary>
    class CBP2Reader : ITraceReader
    {
        private FileStream inputFileStream;
        private BZip2InputStream bzip2InputStream;

        private CBP2Branch branch;
        private CBP2Branch lastBranch;
        private int numberOfBranches;

        #region File Format details

        private const int RETURN_ADDRESS_STACK_SIZE = 100;
        private const int BRANCH_MEMORY_NUMBER_OF_LINES = (1 << 16);
        private const int BRANCH_MEMORY_ASSOCIATIVITY_SIZE = 8;

        private uint[] returnAddressStack;
        private int returnAddressStackTop;
        private CBP2ReaderBranch[][] branchesMemory;

        private uint now;
        private CBP2ReaderBranch lastReaderBranch;

        private int[] classMisspredicted;

        private CBP2ReaderBranch[] predictRemember()
        {
            return branchesMemory[lastReaderBranch.targetAddress % BRANCH_MEMORY_NUMBER_OF_LINES];
        }

        private void updateRemember(CBP2ReaderBranch readerBranch, CBP2ReaderBranch[] memoryLine, bool correct, int index)
        {
            if (correct)
            {
                memoryLine[index].lruTime = now++;
            }
            else
            {
                // throw out the LRU item and replace it with me
                int lru = 0;
                for (int i = 1; i < BRANCH_MEMORY_ASSOCIATIVITY_SIZE; i++)
                {
                    if (memoryLine[i].lruTime < memoryLine[lru].lruTime)
                    {
                        lru = i;
                    }
                }
                memoryLine[lru] = readerBranch;
                memoryLine[lru].lruTime = now++;
            }
            lastReaderBranch = readerBranch;
        }

        void initReturnAddressStack()
        {
            returnAddressStackTop = RETURN_ADDRESS_STACK_SIZE;
        }

        void pushReturnAddressStack(uint a)
        {
            if (returnAddressStackTop != 0)
            {
                returnAddressStack[--returnAddressStackTop] = a;
            }
        }

        uint popReturnAddressStack()
        {
            if (returnAddressStackTop < RETURN_ADDRESS_STACK_SIZE)
            {
                return returnAddressStack[returnAddressStackTop++];
            }
            return 0;
        }

        uint readUint()
        {
            uint x0 = (uint)bzip2InputStream.ReadByte();
            uint x1 = (uint)bzip2InputStream.ReadByte();
            uint x2 = (uint)bzip2InputStream.ReadByte();
            uint x3 = (uint)bzip2InputStream.ReadByte();
            return (x0 | (x1 << 8) | (x2 << 16) | (x3 << 24));
        }

        #endregion

        public CBP2Reader()
        {
            returnAddressStack = new uint[RETURN_ADDRESS_STACK_SIZE];
            branchesMemory = new CBP2ReaderBranch[BRANCH_MEMORY_NUMBER_OF_LINES][];
            for (int i = 0; i < BRANCH_MEMORY_NUMBER_OF_LINES; i++)
            {
                branchesMemory[i] = new CBP2ReaderBranch[BRANCH_MEMORY_ASSOCIATIVITY_SIZE];
                for (int j = 0; j < BRANCH_MEMORY_ASSOCIATIVITY_SIZE; j++)
                {
                    branchesMemory[i][j] = new CBP2ReaderBranch();
                }
            }

            classMisspredicted = new int[8];

            branch = new CBP2Branch();
            lastBranch = new CBP2Branch();
            lastReaderBranch = new CBP2ReaderBranch();
        }

        #region ITraceReader Members

        public bool openTrace(string folderPath, string filename)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(folderPath + filename + ".bz2");
                inputFileStream = fileInfo.OpenRead();
                bzip2InputStream = new BZip2InputStream(inputFileStream);
            }
            catch
            {
                return false;
            }
            numberOfBranches = 0;

            for (int i = 0; i < BRANCH_MEMORY_NUMBER_OF_LINES; i++)
            {
                for (int j = 0; j < BRANCH_MEMORY_ASSOCIATIVITY_SIZE; j++)
                {
                    branchesMemory[i][j].clear();
                }
            }
            for (int i = 0; i < 8; i++)
            {
                classMisspredicted[i] = 0;
            }

            initReturnAddressStack();
            now = 0;

            return true;
        }

        public IBranch getNextBranch()
        {
            int inputByte = bzip2InputStream.ReadByte();
            if (inputByte < 0)
            {
                return null;
            }
            branch.branchInfo.branchFlags = 0;

            // pass along instruction counts unchanged (we don't care)
            if (inputByte == 0x87)
            {
                int x = 0, y = 0;
                inputByte = bzip2InputStream.ReadByte();
                if (inputByte < 0)
                {
                    return null;
                }
                x = inputByte;
                inputByte = bzip2InputStream.ReadByte();
                if (inputByte < 0)
                {
                    return null;
                }
                y = inputByte;
                y <<= 8;
                x |= y;
                inputByte = bzip2InputStream.ReadByte();
                if (inputByte < 0)
                {
                    return null;
                }
            }

            CBP2ReaderBranch readerBranch = new CBP2ReaderBranch();
            CBP2ReaderBranch[] memoryLine = predictRemember();
            bool returnAddressStackOffBy2 = false;
            bool returnAddressStackOffBy3 = false;
            if ((inputByte & 0x80) != 0)
            {
                if (inputByte == 0x82)
                {
                    returnAddressStackOffBy2 = true;
                }
                else
                {
                    if (inputByte == 0x83)
                    {
                        returnAddressStackOffBy3 = true;
                    }
                    else
                    {
                        return null;
                    }
                }

                inputByte = bzip2InputStream.ReadByte();
                if (inputByte < 0)
                {
                    return null;
                }
            }

            bool correct = inputByte < BRANCH_MEMORY_ASSOCIATIVITY_SIZE * 2;
            if (correct)
            {
                bool returnAddressStackCorrect = inputByte >= BRANCH_MEMORY_ASSOCIATIVITY_SIZE;
                if (returnAddressStackCorrect)
                {
                    inputByte -= BRANCH_MEMORY_ASSOCIATIVITY_SIZE;
                }
                readerBranch.address = memoryLine[inputByte].address;
                readerBranch.targetAddress = memoryLine[inputByte].targetAddress;
                readerBranch.taken = memoryLine[inputByte].taken;
                readerBranch.code = memoryLine[inputByte].code;
                if (readerBranch.code == 0x70)
                {
                    long popped = popReturnAddressStack();
                    if (returnAddressStackCorrect)
                    {
                        readerBranch.targetAddress = (uint)popped;
                        if (returnAddressStackOffBy2)
                        {
                            readerBranch.targetAddress += 2;
                        }
                        else
                        {
                            if (returnAddressStackOffBy3)
                            {
                                readerBranch.targetAddress -= 3;
                            }
                        }
                    }
                    else
                    {
                        initReturnAddressStack();
                    }
                }
                if (!readerBranch.equal(memoryLine[inputByte], returnAddressStackCorrect))
                {
                    return null;
                }

                branch.branchInfo.address = readerBranch.address;
                branch.targetAddress = readerBranch.targetAddress;
                branch.branchTaken = readerBranch.taken;
                updateRemember(readerBranch, memoryLine, true, (int)inputByte);
                inputByte = readerBranch.code;
            }
            else
            {
                branch.branchInfo.address = readUint();
                branch.targetAddress = readUint();
                branch.branchTaken = true;
                readerBranch.address = branch.branchInfo.address;
                readerBranch.targetAddress = branch.targetAddress;
                readerBranch.taken = branch.branchTaken;
                readerBranch.code = inputByte;
                // if this is a return, manage the return address stack
                if (readerBranch.code == 0x70)
                {
                    // could be a correct return address stack prediction but with incorrect call site???
                    long popped = popReturnAddressStack();
                    if (popped != branch.targetAddress && popped != branch.targetAddress - 2 && popped != branch.targetAddress + 3)
                    {
                        initReturnAddressStack();
                    }
                }
                updateRemember(readerBranch, memoryLine, false, -1);
            }
            branch.branchInfo.opcode = (uint)inputByte & 15;
            inputByte >>= 4;
            if (!correct)
            {
                classMisspredicted[inputByte]++;
            }
            switch (inputByte)
            {
                case 1: // taken conditional branch
                    branch.branchInfo.branchFlags |= BranchInfo.BR_CONDITIONAL;
                    break;
                case 2: // not taken conditional branch
                    branch.branchTaken = false;
                    branch.branchInfo.branchFlags |= BranchInfo.BR_CONDITIONAL;
                    break;
                case 3: // unconditional branch
                    break;
                case 4: // indirect branch
                    branch.branchInfo.branchFlags |= BranchInfo.BR_INDIRECT;
                    break;
                case 5: // call
                    branch.branchInfo.branchFlags |= BranchInfo.BR_CALL;
                    pushReturnAddressStack(branch.branchInfo.address + 5);
                    break;
                case 6: // indirect call
                    branch.branchInfo.branchFlags |= BranchInfo.BR_CALL | BranchInfo.BR_INDIRECT;
                    pushReturnAddressStack(branch.branchInfo.address + 2);
                    break;
                case 7: // return
                    branch.branchInfo.branchFlags |= BranchInfo.BR_RETURN;
                    break;
                default:
                    return null;
            }

            lastBranch = branch;
            numberOfBranches++;
            return branch;
        }

        public void closeTrace()
        {
            bzip2InputStream.Close();
            inputFileStream.Close();
        }

        public int getNumberOfBranches()
        {
            return numberOfBranches;
        }

        #endregion
    }
}

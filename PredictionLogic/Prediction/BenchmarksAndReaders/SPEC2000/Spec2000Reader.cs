using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace PredictionLogic.Prediction.BenchmarksAndReaders.SPEC2000
{
    class Spec2000Reader : ITraceReader
    {
        int numberOfBranches;

        byte[] buffer;
        int bufferIndex;

        private Spec2000Branch branch;

        #region ITraceReader Members

        public Spec2000Reader()
        {
            buffer = new byte[4000000];
        }

        public bool openTrace(string folderPath, string filename)
        {
            try
            {
                int fileIndex = -1;
                for (int i = 0; i < 17; i++)
                {
                    if (TraceFileInfo.spec2000Filenames[i].Equals(filename))
                    {
                        fileIndex = i;
                        break;
                    }
                }

                if (fileIndex == -1)
                {
                    throw new FileNotFoundException("Spec2000 " + filename + " could not be found.");
                }

                FileInfo fileInfo = new FileInfo(folderPath + "traces.gz");
                numberOfBranches = 0;

                FileStream inputFile = fileInfo.OpenRead();
                GZipStream gzipInputStream = new GZipStream(inputFile, CompressionMode.Decompress);

                // copy the decompressed information into into the buffer
                for (int k = 0; k <= fileIndex; k++)
                {
                    gzipInputStream.Read(buffer, 0, 4000000);
                }
                gzipInputStream.Close();
                inputFile.Close();

                bufferIndex = 0;

                branch = new Spec2000Branch();
                branch.branchInfo.branchFlags = BranchInfo.BR_CONDITIONAL;
            }
            catch
            {
                return false;
            }
            return true;
        }

        public IBranch getNextBranch()
        {
            if (bufferIndex == 4000000)
            {
                return null;
            }
            int a, b, c, d, address;

            a = buffer[bufferIndex++];
            b = buffer[bufferIndex++];
            c = buffer[bufferIndex++];
            d = buffer[bufferIndex++];
            
            if (d >= 128)
            {
                d -= 128;
                branch.branchTaken = true;
            }
            else
            {
                branch.branchTaken = false;
            }

            // assemble bytes into an address
            address = d;
            address = (address << 8) | c;
            address = (address << 8) | b;
            address = (address << 8) | a;
            address &= 0x7fffffff;

            branch.branchInfo.address = (uint)address;

            numberOfBranches++;
            return branch;
        }

        public void closeTrace()
        {
        }

        public int getNumberOfBranches()
        {
            return numberOfBranches;
        }

        #endregion
    }
}

using System;
using PredictionLogic.Prediction;
using PredictionLogic.Prediction.PredictorPropertyTypes;
using System.Collections.Generic;

namespace PredictionLogic.Prediction.Predictors
{
    public struct BimodalTableEntry
    {
        public sbyte hysteresis;
        public sbyte prediction;
    }

    public struct TaggedTableEntry
    {
        public sbyte counter;
        public short tag;
        public byte usefulBit; // value ranges from 0 to 3, so actually only 2 bits
    }

    public class TAGE : IPredictor
    {
        private const int numberOfTablesMinimum = 2;
        private const int numberOfTablesMaximum = 50;
        private const int numberOfTablesDefault = 10;

        private const int tableLengthBitsMinimum = 2;
        private const int tableLengthBitsMaximum = 24;
        private const int tableLengthBitsDefault = 16;

        private const int counterBitsMinimum = 2;
        private const int counterBitsMaximum = 8;
        private const int counterBitsDefault = 3;

        private const int tagBitsMinimum = 2;
        private const int tagBitsMaximum = 16;
        private const int tagBitsDefault = 16;

        private const int minimumHistoryLengthMinimum = 1;
        private const int minimumHistoryLengthMaximum = 200;
        private const int minimumHistoryLengthDefault = 5;

        private const int maximumHistoryLengthMinimum = 5;
        private const int maximumHistoryLengthMaximum = 1048576;
        private const int maximumHistoryLengthDefault = 301;

        private const int bimodalTableLengthBitsMinimum = 2;
        private const int bimodalTableLengthBitsMaximum = 24;
        private const int bimodalTableLengthBitsDefault = 16;

        private const int pathHistoryLengthMinimum = 0;
        private const int pathHistoryLengthMaximum = 32;
        private const int pathHistoryLengthDefault = 16;

        private const int usefulResetCounterBitsMinimum = 1;
        private const int usefulResetCounterBitsMaximum = 32;
        private const int usefulResetCounterBitsDefault = 18;

        private const int newlyAllocatedEntriesTrustCounterBitsMinimum = 1;
        private const int newlyAllocatedEntriesTrustCounterBitsMaximum = 8;
        private const int newlyAllocatedEntriesTrustCounterBitsDefault = 4;

        public static readonly List<PredictorPropertyBase> Properties = new List<PredictorPropertyBase>();

        static TAGE()
        {
            Properties.Add(new PredictorInt32Property("numberOfTables", "Number of tables", numberOfTablesDefault, numberOfTablesMinimum, numberOfTablesMaximum));
            Properties.Add(new PredictorInt32Property("tableLengthBits", "Table length bits", tableLengthBitsDefault, tableLengthBitsMinimum, tableLengthBitsMaximum));
            Properties.Add(new PredictorInt32Property("counterBits", "Counter bits", counterBitsDefault, counterBitsMinimum, counterBitsMaximum));
            Properties.Add(new PredictorInt32Property("tagBits", "Tag bits", tagBitsDefault, tagBitsMinimum, tagBitsMaximum));
            Properties.Add(new PredictorInt32Property("minimumHistoryLength", "Minimum history length", minimumHistoryLengthDefault, minimumHistoryLengthMinimum, minimumHistoryLengthMaximum));
            Properties.Add(new PredictorInt32Property("maximumHistoryLength", "Maximum history length", maximumHistoryLengthDefault, maximumHistoryLengthMinimum, maximumHistoryLengthMaximum));
            Properties.Add(new PredictorInt32Property("bimodalTableLengthBits", "Bimodal table length bits", bimodalTableLengthBitsDefault, bimodalTableLengthBitsMinimum, bimodalTableLengthBitsMaximum));
            Properties.Add(new PredictorInt32Property("pathHistoryLength", "Path history length", pathHistoryLengthDefault, pathHistoryLengthMinimum, pathHistoryLengthMaximum));
            Properties.Add(new PredictorInt32Property("usefulResetCounterBits", "Useful reset counter bits", usefulResetCounterBitsDefault, usefulResetCounterBitsMinimum, usefulResetCounterBitsMaximum));
            Properties.Add(new PredictorInt32Property("newlyAllocatedEntriesTrustCounterBits", "New entry trust counter bits", newlyAllocatedEntriesTrustCounterBitsDefault, newlyAllocatedEntriesTrustCounterBitsMinimum, newlyAllocatedEntriesTrustCounterBitsMaximum));
        }

        int numberOfTables;
        int tableLengthBits;
        int counterBits;
        int tagBits;

        int minimumHistoryLength;
        int maximumHistoryLength;

        int bimodalTableLengthBits;

        int pathHistoryLength;
        int usefulResetCounterBits;
        int newlyAllocatedEntriesTrustCounterBits;

        int pathHistory;
        int usefulResetCounter;
        int newlyAllocatedEntriesTrustCounter; // determine whether newly allocated entries should be trusted for delivering the prediction

        TaggedTableEntry[][] taggedTables;
        BimodalTableEntry[] bimodalTable;
        byte[] globalHistory;
        HistoryRollingHash[] taggedTableHistoryHash;
        HistoryRollingHash[,] tagHistoryHash;

        int[] tableHistoryLength;
        int[] taggedTableIndexes;
        int bimodalTableIndex;

        bool prediction;
        bool alternativePrediction;
        int tableIndex;
        int alternativeTableIndex;

        public int randomSeed;

        public TAGE(int numberOfTables, int tableLengthBits, int counterBits, int tagBits, int minimumHistoryLength, int maximumHistoryLength, int bimodalTableLengthBits,
            int pathHistoryLength, int usefulResetCounterBits, int newlyAllocatedEntriesTrustCounterBits)
        {
            this.numberOfTables = numberOfTables;
            this.tableLengthBits = tableLengthBits;
            this.counterBits = counterBits;
            this.tagBits = tagBits;
            this.minimumHistoryLength = minimumHistoryLength;
            this.maximumHistoryLength = maximumHistoryLength;
            this.bimodalTableLengthBits = bimodalTableLengthBits;
            this.pathHistoryLength = pathHistoryLength;
            this.usefulResetCounterBits = usefulResetCounterBits;
            this.newlyAllocatedEntriesTrustCounterBits = newlyAllocatedEntriesTrustCounterBits;

            // just to be sure
            if (minimumHistoryLength >= maximumHistoryLength)
            {
                minimumHistoryLength = maximumHistoryLength - 1;
            }

            globalHistory = new byte[maximumHistoryLength];

            // computes the geometric history lengths
            tableHistoryLength = new int[numberOfTables];
            tableHistoryLength[0] = maximumHistoryLength - 1;
            tableHistoryLength[numberOfTables - 1] = minimumHistoryLength;
            for (int i = 1; i < numberOfTables - 1; i++)
            {
                tableHistoryLength[numberOfTables - 1 - i] = (int)(((double)minimumHistoryLength * Math.Pow((double)(maximumHistoryLength - 1) / (double)minimumHistoryLength, (double)i / (double)(numberOfTables - 1))) + 0.5);
            }

            taggedTableHistoryHash = new HistoryRollingHash[numberOfTables];
            for (int i = numberOfTables - 1; i >= 0; i--)
            {
                taggedTableHistoryHash[i] = new HistoryRollingHash(tableHistoryLength[i], tableLengthBits);
            }

            tagHistoryHash = new HistoryRollingHash[2, numberOfTables];
            for (int i = 0; i < numberOfTables; i++)
            {
                tagHistoryHash[0, i] = new HistoryRollingHash(taggedTableHistoryHash[i].historyLength, tagBits - ((i + (numberOfTables & 1)) / 2));
                tagHistoryHash[1, i] = new HistoryRollingHash(taggedTableHistoryHash[i].historyLength, tagBits - ((i + (numberOfTables & 1)) / 2) - 1);
            }

            bimodalTable = new BimodalTableEntry[1 << bimodalTableLengthBits];
            for (int i = 0; i < bimodalTable.Length; i++)
            {
                bimodalTable[i].hysteresis = 1;
            }
            taggedTableIndexes = new int[numberOfTables];
            taggedTables = new TaggedTableEntry[numberOfTables][];
            for (int i = 0; i < numberOfTables; i++)
            {
                taggedTables[i] = new TaggedTableEntry[1 << tableLengthBits];
            }
        }

        public int computeBimodalTableIndex(uint branchAddress)
        {
            return (int)(branchAddress & ((1 << (bimodalTableLengthBits)) - 1));
        }

        public int mixPathHistory(int pathHistory, int pathHistoryBits, int tableIndex)
        {
            pathHistory = pathHistory & ((1 << pathHistoryBits) - 1);
            int A1 = (pathHistory & ((1 << tableLengthBits) - 1));
            int A2 = (pathHistory >> tableLengthBits);
            A2 = ((A2 << tableIndex) & ((1 << tableLengthBits) - 1)) + (A2 >> (tableLengthBits - tableIndex));
            pathHistory = A1 ^ A2;
            pathHistory = ((pathHistory << tableIndex) & ((1 << tableLengthBits) - 1)) + (pathHistory >> (tableLengthBits - tableIndex));
            return (pathHistory);
        }

        public int computeTaggedTableIndex(uint branchAddress, int tableIndex)
        {
            int index = (int)branchAddress ^ ((int)branchAddress >> (tableLengthBits - numberOfTables + tableIndex + 1)) ^ taggedTableHistoryHash[tableIndex].historyHash;
            if (tableHistoryLength[tableIndex] >= pathHistoryLength)
            {
                index ^= mixPathHistory(pathHistory, pathHistoryLength, tableIndex);
            }
            else
            {
                index ^= mixPathHistory(pathHistory, tableHistoryLength[tableIndex], tableIndex);
            }
            return index & ((1 << tableLengthBits) - 1);
        }

        public short computeTableTag(uint branchAddress, int tableIndex)
        {
            uint tag = (uint)(branchAddress ^ tagHistoryHash[0, tableIndex].historyHash ^ (tagHistoryHash[1, tableIndex].historyHash << 1));
            return (short)(tag & ((1 << (tagBits - ((tableIndex + (numberOfTables & 1)) >> 1))) - 1)); // tag does not have the same length for all the tables
        }

        public sbyte saturatedCounterUpdate(sbyte currentCounter, bool taken, int counterBits)
        {
            if (taken)
            {
                if (currentCounter < ((1 << (counterBits - 1)) - 1))
                {
                    currentCounter++;
                }
            }
            else
            {
                if (currentCounter > -(1 << (counterBits - 1)))
                {
                    currentCounter--;
                }
            }
            return currentCounter;
        }

        public bool predictBranch(BranchInfo branch)
        {
            if ((branch.branchFlags & BranchInfo.BR_CONDITIONAL) > 0)
            {
                uint branchAddress = branch.address;

                for (int i = 0; i < numberOfTables; i++)
                {
                    taggedTableIndexes[i] = computeTaggedTableIndex(branchAddress, i);
                }
                bimodalTableIndex = computeBimodalTableIndex(branchAddress);

                tableIndex = numberOfTables;
                alternativeTableIndex = numberOfTables;
                for (int i = 0; i < numberOfTables; i++)
                {
                    if (taggedTables[i][taggedTableIndexes[i]].tag == computeTableTag(branchAddress, i))
                    {
                        tableIndex = i;
                        break;
                    }
                }
                for (int i = tableIndex + 1; i < numberOfTables; i++)
                {
                    if (taggedTables[i][taggedTableIndexes[i]].tag == computeTableTag(branchAddress, i))
                    {
                        alternativeTableIndex = i;
                        break;
                    }
                }
                if (tableIndex < numberOfTables)
                {
                    if (alternativeTableIndex < numberOfTables)
                    {
                        alternativePrediction = (taggedTables[alternativeTableIndex][taggedTableIndexes[alternativeTableIndex]].counter >= 0);
                    }
                    else
                    {
                        alternativePrediction = predictUsingBimodal();
                    }

                    // if the entry is recognized as new and newlyAllocatedEntriesTrustCounter is negative then use the alternate prediction
                    if ((newlyAllocatedEntriesTrustCounter < 0) ||
                        (Math.Abs(2 * taggedTables[tableIndex][taggedTableIndexes[tableIndex]].counter + 1) != 1) || (taggedTables[tableIndex][taggedTableIndexes[tableIndex]].usefulBit != 0))
                    {
                        prediction = (taggedTables[tableIndex][taggedTableIndexes[tableIndex]].counter >= 0);
                    }
                    else
                    {
                        prediction = alternativePrediction;
                    }
                }
                else
                {
                    alternativePrediction = predictUsingBimodal();
                    prediction = alternativePrediction;
                }
            }
            else
            {
                prediction = true;
                alternativePrediction = true;
            }
            return prediction;
        }

        public bool predictUsingBimodal()
        {
            return (bimodalTable[bimodalTableIndex].prediction > 0);
        }

        public void updateBimodalPredictor(bool taken)
        {
            // just a normal 2-bit counter apart that hysteresis is shared
            if (taken == predictUsingBimodal())
            {
                if (taken)
                {
                    bimodalTable[bimodalTableIndex >> 2].hysteresis = 1;
                }
                else
                {
                    bimodalTable[bimodalTableIndex >> 2].hysteresis = 0;
                }
            }
            else
            {
                int inter = (bimodalTable[bimodalTableIndex].prediction << 1) + bimodalTable[bimodalTableIndex >> 2].hysteresis;
                if (taken)
                {
                    if (inter < 3)
                    {
                        inter += 1;
                    }
                }
                else
                {
                    if (inter > 0)
                    {
                        inter--;
                    }
                }
                bimodalTable[bimodalTableIndex].prediction = (sbyte)(inter >> 1);
                bimodalTable[bimodalTableIndex >> 2].hysteresis = (sbyte)(inter & 1);
            }
        }

        public int updateRandom()
        {
            randomSeed = ((1 << 2 * numberOfTables) + 1) * randomSeed + 0xf3f531;
            randomSeed = randomSeed & ((1 << (2 * numberOfTables)) - 1);
            return randomSeed;
        }

        public void update(IBranch branch)
        {
            int randomNumber = updateRandom();
            bool branchTaken = branch.taken();

            if ((branch.getBranchInfo().branchFlags & BranchInfo.BR_CONDITIONAL) > 0)
            {
                uint branchAddress = branch.getBranchInfo().address;
                bool shouldAllocate = ((prediction != branchTaken) && (tableIndex > 0));

                // there was a hit
                if (tableIndex < numberOfTables)
                {
                    bool predictionInLocation = taggedTables[tableIndex][taggedTableIndexes[tableIndex]].counter >= 0;
                    bool entryLooksFreshlyAllocated = (Math.Abs(2 * taggedTables[tableIndex][taggedTableIndexes[tableIndex]].counter + 1) == 1)
                        && (taggedTables[tableIndex][taggedTableIndexes[tableIndex]].usefulBit == 0);

                    if (entryLooksFreshlyAllocated)
                    {
                        if (predictionInLocation == branchTaken)
                        {
                            shouldAllocate = false;
                        }
                        // if the provider component was delivering the correct prediction, no need to allocate a new entry even if the overall prediction was false
                        if (predictionInLocation != alternativePrediction)
                        {
                            if (alternativePrediction == branchTaken)
                            {
                                if (newlyAllocatedEntriesTrustCounter < (1 << newlyAllocatedEntriesTrustCounterBits) - 1)
                                {
                                    newlyAllocatedEntriesTrustCounter++;
                                }
                            }
                            else
                            {
                                if (newlyAllocatedEntriesTrustCounter > -(1 << newlyAllocatedEntriesTrustCounterBits))
                                {
                                    newlyAllocatedEntriesTrustCounter--;
                                }
                            }
                        }
                    }
                }

                // try to allocate a new entries only if prediction was wrong
                if (shouldAllocate)
                {
                    // is there some "unuseful" entry to allocate
                    byte minimumUsefulness = 3;
                    for (int i = 0; i < tableIndex; i++)
                    {
                        if (taggedTables[i][taggedTableIndexes[i]].usefulBit < minimumUsefulness)
                        {
                            minimumUsefulness = taggedTables[i][taggedTableIndexes[i]].usefulBit;
                        }
                    }
                    if (minimumUsefulness > 0)
                    {
                        // if no "unuseful" entry found age all possible targets, but do not allocate
                        for (int i = tableIndex - 1; i >= 0; i--)
                        {
                            taggedTables[i][taggedTableIndexes[i]].usefulBit--;
                        }
                    }
                    else
                    {
                        // maximum (tableIndex - 1) number of 1 bits
                        int randomBits = randomNumber & ((1 << (tableIndex - 1)) - 1);
                        // decrease the table index with as many 1 bits there are
                        int startTable = tableIndex - 1;
                        while ((randomBits & 1) != 0)
                        {
                            startTable--;
                            randomBits >>= 1;
                        }
                        for (int i = startTable; i >= 0; i--)
                        {
                            if (taggedTables[i][taggedTableIndexes[i]].usefulBit == minimumUsefulness)
                            {
                                taggedTables[i][taggedTableIndexes[i]].tag = computeTableTag(branchAddress, i);
                                taggedTables[i][taggedTableIndexes[i]].counter = (sbyte)(branchTaken ? 0 : -1);
                                taggedTables[i][taggedTableIndexes[i]].usefulBit = 0;
                                break;
                            }
                        }
                    }
                }

                // periodic reset of usefulBit: reset is bit by bit
                usefulResetCounter++;
                if ((usefulResetCounter & ((1 << usefulResetCounterBits) - 1)) == 0)
                {
                    int resetBitMask = (usefulResetCounter >> usefulResetCounterBits) & 1;
                    if ((resetBitMask & 1) == 0)
                    {
                        resetBitMask = 2;
                    }
                    for (int i = 0; i < numberOfTables; i++)
                    {
                        for (int j = 0; j < (1 << tableLengthBits); j++)
                        {
                            taggedTables[i][j].usefulBit = (byte)(taggedTables[i][j].usefulBit & resetBitMask);
                        }
                    }
                }

                // update the counter that provided the prediction, and only this counter
                if (tableIndex < numberOfTables)
                {
                    taggedTables[tableIndex][taggedTableIndexes[tableIndex]].counter = saturatedCounterUpdate(taggedTables[tableIndex][taggedTableIndexes[tableIndex]].counter, branchTaken, counterBits);
                }
                else
                {
                    updateBimodalPredictor(branchTaken);
                }
                // update the ubit counter
                if ((prediction != alternativePrediction))
                {
                    if (prediction == branchTaken)
                    {
                        if (taggedTables[tableIndex][taggedTableIndexes[tableIndex]].usefulBit < 3)
                        {
                            taggedTables[tableIndex][taggedTableIndexes[tableIndex]].usefulBit++;
                        }
                    }
                    else
                    {
                        if (taggedTables[tableIndex][taggedTableIndexes[tableIndex]].usefulBit > 0)
                        {
                            taggedTables[tableIndex][taggedTableIndexes[tableIndex]].usefulBit--;
                        }
                    }
                }
            }

            for (int i = maximumHistoryLength - 1; i > 0; i--)
            {
                globalHistory[i] = globalHistory[i - 1];
            }
            if ((!((branch.getBranchInfo().branchFlags & BranchInfo.BR_CONDITIONAL) > 0)) || branchTaken)
            {
                globalHistory[0] = 1;
            }
            else
            {
                globalHistory[0] = 0;
            }

            pathHistory = (pathHistory << 1) + (int)(branch.getBranchInfo().address & 1);
            pathHistory = pathHistory & ((1 << pathHistoryLength) - 1);
            for (int i = 0; i < numberOfTables; i++)
            {
                taggedTableHistoryHash[i].update(globalHistory);
                tagHistoryHash[0, i].update(globalHistory);
                tagHistoryHash[1, i].update(globalHistory);
            }
        }

        public void reset()
        {
            randomSeed = 0;
            usefulResetCounter = 0;
            pathHistory = 0;
            newlyAllocatedEntriesTrustCounter = 0;
            for (int i = 0; i < globalHistory.Length; i++)
            {
                globalHistory[i] = 0;
            }

            for (int i = numberOfTables - 1; i >= 0; i--)
            {
                taggedTableHistoryHash[i].reset();
            }

            for (int i = 0; i < numberOfTables; i++)
            {
                tagHistoryHash[0, i].reset();
                tagHistoryHash[1, i].reset();
            }

            for (int i = 0; i < bimodalTable.Length; i++)
            {
                bimodalTable[i].prediction = 0;
                bimodalTable[i].hysteresis = 1;
            }

            for (int i = 0; i < numberOfTables; i++)
            {
                for (int j = 0; j < (1 << tableLengthBits); j++)
                {
                    taggedTables[i][j].counter = 0;
                    taggedTables[i][j].tag = 0;
                    taggedTables[i][j].usefulBit = 0;
                }
            }
        }
    }

    class HistoryRollingHash
    {
        public int historyHash;
        public int compressedLengthBits;
        public int historyLength;
        public int lastEntryHashIndex;

        public HistoryRollingHash(int historyLength, int historyHashBits)
        {
            historyHash = 0;
            this.historyLength = historyLength;
            this.compressedLengthBits = historyHashBits;
            lastEntryHashIndex = historyLength % historyHashBits;
        }

        public void update(byte[] binaryHistory)
        {
            historyHash = (historyHash << 1) | binaryHistory[0]; // add the new bit
            historyHash ^= binaryHistory[historyLength] << lastEntryHashIndex; // remove the last bit
            historyHash ^= historyHash >> compressedLengthBits; // re-add the shifted bit
            historyHash &= (1 << compressedLengthBits) - 1; // clear excess
        }

        public void reset()
        {
            historyHash = 0;
        }
    }
}
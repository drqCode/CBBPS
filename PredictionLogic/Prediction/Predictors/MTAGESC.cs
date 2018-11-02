using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PredictionLogic.Prediction.PredictorPropertyTypes;

namespace PredictionLogic.Prediction.Predictors
{
    public class MTAGESC : IPredictor
    {
        public const int numberOfTagePredictors = 6;

        // SPSIZE : spectrum size (number of subpaths) for each tage
        // P0 = global, P1 = per-address, P2 = per-set, P3 = per-set, P4 = frequency
        public const int P0_SPSIZE = 1;
        public const int P1_SPSIZE = 4096;
        public const int P2_SPSIZE = 64;
        public const int P3_SPSIZE = 16;
        public const int P4_SPSIZE = 8;
        public const int P5_SPSIZE = 1;
        // P2_PARAM and P3_PARAM are the log2 of the set sizes in the per-set tages
        public const int P2_PARAM = 7;
        public const int P3_PARAM = 4;

        // tage parameters:

        // NUMG = number of tagged tables
        // LOGB = log2 of the number of entries of the tagless (bimodal) table
        // LOGG = log2 of the number of entries of each tagged table
        // MAXHIST = maximum path length ("rightmost" tagged table), in branches
        // MINHIST = minimum path length ("leftmost" tagged table), in branches
        // HASHPARAM = parameter used in the hash functions (may need to be changed with predictor size)
        // RAMPUP = ramp-up period in mispredictions (should be kept roughly proportional to predictor size)
        // TAGBITS = tag width in bits
        // CTRBITS = width of the taken/not-taken counters in the tagless (bimodal) and tagged tables
        // PATHBITS = number of per-branch address bits injected in the path hashing
        // POSTPBITS = width of the taken/not-taken counters in the post-predictor
        // ALLOCFAILMAX : used for clearing u bits (cf. ISL_TAGE, Andre Seznec, MICRO 2011)
        // MAXALLOC = maximum number of entries stolen upon a misprediction (cf. ISL_TAGE)
        // CAPHIST = path length beyond which aggressive update (ramp-up) is made sligtly less aggressive

        // parameters specific to the global tage
        public const int P0_NUMG = 25;
        public const int P0_LOGB = 21;
        public const int P0_LOGG = 21;
        public const int P0_MAXHIST = 5000;
        public const int P0_MINHIST = 7;
        public const int P0_HASHPARAM = 3;
        public const int P0_RAMPUP = 100000;

        // parameters specific to the per-address tage
        public const int P1_NUMG = 22;
        public const int P1_LOGB = 20;
        public const int P1_LOGG = 20;
        public const int P1_MAXHIST = 2000;
        public const int P1_MINHIST = 5;
        public const int P1_HASHPARAM = 3;
        public const int P1_RAMPUP = 100000;

        // parameters specific to the first per-set tage
        public const int P2_NUMG = 21;
        public const int P2_LOGB = 20;
        public const int P2_LOGG = 20;
        public const int P2_MAXHIST = 500;
        public const int P2_MINHIST = 5;
        public const int P2_HASHPARAM = 3;
        public const int P2_RAMPUP = 100000;

        // parameters specific to second per-set tage
        public const int P3_NUMG = 20;
        public const int P3_LOGB = 20;
        public const int P3_LOGG = 20;
        public const int P3_MAXHIST = 500;
        public const int P3_MINHIST = 5;
        public const int P3_HASHPARAM = 3;
        public const int P3_RAMPUP = 100000;

        // parameters specific to the frequency-based tage
        public const int P4_NUMG = 20;
        public const int P4_LOGB = 20;
        public const int P4_LOGG = 20;
        public const int P4_MAXHIST = 500;
        public const int P4_MINHIST = 5;
        public const int P4_HASHPARAM = 3;
        public const int P4_RAMPUP = 100000;

        // parameters specific to the  tage
        public const int P5_NUMG = 20;
        public const int P5_LOGB = 20;
        public const int P5_LOGG = 20;
        public const int P5_MAXHIST = 400;
        public const int P5_MINHIST = 5;
        public const int P5_HASHPARAM = 3;
        public const int P5_RAMPUP = 100000;

        // parameters common to all tages
        public const int TAGBITS = 15;
        public const int CTRBITS = 3;
        public const int PATHBITS = 6;
        public const int POSTPBITS = 5;

        public const int ALLOCFAILMAX = 511;
        public const int MAXALLOC = 3;
        public const int CAPHIST = 200;

        public const int branchFrequencyTableSizeBits = 20;
        public const int branchFrequencyTableSize = (1 << branchFrequencyTableSizeBits);

        // FRATIOBITS = log2 of the ratio between adjacent frequency bins (predictor P3)
        public const int FRATIOBITS = 1;

        // COLT parameters (each COLT entry has 2^NPRED counters)
        // LOGCOLT = log2 of the number of COLT entries 
        // COLTBITS = width of the taken/not-taken COLT counters 
        public const int LOGCOLT = 20;
        public const int COLTBITS = 5;


        public const bool IMLI = true; //just to be able to isolate IMLI impact: marginal on CBP5 traces 

        public const int PERCWIDTH = 8;
        public const int GPSTEP = 6;
        public const int LPSTEP = 6;
        public const int BPSTEP = 6;
        public const int PPSTEP = 6;
        public const int SPSTEP = 6;
        public const int YPSTEP = 6;
        public const int TPSTEP = 6;

        public const int GWIDTH = 60;
        public const int LWIDTH = 60;
        public const int BWIDTH = 42;
        public const int PWIDTH = 60;
        public const int SWIDTH = 60;
        public const int YWIDTH = 60;
        public const int TWIDTH = 60;

        public const int LOGTAB = 19;
        public const int TABSIZE = (1 << LOGTAB);
        public const int LOGB = LOGTAB;
        public const int LOGSIZE = 10;
        public const int LOGSIZEG = LOGSIZE;
        public const int LOGSIZEL = LOGSIZE;
        public const int LOGSIZEB = LOGSIZE;
        public const int LOGSIZES = LOGSIZE;
        public const int LOGSIZEP = LOGSIZE;
        public const int LOGSIZEY = LOGSIZE;
        public const int LOGSIZET = LOGSIZE;

        public const int HISTBUFFERLENGTH = (1 << 18);

        public const int SHIFTFUTURE = 9;

        public const int OPTYPE_BRANCH_COND = 1;

        public static void saturatedCounterUpdate(ref sbyte counter, bool increment, int numberOfBits)
        {
            int counterMin = -(1 << (numberOfBits - 1));
            int counterMax = -counterMin - 1;
            if (increment)
            {
                if (counter < counterMax)
                {
                    counter++;
                }
            }
            else
            {
                if (counter > counterMin)
                {
                    counter--;
                }
            }
        }

        public static bool saturatedCounterUpdateWithReport(ref sbyte counter, bool increment, int numberOfBits)
        {
            int counterMin = -(1 << (numberOfBits - 1));
            int counterMax = -counterMin - 1;
            bool issat = (counter == counterMax) || (counter == counterMin);
            if (increment)
            {
                if (counter < counterMax)
                {
                    counter++;
                }
            }
            else
            {
                if (counter > counterMin)
                {
                    counter--;
                }
            }
            return issat && ((counter == counterMax) || (counter == counterMin));
        }

        // utility class for index computation
        // this is the cyclic shift register for folding 
        // a long global history into a smaller number of bits; see P. Michaud's PPM-like predictor at CBP-1
        class HistoryRollingHash
        {
            public int historyHash;
            public int compressedLengthBits;
            public int historyLength;
            public int lastEntryHashIndex;

            public void init(int historyLength, int compressedLengthBits, int N)
            {
                historyHash = 0;
                this.historyLength = historyLength;
                this.compressedLengthBits = compressedLengthBits;
                lastEntryHashIndex = historyLength % compressedLengthBits;
            }

            public void update(sbyte[] history, int historyPointer)
            {
                historyHash = (historyHash << 1) ^ history[historyPointer & (HISTBUFFERLENGTH - 1)];
                historyHash ^= history[(historyPointer + historyLength) & (HISTBUFFERLENGTH - 1)] << lastEntryHashIndex;
                historyHash ^= (historyHash >> compressedLengthBits);
                historyHash = (historyHash) & ((1 << compressedLengthBits) - 1);
            }

            public void reset()
            {
                historyHash = 0;
            }
        }

        class PathHistory
        {
            public int currentPointer;
            public int historyLength;
            public uint[] history;

            public PathHistory(int historyLength)
            {
                this.historyLength = historyLength;
                history = new uint[historyLength];
                for (int i = 0; i < historyLength; i++)
                {
                    history[i] = 0;
                }
                currentPointer = 0;
            }

            public void insert(uint value)
            {
                currentPointer--;
                if (currentPointer < 0)
                {
                    currentPointer = historyLength - 1;
                }
                history[currentPointer] = value;
            }

            public uint getValueAt(int position)
            {
                int index = currentPointer + position;
                if (index >= historyLength)
                {
                    index -= historyLength;
                }
                return history[index];
            }
        }

        // used in the hash functions 
        class PathHistoryRollingHash
        {
            public uint pathHistoryHash;
            public int compressedLengthBits;
            public int pathHistoryLength;
            public int lastEntryHashIndex;

            public int numberOfInjectedBits;
            public uint compressedLengthMask;
            public uint injectedBitsMask;

            public PathHistoryRollingHash(int pathHistoryLength, int compressedLengthBits, int numberOfInjectedBits)
            {
                this.pathHistoryLength = pathHistoryLength;
                this.compressedLengthBits = compressedLengthBits;
                this.numberOfInjectedBits = numberOfInjectedBits;
                lastEntryHashIndex = pathHistoryLength % compressedLengthBits;
                compressedLengthMask = (uint)((1 << compressedLengthBits) - 1);
                injectedBitsMask = (uint)((1 << numberOfInjectedBits) - 1);
                reset();
            }

            public void rotateleft(ref uint value, int numberOfBits)
            {
                uint temp = value >> (compressedLengthBits - numberOfBits);
                value = (value << numberOfBits) | temp;
                value &= compressedLengthMask;
            }

            public void update(PathHistory ph)
            {
                rotateleft(ref pathHistoryHash, 1);
                uint injectedBits = ph.getValueAt(0) & injectedBitsMask;
                uint outjectedBits = ph.getValueAt(pathHistoryLength) & injectedBitsMask;
                rotateleft(ref outjectedBits, lastEntryHashIndex);
                pathHistoryHash ^= injectedBits ^ outjectedBits;
            }

            public void reset()
            {
                pathHistoryHash = 0;
            }
        }

        class ColtEntry
        {
            public sbyte[] counters = new sbyte[1 << numberOfTagePredictors];

            public ColtEntry()
            {
                for (int i = 0; i < (1 << numberOfTagePredictors); i++)
                {
                    counters[i] = (sbyte)((((i >> (numberOfTagePredictors - 1)) & 1) == 1) ? 1 : -2);
                }
            }

            private int composeIndex(bool[] predtaken)
            {
                int index = 0;
                for (int i = 0; i < numberOfTagePredictors; i++)
                {
                    index = (index << 1) | (predtaken[i] ? 1 : 0);
                }
                return index;
            }

            public sbyte ctr(bool[] predtaken)
            {
                int index = composeIndex(predtaken);
                return counters[index];
            }

            public void update(bool[] predtaken, bool taken)
            {
                int index = composeIndex(predtaken);
                saturatedCounterUpdate(ref counters[index], taken, COLTBITS);
            }
        }

        // This is COLT, a method invented by Gabriel Loh and Dana Henry for combining several different predictors (see PACT 2002)
        class Colt
        {
            public ColtEntry[] coltEntries;

            public Colt()
            {
                coltEntries = new ColtEntry[1 << LOGCOLT];
                for (int i = 0; i < (1 << LOGCOLT); i++)
                {
                    coltEntries[i] = new ColtEntry();
                }
            }

            private int getIndex(uint branchAddress)
            {
                return (int)(branchAddress & ((1 << LOGCOLT) - 1));
            }

            public bool predict(uint branchAddress, bool[] predtaken)
            {
                int i = getIndex(branchAddress);
                return (coltEntries[i].ctr(predtaken) >= 0);
            }

            public void update(uint branchAddress, bool[] predtaken, bool taken)
            {
                int i = getIndex(branchAddress);
                coltEntries[i].update(predtaken, taken);
            }
        }

        // branch frequency table (BFT)
        class BranchFrequencyTable
        {
            public int[] frequencies = new int[branchFrequencyTableSize];
            public BranchFrequencyTable()
            {
                for (int i = 0; i < branchFrequencyTableSize; i++)
                {
                    frequencies[i] = 0;
                }
            }
            public int getFrequency(uint branchAddress)
            {
                int index = (int)(branchAddress & ((1 << branchFrequencyTableSizeBits) - 1));
                return frequencies[index];
            }

            public void incrementFrequency(uint branchAddress)
            {
                int index = (int)(branchAddress & ((1 << branchFrequencyTableSizeBits) - 1));
                frequencies[index]++;
            }
        }

        // path history register and hashing
        class Subpath
        {
            public PathHistory pathHistory;
            public int numberOfTaggedTables;
            public PathHistoryRollingHash[] chg;
            public PathHistoryRollingHash[] chgg;
            public PathHistoryRollingHash[] cht;
            public PathHistoryRollingHash[] chtt;

            public Subpath(int numberOfTaggedTables, int minhist, int maxhist, int logg, int tagbits, int pathbits, int hp)
            {
                int[] historyLengths = new int[numberOfTaggedTables];
                for (int i = 0; i < numberOfTaggedTables; i++)
                {
                    historyLengths[i] = (int)(minhist * Math.Pow((double)maxhist / minhist, (double)i / (numberOfTaggedTables - 1)));
                }
                this.numberOfTaggedTables = numberOfTaggedTables;

                pathHistory = new PathHistory(historyLengths[numberOfTaggedTables - 1] + 1);
                chg = new PathHistoryRollingHash[numberOfTaggedTables];
                chgg = new PathHistoryRollingHash[numberOfTaggedTables];
                cht = new PathHistoryRollingHash[numberOfTaggedTables];
                chtt = new PathHistoryRollingHash[numberOfTaggedTables];
                int historyLength = 0;
                for (int i = numberOfTaggedTables - 1; i >= 0; i--)
                {
                    historyLength = (historyLength < historyLengths[numberOfTaggedTables - 1 - i]) ? historyLengths[numberOfTaggedTables - 1 - i] : historyLength + 1;
                    chg[i] = new PathHistoryRollingHash(historyLength, logg, pathbits);
                    chgg[i] = new PathHistoryRollingHash(historyLength, logg - hp, pathbits);
                    cht[i] = new PathHistoryRollingHash(historyLength, tagbits, pathbits);
                    chtt[i] = new PathHistoryRollingHash(historyLength, tagbits - 1, pathbits);
                }
            }

            public void update(uint branchTarget, bool taken)
            {
                pathHistory.insert((branchTarget << 1) | (uint)(taken ? 1 : 0));
                for (int i = 0; i < numberOfTaggedTables; i++)
                {
                    chg[i].update(pathHistory);
                    chgg[i].update(pathHistory);
                    cht[i].update(pathHistory);
                    chtt[i].update(pathHistory);
                }
            }
            public uint cg(int bank)
            {
                return chg[bank].pathHistoryHash;
            }
            public uint cgg(int bank)
            {
                return chgg[bank].pathHistoryHash << (chg[bank].compressedLengthBits - chgg[bank].compressedLengthBits);
            }
            public uint ct(int bank)
            {
                return cht[bank].pathHistoryHash;
            }
            public uint ctt(int bank)
            {
                return chtt[bank].pathHistoryHash << (cht[bank].compressedLengthBits - chtt[bank].compressedLengthBits);
            }
        }

        // path spectrum (= set of subpaths, aka first-level history)
        class PathSpectrum
        {
            public int size;
            public Subpath[] subpaths;

            public PathSpectrum(int size, int ng, int minhist, int maxhist, int logg, int tagbits, int pathbits, int hp)
            {
                this.size = size;
                subpaths = new Subpath[size];
                for (int i = 0; i < size; i++)
                {
                    subpaths[i] = new Subpath(ng, minhist, maxhist, logg, tagbits, pathbits, hp);
                }
            }
        }

        class FrequencyBins
        {
            public int numberOfBins;
            public int maximumFrequency;

            public void init(int numberOfBins)
            {
                this.numberOfBins = numberOfBins;
                this.maximumFrequency = 0;
            }

            public int find(int branchFrequency)
            {
                int bin = -1;
                int frequency = maximumFrequency;
                for (int i = 0; i < numberOfBins; i++)
                {
                    frequency = frequency >> FRATIOBITS;
                    if (branchFrequency >= frequency)
                    {
                        bin = i;
                        break;
                    }
                }
                if (bin < 0)
                {
                    bin = numberOfBins - 1;
                }
                return bin;
            }

            public void update(int branchFrequency)
            {
                if (branchFrequency > maximumFrequency)
                {
                    maximumFrequency = branchFrequency;
                }
            }
        }

        // tage tagged tables entry
        struct TaggedTableEntry
        {
            public short tag;
            public sbyte counter;
            public sbyte useful;
        }

        // cf. TAGE (Seznec & Michaud JILP 2006, Seznec MICRO 2011)
        class tage
        {
            public string name;

            public sbyte[] bimodalTable; // tagless (bimodal) table
            public TaggedTableEntry[][] taggedTables; // tagged tables
            public int bimodalIndex;
            public int[] taggedTableIndexes;
            public List<int> hit = new List<int>();
            public bool prediction;
            public bool alternativePrediction;
            public int postPredictionIndex;
            public sbyte[] postPredictionTable; // post-predictor 
            public bool postpredtaken;
            public bool mispredicted;
            public int allocationFail;
            public int numberOfMispredictions;

            public int numberOfTables;
            public int bimodalTableLength;
            public int taggedTableLength;
            public int tagBits;
            public int counterBits;
            public int postPredictionCounterBits;
            public int postPredictionTableLength;
            public int rampUp;
            public int caphist;

            public int[] postPredictionCounters = new int[2];

            public tage(string name, int numberOfTables, int bimodalTableLengthBits, int taggedTableLengthBits, int tagBits, int counterBits, int postPredictionCounterBits, int rampUp, int caph)
            {
                this.name = name;
                this.numberOfTables = numberOfTables;
                this.bimodalTableLength = 1 << bimodalTableLengthBits;
                this.taggedTableLength = 1 << taggedTableLengthBits;
                this.tagBits = tagBits;
                this.counterBits = counterBits;
                this.postPredictionCounterBits = postPredictionCounterBits;
                this.postPredictionTableLength = 1 << (2 * counterBits + 1);
                bimodalTable = new sbyte[bimodalTableLength];
                for (int i = 0; i < bimodalTableLength; i++)
                {
                    bimodalTable[i] = 0;
                }
                taggedTables = new TaggedTableEntry[numberOfTables][];
                for (int i = 0; i < numberOfTables; i++)
                {
                    taggedTables[i] = new TaggedTableEntry[taggedTableLength];
                }
                taggedTableIndexes = new int[numberOfTables];
                postPredictionTable = new sbyte[postPredictionTableLength];
                for (int i = 0; i < postPredictionTableLength; i++)
                {
                    postPredictionTable[i] = (sbyte)(-(((i >> 1) >> (counterBits - 1)) & 1));
                }
                numberOfMispredictions = 0;
                allocationFail = 0;
                this.rampUp = rampUp;
                this.caphist = caph;
            }

            public int computeBimodalIndex(uint branchAddress)
            {
                return (int)branchAddress & (bimodalTableLength - 1);
            }

            public int computeTaggedTableIndex(uint branchAddress, Subpath subpath, int tableIndex)
            {
                return (int)(branchAddress ^ subpath.cg(tableIndex) ^ subpath.cgg(tableIndex)) & (taggedTableLength - 1);
            }

            public short computeTag(uint branchAddress, Subpath subpath, int tableIndex)
            {
                return (short)((branchAddress ^ subpath.ct(tableIndex) ^ subpath.ctt(tableIndex)) & ((1 << tagBits) - 1));
            }

            public int computePostPredictionIndex()
            {
                for (int i = 0; i < 2; i++)
                {
                    postPredictionCounters[i] = (i < hit.Count()) ? taggedTables[hit[i]][taggedTableIndexes[hit[i]]].counter : bimodalTable[bimodalIndex];
                }
                int index = 0;
                //for (int i = 2; i >= 0; i--) ??????????????????????????????????????????????????????????????
                for (int i = 1; i >= 0; i--)
                {
                    index = (index << counterBits) | (postPredictionCounters[i] & ((1 << counterBits) - 1));
                }

                int u0 = (hit.Count() > 0) ? (taggedTables[hit[0]][taggedTableIndexes[hit[0]]].useful > 0 ? 1 : 0) : 1; // ?????????????????????????????????????????????????????
                //int u0 = (hit.Count() > 0) ? (taggedTables[hit[0]][taggedTableIndexes[hit[0]]].useful > 0 ? 1 : 0) : 0; partly seems benefic
                index = (index << 1) | u0;
                index &= postPredictionTableLength - 1;
                return index;
            }

            public bool predict(uint pc, Subpath p)
            {
                hit.Clear();
                bimodalIndex = computeBimodalIndex(pc);
                for (int i = 0; i < numberOfTables; i++)
                {
                    taggedTableIndexes[i] = computeTaggedTableIndex(pc, p, i);
                    if (taggedTables[i][taggedTableIndexes[i]].tag == computeTag(pc, p, i))
                    {
                        hit.Add(i);
                    }
                }

                prediction = (hit.Count() > 0) ? (taggedTables[hit[0]][taggedTableIndexes[hit[0]]].counter >= 0) : (bimodalTable[bimodalIndex] >= 0);
                alternativePrediction = (hit.Count() > 1) ? (taggedTables[hit[1]][taggedTableIndexes[hit[1]]].counter >= 0) : (bimodalTable[bimodalIndex] >= 0);
                postPredictionIndex = computePostPredictionIndex();
                postpredtaken = (postPredictionTable[postPredictionIndex] >= 0);
                return postpredtaken;
            }

            public void decrementAllUsefulBits()
            {
                for (int i = 0; i < numberOfTables; i++)
                {
                    for (int j = 0; j < taggedTableLength; j++)
                    {
                        if (taggedTables[i][j].useful > 0)
                        {
                            taggedTables[i][j].useful--;
                        }
                    }
                }
            }

            public void allocateEntry(int tableIndex, uint branchAddress, bool taken, Subpath subpath)
            {
                taggedTables[tableIndex][taggedTableIndexes[tableIndex]].tag = computeTag(branchAddress, subpath, tableIndex);
                taggedTables[tableIndex][taggedTableIndexes[tableIndex]].counter = (sbyte)(taken ? 0 : -1);
                taggedTables[tableIndex][taggedTableIndexes[tableIndex]].useful = 0;
            }

            public void aggressiveUpdate(uint branchAddress, bool taken, Subpath subpath)
            {
                // update policy used during ramp up
                bool allTableEntriesSaturated = true;

                //AS: slightly improved from CBP4
                if (hit.Count() > 0)
                {
                    bool firstHitPrediction = (taggedTables[hit[0]][taggedTableIndexes[hit[0]]].counter >= 0);
                    allTableEntriesSaturated &= saturatedCounterUpdateWithReport(ref taggedTables[hit[0]][taggedTableIndexes[hit[0]]].counter, taken, counterBits);
                    int start = 1;
                    bool skipUpdateBimodal = false;
                    bool stop = false;


                    if (taggedTables[hit[0]][taggedTableIndexes[hit[0]]].useful == 0)
                    {
                        if (hit.Count() > 1)
                        {
                            if ((taggedTables[hit[1]][taggedTableIndexes[hit[1]]].counter >= 0) != firstHitPrediction)
                            {
                                stop = true;
                            }
                            start = 2;
                            allTableEntriesSaturated &= saturatedCounterUpdateWithReport(ref taggedTables[hit[1]][taggedTableIndexes[hit[1]]].counter, taken, counterBits);
                        }
                        else
                        {
                            skipUpdateBimodal = true;
                            allTableEntriesSaturated &= saturatedCounterUpdateWithReport(ref bimodalTable[bimodalIndex], taken, counterBits);
                        }
                    }

                    if (!stop)
                    {
                        for (int i = start; i < hit.Count(); i++)
                        {
                            if ((taggedTables[hit[i]][taggedTableIndexes[hit[i]]].counter >= 0) == firstHitPrediction)
                            {
                                allTableEntriesSaturated &= saturatedCounterUpdateWithReport(ref taggedTables[hit[i]][taggedTableIndexes[hit[i]]].counter, taken, counterBits);
                            }
                            else
                            {
                                skipUpdateBimodal = true;
                                break;
                            }
                        }
                    }
                    if (!skipUpdateBimodal)
                    {
                        if ((bimodalTable[bimodalIndex] >= 0) == firstHitPrediction)
                        {
                            allTableEntriesSaturated &= saturatedCounterUpdateWithReport(ref bimodalTable[bimodalIndex], taken, counterBits);
                        }
                    }
                }
                else
                {
                    saturatedCounterUpdate(ref bimodalTable[bimodalIndex], taken, counterBits);
                }

                int tableIndex = (hit.Count() > 0) ? hit[0] : numberOfTables;
                while (--tableIndex >= 0)
                {
                    if (taggedTables[tableIndex][taggedTableIndexes[tableIndex]].useful != 0)
                    {
                        continue;
                    }
                    if (!allTableEntriesSaturated || (subpath.chg[tableIndex].pathHistoryLength <= caphist))
                    {
                        allocateEntry(tableIndex, branchAddress, taken, subpath);
                    }
                }
            }

            // update policy devised by Andre Seznec for the ISL-TAGE predictor (MICRO 2011)
            public void carefulUpdate(uint branchAddress, bool taken, Subpath subpath)
            {
                if (hit.Count() > 0)
                {
                    saturatedCounterUpdate(ref taggedTables[hit[0]][taggedTableIndexes[hit[0]]].counter, taken, counterBits);

                    if (taggedTables[hit[0]][taggedTableIndexes[hit[0]]].useful == 0)
                    {
                        if (hit.Count() > 1)
                        {
                            saturatedCounterUpdate(ref taggedTables[hit[1]][taggedTableIndexes[hit[1]]].counter, taken, counterBits);
                        }
                        else
                        {
                            saturatedCounterUpdate(ref bimodalTable[bimodalIndex], taken, counterBits);
                        }
                    }
                }
                else
                {
                    saturatedCounterUpdate(ref bimodalTable[bimodalIndex], taken, counterBits);
                }

                if (mispredicted)
                {
                    int numberOfAllocations = 0;
                    int tableIndex = (hit.Count() > 0) ? hit[0] : numberOfTables;
                    while (--tableIndex >= 0)
                    {
                        if (taggedTables[tableIndex][taggedTableIndexes[tableIndex]].useful == 0)
                        {
                            allocateEntry(tableIndex, branchAddress, taken, subpath);
                            if (allocationFail > 0)
                            {
                                allocationFail--;
                            }
                            tableIndex--;
                            numberOfAllocations++;
                            if (numberOfAllocations == MAXALLOC)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (allocationFail < ALLOCFAILMAX)
                            {
                                allocationFail++;
                            }
                            if (allocationFail == ALLOCFAILMAX)
                            {
                                decrementAllUsefulBits();
                            }
                        }
                    }
                }
            }

            public void update(uint branchAddress, bool taken, Subpath subpath)
            {
                mispredicted = (postpredtaken != taken);

                if (mispredicted)
                {
                    numberOfMispredictions++;
                }

                if (numberOfMispredictions < rampUp)
                {
                    aggressiveUpdate(branchAddress, taken, subpath);
                }
                else
                {
                    carefulUpdate(branchAddress, taken, subpath);
                }

                // update u bit (see TAGE, JILP 2006)
                if (prediction != alternativePrediction)
                {
                    if (alternativePrediction != prediction)
                    {
                        if (prediction == taken)
                        {
                            saturatedCounterUpdate(ref taggedTables[hit[0]][taggedTableIndexes[hit[0]]].useful, true, 3);
                        }
                    }
                }

                // update post pred
                saturatedCounterUpdate(ref postPredictionTable[postPredictionIndex], taken, postPredictionCounterBits);
            }
        }

        class PREDICTOR
        {
            bool coltPrediction;

            int statisticalCorrelatorSum;
            bool statisticalCorrelatorPrediction;
            bool combinedTagePrediction;

            // The perceptron-inspired components
            sbyte[][] PERC;//[(1 << LOGSIZEP)][10 * (1 << GPSTEP)];
            sbyte[][] PERCLOC;//[(1 << LOGSIZEL)][10 * (1 << LPSTEP)];
            sbyte[][] PERCBACK;//[(1 << LOGSIZEB)][10 * (1 << BPSTEP)];
            sbyte[][] PERCYHA;//[(1 << LOGSIZEY)][10 * (1 << YPSTEP)];
            sbyte[][] PERCPATH;//[(1 << LOGSIZEP)][10 * (1 << PPSTEP)];
            sbyte[][] PERCSLOC;//[(1 << LOGSIZES)][10 * (1 << SPSTEP)];
            sbyte[][] PERCTLOC;//[(1 << LOGSIZET)][10 * (1 << TPSTEP)];

            public const int LOGLOCAL = 10;
            public const int NLOCAL = (1 << LOGLOCAL);

            long[] L_shist = new long[NLOCAL];

            public const int LNB = 15;
            int[] Lm = { 2, 4, 6, 9, 12, 16, 20, 24, 29, 34, 39, 44, 50, 56, 63 };
            sbyte[][] LGEHLA;//[LNB][TABSIZE]; 
            sbyte[][] LGEHL = new sbyte[LNB][];

            //Local history  +IMLI
            public const int LINB = 10;
            int[] LIm = { 18, 20, 24, 29, 34, 39, 44, 50, 56, 63 };

            sbyte[][] LIGEHLA;//[LNB][TABSIZE]; probably wrong: LINB instead of LNB
            sbyte[][] LIGEHL = new sbyte[LNB][];

            public const int LOGSECLOCAL = 4;
            public const int NSECLOCAL = (1 << LOGSECLOCAL);
            public const int NB = 3;
            long[] S_slhist = new long[NSECLOCAL];

            public const int SNB = 15;
            int[] Sm = { 2, 4, 6, 9, 12, 16, 20, 24, 29, 34, 39, 44, 50, 56, 63 };
            sbyte[][] SGEHLA;//[SNB][TABSIZE]; 
            sbyte[][] SGEHL = new sbyte[SNB][];

            public const int LOGTLOCAL = 4;
            public const int NTLOCAL = (1 << LOGTLOCAL);
            long[] T_slhist = new long[NTLOCAL];

            public const int TNB = 15;
            int[] Tm = { 2, 4, 6, 9, 12, 16, 20, 24, 29, 34, 39, 44, 50, 56, 63 };
            sbyte[][] TGEHLA;//[TNB][TABSIZE]; 
            sbyte[][] TGEHL = new sbyte[TNB][];

            public const int QPSTEP = 6;
            public const int QWIDTH = 60;
            public const int LOGSIZEQ = (LOGSIZE);
            sbyte[][] PERCQLOC;//[(1 << LOGSIZEQ)][10 * (1 << QPSTEP)]; 

            public const int LOGQLOCAL = 15;
            public const int NQLOCAL = (1 << LOGQLOCAL);
            long[] Q_slhist = new long[NQLOCAL];

            public const int QNB = 15;
            int[] Qm = { 2, 4, 6, 9, 12, 16, 20, 24, 29, 34, 39, 44, 50, 56, 63 };
            sbyte[][] QGEHLA;//[QNB][TABSIZE]; 
            sbyte[][] QGEHL = new sbyte[QNB][];

            // correlation at constant local history ? (without PC)
            public const int QQNB = 10;
            int[] QQm = { 16, 20, 24, 29, 34, 39, 44, 50, 56, 63 };
            sbyte[][] QQGEHLA;//[QQNB][TABSIZE];
            sbyte[][] QQGEHL = new sbyte[QQNB][];


            //history at IMLI constant

            public const int LOGTIMLI = 12;
            public const int NTIMLI = (1 << LOGTIMLI);
            long[] IMLIhist = new long[NTIMLI];

            public const int IMLINB = 15;
            int[] IMLIm = { 2, 4, 6, 9, 12, 16, 20, 24, 29, 34, 39, 44, 50, 56, 63 };
            sbyte[][] IMLIGEHLA;//[IMLINB][TABSIZE]; 
            sbyte[][] IMLIGEHL = new sbyte[IMLINB][];

            //about the skeleton histories: see CBP4 
            public const int YNB = 15;
            int[] Ym = { 2, 4, 6, 9, 12, 16, 20, 24, 29, 34, 39, 44, 50, 56, 63 };

            sbyte[][] YGEHLA;//[YNB][TABSIZE]; 
            sbyte[][] YGEHL = new sbyte[YNB][];

            long YHA = 0;
            uint[] lastBranchAddresses = new uint[8];

            //about the IMLI in Micro 2015
            public const int INB = 5;
            int[] Im = { 16, 19, 23, 29, 35 };

            sbyte[][] IGEHLA;//[INB][TABSIZE];
            sbyte[][] IGEHL = new sbyte[INB][];

            long IMLIcount = 0;

            //corresponds to IMLI-OH in Micro 2015
            long futurelocal;

            sbyte[] PAST = new sbyte[64];
            public const int HISTTABLESIZE = 65536;
            sbyte[] histtable = new sbyte[HISTTABLESIZE];


            public const int FNB = 5;
            int[] Fm = { 2, 4, 7, 11, 17 };
            sbyte[][] fGEHLA;//[FNB][TABSIZE];
            sbyte[][] fGEHL = new sbyte[FNB][];

            //inherited from CBP4: usefulness ?
            public const int BNB = 10;
            int[] Bm = { 2, 4, 6, 9, 12, 16, 20, 24, 29, 34 };
            sbyte[][] BGEHLA;//[BNB][TABSIZE];
            sbyte[][] BGEHL = new sbyte[BNB][];


            //close targets
            public const int CNB = 5;
            int[] Cm = { 4, 8, 12, 20, 32 };
            sbyte[][] CGEHLA;//[CNB][TABSIZE];
            sbyte[][] CGEHL = new sbyte[CNB][];

            long CHIST = 0;
            //more distant targets
            public const int RNB = 5;
            int[] Rm = { 4, 8, 12, 20, 32 };
            sbyte[][] RGEHLA;//[RNB][TABSIZE];
            sbyte[][] RGEHL = new sbyte[RNB][];

            long RHIST = 0;

            int[] statisticalCorrelatorPerBranchThreshold = new int[(1 << LOGSIZE)];

            int statisticalCorrelatorThreshold;


            //the GEHL predictor 
            public const int MAXNHISTGEHL = 209;	//inherited from CBP4
            // base 2 logarithm of number of entries  on each GEHL  component
            public const int LOGGEHL = (LOGTAB + 1);
            public const int MINSTEP = 2;
            public const int MINHISTGEHL = 1;
            sbyte[][] gehlTables;//[1 << LOGGEHL][MAXNHISTGEHL + 1];
            int[] mgehl = new int[MAXNHISTGEHL + 1];	//GEHL history lengths
            int[] gehlIndexes = new int[MAXNHISTGEHL + 1];
            int gehlSum;

            //The MACRHSP inspired predictor
            public const int MAXNRHSP = 80;	// inherited from CBP4
            public const int LOGRHSP = LOGGEHL;
            sbyte[][] rhspTables;//[1 << LOGRHSP][MAXNRHSP + 1];
            int[] mrhsp = new int[MAXNRHSP + 1];	//RHSP history lengths
            int[] rhspIndexes = new int[MAXNRHSP + 1];
            int rhspSum;

            public const int perceptronWeightBits = 8;

            // local history management
            long BHIST = 0;
            uint lastBranchAddress = 0;
            long P_phist = 0;
            long globalBranchHistory = 0;


            public const int LOGBIASFULL = LOGTAB;
            sbyte[] BiasFull = new sbyte[(1 << LOGBIASFULL)];

            public const int LOGBIAS = LOGTAB;
            sbyte[] Bias = new sbyte[(1 << LOGBIAS)];

            public const int LOGBIASCOLT = (LOGTAB);
            sbyte[] BiasColt = new sbyte[TABSIZE];

            // variables and tables for computing intermediate predictions
            int[] tagePostPredictionIndexes = new int[numberOfTagePredictors];

            //variables for the finalr
            public const int indexFinalCombinerLengthBits = 7;
            int indexFinalCombiner;
            sbyte[] GFINAL = new sbyte[1 << indexFinalCombinerLengthBits];
            sbyte[] GFINALCOLT = new sbyte[TABSIZE];

            int indexFinalCombinerColt;
            // end finalr

            int tageCombinerSum;
            int tageCombinerThreshold;
            int typeFirstSum;
            sbyte[] FirstBIAS = new sbyte[(1 << LOGTAB)];
            sbyte[] TBias0 = new sbyte[((1 << LOGTAB))];
            sbyte[] TBias1 = new sbyte[((1 << LOGTAB))];
            sbyte[] TBias2 = new sbyte[((1 << LOGTAB))];
            sbyte[] TBias3 = new sbyte[((1 << LOGTAB))];
            sbyte[] TBias4 = new sbyte[((1 << LOGTAB))];
            sbyte[] TBias5 = new sbyte[((1 << LOGTAB))];
            sbyte[] SB0 = new sbyte[((1 << LOGTAB))];
            sbyte[] SB1 = new sbyte[((1 << LOGTAB))];
            sbyte[] SB2 = new sbyte[((1 << LOGTAB))];
            sbyte[] SB3 = new sbyte[((1 << LOGTAB))];
            sbyte[] SB4 = new sbyte[((1 << LOGTAB))];
            sbyte[] SB5 = new sbyte[((1 << LOGTAB))];

            // global history
            sbyte[] globalHistory = new sbyte[HISTBUFFERLENGTH];
            int globalHistoryPointer;

            HistoryRollingHash[] gehlHistory = new HistoryRollingHash[MAXNHISTGEHL + 1];
            HistoryRollingHash[] rhspHistory = new HistoryRollingHash[MAXNRHSP + 1];

            public const int NRHSP = 80;
            public const int NGEHL = 209;
            public const int MAXHISTGEHL = 1393;

            private BranchFrequencyTable branchFrequencyTable = new BranchFrequencyTable();
            private FrequencyBins frequencyBins = new FrequencyBins();
            private PathSpectrum[] pathSpectrums = new PathSpectrum[numberOfTagePredictors];
            private tage[] tagePredictors = new tage[numberOfTagePredictors];
            private Subpath[] subpaths = new Subpath[numberOfTagePredictors];
            private bool[] tagePredictions = new bool[numberOfTagePredictors];
            private Colt colt = new Colt();

            // cached indexes in various tables
            int INDFIRST;
            int INDBIAS0;
            int INDBIAS1;
            int INDBIAS2;
            int INDBIAS3;
            int INDBIAS4;
            int INDBIAS5;

            int INDSB0;
            int INDSB1;
            int INDSB2;
            int INDSB3;
            int INDSB4;
            int INDSB5;

            int INDLOCAL;
            int INDSLOCAL;
            int INDTLOCAL;
            int INDQLOCAL;
            int INDIMLI;
            int INDUPD;

            int INDBIAS;
            int INDBIASFULL;
            int INDBIASCOLT;

            public PREDICTOR()
            {
                PERC = new sbyte[(1 << LOGSIZEP)][];
                for (int i = 0; i < (1 << LOGSIZEP); i++)
                {
                    PERC[i] = new sbyte[10 * (1 << GPSTEP)];
                }
                PERCLOC = new sbyte[(1 << LOGSIZEL)][];
                for (int i = 0; i < (1 << LOGSIZEL); i++)
                {
                    PERCLOC[i] = new sbyte[10 * (1 << LPSTEP)];
                }
                PERCBACK = new sbyte[(1 << LOGSIZEB)][];
                for (int i = 0; i < (1 << LOGSIZEB); i++)
                {
                    PERCBACK[i] = new sbyte[10 * (1 << BPSTEP)];
                }
                PERCYHA = new sbyte[(1 << LOGSIZEY)][];
                for (int i = 0; i < (1 << LOGSIZEY); i++)
                {
                    PERCYHA[i] = new sbyte[10 * (1 << YPSTEP)];
                }
                PERCPATH = new sbyte[(1 << LOGSIZEP)][];
                for (int i = 0; i < (1 << LOGSIZEP); i++)
                {
                    PERCPATH[i] = new sbyte[10 * (1 << PPSTEP)];
                }
                PERCSLOC = new sbyte[(1 << LOGSIZES)][];
                for (int i = 0; i < (1 << LOGSIZES); i++)
                {
                    PERCSLOC[i] = new sbyte[10 * (1 << SPSTEP)];
                }
                PERCTLOC = new sbyte[(1 << LOGSIZET)][];
                for (int i = 0; i < (1 << LOGSIZET); i++)
                {
                    PERCTLOC[i] = new sbyte[10 * (1 << TPSTEP)];
                }

                PERCQLOC = new sbyte[(1 << LOGSIZEQ)][];
                for (int i = 0; i < (1 << LOGSIZEQ); i++)
                {
                    PERCQLOC[i] = new sbyte[10 * (1 << QPSTEP)];
                }

                LGEHLA = new sbyte[LNB][];
                for (int i = 0; i < LNB; i++)
                {
                    LGEHLA[i] = new sbyte[TABSIZE];
                }

                LIGEHLA = new sbyte[LINB][];
                for (int i = 0; i < LINB; i++)
                {
                    LIGEHLA[i] = new sbyte[TABSIZE];
                }

                SGEHLA = new sbyte[SNB][];
                for (int i = 0; i < SNB; i++)
                {
                    SGEHLA[i] = new sbyte[TABSIZE];
                }

                TGEHLA = new sbyte[TNB][];
                for (int i = 0; i < TNB; i++)
                {
                    TGEHLA[i] = new sbyte[TABSIZE];
                }

                QGEHLA = new sbyte[QNB][];
                for (int i = 0; i < QNB; i++)
                {
                    QGEHLA[i] = new sbyte[TABSIZE];
                }

                QQGEHLA = new sbyte[QQNB][];
                for (int i = 0; i < QQNB; i++)
                {
                    QQGEHLA[i] = new sbyte[TABSIZE];
                }
                IMLIGEHLA = new sbyte[IMLINB][];
                for (int i = 0; i < IMLINB; i++)
                {
                    IMLIGEHLA[i] = new sbyte[TABSIZE];
                }

                YGEHLA = new sbyte[YNB][];
                for (int i = 0; i < YNB; i++)
                {
                    YGEHLA[i] = new sbyte[TABSIZE];
                }
                IGEHLA = new sbyte[INB][];
                for (int i = 0; i < INB; i++)
                {
                    IGEHLA[i] = new sbyte[TABSIZE];
                }

                fGEHLA = new sbyte[FNB][];
                for (int i = 0; i < FNB; i++)
                {
                    fGEHLA[i] = new sbyte[TABSIZE];
                }
                BGEHLA = new sbyte[BNB][];
                for (int i = 0; i < BNB; i++)
                {
                    BGEHLA[i] = new sbyte[TABSIZE];
                }
                CGEHLA = new sbyte[CNB][];
                for (int i = 0; i < CNB; i++)
                {
                    CGEHLA[i] = new sbyte[TABSIZE];
                }
                RGEHLA = new sbyte[RNB][];
                for (int i = 0; i < RNB; i++)
                {
                    RGEHLA[i] = new sbyte[TABSIZE];
                }

                gehlTables = new sbyte[1 << LOGGEHL][];
                for (int i = 0; i < (1 << LOGGEHL); i++)
                {
                    gehlTables[i] = new sbyte[MAXNHISTGEHL + 1];
                }
                rhspTables = new sbyte[1 << LOGRHSP][];
                for (int i = 0; i < (1 << LOGRHSP); i++)
                {
                    rhspTables[i] = new sbyte[MAXNRHSP + 1];
                }

                pathSpectrums[0] = new PathSpectrum(P0_SPSIZE, P0_NUMG, P0_MINHIST, P0_MAXHIST, P0_LOGG, TAGBITS, PATHBITS, P0_HASHPARAM);
                pathSpectrums[1] = new PathSpectrum(P1_SPSIZE, P1_NUMG, P1_MINHIST, P1_MAXHIST, P1_LOGG, TAGBITS, PATHBITS, P1_HASHPARAM);
                pathSpectrums[2] = new PathSpectrum(P2_SPSIZE, P2_NUMG, P2_MINHIST, P2_MAXHIST, P2_LOGG, TAGBITS, PATHBITS, P2_HASHPARAM);
                pathSpectrums[3] = new PathSpectrum(P3_SPSIZE, P3_NUMG, P3_MINHIST, P3_MAXHIST, P3_LOGG, TAGBITS, PATHBITS, P3_HASHPARAM);
                pathSpectrums[4] = new PathSpectrum(P4_SPSIZE, P4_NUMG, P4_MINHIST, P4_MAXHIST, P4_LOGG, TAGBITS, PATHBITS, P4_HASHPARAM);
                pathSpectrums[5] = new PathSpectrum(P5_SPSIZE, P5_NUMG, P5_MINHIST, P5_MAXHIST, P5_LOGG, TAGBITS, PATHBITS, P5_HASHPARAM);

                tagePredictors[0] = new tage("G", P0_NUMG, P0_LOGB, P0_LOGG, TAGBITS, CTRBITS, POSTPBITS, P0_RAMPUP, CAPHIST);
                tagePredictors[1] = new tage("A", P1_NUMG, P1_LOGB, P1_LOGG, TAGBITS, CTRBITS, POSTPBITS, P1_RAMPUP, CAPHIST);
                tagePredictors[2] = new tage("S", P2_NUMG, P2_LOGB, P2_LOGG, TAGBITS, CTRBITS, POSTPBITS, P2_RAMPUP, CAPHIST);
                tagePredictors[3] = new tage("s", P3_NUMG, P3_LOGB, P3_LOGG, TAGBITS, CTRBITS, POSTPBITS, P3_RAMPUP, CAPHIST);
                tagePredictors[4] = new tage("F", P4_NUMG, P4_LOGB, P4_LOGG, TAGBITS, CTRBITS, POSTPBITS, P4_RAMPUP, CAPHIST);
                tagePredictors[5] = new tage("g", P5_NUMG, P5_LOGB, P5_LOGG, TAGBITS, CTRBITS, POSTPBITS, P5_RAMPUP, CAPHIST);

                frequencyBins.init(P4_SPSIZE);	// number of frequency bins = P4 spectrum size

                initStatisticalCorrelator();
            }

            public void initStatisticalCorrelator()
            {
                for (int i = 0; i < HISTBUFFERLENGTH; i++)
                {
                    globalHistory[0] = 0;
                }

                globalHistoryPointer = 0;

                //GEHL initialization
                mgehl[0] = 0;
                mgehl[1] = MINHISTGEHL;
                mgehl[NGEHL] = MAXHISTGEHL;

                for (int i = 2; i <= NGEHL; i++)
                {
                    mgehl[i] = (int)(((double)MINHISTGEHL * Math.Pow((double)MAXHISTGEHL / (double)MINHISTGEHL, (double)(i - 1) / (double)(NGEHL - 1))) + 0.5);
                }

                // just guarantee that all history lengths are distinct
                for (int i = 1; i <= NGEHL; i++)
                {
                    if (mgehl[i] <= mgehl[i - 1] + MINSTEP)
                    {
                        mgehl[i] = mgehl[i - 1] + MINSTEP;
                    }
                }

                for (int i = 1; i <= NGEHL; i++)
                {
                    gehlHistory[i] = new HistoryRollingHash();
                    gehlHistory[i].init(mgehl[i], LOGGEHL, ((i & 1) == 1) ? i : 1);
                }

                // initialization of GEHL tables
                for (int j = 0; j < (1 << LOGGEHL); j++)
                {
                    for (int i = 0; i <= NGEHL; i++)
                    {
                        gehlTables[j][i] = (sbyte)((i & 1) == 1 ? -4 : 3);
                    }
                }

                // RHSP initialization
                for (int i = 1; i <= NRHSP; i++)
                {
                    mrhsp[i] = 6 * i;
                }

                for (int i = 1; i <= NRHSP; i++)
                {
                    rhspHistory[i] = new HistoryRollingHash();
                    rhspHistory[i].init(mrhsp[i], LOGRHSP, ((i & 1) == 1) ? i : 1);
                }

                // initialization of RHSP tables
                for (int j = 0; j < (1 << LOGRHSP); j++)
                {
                    for (int i = 0; i <= NRHSP; i++)
                    {
                        rhspTables[j][i] = (sbyte)((i & 1) == 1 ? -4 : 3);
                    }
                }

                statisticalCorrelatorThreshold = 100;

                for (int i = 0; i < (1 << LOGSIZE); i++)
                {
                    statisticalCorrelatorPerBranchThreshold[i] = 0;
                }

                for (int i = 0; i < LNB; i++)
                {
                    LGEHL[i] = LGEHLA[i];
                }
                for (int i = 0; i < LINB; i++)
                {
                    LIGEHL[i] = LIGEHLA[i];
                }
                for (int i = 0; i < SNB; i++)
                {
                    SGEHL[i] = SGEHLA[i];
                }

                for (int i = 0; i < QNB; i++)
                {
                    QGEHL[i] = QGEHLA[i];
                }

                for (int i = 0; i < TNB; i++)
                {
                    TGEHL[i] = TGEHLA[i];
                }
                for (int i = 0; i < IMLINB; i++)
                {
                    IMLIGEHL[i] = IMLIGEHLA[i];
                }



                for (int i = 0; i < BNB; i++)
                {
                    BGEHL[i] = BGEHLA[i];
                }

                for (int i = 0; i < YNB; i++)
                {
                    YGEHL[i] = YGEHLA[i];
                }
                for (int i = 0; i < INB; i++)
                {
                    IGEHL[i] = IGEHLA[i];
                }

                for (int i = 0; i < FNB; i++)
                {
                    fGEHL[i] = fGEHLA[i];
                }


                for (int i = 0; i < LNB; i++)
                {
                    for (int j = 0; j < TABSIZE; j++)
                    {
                        if ((j & 1) == 1)
                        {
                            LGEHL[i][j] = -1;
                        }
                    }
                }

                for (int i = 0; i < SNB; i++)
                {
                    for (int j = 0; j < TABSIZE; j++)
                    {
                        if ((j & 1) == 1)
                        {
                            SGEHL[i][j] = -1;
                        }
                    }
                }

                for (int i = 0; i < QNB; i++)
                {
                    for (int j = 0; j < TABSIZE; j++)
                    {
                        if ((j & 1) == 1)
                        {
                            QGEHL[i][j] = -1;
                        }
                    }
                }
                for (int i = 0; i < LINB; i++)
                {
                    for (int j = 0; j < TABSIZE; j++)
                    {
                        if ((j & 1) == 1)
                        {
                            LIGEHL[i][j] = -1;
                        }
                    }
                }

                for (int i = 0; i < TNB; i++)
                {
                    for (int j = 0; j < TABSIZE; j++)
                    {
                        if ((j & 1) == 1)
                        {
                            TGEHL[i][j] = -1;
                        }
                    }
                }
                for (int i = 0; i < IMLINB; i++)
                {
                    for (int j = 0; j < TABSIZE; j++)
                    {
                        if ((j & 1) == 1)
                        {
                            IMLIGEHL[i][j] = -1;
                        }
                    }
                }

                for (int i = 0; i < BNB; i++)
                {
                    for (int j = 0; j < TABSIZE; j++)
                    {
                        if ((j & 1) == 1)
                        {
                            BGEHL[i][j] = -1;
                        }
                    }
                }

                for (int i = 0; i < YNB; i++)
                {
                    for (int j = 0; j < TABSIZE; j++)
                    {
                        if ((j & 1) == 1)
                        {
                            YGEHL[i][j] = -1;
                        }
                    }
                }
                for (int i = 0; i < INB; i++)
                {
                    for (int j = 0; j < TABSIZE; j++)
                    {
                        if ((j & 1) == 1)
                        {
                            IGEHL[i][j] = -1;
                        }
                    }
                }
                for (int i = 0; i < FNB; i++)
                {
                    for (int j = 0; j < TABSIZE; j++)
                    {
                        if ((j & 1) == 1)
                        {
                            fGEHL[i][j] = -1;
                        }
                    }
                }
                for (int i = 0; i < CNB; i++)
                {
                    CGEHL[i] = CGEHLA[i];
                }
                for (int i = 0; i < CNB; i++)
                {
                    for (int j = 0; j < TABSIZE; j++)
                    {
                        if ((j & 1) == 1)
                        {
                            CGEHL[i][j] = -1;
                        }
                    }
                }
                for (int i = 0; i < RNB; i++)
                {
                    RGEHL[i] = RGEHLA[i];
                }
                for (int i = 0; i < RNB; i++)
                {
                    for (int j = 0; j < TABSIZE; j++)
                    {
                        if ((j & 1) == 1)
                        {
                            RGEHL[i][j] = -1;
                        }
                    }
                }

                for (int i = 0; i < QQNB; i++)
                {
                    QQGEHL[i] = QQGEHLA[i];
                }
                for (int i = 0; i < QQNB; i++)
                {
                    for (int j = 0; j < TABSIZE; j++)
                    {
                        if ((j & 1) == 1)
                        {
                            QQGEHL[i][j] = -1;
                        }
                    }
                }


                for (int j = 0; j < (1 << LOGBIAS); j++)
                {
                    Bias[j] = (sbyte)(((j & 1) == 1) ? 15 : -16);
                }

                for (int j = 0; j < (1 << LOGBIASCOLT); j++)
                {
                    BiasColt[j] = (sbyte)(((j & 1) == 1) ? 0 : -1);
                }

                for (int i = 0; i < (1 << LOGSIZES); i++)
                {
                    for (int j = 0; j < (SWIDTH / SPSTEP) * (1 << SPSTEP); j++)
                    {
                        if ((j & 1) == 1)
                        {
                            PERCSLOC[i][j] = -1;
                        }
                    }
                }

                for (int i = 0; i < (1 << LOGSIZEQ); i++)
                {
                    for (int j = 0; j < (QWIDTH / QPSTEP) * (1 << QPSTEP); j++)
                    {
                        if ((j & 1) == 1)
                        {
                            PERCQLOC[i][j] = -1;
                        }
                    }
                }

                for (int i = 0; i < (1 << LOGSIZEP); i++)
                {
                    for (int j = 0; j < (GWIDTH / GPSTEP) * (1 << GPSTEP); j++)
                    {
                        if ((j & 1) == 1)
                        {
                            PERC[i][j] = -1;
                        }
                    }
                }

                for (int i = 0; i < (1 << LOGSIZEL); i++)
                {
                    for (int j = 0; j < (LWIDTH / LPSTEP) * (1 << LPSTEP); j++)
                    {
                        if ((j & 1) == 1)
                        {
                            PERCLOC[i][j] = -1;
                        }
                    }
                }

                for (int i = 0; i < (1 << LOGSIZEB); i++)
                {
                    for (int j = 0; j < ((BWIDTH / BPSTEP)) * (1 << BPSTEP); j++)
                    {
                        if ((j & 1) == 1)
                        {
                            PERCBACK[i][j] = -1;
                        }
                    }
                }

                for (int i = 0; i < (1 << LOGSIZEY); i++)
                {
                    for (int j = 0; j < (YWIDTH / YPSTEP) * (1 << YPSTEP); j++)
                    {
                        if ((j & 1) == 1)
                        {
                            PERCYHA[i][j] = -1;
                        }
                    }
                }

                for (int i = 0; i < (1 << LOGSIZEP); i++)
                {
                    for (int j = 0; j < (PWIDTH / PPSTEP) * (1 << PPSTEP); j++)
                    {
                        if ((j & 1) == 1)
                        {
                            PERCPATH[i][j] = -1;
                        }
                    }
                }
            }

            public bool getPrediction(uint branchAddress)
            {
                subpaths[0] = pathSpectrums[0].subpaths[0];	// global path
                subpaths[1] = pathSpectrums[1].subpaths[branchAddress % P1_SPSIZE];	// per-address subpath
                subpaths[2] = pathSpectrums[2].subpaths[(branchAddress >> P2_PARAM) % P2_SPSIZE];	// per-set subpath
                subpaths[3] = pathSpectrums[3].subpaths[(branchAddress >> P3_PARAM) % P3_SPSIZE];	// another per-set subpath
                int f = frequencyBins.find(branchFrequencyTable.getFrequency(branchAddress));
                subpaths[4] = pathSpectrums[4].subpaths[f];	// frequency subpath 
                subpaths[5] = pathSpectrums[5].subpaths[0]; // global backward path

                for (int i = 0; i < numberOfTagePredictors; i++)
                {
                    tagePredictions[i] = tagePredictors[i].predict(branchAddress, subpaths[i]);
                    tagePostPredictionIndexes[i] = tagePredictors[i].postPredictionIndex; // 7 bits of information: the two longest hitting counters + the u bit
                }

                int tagePredictionsAsInt = (tagePredictions[0] ? 1 : 0) ^ ((tagePredictions[1] ? 1 : 0) << 1) ^ ((tagePredictions[2] ? 1 : 0) << 2) ^ ((tagePredictions[3] ? 1 : 0) << 3) ^ ((tagePredictions[4] ? 1 : 0) << 4) ^ ((tagePredictions[5] ? 1 : 0) << 5);

                INDFIRST = ((int)(branchAddress << 6) ^ tagePredictionsAsInt) & ((1 << LOGTAB) - 1);
                INDBIAS0 = ((int)(branchAddress << 7) ^ tagePostPredictionIndexes[0]) & ((1 << LOGTAB) - 1);
                INDBIAS1 = ((int)(branchAddress << 7) ^ tagePostPredictionIndexes[1]) & ((1 << LOGTAB) - 1);
                INDBIAS2 = ((int)(branchAddress << 7) ^ tagePostPredictionIndexes[2]) & ((1 << LOGTAB) - 1);
                INDBIAS3 = ((int)(branchAddress << 7) ^ tagePostPredictionIndexes[3]) & ((1 << LOGTAB) - 1);
                INDBIAS4 = ((int)(branchAddress << 7) ^ tagePostPredictionIndexes[4]) & ((1 << LOGTAB) - 1);
                INDBIAS5 = ((int)(branchAddress << 3) ^ tagePostPredictionIndexes[5]) & ((1 << LOGTAB) - 1);

                INDSB0 = ((int)(branchAddress << 13) ^ (tagePredictionsAsInt << 7) ^ tagePostPredictionIndexes[0]) & ((1 << LOGTAB) - 1);
                INDSB1 = ((int)(branchAddress << 13) ^ (tagePredictionsAsInt << 7) ^ tagePostPredictionIndexes[1]) & ((1 << LOGTAB) - 1);
                INDSB2 = ((int)(branchAddress << 13) ^ (tagePredictionsAsInt << 7) ^ tagePostPredictionIndexes[2]) & ((1 << LOGTAB) - 1);
                INDSB3 = ((int)(branchAddress << 13) ^ (tagePredictionsAsInt << 7) ^ tagePostPredictionIndexes[3]) & ((1 << LOGTAB) - 1);
                INDSB4 = ((int)(branchAddress << 13) ^ (tagePredictionsAsInt << 7) ^ tagePostPredictionIndexes[4]) & ((1 << LOGTAB) - 1);
                INDSB5 = ((int)(branchAddress << 13) ^ (tagePredictionsAsInt << 7) ^ tagePostPredictionIndexes[5]) & ((1 << LOGTAB) - 1);

                tageCombinerSum = 0;
                tageCombinerSum += (2 * FirstBIAS[INDFIRST] + 1);
                tageCombinerSum += (2 * TBias0[INDBIAS0] + 1);
                tageCombinerSum += (2 * TBias1[INDBIAS1] + 1);
                tageCombinerSum += (2 * TBias2[INDBIAS2] + 1);
                tageCombinerSum += (2 * TBias3[INDBIAS3] + 1);
                tageCombinerSum += (2 * TBias4[INDBIAS4] + 1);
                tageCombinerSum += (2 * TBias5[INDBIAS5] + 1);

                tageCombinerSum += (2 * SB0[INDSB0] + 1);
                tageCombinerSum += (2 * SB1[INDSB1] + 1);
                tageCombinerSum += (2 * SB2[INDSB2] + 1);
                tageCombinerSum += (2 * SB3[INDSB3] + 1);
                tageCombinerSum += (2 * SB4[INDSB4] + 1);
                tageCombinerSum += (2 * SB5[INDSB5] + 1);

                combinedTagePrediction = (tageCombinerSum >= 0);

                // Extracting the confidence level
                int tageCombinerMagnitude = Math.Abs(tageCombinerSum);
                if (tageCombinerMagnitude < tageCombinerThreshold / 4)
                {
                    typeFirstSum = 0;
                }
                else
                {
                    if (tageCombinerMagnitude < tageCombinerThreshold / 2)
                    {
                        typeFirstSum = 1;
                    }
                    else
                    {
                        if (tageCombinerMagnitude < tageCombinerThreshold)
                        {
                            typeFirstSum = 2;
                        }
                        else
                        {
                            typeFirstSum = 3;
                        }
                    }
                }

                // the COLT prediction
                coltPrediction = colt.predict(branchAddress, tagePredictions);

                // the statistical correlator
                statisticalCorrelatorPrediction = statisticalCorrelatorPredict(branchAddress);

                bool finalPrediction = finalCombinerPredict(branchAddress);
                return finalPrediction;
            }

            public void updatePredictor(uint branchAddress, bool branchTaken, uint branchTarget, bool branchIsConditional)
            {
                // the TAGE stage
                uint branchTargetForUpdate = branchTaken ? (branchTarget << 1) ^ branchAddress : branchAddress;
                for (int i = 0; i < numberOfTagePredictors - 1; i++)
                {
                    tagePredictors[i].update(branchAddress, branchTaken, subpaths[i]);
                    subpaths[i].update(branchTargetForUpdate, branchTaken);
                }
                tagePredictors[numberOfTagePredictors - 1].update(branchAddress, branchTaken, subpaths[numberOfTagePredictors - 1]);
                if (branchTarget < branchAddress)
                {
                    subpaths[numberOfTagePredictors - 1].update(branchTargetForUpdate, ((branchTarget < branchAddress) && branchTaken));
                }

                frequencyBins.update(branchFrequencyTable.getFrequency(branchAddress));
                branchFrequencyTable.incrementFrequency(branchAddress);

                // update of the TAGE combiner
                if ((combinedTagePrediction != branchTaken) || (Math.Abs(tageCombinerSum) < tageCombinerThreshold))
                {
                    if (combinedTagePrediction != branchTaken)
                    {
                        tageCombinerThreshold += 1;
                    }
                    else
                    {
                        tageCombinerThreshold -= 1;
                    }

                    saturatedCounterUpdate(ref FirstBIAS[INDFIRST], branchTaken, perceptronWeightBits);
                    saturatedCounterUpdate(ref TBias0[INDBIAS0], branchTaken, perceptronWeightBits);
                    saturatedCounterUpdate(ref TBias1[INDBIAS1], branchTaken, perceptronWeightBits);
                    saturatedCounterUpdate(ref TBias2[INDBIAS1], branchTaken, perceptronWeightBits);
                    saturatedCounterUpdate(ref TBias3[INDBIAS3], branchTaken, perceptronWeightBits);
                    saturatedCounterUpdate(ref TBias4[INDBIAS4], branchTaken, perceptronWeightBits);
                    saturatedCounterUpdate(ref TBias5[INDBIAS5], branchTaken, perceptronWeightBits);
                    saturatedCounterUpdate(ref SB0[INDSB0], branchTaken, perceptronWeightBits);
                    saturatedCounterUpdate(ref SB1[INDSB1], branchTaken, perceptronWeightBits);
                    saturatedCounterUpdate(ref SB2[INDSB1], branchTaken, perceptronWeightBits);
                    saturatedCounterUpdate(ref SB3[INDSB3], branchTaken, perceptronWeightBits);
                    saturatedCounterUpdate(ref SB4[INDSB4], branchTaken, perceptronWeightBits);
                    saturatedCounterUpdate(ref SB5[INDSB5], branchTaken, perceptronWeightBits);
                }
                colt.update(branchAddress, tagePredictions, branchTaken);
                //end of the TAGE combiner

                //the statistical corrector
                updateStatisticalCorrelator(branchAddress, branchTaken);

                // The final stage
                updateFinalCombiner(branchAddress, branchTaken);

                sbyte branchType = (sbyte)(branchIsConditional ? OPTYPE_BRANCH_COND : 0);
                historyUpdate(branchAddress, branchType, branchTaken, branchTarget);
            }

            public bool statisticalCorrelatorPredict(uint branchAddress)
            {
                INDLOCAL = (int)(branchAddress & (NLOCAL - 1));
                INDSLOCAL = (int)(((branchAddress ^ (branchAddress >> 5)) >> NB) & (NSECLOCAL - 1));
                INDTLOCAL = (int)(((branchAddress ^ (branchAddress >> 3) ^ (branchAddress >> 6))) & (NTLOCAL - 1));
                INDQLOCAL = (int)(((branchAddress ^ (branchAddress >> 2) ^ (branchAddress >> 4) ^ (branchAddress >> 8))) & (NQLOCAL - 1));
                INDIMLI = (int)(IMLIcount & (NTIMLI - 1));
                INDUPD = (int)(branchAddress & ((1 << LOGSIZE) - 1));

                INDBIAS = (((int)(branchAddress << 1) ^ (combinedTagePrediction ? 1 : 0)) & ((1 << LOGBIAS) - 1));
                INDBIASFULL = ((int)(branchAddress << 4) ^ (typeFirstSum + (((coltPrediction ? 1 : 0) + ((combinedTagePrediction ? 1 : 0) << 1)) << 2))) & ((1 << LOGBIASFULL) - 1);
                INDBIASCOLT = (int)(((int)(branchAddress << 7) ^ (combinedTagePrediction ? 1 : 0) ^ (((tagePredictions[0] ? 1 : 0) ^ ((tagePredictions[1] ? 1 : 0) << 1) ^ ((tagePredictions[2] ? 1 : 0) << 2) ^ ((tagePredictions[3] ? 1 : 0) << 3) ^ ((tagePredictions[4] ? 1 : 0) << 4) ^ ((tagePredictions[5] ? 1 : 0) << 5)) << 1)) & ((1 << LOGBIASCOLT) - 1));

                statisticalCorrelatorSum = 0;
                predictUsingGehl(branchAddress);
                predictUsingRhsp(branchAddress);
                statisticalCorrelatorSum += gehlSum;
                statisticalCorrelatorSum += rhspSum;

                statisticalCorrelatorSum += 2 * (2 * Bias[INDBIAS] + 1);
                statisticalCorrelatorSum += 2 * (2 * BiasFull[INDBIASFULL] + 1);
                statisticalCorrelatorSum += 2 * (2 * BiasColt[INDBIASCOLT] + 1);

                statisticalCorrelatorSum += predictUsingPerceptronStyle(branchAddress, globalBranchHistory, PERC[branchAddress & ((1 << LOGSIZEG) - 1)], GPSTEP, GWIDTH);
                statisticalCorrelatorSum += predictUsingPerceptronStyle(branchAddress, L_shist[INDLOCAL], PERCLOC[branchAddress & ((1 << LOGSIZEL) - 1)], LPSTEP, LWIDTH);
                statisticalCorrelatorSum += predictUsingPerceptronStyle(branchAddress, BHIST, PERCBACK[branchAddress & ((1 << LOGSIZEB) - 1)], BPSTEP, BWIDTH);
                statisticalCorrelatorSum += predictUsingPerceptronStyle(branchAddress, YHA, PERCYHA[branchAddress & ((1 << LOGSIZEY) - 1)], YPSTEP, YWIDTH);
                statisticalCorrelatorSum += predictUsingPerceptronStyle(branchAddress, P_phist, PERCPATH[branchAddress & ((1 << LOGSIZEP) - 1)], PPSTEP, PWIDTH);
                statisticalCorrelatorSum += predictUsingPerceptronStyle(branchAddress, S_slhist[INDSLOCAL], PERCSLOC[branchAddress & ((1 << LOGSIZES) - 1)], SPSTEP, SWIDTH);
                statisticalCorrelatorSum += predictUsingPerceptronStyle(branchAddress, T_slhist[INDTLOCAL], PERCTLOC[branchAddress & ((1 << LOGSIZET) - 1)], TPSTEP, TWIDTH);
                statisticalCorrelatorSum += predictUsingPerceptronStyle(branchAddress, Q_slhist[INDQLOCAL], PERCQLOC[branchAddress & ((1 << LOGSIZEQ) - 1)], QPSTEP, QWIDTH);

                statisticalCorrelatorSum += predictUsingGehlStyle(branchAddress, L_shist[INDLOCAL], Lm, LGEHL, LNB);
                statisticalCorrelatorSum += predictUsingGehlStyle(branchAddress, T_slhist[INDTLOCAL], Tm, TGEHL, TNB);
                statisticalCorrelatorSum += predictUsingGehlStyle(branchAddress, Q_slhist[INDQLOCAL], Qm, QGEHL, QNB);
                statisticalCorrelatorSum += predictUsingGehlStyle(0, Q_slhist[INDQLOCAL], QQm, QQGEHL, QQNB);
                statisticalCorrelatorSum += predictUsingGehlStyle(branchAddress, S_slhist[INDSLOCAL], Sm, SGEHL, SNB);
                statisticalCorrelatorSum += predictUsingGehlStyle(branchAddress, BHIST, Bm, BGEHL, BNB);
                statisticalCorrelatorSum += predictUsingGehlStyle(branchAddress, CHIST, Cm, CGEHL, CNB);
                statisticalCorrelatorSum += predictUsingGehlStyle(branchAddress, RHIST, Rm, RGEHL, RNB);
                statisticalCorrelatorSum += predictUsingGehlStyle(branchAddress, YHA, Ym, YGEHL, BNB);
                statisticalCorrelatorSum += predictUsingGehlStyle(branchAddress, (IMLIcount + (globalBranchHistory << 16)), Im, IGEHL, INB);
                statisticalCorrelatorSum += predictUsingGehlStyle(branchAddress, IMLIhist[INDIMLI], IMLIm, IMLIGEHL, IMLINB);
                statisticalCorrelatorSum += predictUsingGehlStyle(branchAddress, (L_shist[INDLOCAL] << 16) ^ IMLIcount, LIm, LIGEHL, LINB);

                futurelocal = -1;
                for (int i = Fm[FNB - 1]; i >= 0; i--)
                {
                    futurelocal = histtable[(((int)(branchAddress ^ (branchAddress >> 2)) << SHIFTFUTURE) + IMLIcount + i) & (HISTTABLESIZE - 1)] + (futurelocal << 1);
                }
                futurelocal = PAST[branchAddress & 63] + (futurelocal << 1);
                statisticalCorrelatorSum += predictUsingGehlStyle((branchAddress << 8), futurelocal, Fm, fGEHL, FNB);

                return (statisticalCorrelatorSum >= 0);
            }

            public void updateStatisticalCorrelator(uint branchAddress, bool taken)
            {
                if ((statisticalCorrelatorPrediction != taken) || ((Math.Abs(statisticalCorrelatorSum) < statisticalCorrelatorThreshold + statisticalCorrelatorPerBranchThreshold[INDUPD])))
                {
                    if (statisticalCorrelatorPrediction != taken)
                    {
                        statisticalCorrelatorThreshold += 1;
                        statisticalCorrelatorPerBranchThreshold[INDUPD] += 1;
                    }
                    else
                    {
                        statisticalCorrelatorThreshold -= 1;
                        statisticalCorrelatorPerBranchThreshold[INDUPD] -= 1;
                    }

                    updateGehl(branchAddress, taken);
                    updateRhsp(branchAddress, taken);
                    saturatedCounterUpdate(ref BiasFull[INDBIASFULL], taken, perceptronWeightBits);
                    saturatedCounterUpdate(ref Bias[INDBIAS], taken, perceptronWeightBits);
                    saturatedCounterUpdate(ref BiasColt[INDBIASCOLT], taken, perceptronWeightBits);

                    updatePerceptronStyle(taken, PERC[branchAddress & ((1 << LOGSIZEG) - 1)], globalBranchHistory, GPSTEP, GWIDTH);
                    updatePerceptronStyle(taken, PERCLOC[branchAddress & ((1 << LOGSIZEL) - 1)], L_shist[INDLOCAL], LPSTEP, LWIDTH);
                    updatePerceptronStyle(taken, PERCBACK[branchAddress & ((1 << LOGSIZEB) - 1)], BHIST, BPSTEP, BWIDTH);
                    updatePerceptronStyle(taken, PERCYHA[branchAddress & ((1 << LOGSIZEB) - 1)], YHA, YPSTEP, YWIDTH);
                    updatePerceptronStyle(taken, PERCPATH[branchAddress & ((1 << LOGSIZEP) - 1)], P_phist, PPSTEP, PWIDTH);
                    updatePerceptronStyle(taken, PERCSLOC[branchAddress & ((1 << LOGSIZES) - 1)], S_slhist[INDSLOCAL], SPSTEP, SWIDTH);
                    updatePerceptronStyle(taken, PERCTLOC[branchAddress & ((1 << LOGSIZES) - 1)], T_slhist[INDTLOCAL], SPSTEP, SWIDTH);
                    updatePerceptronStyle(taken, PERCQLOC[branchAddress & ((1 << LOGSIZEQ) - 1)], Q_slhist[INDQLOCAL], QPSTEP, QWIDTH);

                    updateGehlStyle(branchAddress, taken, L_shist[INDLOCAL], Lm, LGEHL, LNB, perceptronWeightBits);
                    // for IMLI
                    updateGehlStyle(branchAddress, taken, (L_shist[INDLOCAL] << 16) ^ IMLIcount, LIm, LIGEHL, LINB, perceptronWeightBits);
                    updateGehlStyle(branchAddress, taken, S_slhist[INDSLOCAL], Sm, SGEHL, SNB, perceptronWeightBits);
                    updateGehlStyle(branchAddress, taken, T_slhist[INDTLOCAL], Tm, TGEHL, TNB, perceptronWeightBits);
                    updateGehlStyle(branchAddress, taken, IMLIhist[INDIMLI], IMLIm, IMLIGEHL, IMLINB, perceptronWeightBits);
                    updateGehlStyle(branchAddress, taken, Q_slhist[INDQLOCAL], Qm, QGEHL, QNB, perceptronWeightBits);
                    updateGehlStyle(0, taken, Q_slhist[INDQLOCAL], QQm, QQGEHL, QQNB, perceptronWeightBits);
                    updateGehlStyle(branchAddress, taken, BHIST, Bm, BGEHL, BNB, perceptronWeightBits);
                    updateGehlStyle(branchAddress, taken, CHIST, Cm, CGEHL, CNB, perceptronWeightBits);
                    updateGehlStyle(branchAddress, taken, RHIST, Rm, RGEHL, RNB, perceptronWeightBits);
                    updateGehlStyle(branchAddress, taken, YHA, Ym, YGEHL, YNB, perceptronWeightBits);
                    updateGehlStyle(branchAddress, taken, (IMLIcount + (globalBranchHistory << 16)), Im, IGEHL, INB, perceptronWeightBits);
                    updateGehlStyle((branchAddress << 8), taken, futurelocal, Fm, fGEHL, FNB, perceptronWeightBits);
                }
            }

            public bool finalCombinerPredict(uint branchAddress)
            {
                int typeSecondSum;
                int statisticalCorrelatorMagnitude = Math.Abs(statisticalCorrelatorSum);
                int computedThreshold = statisticalCorrelatorThreshold + statisticalCorrelatorPerBranchThreshold[INDUPD];
                if (statisticalCorrelatorMagnitude < computedThreshold / 4)
                {
                    typeSecondSum = 0;
                }
                else
                {
                    if (statisticalCorrelatorMagnitude < computedThreshold / 2)
                    {
                        typeSecondSum = 1;
                    }
                    else
                    {
                        if (statisticalCorrelatorMagnitude < computedThreshold)
                        {
                            typeSecondSum = 2;
                        }
                        else
                        {
                            typeSecondSum = 3;
                        }
                    }
                }
                indexFinalCombiner = typeFirstSum + (typeSecondSum << 2) + (((coltPrediction ? 1 : 0) + ((combinedTagePrediction ? 1 : 0) << 1) + ((statisticalCorrelatorPrediction ? 1 : 0) << 2)) << 4);
                indexFinalCombinerColt = ((int)(branchAddress << 7) + indexFinalCombiner) & (TABSIZE - 1);

                if (GFINALCOLT[indexFinalCombinerColt] < -8 || GFINALCOLT[indexFinalCombinerColt] > 7)
                {
                    return GFINALCOLT[indexFinalCombinerColt] >= 0;
                }
                return GFINAL[indexFinalCombiner] >= 0;
            }

            public void updateFinalCombiner(uint branchAddress, bool taken)
            {
                // using only the GFINAL table would result in 0.004 MPKI more
                saturatedCounterUpdate(ref GFINALCOLT[indexFinalCombinerColt], taken, 8);
                saturatedCounterUpdate(ref GFINAL[indexFinalCombiner], taken, 8);
            }

            public void historyUpdate(uint branchAddress, sbyte brtype, bool taken, uint branchTarget)
            {
                // History skeleton
                bool branchSeenInRecentPast = false;
                for (int i = 0; i <= 7; i++)
                {
                    if (lastBranchAddresses[i] == branchAddress)
                    {
                        branchSeenInRecentPast = true;
                        break;
                    }
                }

                for (int i = 7; i >= 1; i--)
                {
                    lastBranchAddresses[i] = lastBranchAddresses[i - 1];
                }

                lastBranchAddresses[0] = branchAddress;

                int takenBit = (taken ? 1 : 0);

                if (!branchSeenInRecentPast)
                {
                    YHA = (YHA << 1) ^ (takenBit ^ ((branchAddress >> 5) & 1));
                }

                //Path history
                P_phist = (P_phist << 1) ^ (takenBit ^ ((branchAddress >> 5) & 1));
                IMLIhist[INDIMLI] = (IMLIhist[INDIMLI] << 1) ^ (long)(takenBit ^ ((branchAddress >> 5) & 1));

                if (brtype == OPTYPE_BRANCH_COND)
                {
                    // local history 
                    L_shist[INDLOCAL] = (L_shist[INDLOCAL] << 1) + takenBit;
                    Q_slhist[INDQLOCAL] = (Q_slhist[INDQLOCAL] << 1) + takenBit;
                    S_slhist[INDSLOCAL] = ((S_slhist[INDSLOCAL] << 1) + takenBit) ^ (long)((branchAddress >> LOGSECLOCAL) & 15);
                    T_slhist[INDTLOCAL] = ((T_slhist[INDTLOCAL] << 1) + takenBit) ^ (long)((branchAddress >> LOGTLOCAL) & 15);

                    // global branch history
                    globalBranchHistory = (globalBranchHistory << 1) + takenBit;

                    if ((branchTarget > branchAddress + 64) || (branchTarget < branchAddress - 64))
                    {
                        RHIST = (RHIST << 1) + takenBit;
                    }
                    if (taken)
                    {
                        if ((branchTarget > branchAddress + 64) || (branchTarget < branchAddress + 64))
                        {
                            CHIST = (CHIST << 1) ^ (long)(branchAddress & 63);
                        }
                    }

                    //IMLI related 
                    if (branchTarget < branchAddress)
                    {
                        //This branch corresponds to a loop
                        if (taken)
                        {
                            if (IMLIcount < ((1 << Im[0]) - 1))
                            {
                                IMLIcount++;
                            }
                        }
                        else
                        {
                            //exit of the "loop"
                            IMLIcount = 0;
                        }
                    }
                    else
                    {
                        // IMLI OH history, see IMLI paper at Micro 2015
                        // (branchTarget >= branchAddress)
                        PAST[branchAddress & 63] = histtable[((long)((branchAddress ^ (branchAddress >> 2)) << SHIFTFUTURE) + IMLIcount) & (HISTTABLESIZE - 1)];
                        histtable[((long)((branchAddress ^ (branchAddress >> 2)) << SHIFTFUTURE) + IMLIcount) & (HISTTABLESIZE - 1)] = (sbyte)takenBit;
                    }
                }

                //is it really useful ?
                if ((branchAddress + 16 < lastBranchAddress) || (branchAddress > lastBranchAddress + 128))
                {
                    BHIST = (BHIST << 1) ^ (long)(branchAddress & 15);
                }
                lastBranchAddress = branchAddress;

                int T = (int)((branchTarget ^ (branchTarget >> 3) ^ branchAddress) << 1) + takenBit;
                sbyte DIR = (sbyte)(T & 127);

                //update  history
                globalHistoryPointer--;
                globalHistory[globalHistoryPointer & (HISTBUFFERLENGTH - 1)] = DIR; // ??????????????????????????????????????????????????????????????????????????????????????????????????
                //globalHistory[globalHistoryPointer & (HISTBUFFERLENGTH - 1)] = (sbyte)takenBit;

                //prepare next index and tag computations 
                for (int i = 1; i <= NGEHL; i++)
                {
                    gehlHistory[i].update(globalHistory, globalHistoryPointer);
                }

                for (int i = 1; i <= NRHSP; i++)
                {
                    rhspHistory[i].update(globalHistory, globalHistoryPointer);
                }
            }

            public int predictUsingPerceptronStyle(uint branchAddress, long branchHistory, sbyte[] weights, int PSTEP, int WIDTH)
            {
                int pointer = 0;
                int perceptronSum = 0;
                for (int i = 0; i < WIDTH; i += PSTEP)
                {
                    int index = (int)branchHistory & ((1 << PSTEP) - 1);
                    perceptronSum += 2 * weights[pointer + index] + 1;

                    branchHistory >>= PSTEP;
                    pointer += (1 << PSTEP);
                }
                return perceptronSum;
            }

            public void updatePerceptronStyle(bool taken, sbyte[] weights, long branchHistory, int PSTEP, int WIDTH)
            {
                int pointer = 0;
                for (int i = 0; i < WIDTH; i += PSTEP)
                {
                    int index = (int)branchHistory & ((1 << PSTEP) - 1);
                    saturatedCounterUpdate(ref weights[pointer + index], taken, perceptronWeightBits);

                    branchHistory >>= PSTEP;
                    pointer += (1 << PSTEP);
                }
            }

            public int predictUsingGehlStyle(ulong branchAddress, long branchHistory, int[] length, sbyte[][] tables, int numberOfTables)
            {
                int gehlSum = 0;
                for (int i = 0; i < numberOfTables; i++)
                {
                    long branchHistoryHash = branchHistory & ((long)((1 << length[i]) - 1));
                    int index = (int)((((long)branchAddress) ^ branchHistoryHash ^ (branchHistoryHash >> (LOGTAB - i)) ^ (branchHistoryHash >> (40 - 2 * i)) ^ (branchHistoryHash >> (60 - 3 * i))) & (TABSIZE - 1));
                    //gehlSum += 2 * tables[i][index] + 1;
                    gehlSum += tables[i][index];
                }
                //return gehlSum;
                return 2 * gehlSum + numberOfTables;
            }

            public void updateGehlStyle(ulong branchAddress, bool taken, long branchHistory, int[] length, sbyte[][] tables, int numberOfTables, int WIDTH)
            {
                for (int i = 0; i < numberOfTables; i++)
                {
                    long branchHistoryHash = branchHistory & ((long)((1 << length[i]) - 1));
                    int index = (int)((((long)branchAddress) ^ branchHistoryHash ^ (branchHistoryHash >> (LOGTAB - i)) ^ (branchHistoryHash >> (40 - 2 * i)) ^ (branchHistoryHash >> (60 - 3 * i))) & (TABSIZE - 1));
                    saturatedCounterUpdate(ref tables[i][index], taken, WIDTH);
                }
            }

            // index function for the GEHL and MAC-RHSP tables
            public int computeGehlIndex(uint branchAddress, int tableIndex)
            {
                int index = (int)(branchAddress ^ (branchAddress >> ((mgehl[tableIndex] % LOGGEHL) + 1)) ^ gehlHistory[tableIndex].historyHash);
                return index & ((1 << LOGGEHL) - 1);
            }

            public int computeRhspIndex(uint branchAddress, int tableIndex)
            {
                int index = (int)(branchAddress ^ (branchAddress >> ((mrhsp[tableIndex] % LOGRHSP) + 1)) ^ rhspHistory[tableIndex].historyHash);
                if (tableIndex > 1)
                {
                    index ^= (int)(rhspHistory[tableIndex - 1].historyHash);
                }
                if (tableIndex > 3)
                {
                    index ^= (int)(rhspHistory[tableIndex / 3].historyHash);
                }
                return index & ((1 << LOGRHSP) - 1);
            }

            public void predictUsingGehl(uint branchAddress)
            {
                gehlIndexes[0] = (int)branchAddress & ((1 << LOGGEHL) - 1);
                for (int tableIndex = 1; tableIndex <= NGEHL; tableIndex++)
                {
                    gehlIndexes[tableIndex] = computeGehlIndex(branchAddress, tableIndex);
                }

                gehlSum = 0;
                for (int tableIndex = 0; tableIndex <= NGEHL; tableIndex++)
                {
                    //gehlSum += 2 * gehlTables[gehlIndexes[tableIndex]][tableIndex] + 1;
                    gehlSum += gehlTables[gehlIndexes[tableIndex]][tableIndex];
                }
                gehlSum = 2 * gehlSum + NGEHL + 1; // speed optimization
            }

            public void updateGehl(uint branchAddress, bool taken)
            {
                for (int i = NGEHL; i >= 0; i--)
                {
                    saturatedCounterUpdate(ref gehlTables[gehlIndexes[i]][i], taken, perceptronWeightBits);
                }
            }

            public void predictUsingRhsp(uint branchAddress)
            {
                rhspIndexes[0] = (int)branchAddress & ((1 << LOGRHSP) - 1);
                for (int i = 1; i <= NRHSP; i++)
                {
                    rhspIndexes[i] = computeRhspIndex(branchAddress, i);
                }

                rhspSum = 0;
                for (int i = 1; i <= NRHSP; i++)
                {
                    //rhspSum += 2 * rhspTables[rhspIndexes[i]][i] + 1;
                    rhspSum += rhspTables[rhspIndexes[i]][i];
                }
                rhspSum = 2 * rhspSum + NRHSP; // speed optimization
            }

            public void updateRhsp(uint branchAddress, bool taken)
            {
                for (int i = NRHSP; i >= 1; i--)
                {
                    saturatedCounterUpdate(ref rhspTables[rhspIndexes[i]][i], taken, perceptronWeightBits);
                }
            }
        }

        public static readonly List<PredictorPropertyBase> Properties = new List<PredictorPropertyBase>();

        static MTAGESC()
        {
            Properties.Add(new PredictorBoolProperty("predictTaken", "Always predict:", "Taken", "Not Taken", true));
        }

        private PREDICTOR predictor;

        public MTAGESC(bool predictTaken)
        {
            //predictor = new PREDICTOR();
        }

        public bool predictBranch(BranchInfo branch)
        {
            return predictor.getPrediction(branch.address);
        }

        public void update(IBranch branch)
        {
            bool branchIsConditional = (branch.getBranchInfo().branchFlags & BranchInfo.BR_CONDITIONAL) > 0;
            predictor.updatePredictor(branch.getBranchInfo().address, branch.taken(), branch.getTargetAddress(), branchIsConditional);
        }

        public void reset()
        {
            predictor = new PREDICTOR();
        }
    }

}

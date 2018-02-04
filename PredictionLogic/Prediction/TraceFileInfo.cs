using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace PredictionLogic.Prediction
{
    public class TraceFileInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static PropertyChangedEventArgs filenamePropertyChangedEventArgs = new PropertyChangedEventArgs("Filename");
        private static PropertyChangedEventArgs selectedPropertyChangedEventArgs = new PropertyChangedEventArgs("Selected");

        private string filename;
        public string Filename
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, filenamePropertyChangedEventArgs);
                }
            }
        }

        private bool selected;
        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, selectedPropertyChangedEventArgs);
                }
            }
        }

        public static string[] stanfordFilenames;
        public static string[] cbp2Filenames;
        public static string[] spec2000Filenames;

        static TraceFileInfo()
        {
            stanfordFilenames = new string[8];
            stanfordFilenames[0] = "fbubble.tra";
            stanfordFilenames[1] = "fmatrix.tra";
            stanfordFilenames[2] = "fperm.tra";
            stanfordFilenames[3] = "fpuzzle.tra";
            stanfordFilenames[4] = "fqueens.tra";
            stanfordFilenames[5] = "fsort.tra";
            stanfordFilenames[6] = "ftower.tra";
            stanfordFilenames[7] = "ftree.tra";

            cbp2Filenames = new string[20];
            cbp2Filenames[0] = "bzip2.trace";
            cbp2Filenames[1] = "compress.trace";
            cbp2Filenames[2] = "crafty.trace";
            cbp2Filenames[3] = "db.trace";
            cbp2Filenames[4] = "eon.trace";
            cbp2Filenames[5] = "gap.trace";
            cbp2Filenames[6] = "gcc.trace";
            cbp2Filenames[7] = "gzip.trace";
            cbp2Filenames[8] = "jack.trace";
            cbp2Filenames[9] = "javac.trace";
            cbp2Filenames[10] = "jess.trace";
            cbp2Filenames[11] = "mcf.trace";
            cbp2Filenames[12] = "mpegaudio.trace";
            cbp2Filenames[13] = "mtrt.trace";
            cbp2Filenames[14] = "parser.trace";
            cbp2Filenames[15] = "perlbmk.trace";
            cbp2Filenames[16] = "raytrace.trace";
            cbp2Filenames[17] = "twolf.trace";
            cbp2Filenames[18] = "vortex.trace";
            cbp2Filenames[19] = "vpr.trace";

            spec2000Filenames = new string[17];
            spec2000Filenames[0] = "099.go";
            spec2000Filenames[1] = "124.m88ksim";
            spec2000Filenames[2] = "129.compress";
            spec2000Filenames[3] = "130.li";
            spec2000Filenames[4] = "132.ijpeg";
            spec2000Filenames[5] = "164.gzip";
            spec2000Filenames[6] = "175.vpr";
            spec2000Filenames[7] = "176.gcc";
            spec2000Filenames[8] = "181.mcf";
            spec2000Filenames[9] = "186.crafty";
            spec2000Filenames[10] = "197.parser";
            spec2000Filenames[11] = "252.eon";
            spec2000Filenames[12] = "253.perlbmk";
            spec2000Filenames[13] = "254.gap";
            spec2000Filenames[14] = "255.vortex";
            spec2000Filenames[15] = "256.bzip2";
            spec2000Filenames[16] = "300.twolf";
        }
    }
}

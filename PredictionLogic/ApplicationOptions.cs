using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Configuration;

namespace PredictionLogic
{
    public class ApplicationOptions
    {
        public ApplicationOptions()
        {
        }

        #region trace paths

        protected string folderSpec2000 = "spec2000\\";
        protected string folderCbp2 = "cbp2\\";
        protected string folderStanford = "stanford\\";

        protected string tracePathMain = "";
        public virtual string TracePathMain
        {
            get
            {
                return tracePathMain;
            }
            set
            {
                tracePathMain = value;
                tracePathCBP2 = Path.Combine(value, folderCbp2);
                tracePathSpec2000 = Path.Combine(value, folderSpec2000);
                tracePathStanford = Path.Combine(value, folderStanford);
            }
        }

        protected string tracePathSpec2000;
        public string TracePathSpec2000
        {
            get
            {
                return tracePathSpec2000;
            }
            set
            {
                tracePathSpec2000 = value;
            }
        }

        protected string tracePathCBP2;
        public string TracePathCBP2
        {
            get
            {
                return tracePathCBP2;
            }
            set
            {
                tracePathCBP2 = value;
            }
        }

        protected string tracePathStanford;
        public string TracePathStanford
        {
            get
            {
                return tracePathStanford;
            }
            set
            {
                tracePathStanford = value;
            }
        }

        #endregion
    }
}

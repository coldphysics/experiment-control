using Model.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buffer.DatabaseAccess
{
    public class EntryPOCO
    {
        public int GlobalCounter { set; get; }
        public int StartCounterOfScansOfCurrentModel { set; get; }
        public int StartGlobalCounterOfMeasurementRoutine { set; get; }
        public int IterationOfScan { set; get; }
        public int CompletedScans { set; get; }
        public int NumberOfIterations { set; get; }
        public double CycleDuration { set; get; }
        public bool IsMeasurementRoutineMode { set; get; }
        public bool IsIterating { set; get; }
        public int ModelIndex { set; get; }

        public List<VariableModel> VariableList { set; get; }
        public DateTime EstimatedStartTime { set; get; }
    }
}

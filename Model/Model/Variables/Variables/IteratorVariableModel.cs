using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Variables.Variables
{
    public class IteratorVariableModel:BasicVariableModel
    {
        private double startValue = 0.0;
        private double endValue = 0.0;
        private double increment = 0.0;

        public double Increment
        {
            get { return increment; }
            set { increment = value; }
        }

        public double EndValue
        {
            get { return endValue; }
            set { endValue = value; }
        }

        public double StartValue
        {
            get { return startValue; }
            set { startValue = value; }
        }
    }
}

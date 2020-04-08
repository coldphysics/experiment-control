using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Model.Variables.Variables
{
    [Serializable]
    public abstract class BasicVariableModel
    {
        private string name = "";
        private double duration = 0.0;
        private double value = 0.0;

        public double Value
        {
            get { return this.value; }
            set { this.value = value; }
        }


        public double Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public BasicVariableModel DeepClone()
        {
            object objResult = null;

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, this);

                ms.Position = 0;
                objResult = bf.Deserialize(ms);
            }

            return (BasicVariableModel)objResult;
        }
    }
}

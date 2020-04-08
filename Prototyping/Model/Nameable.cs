using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyping.Model
{
    class Nameable
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Nameable(string name)
        {
            Name = name;
        }

        public Nameable()
            : this("")
        { }

    }
}

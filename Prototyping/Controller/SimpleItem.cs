using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prototyping.Model;

namespace Prototyping.Controller
{
    class SimpleItem : CustomElements.CheckableTreeView.CTVItemViewModel
    {
        private Nameable localObject;

        internal Nameable LocalObject
        {
            get { return localObject; }
            set { localObject = value; }
        }

        public override string Name
        {
            get
            {
                return localObject.Name;
            }
        }
    }
}

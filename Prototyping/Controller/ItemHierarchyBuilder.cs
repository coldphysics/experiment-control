using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomElements.CheckableTreeView;

namespace Prototyping.Controller
{
    class ItemHierarchyBuilder
    {
        public CTVViewModel GenerateHierarchy()
        {
            CTVItemViewModel root = new SimpleItem
            {
                LocalObject = new Model.Nameable("All"),

                Children = new List<CTVItemViewModel>()
                {
                    new SimpleItem()
                    {
                        LocalObject = new Model.Nameable("AO1"),
                        Children = new List<CTVItemViewModel>()
                        {
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch0")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch1")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch2")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch3")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch4")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch5")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch6")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch7")},
                        }
                    },
                    new SimpleItem()
                    {
                        LocalObject = new Model.Nameable("AO2"),
                        Children = new List<CTVItemViewModel>()
                        {
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch0")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch1")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch2")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch3")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch4")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch5")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch6")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch7")},
                        }
                    },
                    new SimpleItem()
                    {
                        LocalObject = new Model.Nameable("AO3"),
                        Children = new List<CTVItemViewModel>()
                        {
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch0")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch1")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch2")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch3")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch4")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch5")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch6")},
                             new SimpleItem(){LocalObject = new Model.Nameable("Ch7")},
                        }
                    }

                }
            };


            return new CTVViewModel(){root};
        }
    }
}

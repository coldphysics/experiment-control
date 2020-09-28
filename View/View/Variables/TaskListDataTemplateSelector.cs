using Controller.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace View.Variables
{
    public class TaskListDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            if (container is FrameworkElement element && item != null && item is VariableController)
            {
                VariableController taskitem = item as VariableController;

                //System.Console.Write("Iswhat: {0}\n", taskitem.isGroupHeader);
                if (taskitem.IsGroupHeader == true)
                    return element.FindResource("VariableStaticGroupHeaderTemplate") as DataTemplate;
                else
                    return element.FindResource("VariableStaticTemplate") as DataTemplate;
            }

            return null;
        }
    }
}

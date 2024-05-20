using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace InventoryUtility.Utils
{
    public class Functions
    {
        public static bool BringControlWithErrorIntoView(FrameworkElement view)
        {
            if (view == null) return false;
            foreach (FrameworkElement child in FindVisualChilds<FrameworkElement>(view))
            {
                if (Validation.GetHasError(child))
                {
                    child.BringIntoView();
                    child.Focus();
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<T> FindVisualChilds<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                    if (child != null && child is T t)
                    {
                        yield return t;
                    }

                    foreach (T childOfChild in FindVisualChilds<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}

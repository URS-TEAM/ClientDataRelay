using System.Windows.Controls;
using System.Windows;
using InventoryUtility.Models;

namespace InventoryUtility.Utils
{
    public class ButtonStateTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate WaitingTemplate { get; set; }
        public DataTemplate DoneTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ButtonState viewType)
            {
                switch (viewType)
                {
                    case ButtonState.Default:
                        return DefaultTemplate;
                    case ButtonState.Waiting:
                        return WaitingTemplate;
                    case ButtonState.Done:
                        return DoneTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}

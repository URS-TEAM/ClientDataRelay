using System.Windows.Controls;
using System.Windows;
using InventoryUtility.Models;
using InventoryUtility.Models.Functionalities;
using System;

namespace InventoryUtility.Utils
{
    public class FunctionalityTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate ManualStoreDataTransferTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null && item is IFunctionality)
            {
                switch (item.GetType())
                {
                    case Type type when type == typeof(StoreDataTransferFunctionality):
                        return ManualStoreDataTransferTemplate;
                    default:
                        return DefaultTemplate;
                }
            }

            return DefaultTemplate;
        }

    }
}

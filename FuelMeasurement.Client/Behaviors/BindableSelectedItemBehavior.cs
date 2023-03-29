using FuelMeasurement.Client.Models;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FuelMeasurement.Client.Behaviors
{
    public class BindableSelectedItemBehavior : Behavior<TreeView>
    {
        public static readonly DependencyProperty SelectedItemProperty = 
            DependencyProperty.Register(                                                                                 
                "SelectedItem",                                                                              
                typeof(ModelBase),                                                                                
                typeof(BindableSelectedItemBehavior),                                                               
                new UIPropertyMetadata(null));

        public ModelBase SelectedItem
        {
            get => (ModelBase)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject != null)
            {
                AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            }
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = (ModelBase)e.NewValue;
        }
    }
}

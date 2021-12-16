using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Windows.Interactivity;
namespace Byster.Models.Utilities
{

    public class ScrollToViewAfterSelectionBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += ItemLoaded;
            AssociatedObject.Unloaded += ItemUnloaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= ItemLoaded;
            ItemUnloaded(AssociatedObject, new RoutedEventArgs());
            AssociatedObject.Unloaded -= ItemUnloaded;
        }
        protected override void OnChanged()
        {
            base.OnChanged();
            foreach(var item in AssociatedObject.Items)
            {
                var listboxitem = (ListBoxItem)AssociatedObject.ItemContainerGenerator.ContainerFromItem(item);
                if (listboxitem == null) continue;
                var storyboard = getStoryboardOfSelectedStateOfListBox(listboxitem);
                if(storyboard == null) continue;
                storyboard.Completed -= SelectionCompleted; 
                storyboard.Completed += SelectionCompleted;
            }
        }

        private void ItemUnloaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Unloaded -= ItemUnloaded;
            foreach (var item in AssociatedObject.Items)
            {
                var listboxitem = (ListBoxItem)AssociatedObject.ItemContainerGenerator.ContainerFromItem(item);
                if (listboxitem == null) continue;
                var storyboard = getStoryboardOfSelectedStateOfListBox(listboxitem);
                if (storyboard == null) continue;
                storyboard.Completed -= SelectionCompleted;
            }
        }

        private void ItemLoaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Loaded -= ItemLoaded;
            for (int i = 0; i < AssociatedObject.Items.Count; i++)
            {
                var item = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(i);
                if(item == null) continue;
                var listboxitem = item as ListBoxItem;
                if (listboxitem == null) continue;
                var storyboard = getStoryboardOfSelectedStateOfListBox(listboxitem);
                if (storyboard == null) continue;
                storyboard.Completed += SelectionCompleted;
            }
        }
        private void SelectionCompleted(object sender, EventArgs e)
        {
            var listbox = AssociatedObject;
            listbox.ScrollIntoView(listbox.SelectedItem);
        }

        private Storyboard getStoryboardOfSelectedStateOfListBox(ListBoxItem listboxitem)
        {
            VisualState selectedState = null;
            VisualStateGroup selectionGroup = null;
            var groups = VisualStateManager.GetVisualStateGroups((Border)listboxitem.Template.FindName("borderItem", listboxitem));
            foreach (var group in groups)
            {
                var gr = group as VisualStateGroup;
                if (gr.Name == "SelectionStates")
                {
                    selectionGroup = gr;
                    break;
                }
            }
            if (selectionGroup == null) return null;
            var states = selectionGroup.States;
            foreach (var state in states)
            {
                var st = state as VisualState;
                if (st.Name == "Selected")
                {
                    selectedState = st;
                    break;
                }
            }
            if (selectedState == null) return null;
            return selectedState.Storyboard;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media.Animation;

namespace Byster.Models.Utilities
{
    public class ListBoxBehaviour
    {
        static readonly Dictionary<ListBoxItem, Capture> Associations = new Dictionary<ListBoxItem, Capture>();
        public static bool GetScrollToViewAfterAnimation(DependencyObject obj)
        {
            return (bool)obj.GetValue(ScrollToViewAfterAnimationProperty);
        }

        public static void SetScrollToViewAfterAnimation(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollToViewAfterAnimationProperty, value);
        }

        public static readonly DependencyProperty ScrollToViewAfterAnimationProperty = 
            DependencyProperty.RegisterAttached("ScrollToViewAfterAnimation", typeof(bool), typeof(ListBoxItem),
                new UIPropertyMetadata(false, new PropertyChangedCallback(OnScrollToViewAfterAnimationChanged)));

        public static void OnScrollToViewAfterAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listboxitem = (ListBoxItem)d;
            if (listboxitem == null) return;
            bool oldVal = (bool)e.OldValue, newVal = (bool)e.NewValue;
            if (newVal == oldVal) return;
            if(newVal)
            {
                listboxitem.Loaded += Listboxitem_Loaded;
                listboxitem.Unloaded += Listboxitem_Unloaded;
            }
            else
            {
                listboxitem.Loaded -= Listboxitem_Loaded;
                listboxitem.Unloaded -= Listboxitem_Unloaded;
                if (Associations.ContainsKey(listboxitem)) Associations[listboxitem].Dispose();
            }
        }

        private static void Listboxitem_Unloaded(object sender, RoutedEventArgs e)
        {
            var listboxitem = (ListBoxItem)sender;
            listboxitem.Unloaded -= Listboxitem_Unloaded;
            if(Associations.ContainsKey(listboxitem))
                Associations[listboxitem].Dispose();
        }

        private static void Listboxitem_Loaded(object sender, RoutedEventArgs e)
        {
            var listboxitem = (ListBoxItem)sender;
            listboxitem.Loaded -= Listboxitem_Loaded;
            Associations[listboxitem] = new Capture(listboxitem);
        }
    }

    public class Capture : IDisposable
    {
        private readonly ListBoxItem listBoxItem;

        public Capture(ListBoxItem listboxitem)
        {
            this.listBoxItem = listboxitem;
            Storyboard storyboard = getStoryboardOfSelectedStateOfListBox(listBoxItem);
            if(storyboard != null)
            {
                storyboard.Completed += OnSelectionChanged;
            }
        }
        public void Dispose()
        {
            if (listBoxItem != null)
            {
                Storyboard storyboard = getStoryboardOfSelectedStateOfListBox(listBoxItem);
                if(storyboard == null) return;
                storyboard.Completed -= OnSelectionChanged;
            }
        }
        private void OnSelectionChanged(object sender, EventArgs e)
        {
            (listBoxItem.Parent as ListBox).ScrollIntoView((listBoxItem.Parent as ListBox).SelectedItem);
        }

        private Storyboard getStoryboardOfSelectedStateOfListBox(ListBoxItem listboxitem)
        {
            VisualState selectedState = null;
            VisualStateGroup selectionGroup = null;
            var groups = VisualStateManager.GetVisualStateGroups(listboxitem);
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

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
using System.Collections.Specialized;
using System.Windows.Media;
using System.Windows.Data;

namespace Byster.Models.Utilities
{

    //public class ScrollToViewAfterSelectionBehavior : Behavior<ListBox>
    //{
    //    protected override void OnAttached()
    //    {
    //        base.OnAttached();
    //        AssociatedObject.Loaded += ItemLoaded;
    //        AssociatedObject.Unloaded += ItemUnloaded;
    //    }

    //    protected override void OnDetaching()
    //    {
    //        base.OnDetaching();
    //        AssociatedObject.Loaded -= ItemLoaded;
    //        ItemUnloaded(AssociatedObject, new RoutedEventArgs());
    //        AssociatedObject.Unloaded -= ItemUnloaded;
    //    }
    //    protected override void OnChanged()
    //    {
    //        base.OnChanged();
    //        foreach(var item in AssociatedObject.Items)
    //        {
    //            var listboxitem = (ListBoxItem)AssociatedObject.ItemContainerGenerator.ContainerFromItem(item);
    //            if (listboxitem == null) continue;
    //            var storyboard = getStoryboardOfSelectedStateOfListBox(listboxitem);
    //            if(storyboard == null) continue;
    //            storyboard.Completed -= SelectionCompleted; 
    //            storyboard.Completed += SelectionCompleted;
    //        }
    //    }

    //    private void ItemUnloaded(object sender, RoutedEventArgs e)
    //    {
    //        AssociatedObject.Unloaded -= ItemUnloaded;
    //        foreach (var item in AssociatedObject.Items)
    //        {
    //            var listboxitem = (ListBoxItem)AssociatedObject.ItemContainerGenerator.ContainerFromItem(item);
    //            if (listboxitem == null) continue;
    //            var storyboard = getStoryboardOfSelectedStateOfListBox(listboxitem);
    //            if (storyboard == null) continue;
    //            storyboard.Completed -= SelectionCompleted;
    //        }
    //    }

    //    private void ItemLoaded(object sender, RoutedEventArgs e)
    //    {
    //        AssociatedObject.Loaded -= ItemLoaded;
    //        for (int i = 0; i < AssociatedObject.Items.Count; i++)
    //        {
    //            var item = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(i);
    //            if(item == null) continue;
    //            var listboxitem = item as ListBoxItem;
    //            if (listboxitem == null) continue;
    //            var storyboard = getStoryboardOfSelectedStateOfListBox(listboxitem);
    //            if (storyboard == null) continue;
    //            storyboard.Completed += SelectionCompleted;
    //        }
    //        var incc = AssociatedObject.ItemsSource as INotifyCollectionChanged;
    //        if(incc != null)
    //            incc.CollectionChanged += incc_CollectionChanged;
    //    }

    //    private void incc_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    //    {
    //        if(e.Action != NotifyCollectionChangedAction.Add) return;
    //        foreach (var col_item in e.NewItems)
    //        {
    //            var item = AssociatedObject.ItemContainerGenerator.ContainerFromItem(col_item);
    //            if (item == null) continue;
    //            var listboxitem = item as ListBoxItem;
    //            if (listboxitem == null) continue;
    //            if(listboxitem.IsLoaded)
    //            {
    //                if (listboxitem.IsVisible)
    //                {
    //                    var storyboard = getStoryboardOfSelectedStateOfListBox(listboxitem);
    //                    if (storyboard != null)
    //                        storyboard.Completed += SelectionCompleted;
    //                }
    //                listboxitem.IsVisibleChanged += (o, args) =>
    //                {
    //                    if (!Convert.ToBoolean(args.NewValue)) return;
    //                    var storyboard = getStoryboardOfSelectedStateOfListBox(listboxitem);
    //                    if (storyboard == null) return;
    //                    storyboard.Completed -= SelectionCompleted;
    //                    storyboard.Completed += SelectionCompleted;
    //                };
    //            }
    //            else
    //            {
    //                listboxitem.Loaded += (o, args) =>
    //                {
    //                    if (listboxitem.IsVisible)
    //                    {
    //                        var storyboard = getStoryboardOfSelectedStateOfListBox(listboxitem);
    //                        if (storyboard != null)
    //                            storyboard.Completed += SelectionCompleted;
    //                    }
    //                    listboxitem.IsVisibleChanged += (_o, _args) =>
    //                    {
    //                        if (!Convert.ToBoolean(_args.NewValue)) return;
    //                        var storyboard = getStoryboardOfSelectedStateOfListBox(listboxitem);
    //                        if (storyboard == null) return;
    //                        storyboard.Completed -= SelectionCompleted;
    //                        storyboard.Completed += SelectionCompleted;
    //                    };
    //                };
    //            }
    //        }
    //    }

    //    private void SelectionCompleted(object sender, EventArgs e)
    //    {
    //        var listbox = AssociatedObject;
    //        listbox.ScrollIntoView(listbox.SelectedItem);
    //    }

    //    private Storyboard getStoryboardOfSelectedStateOfListBox(ListBoxItem listboxitem)
    //    {
    //        VisualState selectedState = null;
    //        VisualStateGroup selectionGroup = null;
    //        var element = (Border)listboxitem.Template.FindName("borderItem", listboxitem);
    //        if(element == null) return null;
    //        var groups = VisualStateManager.GetVisualStateGroups(element);
    //        foreach (var group in groups)
    //        {
    //            var gr = group as VisualStateGroup;
    //            if (gr.Name == "SelectionStates")
    //            {
    //                selectionGroup = gr;
    //                break;
    //            }
    //        }
    //        if (selectionGroup == null) return null;
    //        var states = selectionGroup.States;
    //        foreach (var state in states)
    //        {
    //            var st = state as VisualState;
    //            if (st.Name == "Selected")
    //            {
    //                selectedState = st;
    //                break;
    //            }
    //        }
    //        if (selectedState == null) return null;
    //        return selectedState.Storyboard;
    //    }
    //}

    public class AnimatedOffsets : DependencyObject
    {
        public static readonly DependencyProperty AnimatedHorizontalOffsetProperty = DependencyProperty.RegisterAttached("AnimatedHorizontalOffset", typeof(double), typeof(ScrollViewer), new PropertyMetadata()
        {
            DefaultValue = 0.0,
            PropertyChangedCallback = (dp, args) =>
            {
                if (args.NewValue == args.OldValue) return;
                if (args.NewValue == null) return;
                if (dp == null || !(dp is ScrollViewer)) return;
                (dp as ScrollViewer).ScrollToHorizontalOffset((double)args.NewValue);
                //dp.SetValue(ScrollViewer.HorizontalOffsetProperty, args.NewValue);
            },
        });
        public static readonly DependencyProperty AnimatedVerticalOffsetProperty = DependencyProperty.RegisterAttached("AnimatedVerticalOffset", typeof(double), typeof(ScrollViewer), new PropertyMetadata()
        {
            DefaultValue = 0.0,
            PropertyChangedCallback = (dp, args) =>
            {
                if (args.NewValue == args.OldValue) return;
                if (args.NewValue == null) return;
                if (dp == null || !(dp is ScrollViewer)) return;
                (dp as ScrollViewer).ScrollToVerticalOffset((double)args.NewValue);
                //dp.SetValue(ScrollViewer.HorizontalOffsetProperty, args.NewValue);
            },
        });
    }

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
            foreach (var item in AssociatedObject.Items)
            {
                var listboxitem = (ListBoxItem)AssociatedObject.ItemContainerGenerator.ContainerFromItem(item);
                if (listboxitem == null) continue;
                var storyboard = getStoryboardOfSelectedStateOfListBox(listboxitem);
                if (storyboard == null) continue;
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
                if (item == null) continue;
                var listboxitem = item as ListBoxItem;
                if (listboxitem == null) continue;
                var storyboard = getStoryboardOfSelectedStateOfListBox(listboxitem);
                if (storyboard == null) continue;
                storyboard.Completed += SelectionCompleted;
            }
            var incc = AssociatedObject.ItemsSource as INotifyCollectionChanged;
            if (incc != null)
                incc.CollectionChanged += incc_CollectionChanged;
        }

        private void incc_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;
            foreach (var col_item in e.NewItems)
            {
                var item = AssociatedObject.ItemContainerGenerator.ContainerFromItem(col_item);
                if (item == null) continue;
                var listboxitem = item as ListBoxItem;
                if (listboxitem == null) continue;
                if (listboxitem.IsLoaded)
                {
                    if (listboxitem.IsVisible)
                    {
                        var storyboard = getStoryboardOfSelectedStateOfListBox(listboxitem);
                        if (storyboard != null)
                            storyboard.Completed += SelectionCompleted;
                    }
                    listboxitem.IsVisibleChanged += (o, args) =>
                    {
                        if (!Convert.ToBoolean(args.NewValue)) return;
                        var storyboard = getStoryboardOfSelectedStateOfListBox(listboxitem);
                        if (storyboard == null) return;
                        storyboard.Completed -= SelectionCompleted;
                        storyboard.Completed += SelectionCompleted;
                    };
                }
                else
                {
                    listboxitem.Loaded += (o, args) =>
                    {
                        if (listboxitem.IsVisible)
                        {
                            var storyboard = getStoryboardOfSelectedStateOfListBox(listboxitem);
                            if (storyboard != null)
                                storyboard.Completed += SelectionCompleted;
                        }
                        listboxitem.IsVisibleChanged += (_o, _args) =>
                        {
                            if (!Convert.ToBoolean(_args.NewValue)) return;
                            var storyboard = getStoryboardOfSelectedStateOfListBox(listboxitem);
                            if (storyboard == null) return;
                            storyboard.Completed -= SelectionCompleted;
                            storyboard.Completed += SelectionCompleted;
                        };
                    };
                }
            }
        }

        private void SelectionCompleted(object sender, EventArgs e)
        {
            var listbox = AssociatedObject;
            if(listbox.SelectedItem == null) return;
            if(listbox.SelectedIndex + 1 < listbox.Items.Count)
            {
                ScrollViewer scrollViewer = getFirstChildOfType<ScrollViewer>(listbox);
                if (scrollViewer == null) return;
                UIElement ancestorPanel = getFirstChildOfType<WrapPanel>(scrollViewer);
                var itemPoint = (listbox.ItemContainerGenerator.ContainerFromIndex(listbox.SelectedIndex) as UIElement).TranslatePoint(new Point(0,0), ancestorPanel);
                double startOffsetX = scrollViewer.HorizontalOffset;
                double startOffsetY = scrollViewer.VerticalOffset;
                double newOffsetX = itemPoint.X;
                double newOffsetY = itemPoint.Y;
                //scrollViewer.ScrollToHorizontalOffset(itemPoint.X);
                scrollViewer.BeginAnimation(AnimatedOffsets.AnimatedHorizontalOffsetProperty, new DoubleAnimation()
                {
                    From = startOffsetX,
                    To = newOffsetX,
                    Duration = new Duration(new TimeSpan(0, 0, 0, 0, 300)),
                });
                scrollViewer.BeginAnimation(AnimatedOffsets.AnimatedVerticalOffsetProperty, new DoubleAnimation()
                {
                    From = startOffsetY,
                    To = newOffsetY,
                    Duration = new Duration(new TimeSpan(0, 0, 0, 0, 300)),
                });
                //listbox.ScrollIntoView(listbox.Items[listbox.SelectedIndex + 1]);
            }
            else
            {
                listbox.ScrollIntoView(listbox.SelectedItem);
            }
        }

        private T getFirstChildOfType<T>(DependencyObject ancestorDO) where T: DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(ancestorDO);
            for(int i = 0; i < childCount; i++)
            {
                DependencyObject _do = VisualTreeHelper.GetChild(ancestorDO, i);
                if(_do is T)
                {
                    return _do as T;
                }
                T _doChild = getFirstChildOfType<T>(_do);
                if (_doChild != null) return _doChild;
            }
            return null;
        }

        private Storyboard getStoryboardOfSelectedStateOfListBox(ListBoxItem listboxitem)
        {
            VisualState selectedState = null;
            VisualStateGroup selectionGroup = null;
            var element = (Border)listboxitem.Template.FindName("borderItem", listboxitem);
            if (element == null) return null;
            var groups = VisualStateManager.GetVisualStateGroups(element);
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

    public class UseHorizontalScroll : Behavior<ScrollViewer>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            if(AssociatedObject == null) return;
            AssociatedObject.PreviewMouseWheel += mouseWheelCallback;
        }

        private void mouseWheelCallback(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            IEnumerable<ScrollViewer> scrollviewers = getAllVisualElementsByType<ScrollViewer>(AssociatedObject);
            foreach(var scrolviewer in scrollviewers)
            {
                if (scrolviewer.IsMouseOver) return;
            }
            AssociatedObject.ScrollToHorizontalOffset(AssociatedObject.HorizontalOffset - e.Delta);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject == null) return;
            AssociatedObject.PreviewMouseWheel -= mouseWheelCallback;
        }

        private static IEnumerable<T> getAllVisualElementsByType<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            List<T> children = new List<T>();
            if (dependencyObject == null) return null;

            for(int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                var rawChild = VisualTreeHelper.GetChild(dependencyObject, i);
                if(rawChild is T)
                {
                    children.Add((T)rawChild);
                }
                children.AddRange(getAllVisualElementsByType<T>(rawChild));
            }
            return children;
        }
    }

    public class CopyToClipboardOnClick : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotFocus += clickToCopyToClipboard;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.GotFocus -= clickToCopyToClipboard;
        }

        private void clickToCopyToClipboard(object sender, RoutedEventArgs eventArgs)
        {
            Clipboard.SetDataObject(AssociatedObject.Text);
        }
    }
}

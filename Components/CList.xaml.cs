using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Launcher.Components
{
    #region Enums
    #region EListControl
    public enum EListControl
    {
        Exception,
        FailLoadItems
    }
    #endregion
    #region ELoadItems
    public enum ELoadItems
    {
        Exception,
        ItemsCountIsZero
    }
    #endregion
    #endregion

    /// <summary>
    /// Логика взаимодействия для CList.xaml
    /// </summary>
    public partial class CList : UserControl, ITranslatable
    {
        public CList()
        {
            InitializeComponent();

            TranslationHub.Register(this);
            DataContext = this;
            this.MouseLeave += EMouseLeave;
            this.PreviewMouseLeftButtonDown += ComponentLeftDown;
        }

        #region Делегаты
        public delegate void NewSelectedItemDelegate(CListItem item);
        #endregion
        #region События
        public event NewSelectedItemDelegate NewSelectedItem;
        #endregion


        #region Классы
        #region CListItem
        public class CListItem(long id, string name)
        {
            public long Id { get; set; } = id;
            public string Name { get; set; } = name;
        }
        #endregion
        #endregion

        #region Переменные
        public bool IsEnabledLeaveHide { get; set; } = true;
        public bool IsEnabledSameSelect { get; set; } = false;
        private bool IsStateChanging { get; set; } = false;
        private bool IsNewSelected { get; set; } = false;
        private LogBox Pref { get; set; } = new("List");
        public List<CListItem> Items { get; set; } = new();
        public List<CListItem> ShowenItems { get; set; } = new();
        private Dictionary<Grid, CListItem> ItemsComponents { get; set; } = [];
        private Dictionary<Grid, CScrollingTextBlock> ItemsScrollingComponents { get; set; } = [];
        #endregion

        #region Параметры     
        static readonly Type ElementType = typeof(CList);
        #region ElementHeight
        public double ElementHeight
        {
            get { return (double)GetValue(ElementHeightProperty); }
            set { SetValue(ElementHeightProperty, value); }
        }
        public static readonly DependencyProperty ElementHeightProperty =
            DependencyProperty.Register(nameof(ElementHeight), typeof(double), ElementType,
                new PropertyMetadata((double)50));
        #endregion    
        #region BorderRadius
        public double BorderRadius
        {
            get { return (double)GetValue(BorderRadiusProperty); }
            set { SetValue(BorderRadiusProperty, value); }
        }
        public static readonly DependencyProperty BorderRadiusProperty =
            DependencyProperty.Register(nameof(BorderRadius), typeof(double), ElementType,
                new PropertyMetadata((double)3));
        #endregion 
        #region SelectorColor
        public Brush SelectorColor
        {
            get { return (Brush)GetValue(SelectorColorProperty); }
            set { SetValue(SelectorColorProperty, value); }
        }
        public static readonly DependencyProperty SelectorColorProperty =
            DependencyProperty.Register(nameof(SelectorColor), typeof(Brush), ElementType,
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb((byte)(255 * 0.1), 255, 255, 255))));
        #endregion
        #region Background
        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }
        public static new readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register(nameof(Background), typeof(Brush), ElementType,
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb((byte)(255 * 0.1), 255, 255, 255))));
        #endregion
        #region Foreground
        public new Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }
        public static new readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register(nameof(Foreground), typeof(Brush), ElementType,
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))));
        #endregion
        #region IsOpened
        public bool IsOpened
        {
            get { return (bool)GetValue(IsOpenedProperty); }
            set { SetValue(IsOpenedProperty, value); }
        }
        public static readonly DependencyProperty IsOpenedProperty =
            DependencyProperty.Register(nameof(IsOpened), typeof(bool), ElementType,
                new PropertyMetadata(false, OnOpenedChanged));

        private static void OnOpenedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CList)d;
            _ = control.ChangeState((bool)e.NewValue);
        }
        #endregion
        #region IsSelectedItem
        public CListItem? SelectedItem
        {
            get { return (CListItem)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(CListItem), ElementType,
                new PropertyMetadata(null, OnSelectedItemChanged));

        private static async void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CList)d;

            control.IsNewSelected = true;
            await control.ChangeState(false);
            control.UpdateViewModel();
            control.IsNewSelected = false;

            if (e.OldValue is not null) control.NewSelectedItem?.Invoke((CListItem)e.NewValue);
        }
        #endregion
        #endregion

        #region События
        #region ItemMouseEnter
        private void ItemMouseEnter(object sender, MouseEventArgs e)
        {
            var control = (Grid)sender;
            var index = items.Children.IndexOf(control);
            _ = MoveSelector(index);
        }
        #endregion
        #region ItemClick
        private void ItemClick(object sender, MouseButtonEventArgs e)
        {
            var control = (Grid)sender;
            var index = items.Children.IndexOf(control);

            if (index is 0 && !IsEnabledSameSelect)
            {
                _ = ChangeState(!IsOpened);
                return;
            }

            var getItem = ItemsComponents.TryGetValue(control, out CListItem? item);
            if (getItem)
            {
                SelectedItem = item!;
                if (item == SelectedItem && IsEnabledSameSelect) NewSelectedItem?.Invoke(SelectedItem);
            }
        }
        #endregion
        #region ComponentLeftDown
        private void ComponentLeftDown(object sender, MouseButtonEventArgs e)
        {
            //_ = ChangeState(!IsOpened);
        }
        #endregion
        #region ELostMouseCapture
        //private void ELostMouseCapture(object sender, MouseEventArgs e) => _ = ChangeState(false);
        #endregion       
        #region EMouseLeave
        private void EMouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsNewSelected)
                if (IsEnabledLeaveHide) _ = ChangeState(false);
                else _ = MoveSelector(0);
        }
        #endregion
        #endregion

        #region Функции
        #region SelectItemById
        public void SelectItemById(int id)
        {
            var item = ShowenItems.FirstOrDefault(x => x.Id == id);
            if (item is null) return;

            SelectedItem = item;
        }
        #endregion
        #region LoadItems
        public async Task<UResponse> LoadItems(List<CListItem> items, int selectedIndex = 0)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName()).AddTrace("item");
            var _failinf = $"Не удалось загрузить элементы";

            #region try
            try
            {
                #region Проверка на количество
                if (items.Count is 0)
                {
                    throw new UExcept(ELoadItems.ItemsCountIsZero, $"Список должен состоять хотя бы с одного элемента");
                }
                #endregion
                #region Установка
                Items = items;
                ShowenItems = [];
                var list = new List<CListItem>();
                foreach (var item in Items) ShowenItems.Add(new CListItem(item.Id, Dictionary.Translate(item.Name)));
                #endregion
                #region Выбор первого элемента
                SelectedItem = selectedIndex >= 0 && selectedIndex < ShowenItems.Count ? Items[selectedIndex] : Items[0];
                #endregion

                return new() { IsSuccess = true };
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                Functions.Error(ex, _failinf, _proc);
                return new(ex.Error);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uerror = new UError(ELoadItems.Exception, $"Исключение: {ex.Message}");
                Functions.Error(ex, uerror, $"{_failinf}: исключение", _proc);
                return new(uerror);
            }
            #endregion
        }
        public async Task<UResponse> LoadItems(List<string> items)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName()).AddTrace("string");
            var _failinf = $"Не удалось загрузить элементы";

            #region try
            try
            {
                #region Проверка на количество
                if (items.Count is 0)
                {
                    throw new UExcept(ELoadItems.ItemsCountIsZero, $"Список должен состоять хотя бы с одного элемента");
                }
                #endregion
                #region Установка
                Items = new();
                ShowenItems = new();
                for (int i = 0; i < items.Count; i++)
                {
                    Items.Add(new(i, items[i]));
                    ShowenItems.Add(new(i, Dictionary.Translate(items[i])));
                }
                #endregion
                #region Выбор первого элемента
                SelectedItem = Items[0];
                #endregion

                return new() { IsSuccess = true };
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                Functions.Error(ex, _failinf, _proc);
                return new(ex.Error);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uerror = new UError(ELoadItems.Exception, $"Исключение: {ex.Message}");
                Functions.Error(ex, uerror, $"{_failinf}: исключение", _proc);
                return new(uerror);
            }
            #endregion
        }
        #endregion
        #region UpdateViewModel
        private void UpdateViewModel()
        {
            ItemsComponents = [];
            ItemsScrollingComponents = [];
            items.Children.Clear();

            var first = ShowenItems.First(x => x.Id == SelectedItem!.Id);
            var selectedItemComponent = CreateItemObject(first);
            items.Children.Add(selectedItemComponent.Item1);
            ItemsComponents.Add(selectedItemComponent.Item1, SelectedItem!);
            ItemsScrollingComponents.Add(selectedItemComponent.Item1, selectedItemComponent.Item2);

            foreach (var item in ShowenItems)
            {
                if (item.Id != SelectedItem!.Id)
                {
                    var itemComponent = CreateItemObject(item);
                    items.Children.Add(itemComponent.Item1);
                    ItemsComponents.Add(itemComponent.Item1, item);
                    ItemsScrollingComponents.Add(itemComponent.Item1, itemComponent.Item2);
                }
            }
        }
        #endregion
        #region CreateItemObject
        private (Grid, CScrollingTextBlock) CreateItemObject(CListItem item)
        {
            Grid grid = new Grid();
            grid.DataContext = this;

            CScrollingTextBlock textBlock = new CScrollingTextBlock()
            {
                Text = item.Name,
                FontSize = 20,
                FontWeight = this.FontWeight,
                Foreground = this.Foreground,
                ScrollingType = EScrollingType.Scrolling,
                Padding = new Thickness(10, 0, 10, 0)
            };

            #region Настройка высоты
            Binding heightBinding = new("ElementHeight")
            {
                RelativeSource = new RelativeSource
                {
                    AncestorType = typeof(CList)
                }
            };
            textBlock.SetBinding(FrameworkElement.HeightProperty, heightBinding);
            grid.SetBinding(FrameworkElement.HeightProperty, heightBinding);
            #endregion
            #region Для выбора по площади
            Rectangle background = new() { Fill = new SolidColorBrush(Colors.White), Opacity = 0 };
            grid.Children.Add(background);
            #endregion
            #region Стили
            #region Foreground
            Binding foregroundBinding = new("Foreground") { ElementName = "root" };
            textBlock.SetBinding(CScrollingTextBlock.ForegroundProperty, foregroundBinding);
            #endregion           
            #region FontFamily
            Binding fontFamilyBinding = new("FontFamily") { ElementName = "root" };
            textBlock.SetBinding(CScrollingTextBlock.FontFamilyProperty, fontFamilyBinding);
            #endregion
            #endregion
            #region Добавление
            grid.Children.Add(textBlock);
            #endregion

            grid.MouseEnter += ItemMouseEnter;
            grid.PreviewMouseLeftButtonDown += ItemClick;

            return new(grid, textBlock);
        }
        #endregion
        #region ChangeState
        public async Task ChangeState(bool open)
        {
            var duration = TimeSpan.FromMilliseconds(200);
            var function = new PowerEase() { EasingMode = EasingMode.EaseInOut };
            var storyboard = new Storyboard();

            IsStateChanging = true;

            if (open)
            {
                var animationHeight = new DoubleAnimation(this.ActualHeight, items.ActualHeight is 0 ? ShowenItems.Count * ElementHeight : items.ActualHeight, duration) { EasingFunction = function };
                Storyboard.SetTarget(animationHeight, this);
                Storyboard.SetTargetProperty(animationHeight, new PropertyPath(HeightProperty));
                storyboard.Children.Add(animationHeight);

                foreach (var scrollingTextBlock in ItemsScrollingComponents)
                {
                    var textBlock = scrollingTextBlock.Value;
                    textBlock.StartScrolling();
                }
            }
            else
            {
                var animationHeight = new DoubleAnimation(this.ActualHeight, ElementHeight, duration) { EasingFunction = function };
                Storyboard.SetTarget(animationHeight, this);
                Storyboard.SetTargetProperty(animationHeight, new PropertyPath(HeightProperty));
                storyboard.Children.Add(animationHeight);
                _ = MoveSelector(0);

                foreach (var scrollingTextBlock in ItemsScrollingComponents)
                {
                    var textBlock = scrollingTextBlock.Value;
                    textBlock.StopScrolling();
                }
            }

            IsOpened = open;

            storyboard.Completed += (sender, e) => { IsStateChanging = false; };

            storyboard.Begin(this, HandoffBehavior.SnapshotAndReplace, true);

            await Task.Run(() => { Thread.Sleep(duration); });
        }
        #endregion
        #region MoveSelector
        private async Task MoveSelector(int index)
        {
            var duration = AnimationHelper.AnimationDuration;
            var function = new PowerEase() { EasingMode = EasingMode.EaseInOut };

            var topMargin = index * ElementHeight;
            var newMargin = new Thickness(0, topMargin, 0, 0);

            var storyboard = new Storyboard();

            var animationMargin = new ThicknessAnimation(selector.Margin, newMargin, duration) { EasingFunction = function };
            Storyboard.SetTarget(animationMargin, selector);
            Storyboard.SetTargetProperty(animationMargin, new PropertyPath(Rectangle.MarginProperty));
            storyboard.Children.Add(animationMargin);

            storyboard.Begin(selector, HandoffBehavior.SnapshotAndReplace, true);

            await Task.Run(() => Thread.Sleep(duration));
        }
        #endregion
        #region UpdateAllValues
        public async Task UpdateAllValues()
        {
            #region Настройка 
            if (Items is not null)
            {
                foreach (var item in Items)
                {
                    var fromShowen = ShowenItems.FirstOrDefault(x => x.Id == item.Id);
                    if (fromShowen is not null) { fromShowen.Name = Dictionary.Translate(item.Name); }
                }
                UpdateViewModel();
            }
            #endregion
        }
        #endregion
        #endregion
    }
}

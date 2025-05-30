using Byster.Models.BysterModels.Settings;
using Byster.Views.ModelsTemp;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для ClientsListItem.xaml
    /// </summary>
    public partial class ClientsListItem : UserControl
    {
        public ClientsListItem()
        {
            InitializeComponent();
            
            this.MouseEnter += FMouseEnter;
            this.MouseLeave += FMouseLeave;
            removeButton.Click += FRemoveClient;
            gridMain.MouseLeftButtonDown += FSelectClient;
        }


        #region События
        public event EventHandler RemoveClientRequest;
        public event EventHandler SelectClientRequest;
        #endregion


        #region Свойства
        #region IsSelected
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(ClientsListItem),
                new PropertyMetadata(false, OnIsSelectedChanged));

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ClientsListItem)d;
            control.background.Opacity = (bool)e.NewValue ? 0.06 : 0.02;
        }
        #endregion
        #region Client
        public ClientModel Client
        {
            get => (ClientModel)GetValue(ClientProperty);
            set => SetValue(ClientProperty, value);
        }

        public static readonly DependencyProperty ClientProperty =
            DependencyProperty.Register(nameof(Client), typeof(ClientModel), typeof(ClientsListItem),
                new PropertyMetadata(null));
        #endregion
        #endregion

        #region Обработчики событий
        private void FMouseEnter(object sender, MouseEventArgs e) 
        { 
            removeButton.Visibility = Visibility.Visible;
            background.Opacity = 0.05;
        }
        private void FMouseLeave(object sender, MouseEventArgs e)
        { 
            removeButton.Visibility = Visibility.Collapsed;
            background.Opacity = IsSelected ? 0.06 : 0.02;
        }
        private void FRemoveClient(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            RemoveClientRequest?.Invoke(this, EventArgs.Empty);
        }
        private void FSelectClient(object sender, MouseButtonEventArgs e)
        {
            SelectClientRequest?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}

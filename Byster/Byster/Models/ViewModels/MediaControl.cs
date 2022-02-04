using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Reflection;
using Byster.Models.Utilities;
using Byster.Models.BysterModels;

namespace Byster.Models.ViewModels
{
    public class MediaControl : Control
    {
        public static readonly DependencyProperty MediaElementProperty;
        public static Action<string> OpenAction;

        static MediaControl()
        {
            OpenAction = (uri) =>
            {
                MessageBox.Show(uri, "Действие не определено");
            };
            MediaElementProperty = DependencyProperty.Register("MediaElement", typeof(Image), typeof(MediaControl), new PropertyMetadata()
            {
                DefaultValue = null,
                PropertyChangedCallback = (sender, e) =>
                {
                    if (e.NewValue == null || e.NewValue == e.OldValue) return;
                    var mediaControl = sender as MediaControl;
                    var mediaElement = e.NewValue as Image;
                },
            });
        }

        public Image MediaElement
        {
            get
            {
                return (Image)GetValue(MediaElementProperty);
            }
            set
            {
                SetValue(MediaElementProperty, value);
            }
        }

        private RelayCommand openCommand;
        public RelayCommand OpenCommand
        {
            get
            {
                return openCommand ?? (openCommand = new RelayCommand(() =>
                {
                   OpenAction?.Invoke((MediaElement.DataContext as Byster.Models.BysterModels.Media).ImageItem.PathOfCurrentLocalSource == "/Resources/Images/video-placeholder.png" ? (MediaElement.DataContext as Byster.Models.BysterModels.Media).ImageItem.PathOfNetworkSource : (MediaElement.DataContext as Byster.Models.BysterModels.Media).ImageItem.PathOfCurrentLocalSource);
                }));
            }
        }
    }





    public class MediaPresenterControl : Control
    {
        public static readonly DependencyProperty ControlVisibilityProperty;
        public static readonly DependencyProperty PlayerProperty;
        static MediaPresenterControl()
        {
            PlayerProperty = DependencyProperty.Register("Player", typeof(MediaElement), typeof(MediaPresenterControl), new PropertyMetadata()
            {
                DefaultValue = null,
                PropertyChangedCallback = propertyChangedCallback,
            });

            ControlVisibilityProperty = DependencyProperty.Register("ControlVisibility", typeof(Visibility), typeof(MediaPresenterControl), new PropertyMetadata()
            {
                DefaultValue = Visibility.Visible,
            });
        }

        private static void propertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue || e.NewValue == null) return;
            MediaPresenterControl mediaPresenterControl = (MediaPresenterControl)sender;
            MediaElement mediaElement = (MediaElement)e.NewValue;
            Binding.AddSourceUpdatedHandler(mediaElement, (mediaElementSender, sce) =>
            {
                if (sce.Property != MediaElement.SourceProperty) return;
                try
                {
                    string source = mediaElement.Source.AbsolutePath;
                    mediaPresenterControl.ControlVisibility = source.EndsWith(".mp4") ? Visibility.Visible : Visibility.Collapsed;
                }
                catch
                {
                    mediaPresenterControl.ControlVisibility = Visibility.Collapsed;
                }
                mediaElement.Play();
            });
            if(e.OldValue != null) { }
        }

        public MediaElement Player
        {
            get
            {
                return (MediaElement)GetValue(PlayerProperty);
            }
            set
            {
                SetValue(PlayerProperty, value);
            }
        }

        public Visibility ControlVisibility
        {
            get
            {
                return (Visibility)GetValue(ControlVisibilityProperty);
            }
            set
            {
                SetValue(ControlVisibilityProperty, value);
            }
        }
    }
}

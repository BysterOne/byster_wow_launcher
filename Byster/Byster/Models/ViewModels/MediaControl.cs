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
            MediaElementProperty = DependencyProperty.Register("MediaElement", typeof(MediaElement), typeof(MediaControl), new PropertyMetadata()
            {
                DefaultValue = null,
                PropertyChangedCallback = (sender, e) =>
                {
                    if (e.NewValue == null || e.NewValue == e.OldValue) return;
                    var mediaControl = sender as MediaControl;
                    var mediaElement = e.NewValue as MediaElement;
                    mediaElement.MediaOpened += (elementSender, args) =>
                    {
                        mediaElement.LoadedBehavior = MediaState.Manual;
                        mediaElement.Play();
                        mediaElement.Pause();
                    };
                },
            });
        }

        public MediaElement MediaElement
        {
            get
            {
                return (MediaElement)GetValue(MediaElementProperty);
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
                    OpenAction?.Invoke(MediaElement?.Source?.AbsoluteUri ?? "");
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
            if (e.NewValue == null || e.NewValue == e.OldValue) return;
            var player = (MediaElement)e.NewValue;
            if (!string.IsNullOrEmpty(player.Source?.AbsolutePath ?? null) || (player.Source?.AbsolutePath.ToLower().EndsWith(".mp4") ?? false)) (sender as MediaPresenterControl).ControlVisibility = Visibility.Visible;
            else (sender as MediaPresenterControl).ControlVisibility = Visibility.Collapsed;
            Binding.AddTargetUpdatedHandler(player, (playerSender, args) =>
            {
                if (args.Property != MediaElement.SourceProperty) return;
                try
                {
                    (sender as MediaPresenterControl).ControlVisibility = (!string.IsNullOrEmpty(player.Source?.AbsolutePath ?? "") && (player.Source?.AbsoluteUri.ToLower().EndsWith(".mp4") ?? false)) ? Visibility.Visible : Visibility.Hidden;
                    player.Play();
                }
                catch
                {
                    (sender as MediaPresenterControl).ControlVisibility = Visibility.Hidden;
                }
            });
            Binding.RemoveTargetUpdatedHandler(player, (playerSender, args) =>
            {
                if (args.Property != MediaElement.SourceProperty) return;
                try
                {
                    (sender as MediaPresenterControl).ControlVisibility = (!string.IsNullOrEmpty(player.Source?.AbsolutePath ?? "") && (player.Source?.AbsoluteUri.ToLower().EndsWith(".mp4") ?? false)) ? Visibility.Visible : Visibility.Hidden;
                }
                catch
                {
                    (sender as MediaPresenterControl).ControlVisibility = Visibility.Hidden;
                }
            });
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

        private RelayCommand playCommand;
        private RelayCommand pauseCommand;
        public RelayCommand PlayCommand
        {
            get {
                return playCommand ?? (playCommand = new RelayCommand(() =>
                {
                    Player.LoadedBehavior = MediaState.Manual;
                    Player?.Play();
                }));

            }
        }

        public RelayCommand PauseCommand
        {
            get
            {
                return pauseCommand ?? (pauseCommand = new RelayCommand(() =>
                {
                    Player.LoadedBehavior = MediaState.Manual;
                    Player?.Pause();
                }));
            }
        }

    }
}

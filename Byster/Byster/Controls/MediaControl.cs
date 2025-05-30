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
using System.Runtime.CompilerServices;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Byster.Models.ViewModels
{
    public class MediaControl : Control
    {
        public static readonly DependencyProperty MediaElementProperty;
        public static Action<Media> OpenAction;

        static MediaControl()
        {
            OpenAction = (media) =>
            {
                MessageBox.Show(media.Uri, "Действие не определено");
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

        public Media SourceMedia
        {
            get => (MediaElement?.DataContext ?? null) as Media;
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
                    OpenAction?.Invoke(SourceMedia);
                }));
            }
        }
    }





    public class MediaPresenterControl : Control, INotifyPropertyChanged
    {
        private static readonly string[] videoExtens = { ".mp4", ".avi", ".mkv" };
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public static readonly DependencyProperty ControlsPanelProperty;
        public static readonly DependencyProperty VideoPlayerProperty;
        public static readonly DependencyProperty ImagePlayerProperty;
        public static readonly DependencyProperty MediaPresenterVisibilityProperty;
        public static readonly DependencyProperty VideoPositionProperty;
        public static readonly DependencyProperty VideoDurationProperty;
        public static readonly DependencyProperty PositionControlVisibilityProperty;
        static MediaPresenterControl()
        {
            VideoPlayerProperty = DependencyProperty.Register("VideoPlayer", typeof(MediaElement), typeof(MediaPresenterControl), new PropertyMetadata()
            {
                DefaultValue = null,
                PropertyChangedCallback = videoplayerpropertyChangedCallback,
            });

            ImagePlayerProperty = DependencyProperty.Register("ImagePlayer", typeof(Image), typeof(MediaPresenterControl), new PropertyMetadata()
            {
                DefaultValue = null,
                PropertyChangedCallback = imageplayerpropertyChangedCallback,
            });

            ControlsPanelProperty = DependencyProperty.Register("ControlsPanel", typeof(StackPanel), typeof(MediaPresenterControl), new PropertyMetadata()
            {
                DefaultValue = null,
            });

            MediaPresenterVisibilityProperty = DependencyProperty.Register("MediaPresenterVisibility", typeof(Visibility), typeof(MediaPresenterControl), new PropertyMetadata()
            {
                DefaultValue = Visibility.Collapsed,
            });

            VideoPositionProperty = DependencyProperty.Register("VideoPosition", typeof(double), typeof(MediaPresenterControl), new PropertyMetadata()
            {
                DefaultValue = 0.0,
                PropertyChangedCallback = videopositionpropertyChangedCallback,
            });

            VideoDurationProperty = DependencyProperty.Register("VideoDuration", typeof(double), typeof(MediaPresenterControl), new PropertyMetadata()
            {
                DefaultValue = 0.0,
            });
            PositionControlVisibilityProperty = DependencyProperty.Register("PositionControlVisibility", typeof(Visibility), typeof(MediaPresenterControl), new PropertyMetadata()
            {
                DefaultValue = Visibility.Hidden,
            });
        }

        public MediaPresenterControl() : base()
        {
            PropertyChanged += sourceChangedCallback;
        }

        private void sourceChangedCallback(object sender, PropertyChangedEventArgs e)
        {

        }

        public void PlayMedia(Media media)
        {
            if (media == null) return;
            if (VideoPlayer.Visibility == Visibility.Visible) VideoPlayer.Close();
            VideoPlayer.Visibility = Visibility.Collapsed;
            ImagePlayer.Visibility = Visibility.Collapsed;
            ControlsPanel.Visibility = Visibility.Collapsed;

            string url;
            if (Path.GetFileName(media.ImageItem.PathOfCurrentLocalSource).ToLower().Contains("placeholder"))
            {
                url = media.ImageItem.PathOfNetworkSource;
            }
            else
            {
                url = media.ImageItem.PathOfCurrentLocalSource;
            }
            string ext = Path.GetExtension(url);
            Source = media;
            MediaPresenterVisibility = Visibility.Visible;
            if (videoExtens.Contains(ext))
            {
                VideoPlayer.Visibility = Visibility.Visible;
                ImagePlayer.Visibility = Visibility.Collapsed;
                ControlsPanel.Visibility = Visibility.Visible;

                VideoPlayer.Source = new Uri(url);
                VideoPlayer.Position = new TimeSpan(0, 0, 0);
                VideoPlayer.Play();
            }
            else
            {
                VideoPlayer.Visibility = Visibility.Collapsed;
                ImagePlayer.Visibility = Visibility.Visible;
                ControlsPanel.Visibility = Visibility.Collapsed;
                ImagePlayer.Source = new BitmapImage(new Uri(url));
            }
        }

        private RelayCommand forwardVideoCommand;
        public RelayCommand ForwardVideoCommand
        {
            get => forwardVideoCommand ?? (forwardVideoCommand = new RelayCommand(() =>
            {
                if (VideoPlayer.Visibility == Visibility.Visible)
                {
                    VideoPlayer.Position = VideoPlayer.Position.Add(new TimeSpan(0, 0, 10));
                }
            }));
        }

        private RelayCommand backwardVideoCommand;
        public RelayCommand BackwardVideoCommand
        {
            get => backwardVideoCommand ?? (backwardVideoCommand = new RelayCommand(() =>
            {
                if (VideoPlayer.Visibility == Visibility.Visible)
                {
                    VideoPlayer.Position = VideoPlayer.Position.Subtract(new TimeSpan(0, 0, 10));
                }
            }));
        }

        private RelayCommand playVideoCommand;
        public RelayCommand PlayVideoCommand
        {
            get => playVideoCommand ?? (playVideoCommand = new RelayCommand(() =>
            {
                if (VideoPlayer.Visibility == Visibility.Visible)
                {
                    VideoPlayer.Play();
                }
            }));
        }

        private RelayCommand pauseVideoCommand;
        public RelayCommand PauseVideoCommand
        {
            get => pauseVideoCommand ?? (pauseVideoCommand = new RelayCommand(() =>
            {
                if (VideoPlayer.Visibility == Visibility.Visible)
                {
                    VideoPlayer.Pause();
                }
            }));
        }

        private RelayCommand closeVideoCommand;
        public RelayCommand CloseVideoCommand
        {
            get => closeVideoCommand ?? (closeVideoCommand = new RelayCommand(() =>
            {
                if (VideoPlayer.Visibility == Visibility.Visible) VideoPlayer.Close();
                MediaPresenterVisibility = Visibility.Collapsed;
            }));
        }

        private RelayCommand nextMediaCommand;
        public RelayCommand NextMediaCommand
        {
            get => nextMediaCommand ?? (nextMediaCommand = new RelayCommand(() =>
            {
                PlayMedia(Source.NextMedia);
            }, () =>
            {
                if ((Source?.NextMedia ?? null) == null) return false;
                return true;
            }));
        }

        private RelayCommand prevMediaCommand;
        public RelayCommand PrevMediaCommand
        {
            get => prevMediaCommand ?? (prevMediaCommand = new RelayCommand(() =>
            {
                PlayMedia(Source.PreviousMedia);
            }, () =>
            {
                if ((Source?.PreviousMedia ?? null) == null) return false;
                return true;
            }));
        }
        private static void videoplayerpropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue || e.NewValue == null) return;
            MediaPresenterControl mediaPresenterControl = (MediaPresenterControl)sender;
            MediaElement mediaElement = (MediaElement)e.NewValue;
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.Visibility = Visibility.Collapsed;
        }

        private static void imageplayerpropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue || e.NewValue == null) return;
            MediaPresenterControl mediaPresenterControl = (MediaPresenterControl)sender;
            Image imageElement = (Image)e.NewValue;
            imageElement.Visibility = Visibility.Collapsed;
        }

        private static void videopositionpropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue || e.NewValue == null) return;
            MediaPresenterControl mediaPresenterControl = (MediaPresenterControl)sender;
            if (mediaPresenterControl.VideoPlayer == null) return;
            if (mediaPresenterControl.VideoPlayer.Visibility != Visibility.Visible) return;
            mediaPresenterControl.VideoPlayer.Position = new TimeSpan(Convert.ToInt64(e.NewValue));
        }

        public MediaElement VideoPlayer
        {
            get
            {
                return (MediaElement)GetValue(VideoPlayerProperty);
            }
            set
            {
                SetValue(VideoPlayerProperty, value);
            }
        }

        public Image ImagePlayer
        {
            get
            {
                return (Image)GetValue(ImagePlayerProperty);
            }
            set
            {
                SetValue(ImagePlayerProperty, value);
            }
        }

        public StackPanel ControlsPanel
        {
            get
            {
                return (StackPanel)GetValue(ControlsPanelProperty);
            }
            set
            {
                SetValue(ControlsPanelProperty, value);
            }
        }
        public Visibility MediaPresenterVisibility
        {
            get
            {
                return (Visibility)GetValue(MediaPresenterVisibilityProperty);
            }
            set
            {
                SetValue(MediaPresenterVisibilityProperty, value);
            }
        }

        public double VideoPosition
        {
            get
            {
                return (double)GetValue(VideoPositionProperty);
            }
            set
            {
                SetValue(VideoPositionProperty, value);
            }
        }
        public double VideoDuration
        {
            get
            {
                return (double)GetValue(VideoDurationProperty);
            }
            set
            {
                SetValue(VideoDurationProperty, value);
            }
        }

        public Visibility PositionControlVisibility
        {
            get
            {
                return (Visibility)GetValue(PositionControlVisibilityProperty);
            }
            set
            {
                SetValue(PositionControlVisibilityProperty, value);
            }
        }

        private Media source;
        public Media Source
        {
            get => source;
            set
            {
                source = value;
                OnPropertyChanged("Source");
            }
        }
    }
}

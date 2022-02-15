using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Ink;
using System.Windows.Shapes;
using System.Drawing;
using ZXing.QrCode;
using ZXing.Presentation;
using ZXing;
using ZXing.Common;
using System.IO;
using System.Diagnostics;
using Byster.Models.BysterModels;


namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для LinkPresenterWindow.xaml
    /// </summary>
    public partial class LinkPresenterWindow : Window
    {
        private string linkToPay;
        public LinkPresenterWindow(MainWindowViewModel mainViewModel, PaymentSystem paymentSystem, string link)
        {
            InitializeComponent();
            this.Title = "Оплата";
            titleTextBlock.Text = $"{paymentSystem?.Name ?? "Не указано"} - {mainViewModel.Shop.ResultSum} {mainViewModel.UserInfo.Currency}";
            linkToPay = link;
            infoTextBlock.Text = paymentSystem?.Description ?? "";
            QRCodeWriter writer = new QRCodeWriter();
            BitMatrix bitMatrix = writer.encode(link, BarcodeFormat.QR_CODE, 300, 300);
            Bitmap bitmap = new Bitmap(300, 300);

            for (int i = 0; i < 300; i++)
            {
                for(int j = 0; j < 300; j++)
                {
                    if(bitMatrix[i, j])
                    {
                        bitmap.SetPixel(i, j, System.Drawing.Color.Black);
                    }
                    else
                    {
                        bitmap.SetPixel(i, j, System.Drawing.Color.White);
                    }
                }
            }
            string root = System.IO.Path.GetTempPath() + "\\BysterQRCodes\\";
            if(!Directory.Exists(root)) Directory.CreateDirectory(root);
            int k = 0;
            while (File.Exists(root + "qrCode" + k + ".bmp")) k++;
            bitmap.Save(root + "qrCode" + k + ".bmp");
            linkImagePresenter.Source = new BitmapImage(new Uri(root + "qrCode" + k + ".bmp"));
            this.Closed += (obj, e) =>
            {
                try { File.Delete(root + "qrCode" + k + ".bmp"); }
                catch { }
            };
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void linkBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(linkToPay);
        }
        private void infoTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (infoTextBlock.IsVisible)
            {
                this.Height = 20 + 300 + infoTextBlock.ActualHeight + 37 + 44;
            }
        }
    }
}
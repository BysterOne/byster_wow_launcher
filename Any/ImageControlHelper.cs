using Cls;
using Launcher.Settings;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Launcher.Any
{
    public class ImageControlHelper
    {
        public static string ImagesFolder => Path.Combine(AppSettings.CacheFolder, "images");

        static ImageControlHelper()
        {
            if (!Directory.Exists(ImagesFolder))
                Directory.CreateDirectory(ImagesFolder);
        }

        #region Переменные
        public static LogBox Pref { get; } = new("Image Control Helper");
        #endregion

        #region GetImageFromCache
        public static BitmapSource? GetImageFromCache(string imageUrl, int targetWidth, int targetHeight)
        {
            string cacheFile = Path.Combine(ImagesFolder, GetCacheFileName(imageUrl, targetWidth, targetHeight));
            if (File.Exists(cacheFile)) return LoadFromFile(cacheFile);
            return null;
        }
        #endregion
        #region LoadImageAsync
        public static async Task LoadImageAsync(string imageUrl, int targetWidth, int targetHeight, CancellationToken cancellationToken, Action<ImageSource>? onLoaded = null)
        {
            string cacheFile = Path.Combine(ImagesFolder, GetCacheFileName(imageUrl, targetWidth, targetHeight));

            BitmapSource result;

            if (File.Exists(cacheFile))
            {
                result = LoadFromFile(cacheFile);
            }
            else
            {
                byte[] data;
                using (var client = new HttpClient())
                    data = await client.GetByteArrayAsync(imageUrl, cancellationToken);

                if (targetWidth <= 0 || targetHeight <= 0)
                {
                    using (var stream = new MemoryStream(data))
                    {
                        result = BitmapFrame.Create(
                            stream,
                            BitmapCreateOptions.IgnoreImageCache,
                            BitmapCacheOption.OnLoad
                        );
                    }
                }
                else
                {
                    var original = await LoadBitmapAsync(data, targetWidth, targetHeight);
                    result = ResizeUniformToFill(original, targetWidth, targetHeight);
                }
                
                SaveToFile(result, cacheFile);
            }

            onLoaded?.Invoke(result);
        }
        #endregion
        #region GetCacheFileName
        private static string GetCacheFileName(string url, int w, int h)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes($"{url}_{w}_{h}"));
            var sb = new StringBuilder();
            foreach (var b in hash) sb.Append(b.ToString("X2"));
            return sb + ".png";
        }
        #endregion
        #region LoadBitmapAsync
        private static Task<BitmapImage> LoadBitmapAsync(byte[] data, int targetWidth, int targetHeight)
        {
            return Task.Run(() =>
            {
                var bmp = new BitmapImage();
                using var ms = new MemoryStream(data);
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = ms;
                bmp.DecodePixelWidth = targetWidth;
                bmp.DecodePixelHeight = targetHeight;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            });
        }
        #endregion
        #region ResizeUniformToFill
        private static BitmapSource ResizeUniformToFill(BitmapSource src, int w, int h)
        {
            double scale = Math.Max((double)w / src.PixelWidth, (double)h / src.PixelHeight);
            double nw = src.PixelWidth * scale, nh = src.PixelHeight * scale;
            double x = (w - nw) / 2, y = (h - nh) / 2;

            var target = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            var dv = new System.Windows.Media.DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawImage(src, new System.Windows.Rect(x, y, nw, nh));
            }
            target.Render(dv);
            target.Freeze();
            return target;
        }
        #endregion
        #region SaveToFile
        private static void SaveToFile(BitmapSource bmp, string path)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            encoder.Save(fs);
        }
        #endregion
        #region LoadFromFile
        private static BitmapSource LoadFromFile(string path)
        {
            var bmp = new BitmapImage();
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = fs;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        #endregion
    }
}

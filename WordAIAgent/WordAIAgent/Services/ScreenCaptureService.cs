using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WordAIAgent.Services
{
    /// <summary>
    /// Захватывает экран и окно Word для передачи в vision-модель.
    /// </summary>
    public static class ScreenCaptureService
    {
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        /// <summary>
        /// Скриншот всего экрана
        /// </summary>
        public static Bitmap CaptureFullScreen()
        {
            var bounds = Screen.PrimaryScreen.Bounds;
            var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            return bmp;
        }

        /// <summary>
        /// Скриншот окна Word (только окно, без остального экрана)
        /// </summary>
        public static Bitmap CaptureWordWindow()
        {
            try
            {
                var wordApp = Globals.ThisAddIn.Application;
                // Получаем HWND окна Word
                var hwnd = new IntPtr(wordApp.ActiveWindow.Hwnd);

                if (!GetWindowRect(hwnd, out RECT rect))
                    return CaptureFullScreen();

                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;

                if (width <= 0 || height <= 0)
                    return CaptureFullScreen();

                var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                using var g = Graphics.FromImage(bmp);
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height));
                return bmp;
            }
            catch
            {
                return CaptureFullScreen();
            }
        }

        /// <summary>
        /// Конвертирует Bitmap в base64 строку для передачи в API
        /// </summary>
        public static string BitmapToBase64(Bitmap bmp, int maxWidth = 1280)
        {
            // Ресайз если слишком большой
            if (bmp.Width > maxWidth)
            {
                float ratio = (float)maxWidth / bmp.Width;
                int newHeight = (int)(bmp.Height * ratio);
                var resized = new Bitmap(bmp, new Size(maxWidth, newHeight));
                bmp.Dispose();
                bmp = resized;
            }

            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Jpeg);
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// Захватывает Word окно и возвращает base64 JPEG
        /// </summary>
        public static string CaptureWordAsBase64()
        {
            using var bmp = CaptureWordWindow();
            return BitmapToBase64(bmp);
        }

        /// <summary>
        /// Захватывает весь экран и возвращает base64 JPEG
        /// </summary>
        public static string CaptureScreenAsBase64()
        {
            using var bmp = CaptureFullScreen();
            return BitmapToBase64(bmp);
        }

        /// <summary>
        /// Сохраняет скриншот во временный файл и возвращает путь
        /// </summary>
        public static string SaveScreenshot(bool wordOnly = true)
        {
            using var bmp = wordOnly ? CaptureWordWindow() : CaptureFullScreen();
            var path = Path.Combine(Path.GetTempPath(), $"word_screen_{DateTime.Now:yyyyMMddHHmmss}.jpg");
            bmp.Save(path, ImageFormat.Jpeg);
            return path;
        }
    }
}

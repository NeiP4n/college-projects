using System;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace LabaBond1
{
    public partial class Register : Form
    {
        public Register()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
        }

        private async void Register_Load(object sender, EventArgs e)
        {
            try
            {
                // Инициализация WebView2
                var env = await CoreWebView2Environment.CreateAsync(@"D:\Учеба\Microsoft.WebView2.FixedVersionRuntime.133.0.3065.82.x86");
                await webView21.EnsureCoreWebView2Async(env);

                // Загружаем HTML из файла
                string filePath = Path.Combine(Application.StartupPath, @"Resources\Index.html");
                if (File.Exists(filePath))
                {
                    string htmlContent = File.ReadAllText(filePath, Encoding.UTF8);
                    webView21.NavigateToString(htmlContent); // WebView2 поддерживает `NavigateToString`
                }
                else
                {
                    MessageBox.Show("Файл не найден: " + filePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка инициализации WebView2: " + ex.Message);
            }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;

namespace OllamaAI
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            try
            {
                AddinLog.WriteInfo("Startup", "ThisAddIn started");
                AppDomain.CurrentDomain.UnhandledException += (s, args) =>
                {
                    var ex = args.ExceptionObject as Exception ?? new Exception("Unhandled exception");
                    AddinLog.Write("Unhandled", ex);
                };

                global::System.Windows.Forms.Application.ThreadException += (s, args) =>
                {
                    AddinLog.Write("ThreadException", args.Exception);
                };
            }
            catch
            {
            }
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region Код, автоматически созданный VSTO

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}

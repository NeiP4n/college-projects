using System.IO;
using System.Reflection;
using Microsoft.Office.Core;

namespace WordAIAgent.Ribbons
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public class MainRibbonXml : IRibbonExtensibility
    {
        public void Ribbon_Load(IRibbonUI ribbonUI)
        {
            // no-op
        }

        public void OnOpenChat(IRibbonControl control)
        {
            ThisAddIn.ToggleChatPane();
        }

        public string GetCustomUI(string ribbonID)
        {
            return ReadResourceText("WordAIAgent.Ribbons.MainRibbon.xml");
        }

        private static string ReadResourceText(string resourceName)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream(resourceName);
            if (stream == null)
                return null;

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}

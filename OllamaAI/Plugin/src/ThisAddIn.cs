using System;
using System.Runtime.InteropServices;
using Microsoft.Office.Core;
using Extensibility;

namespace OllamaWordPlugin
{
    [ComVisible(true)]
    [Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890")]
    [ProgId("OllamaWordPlugin.Connect")]
    public class Connect : IDTExtensibility2, IRibbonExtensibility
    {
        private OllamaRibbon ribbon;

        // COM lifecycle
        public void OnConnection(object Application, ext_ConnectMode ConnectMode, object AddInInst, ref Array custom)
        {
        }

        public void OnDisconnection(ext_DisconnectMode RemoveMode, ref Array custom)
        {
        }

        public void OnAddInsUpdate(ref Array custom)
        {
        }

        public void OnStartupComplete(ref Array custom)
        {
        }

        public void OnBeginShutdown(ref Array custom)
        {
        }

        // Ribbon loading
        public string GetCustomUI(string ribbonID)
        {
            if (ribbon == null)
                ribbon = new OllamaRibbon();

            return ribbon.GetCustomUI(ribbonID);
        }

        public void Ribbon_Load(IRibbonUI ribbonUI)
        {
            if (ribbon != null)
                ribbon.Ribbon_Load(ribbonUI);
        }
    }
}
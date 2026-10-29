using System;
using System.IO;
using Microsoft.Office.Core;
using Newtonsoft.Json;
using WordAIAgent.Forms;
using WordAIAgent.Models;
using WordAIAgent.Ribbons;
using WordAIAgent.Services;

namespace WordAIAgent
{
    public partial class ThisAddIn
    {
        // ── Singletons ────────────────────────────────────────────────────────
        public static AppSettings Settings { get; private set; } = new AppSettings();
        public static WordService WordService { get; private set; }
        public static OllamaService OllamaService { get; private set; }
        public static OllamaAgentService AgentService { get; private set; }
        public static DocumentAnalyzer DocumentAnalyzer { get; private set; }
        public static TemplateManager TemplateManager { get; private set; }

        // Explicitly use Microsoft.Office.Tools.CustomTaskPane to avoid ambiguity
        private Microsoft.Office.Tools.CustomTaskPane _chatPane;
        private ChatPaneHost _chatHost;

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WordAIAgent", "settings.json");

        // ── VSTO events ───────────────────────────────────────────────────────
        private void ThisAddIn_Startup(object sender, EventArgs e)
        {
            LoadSettings();
            WordService = new WordService();
            OllamaService = new OllamaService(Settings);
            AgentService = new OllamaAgentService(Settings);
            DocumentAnalyzer = new DocumentAnalyzer(WordService);
            TemplateManager = new TemplateManager();
        }

        private void ThisAddIn_Shutdown(object sender, EventArgs e)
        {
            _chatPane?.Dispose();
        }

        // ── Task pane toggle ──────────────────────────────────────────────────
        public static void ToggleChatPane()
        {
            var addIn = Globals.ThisAddIn;

            if (addIn._chatPane == null)
            {
                // AIAgentForm is a Form — wrap it in ChatPaneHost (UserControl)
                // because CustomTaskPanes.Add() requires UserControl, not Form.
                addIn._chatHost = new ChatPaneHost();

                addIn._chatPane = Globals.ThisAddIn.CustomTaskPanes.Add(
                    addIn._chatHost, "Word AI Agent");

                addIn._chatPane.Width = 1120;
                addIn._chatPane.DockPosition = MsoCTPDockPosition.msoCTPDockPositionRight;
                addIn._chatPane.Visible = true;
            }
            else
            {
                addIn._chatPane.Visible = !addIn._chatPane.Visible;
            }
        }

        // ── Settings ──────────────────────────────────────────────────────────
        public static void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                    Settings = JsonConvert.DeserializeObject<AppSettings>(
                        File.ReadAllText(SettingsPath)) ?? new AppSettings();
            }
            catch { Settings = new AppSettings(); }

            if (Settings.Normalize()) SaveSettings();
        }

        public static void SaveSettings()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
                File.WriteAllText(SettingsPath,
                    JsonConvert.SerializeObject(Settings, Formatting.Indented));
                OllamaService?.UpdateSettings(Settings);
                AgentService?.UpdateSettings(Settings);
            }
            catch { }
        }

        // ── VSTO ribbon ───────────────────────────────────────────────────────
        protected override IRibbonExtensibility CreateRibbonExtensibilityObject()
            => new MainRibbonXml();

        #region VSTO generated code
        private void InternalStartup() => Startup += ThisAddIn_Startup;
        #endregion
    }
}
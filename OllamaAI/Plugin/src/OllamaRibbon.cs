using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Office.Core;
using Word = Microsoft.Office.Interop.Word;

namespace OllamaWordPlugin
{
    [ComVisible(true)]
    public class OllamaRibbon : IRibbonExtensibility
    {
        private IRibbonUI         _ribbon;
        private OllamaService     _ollama;
        private List<OllamaModel> _models       = new List<OllamaModel>();
        private string            _selectedModel = "";
        private string            _statusLabel   = "Checking...";
        private bool              _chatOpen      = false;
        private ChatForm          _chatForm;
        private PluginSettings    _settings;

        public OllamaRibbon()
        {
            _settings = PluginSettings.Load();
            _ollama   = new OllamaService(_settings.OllamaUrl);
        }

        public string GetCustomUI(string ribbonID)
        {
            var asm    = Assembly.GetExecutingAssembly();
            var resName = "OllamaWordPlugin.RibbonUI.xml";
            Stream stream = asm.GetManifestResourceStream(resName);
            if (stream == null)
            {
                string asmDir = Path.GetDirectoryName(asm.Location);
                string xmlPath = Path.Combine(asmDir, "RibbonUI.xml");
                if (File.Exists(xmlPath))
                    stream = File.OpenRead(xmlPath);
            }
            if (stream == null)
                throw new FileNotFoundException("RibbonUI.xml not found as embedded resource or file.");
            using (stream)
            {
                return new StreamReader(stream).ReadToEnd();
            }
        }

        public void Ribbon_Load(IRibbonUI ribbon)
        {
            _ribbon = ribbon;
            StartRefresh();
        }

        private async void StartRefresh()
        {
            _statusLabel = "Loading...";
            if (_ribbon != null) _ribbon.InvalidateControl("lblStatus");

            bool running = await _ollama.IsRunningAsync();
            if (!running)
            {
                _statusLabel = "[OFF] Ollama not running";
                _models.Clear();
                if (_ribbon != null) _ribbon.Invalidate();
                return;
            }

            _models = await _ollama.GetModelsAsync();
            if (_models.Count == 0)
                _statusLabel = "[!] No models - run: ollama pull llama3.2";
            else
                _statusLabel = string.Format("[ON] {0} model(s)", _models.Count);

            if (_models.Count > 0 && string.IsNullOrEmpty(_selectedModel))
                _selectedModel = _models[0].Name;

            if (_ribbon != null) _ribbon.Invalidate();
        }

        // Ribbon data
        public int    GetModelCount(IRibbonControl c)        { return _models.Count; }
        public string GetModelLabel(IRibbonControl c, int i) { return i < _models.Count ? _models[i].DisplayName + " (" + _models[i].SizeDisplay + ")" : ""; }
        public string GetModelText (IRibbonControl c)        { return _selectedModel.Length > 0 ? _selectedModel.Split(':')[0] : "No models"; }
        public string GetStatusLabel(IRibbonControl c)       { return _statusLabel; }
        public bool   GetChatPressed(IRibbonControl c)       { return _chatOpen; }

        public void OnModelChange(IRibbonControl c, string text)
        {
            foreach (var m in _models)
            {
                if (m.Name.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                {
                    _selectedModel = m.Name;
                    return;
                }
            }
            _selectedModel = text;
        }

        public void OnRefreshModels(IRibbonControl c) { StartRefresh(); }

        public void OnOpenSettings(IRibbonControl c)
        {
            using (var f = new SettingsForm(_settings))
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    _settings = f.Settings;
                    PluginSettings.Save(_settings);
                    _ollama.SetBaseUrl(_settings.OllamaUrl);
                    StartRefresh();
                }
            }
        }

        // Word helpers
        private Word.Application WordApp
        {
            get
            {
                try
                {
                    Type wordType = Type.GetTypeFromProgID("Word.Application");
                    if (wordType == null) return null;

                    return (Word.Application)Activator.CreateInstance(wordType);
                }
                catch
                {
                    return null;
                }
            }
        }

        private string GetSelected()
        {
            var sel = WordApp.Selection;
            if (sel == null) return "";
            if (sel.Type != Word.WdSelectionType.wdSelectionNormal) return "";
            string t = sel.Text;
            return t != null ? t.Trim() : "";
        }

        private string GetDocText()
        {
            try
            {
                var doc = WordApp.ActiveDocument;
                if (doc == null) return "";
                var content = doc.Content;
                if (content == null) return "";
                string t = content.Text;
                return t != null ? t.Trim() : "";
            }
            catch { return ""; }
        }

        private void ReplaceSelected(string t)
        {
            var sel = WordApp.Selection;
            if (sel != null && sel.Type == Word.WdSelectionType.wdSelectionNormal)
                sel.TypeText(t);
        }

        private void InsertAtCursor(string t)
        {
            var sel = WordApp.Selection;
            if (sel != null) sel.TypeText(t);
        }

        private string Clip(string t, int max)
        {
            if (t == null) return "";
            return t.Length > max ? t.Substring(0, max) + "..." : t;
        }

        private bool EnsureModel()
        {
            if (!string.IsNullOrEmpty(_selectedModel)) return true;
            MessageBox.Show(
                "Please select an Ollama model.\n\nIf the list is empty, make sure Ollama is running\nand run:  ollama pull llama3.2",
                "No model selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private async void RunAI(string title, Func<System.Threading.Tasks.Task<string>> fn, Action<string> apply)
        {
            if (!EnsureModel()) return;
            var prog = new ProgressForm(title);
            prog.Show();
            Application.DoEvents();
            try
            {
                string result = await fn();
                prog.Close();
                apply(result);
            }
            catch (Exception ex)
            {
                prog.Close();
                MessageBox.Show("Error: " + ex.Message, "AI Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowResult(string title, string original, string result, Action apply)
        {
            using (var f = new ResultDialog(title, original, result, apply))
                f.ShowDialog();
        }

        private void NoSelection(string msg)
        {
            MessageBox.Show(msg, "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Text operations
        public void OnImproveText(IRibbonControl c)
        {
            string t = GetSelected();
            if (t == "") { NoSelection("Please select text first."); return; }
            RunAI("Improving text...",
                () => _ollama.GenerateAsync(_selectedModel, "Improve style, grammar and readability. Return ONLY the improved text:\n\n" + t),
                r => ShowResult("Improved Text", t, r, () => ReplaceSelected(r)));
        }

        public void OnTranslateText(IRibbonControl c)
        {
            string t = GetSelected();
            if (t == "") { NoSelection("Please select text first."); return; }
            using (var d = new TranslateDialog())
            {
                if (d.ShowDialog() != DialogResult.OK) return;
                string lang = d.Lang;
                RunAI("Translating to " + lang + "...",
                    () => _ollama.GenerateAsync(_selectedModel, "Translate to " + lang + ". Return ONLY the translation:\n\n" + t),
                    r => ShowResult("Translation", t, r, () => ReplaceSelected(r)));
            }
        }

        public void OnSummarizeText(IRibbonControl c)
        {
            string t = GetSelected();
            if (t == "") t = GetDocText();
            if (t == "") { NoSelection("Document is empty."); return; }
            string excerpt = Clip(t, 3000);
            RunAI("Summarizing...",
                () => _ollama.GenerateAsync(_selectedModel, "Write a 3-5 sentence summary. Return ONLY the summary:\n\n" + excerpt),
                r => ShowResult("Summary", null, r, () => InsertAtCursor("\n\n--- SUMMARY ---\n" + r)));
        }

        public void OnExpandText(IRibbonControl c)
        {
            string t = GetSelected();
            if (t == "") { NoSelection("Please select text first."); return; }
            RunAI("Expanding text...",
                () => _ollama.GenerateAsync(_selectedModel, "Expand with details and examples. Return ONLY the expanded text:\n\n" + t),
                r => ShowResult("Expanded Text", t, r, () => ReplaceSelected(r)));
        }

        public void OnShortenText(IRibbonControl c)
        {
            string t = GetSelected();
            if (t == "") { NoSelection("Please select text first."); return; }
            RunAI("Shortening...",
                () => _ollama.GenerateAsync(_selectedModel, "Shorten by half, keep the main idea. Return ONLY the short text:\n\n" + t),
                r => ShowResult("Shortened Text", t, r, () => ReplaceSelected(r)));
        }

        public void OnFixGrammar(IRibbonControl c)
        {
            string t = GetSelected();
            if (t == "") { NoSelection("Please select text first."); return; }
            RunAI("Fixing grammar...",
                () => _ollama.GenerateAsync(_selectedModel, "Fix grammar and spelling errors. Return ONLY the corrected text:\n\n" + t),
                r => ShowResult("Corrected Text", t, r, () => ReplaceSelected(r)));
        }

        public void OnChangeTone(IRibbonControl c)
        {
            string t = GetSelected();
            if (t == "") { NoSelection("Please select text first."); return; }
            using (var d = new ToneDialog())
            {
                if (d.ShowDialog() != DialogResult.OK) return;
                string tone = d.Tone;
                RunAI("Changing tone...",
                    () => _ollama.GenerateAsync(_selectedModel, "Rewrite in " + tone + " tone. Return ONLY the rewritten text:\n\n" + t),
                    r => ShowResult("Rewritten - " + tone, t, r, () => ReplaceSelected(r)));
            }
        }

        // Generate
        public void OnContinueText(IRibbonControl c)
        {
            var sel = WordApp.Selection;
            if (sel == null) return;
            int rangeStart = Math.Max(0, sel.Range.Start - 600);
            string ctx = WordApp.ActiveDocument.Range(rangeStart, sel.Range.Start).Text;
            if (ctx != null) ctx = ctx.Trim(); else ctx = "";
            if (ctx == "") { NoSelection("No text before cursor."); return; }
            RunAI("Continuing text...",
                () => _ollama.GenerateAsync(_selectedModel, "Continue this text naturally (1-3 paragraphs). Return ONLY the continuation:\n\n" + ctx),
                r => InsertAtCursor(r));
        }

        public void OnGenerateFromPrompt(IRibbonControl c)
        {
            if (!EnsureModel()) return;
            using (var d = new PromptDialog("Generate Text", "Describe what to write:"))
            {
                if (d.ShowDialog() != DialogResult.OK) return;
                string prompt = d.Prompt;
                RunAI("Generating...",
                    () => _ollama.GenerateAsync(_selectedModel, prompt),
                    r => ShowResult("Generated Text", null, r, () => InsertAtCursor(r)));
            }
        }

        public void OnGenerateTable(IRibbonControl c)
        {
            if (!EnsureModel()) return;
            using (var d = new PromptDialog("Create Table", "Describe the table (e.g. 'weekly schedule with tasks'):"))
            {
                if (d.ShowDialog() != DialogResult.OK) return;
                string prompt = d.Prompt;
                RunAI("Generating table...",
                    () => _ollama.GenerateAsync(_selectedModel, "Create a table: " + prompt + ". Format as Markdown table with headers."),
                    r =>
                    {
                        var tbl = MdTable.Convert(WordApp, r);
                        if (tbl == null) InsertAtCursor(r);
                    });
            }
        }

        public void OnGenerateList(IRibbonControl c)
        {
            if (!EnsureModel()) return;
            using (var d = new PromptDialog("Create List", "What should be listed?"))
            {
                if (d.ShowDialog() != DialogResult.OK) return;
                string prompt = d.Prompt;
                RunAI("Generating list...",
                    () => _ollama.GenerateAsync(_selectedModel, "Create a bullet list about: " + prompt + ". Each item on a new line starting with '- '."),
                    r =>
                    {
                        foreach (string line in r.Split('\n'))
                        {
                            string item = line.TrimStart('-', '*', ' ').Trim();
                            if (item == "") continue;
                            WordApp.Selection.TypeText(item);
                            WordApp.Selection.Range.ListFormat.ApplyBulletDefault();
                            WordApp.Selection.TypeParagraph();
                        }
                    });
            }
        }

        public void OnGenerateHeadings(IRibbonControl c)
        {
            if (!EnsureModel()) return;
            using (var d = new PromptDialog("Document Structure", "Document topic:"))
            {
                if (d.ShowDialog() != DialogResult.OK) return;
                string prompt = d.Prompt;
                RunAI("Generating headings...",
                    () => _ollama.GenerateAsync(_selectedModel, "Create a heading outline for a document about: " + prompt + ". Return ONLY a list of headings, one per line."),
                    r =>
                    {
                        foreach (string line in r.Split('\n'))
                        {
                            string h = line.Trim().TrimStart('#', ' ').Trim();
                            if (h == "") continue;
                            WordApp.Selection.TypeText(h);
                            WordApp.Selection.set_Style(WordApp.ActiveDocument.Styles[Word.WdBuiltinStyle.wdStyleHeading1]);
                            WordApp.Selection.TypeParagraph();
                        }
                    });
            }
        }

        // Analyze
        public void OnAnalyzeDocument(IRibbonControl c)
        {
            string t = GetDocText();
            if (t == "") { NoSelection("Document is empty."); return; }
            string excerpt = Clip(t, 3000);
            RunAI("Analyzing document...",
                () => _ollama.GenerateAsync(_selectedModel, "Analyze: topic, style, audience, strengths, weaknesses, recommendations:\n\n" + excerpt),
                r => ShowResult("Document Analysis", null, r, null));
        }

        public void OnExtractKeyPoints(IRibbonControl c)
        {
            string t = GetSelected();
            if (t == "") t = GetDocText();
            if (t == "") { NoSelection("Document is empty."); return; }
            string excerpt = Clip(t, 3000);
            RunAI("Extracting key points...",
                () => _ollama.GenerateAsync(_selectedModel, "Extract 5-10 key points. Each on new line starting with '* ':\n\n" + excerpt),
                r => ShowResult("Key Points", null, r, () => InsertAtCursor("\n\n--- KEY POINTS ---\n" + r)));
        }

        public void OnFindErrors(IRibbonControl c)
        {
            string t = GetSelected();
            if (t == "") t = GetDocText();
            if (t == "") { NoSelection("Document is empty."); return; }
            string excerpt = Clip(t, 3000);
            RunAI("Finding errors...",
                () => _ollama.GenerateAsync(_selectedModel, "Find logical, factual and stylistic errors. Format: quote -> problem -> fix:\n\n" + excerpt),
                r => ShowResult("Found Errors", null, r, null));
        }

        public void OnDocStats(IRibbonControl c)
        {
            var doc = WordApp.ActiveDocument;
            if (doc == null) return;
            string t = GetDocText();
            int w  = doc.Words.Count;
            int ch = doc.Characters.Count;
            int p  = doc.Paragraphs.Count;
            string stats = "Words: " + w.ToString("N0") + "\n" +
                           "Characters: " + ch.ToString("N0") + "\n" +
                           "Paragraphs: " + p.ToString("N0") + "\n" +
                           "Reading time: ~" + (w / 200.0).ToString("F0") + " min";
            string sample = t.Length > 800 ? t.Substring(0, 800) : t;
            RunAI("Analyzing readability...",
                () => _ollama.GenerateAsync(_selectedModel, "Rate the readability in one sentence:\n\n" + sample),
                r => MessageBox.Show("Document Statistics\n\n" + stats + "\n\nReadability: " + r,
                    "Statistics", MessageBoxButtons.OK, MessageBoxIcon.Information));
        }

        // Chat
        public void OnToggleChat(IRibbonControl c, bool pressed)
        {
            _chatOpen = pressed;
            if (pressed)
            {
                if (_chatForm == null || _chatForm.IsDisposed)
                    _chatForm = new ChatForm(_ollama, () => _selectedModel, GetDocText);
                _chatForm.Show();
                _chatForm.FormClosed += (s, e) =>
                {
                    _chatOpen = false;
                    if (_ribbon != null) _ribbon.InvalidateControl("btnChat");
                };
            }
            else
            {
                if (_chatForm != null) _chatForm.Close();
            }
        }

        public void OnAskAboutDoc(IRibbonControl c)
        {
            if (!EnsureModel()) return;
            string t = GetDocText();
            if (t == "") { NoSelection("Document is empty."); return; }
            using (var d = new PromptDialog("Ask About Document", "Your question:"))
            {
                if (d.ShowDialog() != DialogResult.OK) return;
                string question = d.Prompt;
                string excerpt  = Clip(t, 2000);
                RunAI("Analyzing...",
                    () => _ollama.GenerateAsync(_selectedModel, "Document:\n" + excerpt + "\n\nQuestion: " + question),
                    r => ShowResult("AI Answer", null, r, null));
            }
        }
    }
}

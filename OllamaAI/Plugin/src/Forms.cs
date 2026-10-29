using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;

namespace OllamaWordPlugin
{
    static class Pal
    {
        public static Color Bg0  = Color.FromArgb(30,  30,  46);
        public static Color Bg1  = Color.FromArgb(49,  50,  68);
        public static Color Bg2  = Color.FromArgb(69,  71,  90);
        public static Color Acc  = Color.FromArgb(99,  102, 241);
        public static Color Grn  = Color.FromArgb(166, 227, 161);
        public static Color Txt  = Color.FromArgb(205, 214, 244);
        public static Color Dim  = Color.FromArgb(147, 153, 178);
        public static Color Cyan = Color.FromArgb(137, 220, 235);
        public static Font  F    = new Font("Segoe UI",  10f);
        public static Font  FB   = new Font("Segoe UI",  10f, FontStyle.Bold);
        public static Font  FS   = new Font("Segoe UI",   9f);
        public static Font  FC   = new Font("Consolas",  9.5f);

        public static Button Btn(string text, Color? bg = null)
        {
            return new Button
            {
                Text      = text,
                FlatStyle = FlatStyle.Flat,
                BackColor = bg.HasValue ? bg.Value : Acc,
                ForeColor = Txt,
                Font      = FB,
                Padding   = new Padding(8, 4, 8, 4),
                FlatAppearance = { BorderSize = 0 }
            };
        }

        public static Label Lbl(string text, bool bold = false)
        {
            return new Label { Text = text, ForeColor = Txt, Font = bold ? FB : F, AutoSize = true };
        }
    }

    // ProgressForm
    public class ProgressForm : Form
    {
        public ProgressForm(string msg)
        {
            Text = "Ollama AI";
            Size = new Size(320, 90);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            ControlBox = false;
            BackColor = Pal.Bg0;

            var lbl = new Label
            {
                Text = msg, ForeColor = Pal.Txt, Font = Pal.F,
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter
            };
            var bar = new ProgressBar { Dock = DockStyle.Bottom, Style = ProgressBarStyle.Marquee, Height = 5 };
            Controls.Add(lbl);
            Controls.Add(bar);
        }
    }

    // ResultDialog
    public class ResultDialog : Form
    {
        public ResultDialog(string title, string original, string result, Action apply)
        {
            Text = title;
            Size = new Size(700, 500);
            MinimumSize = new Size(480, 360);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Pal.Bg0;

            int rowCount = original != null ? 4 : 3;
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, Padding = new Padding(14),
                RowCount = rowCount, ColumnCount = 1
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            if (original != null) layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, original != null ? 70f : 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            layout.Controls.Add(Pal.Lbl(title, true), 0, 0);

            int row = 1;
            if (original != null)
            {
                layout.Controls.Add(BuildBox("Original:", original, true), 0, row);
                row++;
            }

            var resultBox = new RichTextBox
            {
                Text = result, BackColor = Pal.Bg2, ForeColor = Pal.Txt,
                Font = Pal.FC, BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill, ScrollBars = RichTextBoxScrollBars.Vertical
            };
            layout.Controls.Add(BuildBoxCtrl("AI Result:", resultBox), 0, row);
            row++;

            var btnRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
                BackColor = Pal.Bg0, Padding = new Padding(0, 8, 0, 0)
            };

            var bClose = Pal.Btn("Close", Pal.Bg2);
            bClose.Click += delegate { Close(); };
            btnRow.Controls.Add(bClose);

            var bCopy = Pal.Btn("Copy");
            bCopy.Click += delegate { Clipboard.SetText(resultBox.Text); };
            btnRow.Controls.Add(bCopy);

            if (apply != null)
            {
                var bApply = Pal.Btn("Insert into Document", Pal.Grn);
                bApply.ForeColor = Pal.Bg0;
                bApply.Click += delegate { apply(); Close(); };
                btnRow.Controls.Add(bApply);
            }

            layout.Controls.Add(btnRow, 0, row);
            Controls.Add(layout);
        }

        private Panel BuildBox(string label, string text, bool ro)
        {
            var rtb = new RichTextBox
            {
                Text = text, ReadOnly = ro,
                BackColor = Pal.Bg2, ForeColor = Pal.Dim,
                Font = Pal.FC, BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill, ScrollBars = RichTextBoxScrollBars.Vertical
            };
            return BuildBoxCtrl(label, rtb);
        }

        private Panel BuildBoxCtrl(string label, Control content)
        {
            var p   = new Panel { Dock = DockStyle.Fill, BackColor = Pal.Bg0, Padding = new Padding(0, 4, 0, 4) };
            var lbl = Pal.Lbl(label); lbl.Dock = DockStyle.Top;
            var inner = new Panel { Dock = DockStyle.Fill, BackColor = Pal.Bg2, Padding = new Padding(6) };
            inner.Controls.Add(content);
            p.Controls.Add(inner);
            p.Controls.Add(lbl);
            return p;
        }
    }

    // PromptDialog
    public class PromptDialog : Form
    {
        public string Prompt { get; private set; }

        public PromptDialog(string title, string hint)
        {
            Text = title;
            Size = new Size(480, 185);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            BackColor = Pal.Bg0;

            var txt = new TextBox
            {
                Multiline = true, Height = 64,
                BackColor = Pal.Bg2, ForeColor = Pal.Txt,
                Font = Pal.F, BorderStyle = BorderStyle.None
            };
            var bOk   = Pal.Btn("Generate"); bOk.DialogResult   = DialogResult.OK;
            var bCanc = Pal.Btn("Cancel", Pal.Bg2); bCanc.DialogResult = DialogResult.Cancel;
            AcceptButton = bOk; CancelButton = bCanc;
            bOk.Click += delegate
            {
                Prompt = txt.Text.Trim();
                if (Prompt == "") DialogResult = DialogResult.None;
            };

            var lay = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, Padding = new Padding(14),
                RowCount = 3, ColumnCount = 1
            };
            lay.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lay.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            lay.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lay.Controls.Add(Pal.Lbl(hint), 0, 0);
            lay.Controls.Add(txt, 0, 1);
            var bp = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, BackColor = Pal.Bg0 };
            bp.Controls.Add(bOk); bp.Controls.Add(bCanc);
            lay.Controls.Add(bp, 0, 2);
            Controls.Add(lay);
        }
    }

    // TranslateDialog
    public class TranslateDialog : Form
    {
        public string Lang { get; private set; }

        public TranslateDialog()
        {
            Text = "Translate Text";
            Size = new Size(320, 160);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            BackColor = Pal.Bg0;

            var langs = new object[] { "English", "Russian", "German", "French", "Spanish", "Chinese", "Japanese", "Arabic", "Italian", "Portuguese" };
            var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Pal.Bg2, ForeColor = Pal.Txt, Font = Pal.F };
            cb.Items.AddRange(langs); cb.SelectedIndex = 0;

            var bOk   = Pal.Btn("Translate"); bOk.DialogResult   = DialogResult.OK;
            var bCanc = Pal.Btn("Cancel", Pal.Bg2); bCanc.DialogResult = DialogResult.Cancel;
            AcceptButton = bOk; CancelButton = bCanc;
            bOk.Click += delegate { Lang = cb.SelectedItem != null ? cb.SelectedItem.ToString() : "English"; };

            var lay = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(14), RowCount = 3, ColumnCount = 1 };
            lay.Controls.Add(Pal.Lbl("Target language:"), 0, 0);
            lay.Controls.Add(cb, 0, 1);
            var bp = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, BackColor = Pal.Bg0 };
            bp.Controls.Add(bOk); bp.Controls.Add(bCanc);
            lay.Controls.Add(bp, 0, 2);
            Controls.Add(lay);
        }
    }

    // ToneDialog
    public class ToneDialog : Form
    {
        public string Tone { get; private set; }

        public ToneDialog()
        {
            Text = "Change Tone";
            Size = new Size(280, 240);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            BackColor = Pal.Bg0;

            var tones = new object[] { "formal", "informal", "professional", "friendly", "academic", "simple", "persuasive", "neutral" };
            var lb = new ListBox { BackColor = Pal.Bg2, ForeColor = Pal.Txt, Font = Pal.F, BorderStyle = BorderStyle.None, Dock = DockStyle.Fill };
            lb.Items.AddRange(tones); lb.SelectedIndex = 0;

            var bOk   = Pal.Btn("Apply"); bOk.DialogResult   = DialogResult.OK;
            var bCanc = Pal.Btn("Cancel", Pal.Bg2); bCanc.DialogResult = DialogResult.Cancel;
            AcceptButton = bOk; CancelButton = bCanc;
            bOk.Click += delegate { Tone = lb.SelectedItem != null ? lb.SelectedItem.ToString() : "neutral"; };

            var lay = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(14), RowCount = 3, ColumnCount = 1 };
            lay.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lay.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            lay.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lay.Controls.Add(Pal.Lbl("Select tone:"), 0, 0);
            lay.Controls.Add(lb, 0, 1);
            var bp = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, BackColor = Pal.Bg0 };
            bp.Controls.Add(bOk); bp.Controls.Add(bCanc);
            lay.Controls.Add(bp, 0, 2);
            Controls.Add(lay);
        }
    }

    // SettingsForm
    public class SettingsForm : Form
    {
        public PluginSettings Settings { get; private set; }

        public SettingsForm(PluginSettings cur)
        {
            Settings = cur.Clone();
            Text = "Ollama Plugin Settings";
            Size = new Size(460, 220);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            BackColor = Pal.Bg0;

            var urlTxt = new TextBox { Text = cur.OllamaUrl,                BackColor = Pal.Bg2, ForeColor = Pal.Txt, Font = Pal.F,  BorderStyle = BorderStyle.None };
            var toTxt  = new TextBox { Text = cur.TimeoutSeconds.ToString(), BackColor = Pal.Bg2, ForeColor = Pal.Txt, Font = Pal.F,  BorderStyle = BorderStyle.None, Width = 50 };
            var sysTxt = new TextBox { Text = cur.SystemPrompt,              BackColor = Pal.Bg2, ForeColor = Pal.Txt, Font = Pal.FS, BorderStyle = BorderStyle.None, Multiline = true, Height = 50 };

            var bOk   = Pal.Btn("Save"); bOk.DialogResult   = DialogResult.OK;
            var bCanc = Pal.Btn("Cancel", Pal.Bg2); bCanc.DialogResult = DialogResult.Cancel;
            AcceptButton = bOk; CancelButton = bCanc;
            bOk.Click += delegate
            {
                Settings.OllamaUrl    = urlTxt.Text.Trim();
                Settings.SystemPrompt = sysTxt.Text.Trim();
                int t2;
                if (int.TryParse(toTxt.Text, out t2)) Settings.TimeoutSeconds = t2;
            };

            var lay = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(14), RowCount = 5, ColumnCount = 2 };
            lay.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            lay.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            lay.Controls.Add(Pal.Lbl("Ollama URL:"),    0, 0); lay.Controls.Add(urlTxt, 1, 0);
            lay.Controls.Add(Pal.Lbl("Timeout (sec):"), 0, 1); lay.Controls.Add(toTxt,  1, 1);
            lay.Controls.Add(Pal.Lbl("System prompt:"), 0, 2); lay.Controls.Add(sysTxt, 1, 2);
            var bp = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, BackColor = Pal.Bg0 };
            bp.Controls.Add(bOk); bp.Controls.Add(bCanc);
            lay.Controls.Add(bp, 0, 4); lay.SetColumnSpan(bp, 2);
            Controls.Add(lay);
        }
    }

    // ChatForm
    public class ChatForm : Form
    {
        private readonly OllamaService     _ollama;
        private readonly Func<string>      _getModel;
        private readonly Func<string>      _getDoc;
        private readonly List<ChatMessage> _history = new List<ChatMessage>();
        private RichTextBox _log;
        private TextBox     _input;
        private Button      _sendBtn;

        public ChatForm(OllamaService ollama, Func<string> getModel, Func<string> getDoc)
        {
            _ollama = ollama; _getModel = getModel; _getDoc = getDoc;
            Text = "Ollama AI - Chat";
            Size = new Size(420, 600);
            MinimumSize = new Size(340, 400);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            BackColor = Pal.Bg0;
            TopMost = true;
            Location = new Point(Screen.PrimaryScreen.WorkingArea.Right - 440, 60);

            _log = new RichTextBox
            {
                Dock = DockStyle.Fill, ReadOnly = true,
                BackColor = Pal.Bg1, ForeColor = Pal.Txt,
                Font = Pal.F, BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            _input = new TextBox
            {
                Multiline = true, Height = 68, Dock = DockStyle.Fill,
                BackColor = Pal.Bg2, ForeColor = Pal.Txt,
                Font = Pal.F, BorderStyle = BorderStyle.None
            };
            _input.KeyDown += InputKeyDown;

            _sendBtn = Pal.Btn("Send");
            _sendBtn.Dock = DockStyle.Right;
            _sendBtn.Width = 100;
            _sendBtn.Click += delegate { Send(); };

            var bDoc = Pal.Btn("Add Document", Pal.Bg2);
            bDoc.Dock = DockStyle.Left;
            bDoc.Click += delegate
            {
                string d = _getDoc();
                if (string.IsNullOrEmpty(d)) return;
                string ex = d.Length > 1500 ? d.Substring(0, 1500) + "..." : d;
                _history.Add(new ChatMessage { Role = "user", Content = "[Document context]:\n" + ex });
                Log("[Document context added]", Pal.Cyan);
            };

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 38, BackColor = Pal.Bg0, Padding = new Padding(6, 3, 6, 3) };
            bottom.Controls.Add(bDoc);
            bottom.Controls.Add(_sendBtn);

            var inputPanel = new Panel { Dock = DockStyle.Bottom, Height = 74, BackColor = Pal.Bg2, Padding = new Padding(6) };
            inputPanel.Controls.Add(_input);

            var header = new Label
            {
                Text = "  Ollama AI Chat", Dock = DockStyle.Top, Height = 32,
                BackColor = Pal.Acc, ForeColor = Color.White,
                Font = Pal.FB, TextAlign = ContentAlignment.MiddleLeft
            };

            Controls.Add(_log);
            Controls.Add(inputPanel);
            Controls.Add(bottom);
            Controls.Add(header);

            Log("Hello! Ask anything or click 'Add Document' to work with your text.", Pal.Cyan);
        }

        private void InputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                Send();
            }
        }

        private async void Send()
        {
            string msg = _input.Text.Trim();
            if (msg == "") return;
            _input.Clear();
            _sendBtn.Enabled = false;
            Log("You: " + msg, Pal.Txt);
            _history.Add(new ChatMessage { Role = "user", Content = msg });
            try
            {
                Log("Thinking...", Pal.Dim);
                string reply = await _ollama.ChatAsync(_getModel(), _history);
                // remove "Thinking..." line
                string logText = _log.Text;
                int nl = logText.LastIndexOf('\n', logText.Length - 2);
                _log.Select(nl + 1, _log.TextLength - nl - 1);
                _log.SelectedText = "";
                Log("AI: " + reply, Pal.Cyan);
                _history.Add(new ChatMessage { Role = "assistant", Content = reply });
            }
            catch (Exception ex)
            {
                Log("Error: " + ex.Message, Color.IndianRed);
            }
            _sendBtn.Enabled = true;
        }

        private void Log(string text, Color color)
        {
            _log.SelectionStart  = _log.TextLength;
            _log.SelectionLength = 0;
            _log.SelectionColor  = color;
            _log.AppendText(text + "\n\n");
            _log.ScrollToCaret();
        }
    }

    // Markdown table converter
    static class MdTable
    {
        public static object Convert(Word.Application app, string md)
        {
            try
            {
                var rows = new List<List<string>>();
                foreach (string line in md.Split('\n'))
                {
                    string tr = line.Trim();
                    if (!tr.StartsWith("|")) continue;
                    string check = tr.Replace("|", "").Replace("-", "").Replace(" ", "");
                    if (check == "") continue;
                    string[] cells = tr.Trim('|').Split('|');
                    var row = new List<string>();
                    foreach (string cell in cells) row.Add(cell.Trim());
                    rows.Add(row);
                }
                if (rows.Count < 2) return null;
                int cols = rows[0].Count;
                var tbl = app.ActiveDocument.Tables.Add(app.Selection.Range, rows.Count, cols);
                for (int r = 0; r < rows.Count; r++)
                    for (int c2 = 0; c2 < Math.Min(rows[r].Count, cols); c2++)
                        tbl.Cell(r + 1, c2 + 1).Range.Text = rows[r][c2];
                return tbl;
            }
            catch { return null; }
        }
    }
}

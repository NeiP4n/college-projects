using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;

namespace OllamaAI
{
    internal static class Pal
    {
        public static readonly Color Bg0 = Color.FromArgb(30, 30, 46);
        public static readonly Color Bg1 = Color.FromArgb(49, 50, 68);
        public static readonly Color Bg2 = Color.FromArgb(69, 71, 90);
        public static readonly Color Acc = Color.FromArgb(99, 102, 241);
        public static readonly Color Grn = Color.FromArgb(166, 227, 161);
        public static readonly Color Txt = Color.FromArgb(205, 214, 244);
        public static readonly Color Dim = Color.FromArgb(147, 153, 178);
        public static readonly Color Cyan = Color.FromArgb(137, 220, 235);
        public static readonly Font F = new Font("Segoe UI", 10f);
        public static readonly Font FB = new Font("Segoe UI", 10f, FontStyle.Bold);
        public static readonly Font FS = new Font("Segoe UI", 9f);
        public static readonly Font FC = new Font("Consolas", 9.5f);
        public static readonly Image IAdd = Icon16(SystemIcons.Information);
        public static readonly Image IEdit = Icon16(SystemIcons.Application);
        public static readonly Image IDelete = Icon16(SystemIcons.Error);
        public static readonly Image IPin = Icon16(SystemIcons.Warning);
        public static readonly Image ISave = Icon16(SystemIcons.Shield);
        public static readonly Image ICancel = Icon16(SystemIcons.Hand);
        public static readonly Image ICopy = Icon16(SystemIcons.Asterisk);
        public static readonly Image IApply = Icon16(SystemIcons.Shield);
        public static readonly Image ISend = Icon16(SystemIcons.Information);
        public static readonly Image IDoc = Icon16(SystemIcons.WinLogo);

        public static Button Btn(string text, Color? bg = null, Image icon = null)
        {
            var btn = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                BackColor = bg ?? Acc,
                ForeColor = Txt,
                Font = FB,
                Padding = new Padding(10, 5, 10, 5),
                Margin = new Padding(6, 4, 0, 4),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(0, 36),
                UseVisualStyleBackColor = false
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(120, 122, 255);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(80, 90, 190);
            btn.TextAlign = ContentAlignment.MiddleCenter;
            if (icon != null)
            {
                btn.Image = icon;
                btn.ImageAlign = ContentAlignment.MiddleLeft;
                btn.TextImageRelation = TextImageRelation.ImageBeforeText;
            }

            return btn;
        }

        public static Label Lbl(string text, bool bold = false)
        {
            return new Label { Text = text, ForeColor = Txt, Font = bold ? FB : F, AutoSize = true };
        }

        private static Image Icon16(Icon icon)
        {
            if (icon == null)
            {
                return null;
            }

            return new Bitmap(icon.ToBitmap(), new Size(16, 16));
        }
    }

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
            AutoScaleMode = AutoScaleMode.Dpi;

            var lbl = new Label
            {
                Text = msg,
                ForeColor = Pal.Txt,
                Font = Pal.F,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var bar = new ProgressBar { Dock = DockStyle.Bottom, Style = ProgressBarStyle.Marquee, Height = 5 };
            Controls.Add(lbl);
            Controls.Add(bar);
        }
    }

    public class ResultDialog : Form
    {
        public ResultDialog(string title, string original, string result, Action apply)
        {
            Text = title;
            Size = new Size(960, 700);
            MinimumSize = new Size(680, 500);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Pal.Bg0;
            AutoScaleMode = AutoScaleMode.Dpi;

            var rowCount = original != null ? 4 : 3;
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(14),
                RowCount = rowCount,
                ColumnCount = 1
            };

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            if (original != null)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30f));
            }

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, original != null ? 70f : 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            layout.Controls.Add(Pal.Lbl(title, true), 0, 0);

            var row = 1;
            if (original != null)
            {
                layout.Controls.Add(BuildBox("Исходный текст:", original, true), 0, row++);
            }

            var resultBox = new RichTextBox
            {
                Text = result,
                BackColor = Pal.Bg2,
                ForeColor = Pal.Txt,
                Font = Pal.FC,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            layout.Controls.Add(BuildBoxCtrl("Результат ИИ:", resultBox), 0, row++);

            var btnRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor = Pal.Bg0,
                Padding = new Padding(0, 8, 0, 0)
            };

            var closeButton = Pal.Btn("Закрыть", Pal.Bg2, Pal.ICancel);
            closeButton.Click += delegate { Close(); };
            btnRow.Controls.Add(closeButton);

            var copyButton = Pal.Btn("Копировать", null, Pal.ICopy);
            copyButton.Click += delegate { Clipboard.SetText(resultBox.Text); };
            btnRow.Controls.Add(copyButton);

            if (apply != null)
            {
                var applyButton = Pal.Btn("Вставить в документ", Pal.Grn, Pal.IApply);
                applyButton.ForeColor = Pal.Bg0;
                applyButton.Click += delegate
                {
                    apply();
                    Close();
                };
                btnRow.Controls.Add(applyButton);
            }

            layout.Controls.Add(btnRow, 0, row);
            Controls.Add(layout);
        }

        private Panel BuildBox(string label, string text, bool readOnly)
        {
            var rtb = new RichTextBox
            {
                Text = text,
                ReadOnly = readOnly,
                BackColor = Pal.Bg2,
                ForeColor = Pal.Dim,
                Font = Pal.FC,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            return BuildBoxCtrl(label, rtb);
        }

        private Panel BuildBoxCtrl(string label, Control content)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Pal.Bg0, Padding = new Padding(0, 4, 0, 4) };
            var lbl = Pal.Lbl(label);
            lbl.Dock = DockStyle.Top;
            var inner = new Panel { Dock = DockStyle.Fill, BackColor = Pal.Bg2, Padding = new Padding(6) };
            inner.Controls.Add(content);
            panel.Controls.Add(inner);
            panel.Controls.Add(lbl);
            return panel;
        }
    }

    public class PromptDialog : Form
    {
        public string Prompt { get; private set; }

        public PromptDialog(string title, string hint)
        {
            Text = title;
            Size = new Size(760, 360);
            MinimumSize = new Size(620, 300);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = true;
            BackColor = Pal.Bg0;
            AutoScaleMode = AutoScaleMode.Dpi;

            var txt = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                BackColor = Pal.Bg2,
                ForeColor = Pal.Txt,
                Font = Pal.F,
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Vertical
            };

            var okButton = Pal.Btn("Сгенерировать", null, Pal.ISend);
            okButton.DialogResult = DialogResult.OK;
            okButton.Click += delegate
            {
                Prompt = (txt.Text ?? "").Trim();
                if (Prompt.Length == 0)
                {
                    DialogResult = DialogResult.None;
                }
            };

            var cancelButton = Pal.Btn("Отмена", Pal.Bg2, Pal.ICancel);
            cancelButton.DialogResult = DialogResult.Cancel;

            AcceptButton = okButton;
            CancelButton = cancelButton;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(14),
                RowCount = 3,
                ColumnCount = 1
            };

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(Pal.Lbl(hint), 0, 0);
            layout.Controls.Add(txt, 0, 1);

            var btnPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, BackColor = Pal.Bg0 };
            btnPanel.Controls.Add(okButton);
            btnPanel.Controls.Add(cancelButton);
            layout.Controls.Add(btnPanel, 0, 2);
            Controls.Add(layout);
        }
    }

    public class TranslateDialog : Form
    {
        public string Lang { get; private set; }

        public TranslateDialog()
        {
            Text = "Перевод текста";
            Size = new Size(420, 200);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            BackColor = Pal.Bg0;
            AutoScaleMode = AutoScaleMode.Dpi;

            var langs = new object[] { "Английский", "Русский", "Немецкий", "Французский", "Испанский", "Китайский", "Японский", "Арабский", "Итальянский", "Португальский" };
            var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Pal.Bg2, ForeColor = Pal.Txt, Font = Pal.F };
            cb.Items.AddRange(langs);
            cb.SelectedIndex = 0;

            var okButton = Pal.Btn("Перевести", null, Pal.ISend);
            okButton.DialogResult = DialogResult.OK;
            okButton.Click += delegate { Lang = cb.SelectedItem != null ? cb.SelectedItem.ToString() : "Русский"; };

            var cancelButton = Pal.Btn("Отмена", Pal.Bg2, Pal.ICancel);
            cancelButton.DialogResult = DialogResult.Cancel;

            AcceptButton = okButton;
            CancelButton = cancelButton;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(14), RowCount = 3, ColumnCount = 1 };
            layout.Controls.Add(Pal.Lbl("Язык перевода:"), 0, 0);
            layout.Controls.Add(cb, 0, 1);

            var btnPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, BackColor = Pal.Bg0 };
            btnPanel.Controls.Add(okButton);
            btnPanel.Controls.Add(cancelButton);
            layout.Controls.Add(btnPanel, 0, 2);
            Controls.Add(layout);
        }
    }

    public class ToneDialog : Form
    {
        public string Tone { get; private set; }

        public ToneDialog()
        {
            Text = "Изменение тона";
            Size = new Size(420, 340);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            BackColor = Pal.Bg0;
            AutoScaleMode = AutoScaleMode.Dpi;

            var tones = new object[] { "формальный", "неформальный", "профессиональный", "дружелюбный", "академический", "простой", "убеждающий", "нейтральный" };
            var list = new ListBox { BackColor = Pal.Bg2, ForeColor = Pal.Txt, Font = Pal.F, BorderStyle = BorderStyle.None, Dock = DockStyle.Fill };
            list.Items.AddRange(tones);
            list.SelectedIndex = 0;

            var okButton = Pal.Btn("Применить", null, Pal.IApply);
            okButton.DialogResult = DialogResult.OK;
            okButton.Click += delegate { Tone = list.SelectedItem != null ? list.SelectedItem.ToString() : "нейтральный"; };

            var cancelButton = Pal.Btn("Отмена", Pal.Bg2, Pal.ICancel);
            cancelButton.DialogResult = DialogResult.Cancel;

            AcceptButton = okButton;
            CancelButton = cancelButton;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(14), RowCount = 3, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(Pal.Lbl("Выберите тон:"), 0, 0);
            layout.Controls.Add(list, 0, 1);

            var btnPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, BackColor = Pal.Bg0 };
            btnPanel.Controls.Add(okButton);
            btnPanel.Controls.Add(cancelButton);
            layout.Controls.Add(btnPanel, 0, 2);
            Controls.Add(layout);
        }
    }

    public class SettingsForm : Form
    {
        public PluginSettings Settings { get; private set; }

        public SettingsForm(PluginSettings cur)
        {
            Settings = cur.Clone();
            Text = "Настройки Ollama AI";
            Size = new Size(760, 500);
            MinimumSize = new Size(680, 440);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = true;
            BackColor = Pal.Bg0;
            AutoScaleMode = AutoScaleMode.Dpi;

            var urlTxt = new TextBox { Text = cur.OllamaUrl, Dock = DockStyle.Fill, BackColor = Pal.Bg2, ForeColor = Pal.Txt, Font = Pal.F, BorderStyle = BorderStyle.None };
            var timeoutTxt = new TextBox { Text = cur.TimeoutSeconds.ToString(), Dock = DockStyle.Left, BackColor = Pal.Bg2, ForeColor = Pal.Txt, Font = Pal.F, BorderStyle = BorderStyle.None, Width = 80 };
            var autoRefreshChk = new CheckBox
            {
                Checked = cur.AutoRefreshEnabled,
                Text = "Автообновление статуса и моделей",
                ForeColor = Pal.Txt,
                BackColor = Pal.Bg0,
                Font = Pal.F
            };
            var refreshSecTxt = new TextBox { Text = cur.AutoRefreshSeconds.ToString(), Dock = DockStyle.Left, BackColor = Pal.Bg2, ForeColor = Pal.Txt, Font = Pal.F, BorderStyle = BorderStyle.None, Width = 80 };
            var autoDiscoverChk = new CheckBox
            {
                Checked = cur.AutoDiscoverLan,
                Text = "Автопоиск Ollama в локальной сети при OFF",
                ForeColor = Pal.Txt,
                BackColor = Pal.Bg0,
                Font = Pal.F
            };
            var lastLanLbl = Pal.Lbl(string.IsNullOrWhiteSpace(cur.LastDiscoveredUrl) ? "Хост в LAN пока не найден" : ("Последний найденный LAN-хост: " + cur.LastDiscoveredUrl));
            var sysTxt = new TextBox
            {
                Text = cur.SystemPrompt,
                Dock = DockStyle.Fill,
                BackColor = Pal.Bg2,
                ForeColor = Pal.Txt,
                Font = Pal.FS,
                BorderStyle = BorderStyle.None,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            var okButton = Pal.Btn("Сохранить", null, Pal.ISave);
            okButton.DialogResult = DialogResult.OK;
            okButton.Click += delegate
            {
                Settings.OllamaUrl = (urlTxt.Text ?? "").Trim();
                Settings.SystemPrompt = (sysTxt.Text ?? "").Trim();

                int parsed;
                if (int.TryParse(timeoutTxt.Text, out parsed))
                {
                    Settings.TimeoutSeconds = parsed;
                }

                Settings.AutoRefreshEnabled = autoRefreshChk.Checked;
                if (int.TryParse(refreshSecTxt.Text, out parsed))
                {
                    Settings.AutoRefreshSeconds = parsed;
                }

                Settings.AutoDiscoverLan = autoDiscoverChk.Checked;
            };

            var cancelButton = Pal.Btn("Отмена", Pal.Bg2, Pal.ICancel);
            cancelButton.DialogResult = DialogResult.Cancel;

            AcceptButton = okButton;
            CancelButton = cancelButton;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(14), RowCount = 9, ColumnCount = 2 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.Controls.Add(Pal.Lbl("URL Ollama:"), 0, 0);
            layout.Controls.Add(WrapField(urlTxt), 1, 0);
            layout.Controls.Add(Pal.Lbl("Таймаут (сек):"), 0, 1);
            layout.Controls.Add(WrapField(timeoutTxt), 1, 1);
            layout.Controls.Add(Pal.Lbl("Автообновление:"), 0, 2);
            layout.Controls.Add(autoRefreshChk, 1, 2);
            layout.Controls.Add(Pal.Lbl("Интервал обновления (сек):"), 0, 3);
            layout.Controls.Add(WrapField(refreshSecTxt), 1, 3);
            layout.Controls.Add(Pal.Lbl("Поиск в LAN:"), 0, 4);
            layout.Controls.Add(autoDiscoverChk, 1, 4);
            layout.Controls.Add(Pal.Lbl("Последний найденный:"), 0, 5);
            layout.Controls.Add(lastLanLbl, 1, 5);
            layout.Controls.Add(Pal.Lbl("Системный промпт:"), 0, 6);
            layout.Controls.Add(WrapField(sysTxt), 1, 6);
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var btnPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, BackColor = Pal.Bg0 };
            btnPanel.Controls.Add(okButton);
            btnPanel.Controls.Add(cancelButton);
            layout.Controls.Add(btnPanel, 0, 8);
            layout.SetColumnSpan(btnPanel, 2);
            Controls.Add(layout);
        }

        private static Panel WrapField(Control content)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Pal.Bg2, Padding = new Padding(8, 6, 8, 6), Margin = new Padding(3) };
            p.Controls.Add(content);
            return p;
        }
    }

    public class ChatForm : Form
    {
        private readonly OllamaService _ollama;
        private readonly Func<string> _getModel;
        private readonly Func<string> _getDoc;
        private readonly List<ChatMessage> _history = new List<ChatMessage>();
        private readonly RichTextBox _log;
        private readonly TextBox _input;
        private readonly Button _sendBtn;

        public ChatForm(OllamaService ollama, Func<string> getModel, Func<string> getDoc)
        {
            _ollama = ollama;
            _getModel = getModel;
            _getDoc = getDoc;
            _history.Add(new ChatMessage
            {
                Role = "system",
                Content = "Ты помощник для Word. Всегда отвечай на русском языке, если пользователь явно не попросил другой язык."
            });

            Text = "Ollama AI - Чат";
            Size = new Size(560, 760);
            MinimumSize = new Size(420, 560);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            BackColor = Pal.Bg0;
            TopMost = true;
            AutoScaleMode = AutoScaleMode.Dpi;

            var workingArea = Screen.PrimaryScreen != null ? Screen.PrimaryScreen.WorkingArea : Screen.FromControl(this).WorkingArea;
            Location = new Point(workingArea.Right - 440, 60);

            _log = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Pal.Bg1,
                ForeColor = Pal.Txt,
                Font = Pal.F,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            _input = new TextBox
            {
                Multiline = true,
                Height = 68,
                Dock = DockStyle.Fill,
                BackColor = Pal.Bg2,
                ForeColor = Pal.Txt,
                Font = Pal.F,
                BorderStyle = BorderStyle.None
            };
            _input.KeyDown += InputKeyDown;

            _sendBtn = Pal.Btn("Отправить", null, Pal.ISend);
            _sendBtn.Dock = DockStyle.Right;
            _sendBtn.Width = 100;
            _sendBtn.Click += delegate { Send(); };

            var docButton = Pal.Btn("Добавить документ", Pal.Bg2, Pal.IDoc);
            docButton.Dock = DockStyle.Left;
            docButton.Click += delegate
            {
                var doc = _getDoc();
                if (string.IsNullOrEmpty(doc))
                {
                    return;
                }

                var excerpt = doc.Length > 1500 ? doc.Substring(0, 1500) + "..." : doc;
                _history.Add(new ChatMessage { Role = "user", Content = "[Контекст документа]:\n" + excerpt });
                Log("[Контекст документа добавлен]", Pal.Cyan);
            };

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = Pal.Bg0, Padding = new Padding(6, 6, 6, 6) };
            bottom.Controls.Add(docButton);
            bottom.Controls.Add(_sendBtn);

            var inputPanel = new Panel { Dock = DockStyle.Bottom, Height = 82, BackColor = Pal.Bg2, Padding = new Padding(6) };
            inputPanel.Controls.Add(_input);

            var header = new Label
            {
                Text = "  Ollama AI Чат",
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Pal.Acc,
                ForeColor = Color.White,
                Font = Pal.FB,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Controls.Add(_log);
            Controls.Add(inputPanel);
            Controls.Add(bottom);
            Controls.Add(header);

            Log("Здравствуйте! Задайте вопрос или нажмите «Добавить документ», чтобы работать с текстом.", Pal.Cyan);
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
            var msg = "";
            Ui(() =>
            {
                msg = (_input.Text ?? "").Trim();
                if (msg.Length == 0)
                {
                    return;
                }

                _input.Clear();
                _sendBtn.Enabled = false;
            });

            if (msg.Length == 0)
            {
                return;
            }

            Log("Вы: " + msg, Pal.Txt);
            _history.Add(new ChatMessage { Role = "user", Content = msg });

            try
            {
                Log("Думаю...", Pal.Dim);
                var reply = await _ollama.ChatAsync(_getModel(), _history);
                Ui(RemoveThinkingLine);

                Log("ИИ: " + reply, Pal.Cyan);
                _history.Add(new ChatMessage { Role = "assistant", Content = reply });
            }
            catch (Exception ex)
            {
                Log("Ошибка: " + ex.Message, Color.IndianRed);
            }

            Ui(() => { _sendBtn.Enabled = true; });
        }

        private void Log(string text, Color color)
        {
            Ui(() =>
            {
                _log.SelectionStart = _log.TextLength;
                _log.SelectionLength = 0;
                _log.SelectionColor = color;
                _log.AppendText(text + "\n\n");
                _log.ScrollToCaret();
            });
        }

        private void RemoveThinkingLine()
        {
            var logText = _log.Text;
            var lastBreak = logText.LastIndexOf('\n', Math.Max(0, logText.Length - 2));
            if (lastBreak >= 0)
            {
                _log.Select(lastBreak + 1, _log.TextLength - lastBreak - 1);
                _log.SelectedText = "";
            }
        }

        private void Ui(Action action)
        {
            if (action == null)
            {
                return;
            }

            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)(() => action()));
            }
            else
            {
                action();
            }
        }
    }

    internal static class MdTable
    {
        public static object Convert(Word.Application app, string md)
        {
            try
            {
                if (app == null || app.ActiveDocument == null || app.Selection == null)
                {
                    return null;
                }

                var rows = new List<List<string>>();
                foreach (var line in (md ?? "").Split('\n'))
                {
                    var trimmed = line.Trim();
                    if (!trimmed.StartsWith("|"))
                    {
                        continue;
                    }

                    var check = trimmed.Replace("|", "").Replace("-", "").Replace(" ", "");
                    if (check.Length == 0)
                    {
                        continue;
                    }

                    var cells = trimmed.Trim('|').Split('|');
                    var row = new List<string>();
                    foreach (var cell in cells)
                    {
                        row.Add(cell.Trim());
                    }

                    rows.Add(row);
                }

                if (rows.Count < 2)
                {
                    return null;
                }

                var cols = rows[0].Count;
                var table = app.ActiveDocument.Tables.Add(app.Selection.Range, rows.Count, cols);
                for (var r = 0; r < rows.Count; r++)
                {
                    for (var c = 0; c < Math.Min(rows[r].Count, cols); c++)
                    {
                        table.Cell(r + 1, c + 1).Range.Text = rows[r][c];
                    }
                }

                return table;
            }
            catch
            {
                return null;
            }
        }
    }
}

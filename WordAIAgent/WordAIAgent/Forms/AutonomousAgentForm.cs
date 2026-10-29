using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WordAIAgent.Models;
using WordAIAgent.Services;

namespace WordAIAgent.Forms
{
    public class AutonomousAgentForm : Form
    {
        // ── UI Controls ──────────────────────────────────────────────
        private Panel _panelTop;
        private Label _lblTitle;
        private Label _lblModel;
        private Label _lblStatus;
        private Button _btnSettings;
        private Button _btnClear;

        private SplitContainer _splitMain;

        // Left: screenshot preview
        private Panel _panelScreen;
        private Label _lblScreenTitle;
        private PictureBox _picScreen;
        private Button _btnCapture;
        private CheckBox _chkAutoCapture;
        private CheckBox _chkVision;
        private Label _lblScreenInfo;

        // Right: chat
        private RichTextBox _rtbChat;
        private ProgressBar _progress;

        // Bottom: input
        private Panel _panelInput;
        private RichTextBox _rtbInput;
        private Button _btnSend;
        private Button _btnExecCode;
        private CheckBox _chkAutoExec;
        private Label _lblHint;

        // Code panel (collapsible)
        private Panel _panelCode;
        private Label _lblCodeTitle;
        private RichTextBox _rtbCode;
        private Button _btnRunCode;
        private Button _btnCollapseCode;

        // ── State ─────────────────────────────────────────────────────
        private readonly List<ChatMessage> _history = new();
        private readonly OllamaAgentService _ollama;
        private readonly RoslynExecutor _roslyn;
        private bool _busy;
        private string _lastScreenshot;
        private string _lastGeneratedCode;
        private bool _codePanelVisible = false;
        private Timer _autoCaptureTimer;

        public AutonomousAgentForm()
        {
            _ollama = new OllamaAgentService(ThisAddIn.Settings);
            _roslyn = new RoslynExecutor();

            BuildUI();
            ApplyTheme();

            _autoCaptureTimer = new Timer { Interval = 3000 };
            _autoCaptureTimer.Tick += (s, e) => CaptureScreen();

            AppendChat(ChatRole.System,
                "🤖 Autonomous Word Agent запущен\n" +
                "Я вижу ваш экран и могу писать произвольный C# код для управления Word.\n" +
                "Просто скажите что сделать — я напишу и выполню код сам.");

            CheckConnectionAsync();
        }

        // ─────────────────────────────────────────────────────────────
        //  UI BUILD
        // ─────────────────────────────────────────────────────────────
        private void BuildUI()
        {
            this.Text = "🤖 Autonomous Word Agent";
            this.Size = new Size(1200, 820);
            this.MinimumSize = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = C(15, 15, 25);

            BuildTopPanel();
            BuildMainSplit();
            BuildInputPanel();

            this.Controls.Add(_splitMain);
            this.Controls.Add(_panelInput);
            this.Controls.Add(_panelTop);

            _splitMain.BringToFront();
        }

        private void BuildTopPanel()
        {
            _panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = C(10, 10, 20),
                Padding = new Padding(12, 8, 12, 8)
            };

            _lblTitle = new Label
            {
                Text = "⚡ Autonomous Word Agent",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = C(130, 210, 255),
                AutoSize = true,
                Location = new Point(12, 14)
            };

            _lblModel = new Label
            {
                Text = $"  ◈  {ThisAddIn.Settings.ModelName}",
                Font = new Font("Segoe UI", 9f),
                ForeColor = C(140, 140, 170),
                AutoSize = true,
                Location = new Point(280, 18)
            };

            _lblStatus = new Label
            {
                Text = "● Проверка...",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(460, 18)
            };

            _btnClear = MakeIconBtn("🗑", "Очистить чат", new Point(1090, 10), 32);
            _btnSettings = MakeIconBtn("⚙", "Настройки", new Point(1054, 10), 32);
            _btnClear.Click += (s, e) => ClearChat();
            _btnSettings.Click += (s, e) => { using var sf = new SettingsForm(); sf.ShowDialog(); };

            _panelTop.Controls.AddRange(new Control[] { _lblTitle, _lblModel, _lblStatus, _btnClear, _btnSettings });
        }

        private void BuildMainSplit()
        {
            _splitMain = new SplitContainer
            {
                Dock = DockStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Location = new Point(0, 52),
                SplitterDistance = 300,
                BackColor = C(20, 20, 32)
            };

            // LEFT: screen preview
            _panelScreen = new Panel { Dock = DockStyle.Fill, BackColor = C(18, 18, 30), Padding = new Padding(8) };

            _lblScreenTitle = new Label
            {
                Text = "👁 Экран (Word)",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = C(120, 180, 255),
                AutoSize = true,
                Location = new Point(8, 8)
            };

            _picScreen = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = C(10, 10, 20),
                BorderStyle = BorderStyle.None,
                Location = new Point(8, 32),
                Cursor = Cursors.Hand
            };
            _picScreen.Click += (s, e) => CaptureScreen();

            _lblScreenInfo = new Label
            {
                Text = "Нажмите для захвата",
                Font = new Font("Segoe UI", 8f),
                ForeColor = C(100, 100, 130),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(8, 32)
            };

            _btnCapture = new Button
            {
                Text = "📸 Захватить экран",
                Location = new Point(8, 8),
                Size = new Size(150, 28),
                BackColor = C(40, 70, 120),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f)
            };
            _btnCapture.FlatAppearance.BorderSize = 0;
            _btnCapture.Click += (s, e) => CaptureScreen();

            _chkVision = new CheckBox
            {
                Text = "Vision (передавать скрин AI)",
                Location = new Point(8, 40),
                AutoSize = true,
                ForeColor = C(160, 160, 190),
                Font = new Font("Segoe UI", 8f),
                Checked = true
            };

            _chkAutoCapture = new CheckBox
            {
                Text = "Авто-захват (3 сек)",
                Location = new Point(8, 62),
                AutoSize = true,
                ForeColor = C(160, 160, 190),
                Font = new Font("Segoe UI", 8f)
            };
            _chkAutoCapture.CheckedChanged += (s, e) =>
            {
                if (_chkAutoCapture.Checked) _autoCaptureTimer.Start();
                else _autoCaptureTimer.Stop();
            };

            var pnlControls = new Panel
            {
                Dock = DockStyle.Top,
                Height = 96,
                BackColor = C(18, 18, 30),
                Padding = new Padding(8)
            };
            pnlControls.Controls.AddRange(new Control[] { _btnCapture, _chkVision, _chkAutoCapture });

            _panelScreen.Controls.AddRange(new Control[] { pnlControls, _lblScreenInfo });
            _panelScreen.Resize += (s, e) => LayoutScreenPanel();

            _splitMain.Panel1.Controls.Add(_panelScreen);

            // RIGHT: chat + code
            var rightPanel = new Panel { Dock = DockStyle.Fill, BackColor = C(20, 20, 32) };

            _rtbChat = new RichTextBox
            {
                ReadOnly = true,
                BackColor = C(20, 20, 32),
                ForeColor = C(215, 215, 235),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Padding = new Padding(12)
            };

            _progress = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 3,
                Style = ProgressBarStyle.Marquee,
                Visible = false,
                BackColor = C(20, 20, 32),
                ForeColor = C(80, 160, 255)
            };

            // Code panel
            _panelCode = new Panel
            {
                BackColor = C(12, 22, 12),
                Visible = false,
                Padding = new Padding(8)
            };

            var codeTitlePanel = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = C(10, 18, 10) };
            _lblCodeTitle = new Label
            {
                Text = "💻 Сгенерированный код (C#)",
                Font = new Font("Consolas", 9f, FontStyle.Bold),
                ForeColor = C(100, 220, 100),
                AutoSize = true,
                Location = new Point(8, 8)
            };
            _btnRunCode = new Button
            {
                Text = "▶ Выполнить",
                Size = new Size(110, 24),
                BackColor = C(30, 100, 30),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            _btnRunCode.FlatAppearance.BorderSize = 0;
            _btnRunCode.Click += (s, e) => ExecuteCurrentCode();
            _btnCollapseCode = new Button
            {
                Text = "✕",
                Size = new Size(28, 24),
                BackColor = C(60, 30, 30),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnCollapseCode.FlatAppearance.BorderSize = 0;
            _btnCollapseCode.Click += (s, e) => ToggleCodePanel(false);
            codeTitlePanel.Controls.AddRange(new Control[] { _lblCodeTitle, _btnRunCode, _btnCollapseCode });

            _rtbCode = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = C(12, 22, 12),
                ForeColor = C(140, 240, 140),
                Font = new Font("Consolas", 9f),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Both,
                WordWrap = false
            };

            _panelCode.Controls.Add(_rtbCode);
            _panelCode.Controls.Add(codeTitlePanel);

            rightPanel.Controls.Add(_rtbChat);
            rightPanel.Controls.Add(_progress);
            rightPanel.Controls.Add(_panelCode);
            rightPanel.Resize += (s, e) => LayoutRightPanel();

            _splitMain.Panel2.Controls.Add(rightPanel);
        }

        private void BuildInputPanel()
        {
            _panelInput = new Panel
            {
                BackColor = C(12, 12, 22),
                Height = 120,
                Padding = new Padding(10)
            };

            _rtbInput = new RichTextBox
            {
                BackColor = C(28, 28, 45),
                ForeColor = C(215, 215, 235),
                Font = new Font("Segoe UI", 10.5f),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Size = new Size(780, 90),
                Location = new Point(10, 15)
            };
            SetPlaceholder(_rtbInput, "Скажите что сделать... AI увидит экран, напишет и выполнит код");
            _rtbInput.KeyDown += InputKeyDown;
            _rtbInput.GotFocus += (s, e) => ClearPlaceholder(_rtbInput);
            _rtbInput.LostFocus += (s, e) => SetPlaceholderIfEmpty(_rtbInput, "Скажите что сделать...");

            _btnSend = new Button
            {
                Text = "➤ Отправить\n(Ctrl+Enter)",
                Size = new Size(130, 44),
                Location = new Point(800, 15),
                BackColor = C(50, 110, 220),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            _btnSend.FlatAppearance.BorderSize = 0;
            _btnSend.Click += async (s, e) => await SendMessage();

            _btnExecCode = new Button
            {
                Text = "💻 Только код",
                Size = new Size(130, 44),
                Location = new Point(800, 65),
                BackColor = C(30, 80, 30),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            _btnExecCode.FlatAppearance.BorderSize = 0;
            _btnExecCode.Click += async (s, e) => await RequestCodeOnly();

            _chkAutoExec = new CheckBox
            {
                Text = "Авто-выполнить код",
                Location = new Point(940, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = C(160, 160, 190),
                Checked = true
            };

            _lblHint = new Label
            {
                Text = "Ctrl+Enter — отправить  |  F5 — захват экрана  |  Shift+Enter — новая строка",
                Location = new Point(940, 45),
                Size = new Size(240, 40),
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = C(80, 80, 110),
                AutoSize = false
            };

            _panelInput.Controls.AddRange(new Control[] { _rtbInput, _btnSend, _btnExecCode, _chkAutoExec, _lblHint });
        }

        // ─────────────────────────────────────────────────────────────
        //  LAYOUT
        // ─────────────────────────────────────────────────────────────
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            int top = 52;
            int inputH = 120;
            int h = ClientSize.Height - top - inputH;
            _splitMain.SetBounds(0, top, ClientSize.Width, h);
            _panelInput.SetBounds(0, top + h, ClientSize.Width, inputH);
            LayoutRightPanel();
            LayoutScreenPanel();
        }

        private void LayoutRightPanel()
        {
            if (_splitMain?.Panel2 == null) return;
            var p = _splitMain.Panel2;
            int codeH = _codePanelVisible ? 220 : 0;
            int progressH = _progress.Visible ? 3 : 0;
            int chatH = p.Height - codeH - progressH;
            if (chatH < 50) chatH = 50;

            _progress.SetBounds(0, 0, p.Width, progressH > 0 ? progressH : 0);
            _rtbChat.SetBounds(0, progressH, p.Width, chatH);
            if (_codePanelVisible)
            {
                _panelCode.SetBounds(0, progressH + chatH, p.Width, codeH);

                var codeTitlePanel = _panelCode.Controls[1] as Panel;
                if (codeTitlePanel != null)
                {
                    _btnRunCode.Location = new Point(p.Width - 250, 4);
                    _btnCollapseCode.Location = new Point(p.Width - 38, 4);
                }
            }
        }

        private void LayoutScreenPanel()
        {
            if (_panelScreen == null || _picScreen == null) return;
            int ctrlH = 100;
            int w = _panelScreen.Width - 16;
            int h = _panelScreen.Height - ctrlH - 16;
            if (w < 10 || h < 10) return;

            _lblScreenInfo.SetBounds(8, ctrlH + 8, w, h);

            if (_picScreen.Image != null)
            {
                _lblScreenInfo.Visible = false;
                if (!_panelScreen.Controls.Contains(_picScreen))
                    _panelScreen.Controls.Add(_picScreen);
                _picScreen.SetBounds(8, ctrlH + 8, w, h);
                _picScreen.BringToFront();
            }
            else
            {
                _lblScreenInfo.Visible = true;
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  SCREEN CAPTURE
        // ─────────────────────────────────────────────────────────────
        private void CaptureScreen()
        {
            try
            {
                var bmp = ScreenCaptureService.CaptureWordWindow();
                _lastScreenshot = ScreenCaptureService.BitmapToBase64(bmp, 1280);

                if (_picScreen.Image != null) _picScreen.Image.Dispose();
                _picScreen.Image = bmp;

                if (!_panelScreen.Controls.Contains(_picScreen))
                    _panelScreen.Controls.Add(_picScreen);

                _lblScreenInfo.Visible = false;
                _picScreen.Visible = true;
                LayoutScreenPanel();

                _lblScreenInfo.Text = $"Захвачено: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                AppendChat(ChatRole.System, $"⚠ Ошибка захвата экрана: {ex.Message}");
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5) { CaptureScreen(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ─────────────────────────────────────────────────────────────
        //  SEND MESSAGE
        // ─────────────────────────────────────────────────────────────
        private async void InputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && e.Control)
            {
                e.SuppressKeyPress = true;
                await SendMessage();
            }
        }

        private async Task SendMessage()
        {
            if (_busy) return;
            var text = _rtbInput.Text.Trim();
            if (string.IsNullOrEmpty(text) || text == "Скажите что сделать...") return;

            _rtbInput.Clear();
            AppendChat(ChatRole.User, text);

            // Захват экрана перед отправкой если vision включен
            if (_chkVision.Checked)
                CaptureScreen();

            await ProcessMessage(text);
        }

        private async Task RequestCodeOnly()
        {
            if (_busy) return;
            var text = _rtbInput.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;
            _rtbInput.Clear();

            if (_chkVision.Checked) CaptureScreen();

            AppendChat(ChatRole.User, $"[Режим кода] {text}");

            SetBusy(true);
            try
            {
                var docContext = ThisAddIn.WordService.IsDocumentOpen()
                    ? ThisAddIn.WordService.GetDocumentText()
                    : "";

                var screenshot = _chkVision.Checked ? _lastScreenshot : null;
                var response = await _ollama.RequestCodeAsync(text, docContext, screenshot);

                _history.Add(new ChatMessage("user", text));
                _history.Add(new ChatMessage("assistant", response));

                var code = RoslynExecutor.ExtractCode(response);
                ShowGeneratedCode(code);
                AppendChat(ChatRole.Assistant, "Код сгенерирован 👆");

                if (_chkAutoExec.Checked)
                    ExecuteCurrentCode();
            }
            catch (Exception ex)
            {
                AppendChat(ChatRole.System, $"❌ Ошибка: {ex.Message}");
            }
            finally { SetBusy(false); }
        }

        private async Task ProcessMessage(string userText)
        {
            SetBusy(true);
            try
            {
                // Добавляем контекст документа к запросу
                var docInfo = "";
                if (ThisAddIn.WordService.IsDocumentOpen())
                {
                    var docText = ThisAddIn.WordService.GetDocumentText();
                    var stats = ThisAddIn.WordService.GetDocumentStats();
                    docInfo = $"\n\n[ДОКУМЕНТ: {ThisAddIn.WordService.GetDocumentName()}]\n{stats}\n" +
                              $"[СОДЕРЖИМОЕ (первые 1500 симв)]:\n{docText.Substring(0, Math.Min(1500, docText.Length))}";
                }

                var fullPrompt = userText + docInfo;
                var screenshot = _chkVision.Checked ? _lastScreenshot : null;

                var response = await _ollama.ChatWithVisionAsync(_history, fullPrompt, screenshot);

                _history.Add(new ChatMessage("user", userText));
                _history.Add(new ChatMessage("assistant", response));
                if (_history.Count > 50) _history.RemoveRange(0, 2);

                // Проверяем есть ли код в ответе
                var hasCode = response.Contains("```csharp") || response.Contains("```cs") || response.Contains("```");
                if (hasCode)
                {
                    var code = RoslynExecutor.ExtractCode(response);
                    // Показываем текстовую часть ответа
                    var textPart = System.Text.RegularExpressions.Regex.Replace(
                        response, @"```[\s\S]*?```", "").Trim();
                    if (!string.IsNullOrEmpty(textPart))
                        AppendChat(ChatRole.Assistant, textPart);

                    ShowGeneratedCode(code);

                    if (_chkAutoExec.Checked)
                    {
                        AppendChat(ChatRole.System, "⚡ Авто-выполнение кода...");
                        ExecuteCurrentCode();
                    }
                    else
                    {
                        AppendChat(ChatRole.System, "💻 Код готов. Нажмите ▶ Выполнить или используйте панель кода.");
                    }
                }
                else
                {
                    AppendChat(ChatRole.Assistant, response);
                }
            }
            catch (Exception ex)
            {
                AppendChat(ChatRole.System, $"❌ Ошибка: {ex.Message}");
            }
            finally { SetBusy(false); }
        }

        // ─────────────────────────────────────────────────────────────
        //  CODE EXECUTION
        // ─────────────────────────────────────────────────────────────
        private void ShowGeneratedCode(string code)
        {
            _lastGeneratedCode = code;
            _rtbCode.Text = code;
            ToggleCodePanel(true);
        }

        private void ExecuteCurrentCode()
        {
            var code = _rtbCode.Text.Trim();
            if (string.IsNullOrEmpty(code)) return;

            AppendChat(ChatRole.System, "⚙ Компилирую и выполняю...");

            var result = _roslyn.Execute(code);

            if (result.Success)
            {
                AppendChat(ChatRole.System, $"✅ {result.Output}");
            }
            else
            {
                var errMsg = new StringBuilder();
                errMsg.AppendLine("❌ Ошибка выполнения:");
                errMsg.AppendLine(result.Error);
                if (result.CompileErrors.Count > 0)
                {
                    errMsg.AppendLine("\nОшибки компиляции:");
                    foreach (var e in result.CompileErrors)
                        errMsg.AppendLine($"  • {e}");
                    errMsg.AppendLine("\n💡 Отправляю ошибку AI для исправления...");
                }
                AppendChat(ChatRole.System, errMsg.ToString());

                // Автоматически отправляем ошибку обратно AI
                if (result.CompileErrors.Count > 0)
                    _ = FixCodeWithAI(code, result.Error);
            }
        }

        private async Task FixCodeWithAI(string brokenCode, string error)
        {
            SetBusy(true);
            try
            {
                var fixPrompt = $"Этот C# код вызвал ошибку:\n```csharp\n{brokenCode}\n```\n\nОшибка: {error}\n\nИсправь код и верни только исправленный вариант в блоке ```csharp```.";
                var response = await _ollama.ChatWithVisionAsync(_history, fixPrompt, null);

                var fixedCode = RoslynExecutor.ExtractCode(response);
                ShowGeneratedCode(fixedCode);
                AppendChat(ChatRole.Assistant, "Код исправлен, попробуйте выполнить снова.");

                if (_chkAutoExec.Checked)
                    ExecuteCurrentCode();
            }
            catch (Exception ex)
            {
                AppendChat(ChatRole.System, $"Не удалось исправить: {ex.Message}");
            }
            finally { SetBusy(false); }
        }

        private void ToggleCodePanel(bool show)
        {
            _codePanelVisible = show;
            _panelCode.Visible = show;
            LayoutRightPanel();
        }

        // ─────────────────────────────────────────────────────────────
        //  CHAT DISPLAY
        // ─────────────────────────────────────────────────────────────
        private enum ChatRole { User, Assistant, System }

        private void AppendChat(ChatRole role, string text)
        {
            if (_rtbChat.InvokeRequired) { _rtbChat.Invoke(new Action(() => AppendChat(role, text))); return; }

            _rtbChat.SelectionStart = _rtbChat.TextLength;

            switch (role)
            {
                case ChatRole.User:
                    _rtbChat.SelectionColor = C(100, 180, 255);
                    _rtbChat.SelectionFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                    _rtbChat.AppendText("\n👤 Вы\n");
                    _rtbChat.SelectionColor = C(190, 210, 250);
                    _rtbChat.SelectionFont = new Font("Segoe UI", 10f);
                    _rtbChat.AppendText(text + "\n");
                    break;

                case ChatRole.Assistant:
                    _rtbChat.SelectionColor = C(80, 220, 120);
                    _rtbChat.SelectionFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                    _rtbChat.AppendText("\n🤖 Agent\n");
                    _rtbChat.SelectionColor = C(200, 240, 200);
                    _rtbChat.SelectionFont = new Font("Segoe UI", 10f);
                    _rtbChat.AppendText(text + "\n");
                    break;

                case ChatRole.System:
                    _rtbChat.SelectionColor = C(180, 160, 100);
                    _rtbChat.SelectionFont = new Font("Segoe UI", 9f, FontStyle.Italic);
                    _rtbChat.AppendText(text + "\n");
                    break;
            }

            _rtbChat.ScrollToCaret();
        }

        private void ClearChat()
        {
            _history.Clear();
            _rtbChat.Clear();
            AppendChat(ChatRole.System, "Чат очищен. Новая сессия.");
        }

        // ─────────────────────────────────────────────────────────────
        //  HELPERS
        // ─────────────────────────────────────────────────────────────
        private void SetBusy(bool busy)
        {
            if (_progress.InvokeRequired) { _progress.Invoke(new Action(() => SetBusy(busy))); return; }
            _busy = busy;
            _progress.Visible = busy;
            _btnSend.Enabled = !busy;
            _btnExecCode.Enabled = !busy;
            LayoutRightPanel();
        }

        private async void CheckConnectionAsync()
        {
            var ok = await _ollama.TestConnectionAsync();
            _lblStatus.Text = ok ? "● Подключено" : "● Нет связи";
            _lblStatus.ForeColor = ok ? Color.LimeGreen : Color.OrangeRed;
        }

        private void ApplyTheme()
        {
            // Tab stop order
            _rtbInput.TabStop = true;
            _btnSend.TabStop = true;
        }

        private Button MakeIconBtn(string text, string tip, Point loc, int size)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI Emoji", 11f),
                Size = new Size(size, size),
                Location = loc,
                BackColor = C(35, 35, 55),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            new ToolTip().SetToolTip(btn, tip);
            return btn;
        }

        private void SetPlaceholder(RichTextBox rtb, string text)
        {
            rtb.Text = text;
            rtb.ForeColor = C(80, 80, 110);
        }

        private void ClearPlaceholder(RichTextBox rtb)
        {
            if (rtb.ForeColor == C(80, 80, 110))
            {
                rtb.Clear();
                rtb.ForeColor = C(215, 215, 235);
            }
        }

        private void SetPlaceholderIfEmpty(RichTextBox rtb, string text)
        {
            if (string.IsNullOrWhiteSpace(rtb.Text))
                SetPlaceholder(rtb, text);
        }

        private static Color C(int r, int g, int b) => Color.FromArgb(r, g, b);

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _autoCaptureTimer?.Stop();
            _autoCaptureTimer?.Dispose();
            base.OnFormClosed(e);
        }
    }
}

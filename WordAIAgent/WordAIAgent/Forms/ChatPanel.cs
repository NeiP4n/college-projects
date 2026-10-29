using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WordAIAgent.Models;
using WordAIAgent.Services;

namespace WordAIAgent.Forms
{
    public class ChatPanel : UserControl
    {
        private readonly OllamaClient _client = new OllamaClient();
        private readonly ConversationHistory _history = new ConversationHistory(40);
        private readonly WordCommandExecutor _executor;

        private readonly Panel _headerPanel = new Panel();
        private readonly Label _lblTitle = new Label();
        private readonly Label _lblStatus = new Label();
        private readonly Label _lblHost = new Label();
        private readonly TextBox _txtHost = new TextBox();
        private readonly Button _btnApplyHost = new Button();
        private readonly Label _lblModel = new Label();
        private readonly ComboBox _cmbModels = new ComboBox();
        private readonly Label _lblRights = new Label();
        private readonly ComboBox _cmbRights = new ComboBox();

        private readonly RichTextBox _chatBox = new RichTextBox();
        private readonly Panel _inputPanel = new Panel();
        private readonly TextBox _txtInput = new TextBox();
        private readonly Button _btnSend = new Button();
        private readonly Timer _focusTimer = new Timer();

        private bool _busy;
        private string _lastExecutedCommand;

        public ChatPanel()
        {
            _executor = new WordCommandExecutor(ThisAddIn.WordService);
            BuildUi();
            LoadSettings();

            _ = RefreshModelsAsync();
            _ = CheckConnectionAsync();

            _focusTimer.Interval = 120;
            _focusTimer.Tick += (_, __) =>
            {
                _focusTimer.Stop();
                FocusInput();
            };

            Load += (_, __) => RequestFocus();
            VisibleChanged += (_, __) =>
            {
                if (Visible)
                    RequestFocus();
            };
        }

        private void BuildUi()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(20, 20, 30);

            _headerPanel.Dock = DockStyle.Top;
            _headerPanel.Height = 96;
            _headerPanel.BackColor = Color.FromArgb(15, 15, 25);

            _lblTitle.Text = "AI Agent";
            _lblTitle.ForeColor = Color.FromArgb(120, 200, 255);
            _lblTitle.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            _lblTitle.Location = new Point(10, 6);
            _lblTitle.AutoSize = true;

            _lblStatus.Text = "Disconnected";
            _lblStatus.ForeColor = Color.OrangeRed;
            _lblStatus.Font = new Font("Segoe UI", 8f);
            _lblStatus.Location = new Point(120, 10);
            _lblStatus.AutoSize = true;

            _lblHost.Text = "Host";
            _lblHost.ForeColor = Color.Silver;
            _lblHost.Font = new Font("Segoe UI", 8f);
            _lblHost.Location = new Point(10, 32);
            _lblHost.AutoSize = true;

            _txtHost.Size = new Size(200, 20);
            _txtHost.Location = new Point(48, 30);
            _txtHost.BackColor = Color.FromArgb(38, 38, 55);
            _txtHost.ForeColor = Color.White;
            _txtHost.BorderStyle = BorderStyle.FixedSingle;

            _btnApplyHost.Text = "Apply";
            _btnApplyHost.Size = new Size(60, 23);
            _btnApplyHost.Location = new Point(255, 28);
            _btnApplyHost.BackColor = Color.FromArgb(50, 50, 70);
            _btnApplyHost.ForeColor = Color.White;
            _btnApplyHost.FlatStyle = FlatStyle.Flat;
            _btnApplyHost.Click += async (_, __) => await ApplyHostAsync();

            _lblModel.Text = "Model";
            _lblModel.ForeColor = Color.Silver;
            _lblModel.Font = new Font("Segoe UI", 8f);
            _lblModel.Location = new Point(10, 58);
            _lblModel.AutoSize = true;

            _cmbModels.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbModels.Size = new Size(200, 21);
            _cmbModels.Location = new Point(48, 56);
            _cmbModels.BackColor = Color.FromArgb(38, 38, 55);
            _cmbModels.ForeColor = Color.White;
            _cmbModels.SelectedIndexChanged += (_, __) => UpdateModelSelection();

            _lblRights.Text = "Права";
            _lblRights.ForeColor = Color.Silver;
            _lblRights.Font = new Font("Segoe UI", 8f);
            _lblRights.Location = new Point(260, 58);
            _lblRights.AutoSize = true;

            _cmbRights.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbRights.Size = new Size(120, 21);
            _cmbRights.Location = new Point(305, 56);
            _cmbRights.BackColor = Color.FromArgb(38, 38, 55);
            _cmbRights.ForeColor = Color.White;
            _cmbRights.Items.AddRange(new object[] { "Подтверждать", "Полные" });
            _cmbRights.SelectedIndexChanged += (_, __) => UpdateRightsSelection();

            _headerPanel.Controls.AddRange(new Control[]
            {
                _lblTitle, _lblStatus, _lblHost, _txtHost, _btnApplyHost, _lblModel, _cmbModels, _lblRights, _cmbRights
            });
            _headerPanel.Click += (_, __) => FocusInput();

            _chatBox.Dock = DockStyle.Fill;
            _chatBox.ReadOnly = true;
            _chatBox.TabStop = false;
            _chatBox.BackColor = Color.FromArgb(22, 22, 35);
            _chatBox.ForeColor = Color.FromArgb(220, 220, 240);
            _chatBox.BorderStyle = BorderStyle.None;
            _chatBox.Font = new Font("Segoe UI", 10f);
            _chatBox.Click += (_, __) => RequestFocus();
            _chatBox.MouseDown += (_, __) => RequestFocus();

            _inputPanel.Dock = DockStyle.Bottom;
            _inputPanel.Height = 70;
            _inputPanel.Padding = new Padding(10);
            _inputPanel.BackColor = Color.FromArgb(15, 15, 25);

            _txtInput.Multiline = true;
            _txtInput.Height = 45;
            _txtInput.Width = 320;
            _txtInput.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _txtInput.BackColor = Color.FromArgb(35, 35, 50);
            _txtInput.ForeColor = Color.FromArgb(220, 220, 240);
            _txtInput.BorderStyle = BorderStyle.FixedSingle;
            _txtInput.TabStop = true;
            _txtInput.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    e.SuppressKeyPress = true;
                    await SendMessageAsync();
                }
            };

            _btnSend.Text = "Send";
            _btnSend.Size = new Size(70, 28);
            _btnSend.Location = new Point(340, 18);
            _btnSend.BackColor = Color.FromArgb(60, 120, 220);
            _btnSend.ForeColor = Color.White;
            _btnSend.FlatStyle = FlatStyle.Flat;
            _btnSend.Click += async (_, __) => await SendMessageAsync();

            _inputPanel.Controls.Add(_txtInput);
            _inputPanel.Controls.Add(_btnSend);
            _inputPanel.Click += (_, __) => RequestFocus();

            Controls.Add(_chatBox);
            Controls.Add(_inputPanel);
            Controls.Add(_headerPanel);
        }

        private void LoadSettings()
        {
            _txtHost.Text = ThisAddIn.Settings.OllamaHost;
            AppendSystemMessage("ИИ агент готов. Используйте команды с тегами [CMD:...].");
            var mode = (ThisAddIn.Settings.ExecutionMode ?? "manual").Trim().ToLowerInvariant();
            _cmbRights.SelectedIndex = mode == "full" ? 1 : 0;
        }

        private async Task ApplyHostAsync()
        {
            var host = (_txtHost.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(host))
            {
                AppendSystemMessage("Host is empty.");
                return;
            }

            ThisAddIn.Settings.OllamaHost = host;
            ThisAddIn.SaveSettings();
            await RefreshModelsAsync();
            await CheckConnectionAsync();
        }

        private async Task RefreshModelsAsync()
        {
            try
            {
                var models = await _client.GetModelsAsync(ThisAddIn.Settings.OllamaHost);
                Ui(() =>
                {
                    if (models == null || models.Count == 0)
                        return;

                    _cmbModels.Items.Clear();
                    foreach (var model in models)
                        _cmbModels.Items.Add(model);

                    var current = ThisAddIn.Settings.ModelName;
                    if (!string.IsNullOrWhiteSpace(current) && _cmbModels.Items.Contains(current))
                        _cmbModels.SelectedItem = current;
                    else if (_cmbModels.Items.Count > 0)
                        _cmbModels.SelectedIndex = 0;
                });
            }
            catch (Exception ex)
            {
                AppendSystemMessage("Model refresh failed: " + ex.Message);
            }
        }

        private void UpdateModelSelection()
        {
            var selected = _cmbModels.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(selected))
                return;

            ThisAddIn.Settings.ModelName = selected;
            ThisAddIn.SaveSettings();
        }

        private void UpdateRightsSelection()
        {
            var index = _cmbRights.SelectedIndex;
            ThisAddIn.Settings.ExecutionMode = index == 1 ? "full" : "manual";
            ThisAddIn.SaveSettings();
        }

        private async Task CheckConnectionAsync()
        {
            try
            {
                var models = await _client.GetModelsAsync(ThisAddIn.Settings.OllamaHost);
                var connected = models != null && models.Count > 0;
                Ui(() =>
                {
                    _lblStatus.Text = connected ? "Connected" : "Disconnected";
                    _lblStatus.ForeColor = connected ? Color.LimeGreen : Color.OrangeRed;
                });
            }
            catch
            {
                Ui(() =>
                {
                    _lblStatus.Text = "Disconnected";
                    _lblStatus.ForeColor = Color.OrangeRed;
                });
            }
        }

        private async Task SendMessageAsync()
        {
            if (_busy)
                return;

            var text = _txtInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return;

            _txtInput.Clear();
            AppendUserMessage(text);
            SetBusy(true);

            try
            {
                var systemPrompt = BuildSystemPrompt();
                var context = DocumentContextBuilder.Build();
                var userPayload = text + "\n\n[CONTEXT]\n" + context;

                var messages = new List<ChatMessage>
                {
                    new ChatMessage("system", systemPrompt)
                };

                foreach (var msg in _history.Items)
                    messages.Add(msg);

                messages.Add(new ChatMessage("user", userPayload));

                var response = await _client.ChatAsync(
                    ThisAddIn.Settings.OllamaHost,
                    ThisAddIn.Settings.ModelName,
                    messages,
                    ThisAddIn.Settings.Temperature,
                    ThisAddIn.Settings.MaxTokens);

                var displayText = CommandParser.StripCommands(response);
                AppendAssistantMessage(string.IsNullOrWhiteSpace(displayText) ? response : displayText);

                _history.AddUser(text);
                _history.AddAssistant(response);

                var parsed = CommandParser.Parse(response);
                var commandsToRun = parsed.Commands;
                if (commandsToRun.Count == 0)
                {
                    if (LooksLikeJsonProtocol(response))
                        AppendSystemMessage("Ответ в формате JSON, команды не выполнены. Проверьте системный промпт.");

                    var fallback = SimpleCommandInterpreter.TryParse(text, _lastExecutedCommand);
                    if (fallback.Count > 0)
                    {
                        AppendSystemMessage("Команда распознана локально и будет выполнена.");
                        commandsToRun = fallback;
                    }
                }

                foreach (var unknown in parsed.UnknownCommands.Distinct())
                    AppendSystemMessage("Unknown command ignored: " + unknown);

                foreach (var cmd in commandsToRun)
                {
                    if (!ShouldExecute(cmd))
                    {
                        AppendSystemMessage($"Команда пропущена: {cmd.Name}");
                        continue;
                    }

                    var result = _executor.Execute(cmd);
                    if (!string.IsNullOrWhiteSpace(result.Message))
                        AppendSystemMessage(result.Message);
                }

                if (commandsToRun.Count > 0)
                    _lastExecutedCommand = commandsToRun.Last().Name;
            }
            catch (Exception ex)
            {
                AppendSystemMessage("Error: " + ex.Message);
            }
            finally
            {
                SetBusy(false);
                RequestFocus();
            }
        }

        private bool ShouldExecute(ParsedCommand command)
        {
            var mode = (ThisAddIn.Settings.ExecutionMode ?? "manual").Trim().ToLowerInvariant();
            if (mode == "full")
                return true;

            if (CommandCatalog.IsDestructive(command.Name))
                return true;

            var preview = command.Payload ?? string.Empty;
            if (preview.Length > 120)
                preview = preview.Substring(0, 120) + "...";

            var message = string.IsNullOrWhiteSpace(preview)
                ? $"Выполнить команду {command.Name}?"
                : $"Выполнить команду {command.Name} с параметром:\n{preview}";

            return MessageBox.Show(
                message,
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private static bool LooksLikeJsonProtocol(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return false;

            var value = response.Trim();
            if (!value.StartsWith("{") && !value.StartsWith("["))
                return false;

            return value.IndexOf("\"action\"", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   value.IndexOf("\"actions\"", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string BuildSystemPrompt()
        {
            var basePrompt = ThisAddIn.Settings.SystemPrompt ?? string.Empty;
            return basePrompt.Trim() + "\n\n" + CommandCatalog.BuildPrompt();
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            Ui(() =>
            {
                _btnSend.Enabled = !busy;
                _btnApplyHost.Enabled = !busy;
                _cmbModels.Enabled = !busy;
                _txtHost.Enabled = !busy;
                if (!busy)
                    RequestFocus();
            });
        }

        private void AppendUserMessage(string text)
        {
            AppendMessage("You:", text, Color.FromArgb(120, 200, 255), Color.FromArgb(220, 230, 255));
        }

        private void AppendAssistantMessage(string text)
        {
            AppendMessage("AI:", text, Color.FromArgb(120, 220, 120), Color.FromArgb(220, 240, 220));
        }

        private void AppendSystemMessage(string text)
        {
            AppendMessage("System:", text, Color.FromArgb(230, 180, 120), Color.FromArgb(230, 200, 160));
        }

        private void AppendMessage(string header, string text, Color headerColor, Color textColor)
        {
            if (_chatBox.InvokeRequired)
            {
                _chatBox.BeginInvoke(new Action(() => AppendMessage(header, text, headerColor, textColor)));
                return;
            }

            _chatBox.SelectionStart = _chatBox.TextLength;
            _chatBox.SelectionColor = headerColor;
            _chatBox.SelectionFont = new Font("Segoe UI", 9f, FontStyle.Bold);
            _chatBox.AppendText("\n" + header + "\n");

            _chatBox.SelectionColor = textColor;
            _chatBox.SelectionFont = new Font("Segoe UI", 10f);
            _chatBox.AppendText((text ?? string.Empty).Trim() + "\n");
            _chatBox.ScrollToCaret();
        }

        private void Ui(Action action)
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(action);
                return;
            }

            action();
        }

        public void FocusInput()
        {
            Ui(() =>
            {
                if (!_busy)
                {
                    _txtInput.Enabled = true;
                    _txtInput.ReadOnly = false;
                }

                _txtInput.Focus();
                _txtInput.SelectionStart = _txtInput.TextLength;
                _txtInput.SelectionLength = 0;
            });
        }

        public void RequestFocus()
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(RequestFocus));
                return;
            }

            _focusTimer.Stop();
            _focusTimer.Start();
        }
    }
}

using Microsoft.Office.Tools.Ribbon;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;

namespace OllamaAI
{
    public partial class OllamaRibbon
    {
        private class InlineRun
        {
            public string Text;
            public bool Bold;
            public bool Italic;
            public bool Underline;
            public string Link;
        }

        private class ChartSpec
        {
            public string Type;
            public string Title;
            public List<string> Labels = new List<string>();
            public List<double> Values = new List<double>();
        }

        private enum PromptTargetKind
        {
            Cursor,
            ReplaceSelection,
            DocumentStart,
            DocumentEnd,
            AfterHeading,
            BeforeHeading
        }

        private class PromptInsertionPlan
        {
            public PromptTargetKind Target = PromptTargetKind.Cursor;
            public string Anchor = "";
            public bool ApplyGostRules;
        }

        private enum StructuredLineType
        {
            Plain,
            Heading,
            Bullet,
            Numbered
        }

        private OllamaService _ollama;
        private List<OllamaModel> _models = new List<OllamaModel>();
        private PluginSettings _settings;
        private string _selectedModel = "";
        private bool _loadingModels;
        private bool _updatingModelUi;
        private bool _updatingTemplateUi;
        private readonly Dictionary<string, string> _modelLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly System.Windows.Forms.Timer _statusTimer = new System.Windows.Forms.Timer();
        private OllamaStatus _lastStatus = new OllamaStatus();

        private List<KnowledgeTemplate> _templates = new List<KnowledgeTemplate>();
        private KnowledgeTemplate _selectedTemplate;
        private readonly Dictionary<string, string> _templateLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private bool _chatOpen;
        private ChatForm _chatForm;
        private DateTime _lastDiscoverAttemptUtc = DateTime.MinValue;
        private Control _uiInvoker;
        private CancellationTokenSource _activeAiCts;
        private Word.Document _activeAiDoc;
        private int _activeAiStart = -1;
        private int _activeAiEnd = -1;
        private Word.Document _lastAiDoc;
        private int _lastAiStart = -1;
        private int _lastAiEnd = -1;
        private int _forcedTableRows;
        private int _forcedTableCols;

        private Word.Application WordApp => Globals.ThisAddIn?.Application;

        private void OllamaRibbon_Load(object sender, RibbonUIEventArgs e)
        {
            try
            {
                AddinLog.WriteInfo("Ribbon_Load", "Ribbon load started");
                _settings = PluginSettings.Load();
                _ollama = new OllamaService(_settings.OllamaUrl, _settings.TimeoutSeconds);
                _selectedModel = _settings.LastModel ?? "";
                _uiInvoker = new Control();
                _uiInvoker.CreateControl();

                btnAutoRefresh.Checked = _settings.AutoRefreshEnabled;
                _statusTimer.Interval = Math.Max(5000, _settings.AutoRefreshSeconds * 1000);
                _statusTimer.Tick += StatusTimer_Tick;
                _statusTimer.Start();
                ApplyRibbonFallbackIcons();
                ConfigureRibbonVisibility();

                LoadKnowledgeBase();
                AddinLog.WriteInfo("Ribbon_Load", "Knowledge base loaded");
                SetStatus("Проверка Ollama...", false, 0);
                StartRefresh();
            }
            catch (Exception ex)
            {
                AddinLog.Write("Ribbon_Load", ex);
                MessageBox.Show(
                    "Ошибка загрузки надстройки. Детали сохранены в addin.log.",
                    "Ollama AI",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ConfigureRibbonVisibility()
        {
            groupWrite.Visible = false;
            groupReview.Visible = false;
            groupKnowledge.Visible = false;
            groupChat.Visible = false;
        }

        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            if (btnAutoRefresh.Checked)
            {
                StartRefresh();
            }
        }

        private async void StartRefresh()
        {
            if (_loadingModels)
            {
                return;
            }

            _loadingModels = true;
            try
            {
                RunOnUi(() =>
                {
                    btnRefresh.Enabled = false;
                    cboModel.Enabled = false;
                });
                SetStatus("Проверка Ollama...", false, 0);

                _lastStatus = await _ollama.GetStatusAsync();
                if (!_lastStatus.IsRunning)
                {
                    var discovered = await TryDiscoverLanAsync(false);
                    if (discovered)
                    {
                        _lastStatus = await _ollama.GetStatusAsync();
                    }

                    if (!_lastStatus.IsRunning)
                    {
                        _models.Clear();
                        UpdateModelControls();
                        SetStatus(_lastStatus.Message, false, 0);
                        return;
                    }
                }

                _models = await _ollama.GetModelsAsync();
                EnsureSelectedModel();
                UpdateModelControls();
                SetStatus("Подключено", true, _models.Count);
            }
            catch (Exception ex)
            {
                _models.Clear();
                UpdateModelControls();
                SetStatus(ex.Message, false, 0);
            }
            finally
            {
                RunOnUi(() =>
                {
                    btnRefresh.Enabled = true;
                    cboModel.Enabled = true;
                    lblEndpoint.Label = ShortUrl(_settings.OllamaUrl);
                });
                _loadingModels = false;
            }
        }

        private void SetStatus(string details, bool online, int modelCount)
        {
            RunOnUi(() =>
            {
                var state = online ? "[ON]" : "[OFF]";
                var version = !string.IsNullOrWhiteSpace(_lastStatus.Version) ? " v" + _lastStatus.Version : "";
                var models = online ? (" | моделей: " + modelCount) : "";
                lblStatus.Label = state + version + models;

                if (online)
                {
                    lblConnectionInfo.Label = "Ollama запущен";
                }
                else
                {
                    var detailText = string.IsNullOrWhiteSpace(details) ? "Недоступно" : details;
                    if (detailText.Length > 42)
                    {
                        detailText = detailText.Substring(0, 42) + "...";
                    }

                    lblConnectionInfo.Label = detailText;
                }
            });
        }

        private void EnsureSelectedModel()
        {
            if (_models.Count == 0)
            {
                _selectedModel = "";
                _settings.LastModel = "";
                PluginSettings.Save(_settings);
                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedModel))
            {
                _selectedModel = _models[0].Name;
            }

            var exact = _models.FirstOrDefault(x => string.Equals(x.Name, _selectedModel, StringComparison.OrdinalIgnoreCase));
            if (exact == null)
            {
                var display = _models.FirstOrDefault(x => string.Equals(x.DisplayName, _selectedModel, StringComparison.OrdinalIgnoreCase));
                _selectedModel = display != null ? display.Name : _models[0].Name;
            }

            _settings.LastModel = _selectedModel;
            PluginSettings.Save(_settings);
        }

        private void UpdateModelControls()
        {
            RunOnUi(() =>
            {
                _updatingModelUi = true;
                _modelLookup.Clear();
                cboModel.Items.Clear();

                foreach (var model in _models)
                {
                    var label = model.Name;
                    if (!string.IsNullOrWhiteSpace(model.SizeDisplay))
                    {
                        label += " (" + model.SizeDisplay + ")";
                    }

                    var item = Factory.CreateRibbonDropDownItem();
                    item.Label = label;
                    cboModel.Items.Add(item);

                    _modelLookup[label] = model.Name;
                    _modelLookup[model.Name] = model.Name;
                    _modelLookup[model.DisplayName] = model.Name;
                }

                cboModel.Text = _models.Count == 0 ? "Нет моделей" : _selectedModel;
                _updatingModelUi = false;
            });
        }

        private void cboModel_TextChanged(object sender, RibbonControlEventArgs e)
        {
            if (_updatingModelUi)
            {
                return;
            }

            var text = (cboModel.Text ?? "").Trim();
            if (text.Length == 0)
            {
                return;
            }

            if (_modelLookup.ContainsKey(text))
            {
                _selectedModel = _modelLookup[text];
            }
            else
            {
                var prefix = text.Split('(')[0].Trim();
                var found = _models.FirstOrDefault(m =>
                    m.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
                    m.DisplayName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

                if (found != null)
                {
                    _selectedModel = found.Name;
                }
                else
                {
                    _selectedModel = text;
                }
            }

            _settings.LastModel = _selectedModel;
            PluginSettings.Save(_settings);
        }

        private void btnRefresh_Click(object sender, RibbonControlEventArgs e)
        {
            StartRefresh();
        }

        private async void btnDiscoverLan_Click(object sender, RibbonControlEventArgs e)
        {
            var found = await TryDiscoverLanAsync(true);
            if (!found)
            {
                return;
            }

            StartRefresh();
        }

        private void btnSettings_Click(object sender, RibbonControlEventArgs e)
        {
            using (var f = new SettingsForm(_settings))
            {
                if (f.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                _settings = f.Settings;
                _ollama.SetBaseUrl(_settings.OllamaUrl);
                _ollama.SetTimeoutSeconds(_settings.TimeoutSeconds);
                _statusTimer.Interval = Math.Max(5000, _settings.AutoRefreshSeconds * 1000);
                PluginSettings.Save(_settings);
                StartRefresh();
            }
        }

        private void btnAutoRefresh_Click(object sender, RibbonControlEventArgs e)
        {
            _settings.AutoRefreshEnabled = btnAutoRefresh.Checked;
            PluginSettings.Save(_settings);
            if (btnAutoRefresh.Checked)
            {
                StartRefresh();
            }
        }

        private async System.Threading.Tasks.Task<bool> TryDiscoverLanAsync(bool manual)
        {
            if (!manual)
            {
                if (!_settings.AutoDiscoverLan)
                {
                    return false;
                }

                if ((DateTime.UtcNow - _lastDiscoverAttemptUtc).TotalSeconds < 30)
                {
                    return false;
                }

                if (!ShouldAutoDiscoverForCurrentEndpoint())
                {
                    return false;
                }
            }

            _lastDiscoverAttemptUtc = DateTime.UtcNow;
            RunOnUi(() => { btnDiscoverLan.Enabled = false; });
            try
            {
                SetStatus("Поиск Ollama в локальной сети...", false, 0);
                var foundUrl = await _ollama.DiscoverLanEndpointAsync();
                if (string.IsNullOrWhiteSpace(foundUrl))
                {
                    if (manual)
                    {
                        MessageBox.Show(
                            "Не удалось найти Ollama в локальной сети.\n\nПроверьте на удаленном ПК:\n1) запущен ollama serve\n2) в брандмауэре открыт TCP-порт 11434",
                            "Поиск в LAN",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }

                    return false;
                }

                _settings.OllamaUrl = foundUrl;
                _settings.LastDiscoveredUrl = foundUrl;
                PluginSettings.Save(_settings);
                _ollama.SetBaseUrl(foundUrl);
                RunOnUi(() => { lblEndpoint.Label = ShortUrl(foundUrl); });

                if (manual)
                {
                    MessageBox.Show("Найден Ollama: " + foundUrl, "Поиск в LAN", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                RunOnUi(() => { btnDiscoverLan.Enabled = true; });
            }
        }

        private bool ShouldAutoDiscoverForCurrentEndpoint()
        {
            var url = (_settings.OllamaUrl ?? "").Trim().ToLowerInvariant();
            if (url.Length == 0)
            {
                return true;
            }

            if (url.Contains("localhost") || url.Contains("127.0.0.1"))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(_settings.LastDiscoveredUrl) &&
                string.Equals(_settings.LastDiscoveredUrl.Trim(), _settings.OllamaUrl.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private bool EnsureModel()
        {
            if (!string.IsNullOrWhiteSpace(_selectedModel))
            {
                return true;
            }

            MessageBox.Show(
                "Модель не выбрана.\n\n1) Запустите Ollama: ollama serve\n2) Скачайте модель: ollama pull llama3.2\n3) Нажмите «Обновить»",
                "Требуется модель",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        private bool EnsureTemplate()
        {
            if (_selectedTemplate != null)
            {
                return true;
            }

            MessageBox.Show(
                "Шаблон не выбран.\nОткройте «База знаний» и создайте/выберите шаблон.",
                "Требуется шаблон",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        private async void RunAI(string title, Func<System.Threading.Tasks.Task<string>> fn, Action<string> apply)
        {
            if (!EnsureModel())
            {
                return;
            }

            var progress = new ProgressForm(title);
            RunOnUi(() =>
            {
                progress.Show();
                Application.DoEvents();
            });

            try
            {
                var result = await fn();
                RunOnUi(() =>
                {
                    progress.Close();
                    apply(result);
                });
            }
            catch (Exception ex)
            {
                RunOnUi(() =>
                {
                    progress.Close();
                    MessageBox.Show("Ошибка ИИ: " + ex.Message, "Ollama AI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }

        private async void RunAIReplaceSelectedLive(string title, Func<Action<string>, CancellationToken, System.Threading.Tasks.Task<string>> streamFn, bool trackRevisions)
        {
            await RunAILiveCore(title, streamFn, true, trackRevisions, NormalizeForWordSmart, null);
        }

        private async void RunAIInsertLive(string title, Func<Action<string>, CancellationToken, System.Threading.Tasks.Task<string>> streamFn)
        {
            await RunAILiveCore(title, streamFn, false, false, NormalizeForWordSmart, null);
        }

        private async void RunAIGostInsertLive(string title, Func<Action<string>, CancellationToken, System.Threading.Tasks.Task<string>> streamFn)
        {
            await RunAILiveCore(
                title,
                streamFn,
                false,
                false,
                NormalizeGostDraftText,
                finalRange => ApplyGostHeadingStylesToRange(finalRange));
        }

        private async System.Threading.Tasks.Task RunAILiveCore(
            string title,
            Func<Action<string>, CancellationToken, System.Threading.Tasks.Task<string>> streamFn,
            bool replaceSelection,
            bool trackRevisions,
            Func<string, string> normalizeFinal,
            Action<Word.Range> afterApply,
            Func<Word.Application, Word.Range> targetResolver = null,
            bool keepProvidedPosition = false,
            bool allowWordCommands = false,
            bool allowChartBlocks = false)
        {
            if (!EnsureModel())
            {
                return;
            }

            var app = WordApp;
            if (app?.ActiveDocument == null || app.Selection == null)
            {
                return;
            }

            if (replaceSelection && targetResolver == null && app.Selection.Type != Word.WdSelectionType.wdSelectionNormal)
            {
                NoSelection("Сначала выделите текст.");
                return;
            }

            var doc = app.ActiveDocument;
            Word.Range selectionRange = null;
            if (targetResolver != null)
            {
                selectionRange = targetResolver(app);
            }

            if (selectionRange == null)
            {
                selectionRange = app.Selection.Range.Duplicate;
            }

            if (!replaceSelection && (!keepProvidedPosition || targetResolver == null))
            {
                selectionRange.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
            }

            var start = selectionRange.Start;
            if (replaceSelection)
            {
                selectionRange.Text = "";
            }

            CancelActiveAi(deleteOutput: false, silent: true);
            var cts = new CancellationTokenSource();
            _activeAiCts = cts;
            _activeAiDoc = doc;
            _activeAiStart = start;
            _activeAiEnd = start;

            var liveCaret = doc.Range(start, start);
            var collected = new StringBuilder();
            var progress = new ProgressForm(title + " (в реальном времени)");

            RunOnUi(() =>
            {
                progress.Show();
                Application.DoEvents();
            });

            try
            {
                var result = await streamFn(chunk =>
                {
                    if (cts.IsCancellationRequested || string.IsNullOrEmpty(chunk))
                    {
                        return;
                    }

                    collected.Append(chunk);
                    RunOnUi(() =>
                    {
                        if (cts.IsCancellationRequested)
                        {
                            return;
                        }

                        liveCaret.InsertAfter(chunk);
                        liveCaret.SetRange(liveCaret.End, liveCaret.End);
                        _activeAiEnd = liveCaret.End;
                    });
                }, cts.Token);

                if (collected.Length == 0 && !string.IsNullOrWhiteSpace(result))
                {
                    collected.Append(result);
                }

                RunOnUi(() =>
                {
                    var finalText = normalizeFinal != null
                        ? normalizeFinal(collected.ToString())
                        : NormalizeForWordSmart(collected.ToString());
                    var targetRange = doc.Range(start, liveCaret.End);
                    var prevTrack = doc.TrackRevisions;
                    if (trackRevisions)
                    {
                        doc.TrackRevisions = true;
                    }

                    Word.Range finalRange;
                    try
                    {
                        finalRange = ReplaceRangeWithRichContent(targetRange, finalText, allowWordCommands, allowChartBlocks);
                    }
                    finally
                    {
                        if (trackRevisions)
                        {
                            doc.TrackRevisions = prevTrack;
                        }
                    }

                    afterApply?.Invoke(finalRange);
                    finalRange.SetRange(finalRange.End, finalRange.End);
                    finalRange.Select();
                    _lastAiDoc = doc;
                    _lastAiStart = finalRange.Start;
                    _lastAiEnd = finalRange.End;
                });
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                RunOnUi(() =>
                {
                    MessageBox.Show("Ошибка ИИ: " + ex.Message, "Ollama AI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
            finally
            {
                if (ReferenceEquals(_activeAiCts, cts))
                {
                    _activeAiCts = null;
                }

                cts.Dispose();
                _activeAiDoc = null;
                _activeAiStart = -1;
                _activeAiEnd = -1;
                RunOnUi(() => { progress.Close(); });
            }
        }

        private async System.Threading.Tasks.Task RunTablePromptValidated(
            string contextualPrompt,
            string userPrompt,
            Word.Range target,
            bool replaceSelection,
            bool applyGostRules,
            int expectedTables,
            int expectedRows,
            int expectedCols)
        {
            if (!EnsureModel())
            {
                return;
            }

            var app = WordApp;
            if (app?.ActiveDocument == null || target == null)
            {
                return;
            }

            var requiredTables = expectedTables > 0 ? expectedTables : 1;
            var attempts = expectedTables > 1 ? 3 : 2;
            var progress = new ProgressForm("Генерирую таблицу...");
            RunOnUi(() =>
            {
                progress.Show();
                Application.DoEvents();
            });

            string finalText = "";
            var tableCount = 0;
            var prompt = contextualPrompt;
            if (expectedTables > 1)
            {
                var hint = new StringBuilder();
                hint.AppendLine(prompt);
                hint.AppendLine();
                hint.AppendLine("Дополнение: нужно ровно " + expectedTables + " таблиц.");
                if (expectedRows > 0 || expectedCols > 0)
                {
                    var rows = expectedRows > 0 ? expectedRows.ToString() : "?";
                    var cols = expectedCols > 0 ? expectedCols.ToString() : "?";
                    hint.AppendLine("Размер таблицы: " + cols + " столбца(ов) и " + rows + " строки(ок).");
                }
                hint.AppendLine("Если таблиц несколько, верни несколько команд @word:table, каждая с новой строки.");
                prompt = hint.ToString();
            }
            try
            {
                for (var attempt = 1; attempt <= attempts; attempt++)
                {
                    var result = await _ollama.GenerateAsync(
                        _selectedModel,
                        EnsureRussianPrompt(prompt),
                        _settings.SystemPrompt);

                    finalText = NormalizeTableOnly(result);
                    tableCount = CountTablesInText(finalText);
                    if (tableCount >= requiredTables)
                    {
                        break;
                    }

                    prompt = BuildTableRetryPrompt(contextualPrompt, requiredTables, expectedRows, expectedCols);
                }
            }
            catch (Exception ex)
            {
                RunOnUi(() =>
                {
                    MessageBox.Show("Ошибка ИИ: " + ex.Message, "Ollama AI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
                return;
            }
            finally
            {
                RunOnUi(() => { progress.Close(); });
            }

            if (tableCount <= 0)
            {
                RunOnUi(() =>
                {
                    MessageBox.Show(
                        "ИИ не вернул таблицу в корректном формате. Попробуйте уточнить запрос.",
                        "Ollama AI",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                });
                return;
            }

            if (tableCount < requiredTables)
            {
                RunOnUi(() =>
                {
                    MessageBox.Show(
                        "ИИ вернул меньше таблиц, чем требуется (ожидалось " + requiredTables + ", получено " + tableCount + ").",
                        "Ollama AI",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                });
            }

            if (!replaceSelection)
            {
                target.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
            }

            _forcedTableRows = expectedRows;
            _forcedTableCols = expectedCols;
            Word.Range finalRange = null;
            try
            {
                finalRange = ReplaceRangeWithRichContent(target, finalText, true, false);
            }
            finally
            {
                _forcedTableRows = 0;
                _forcedTableCols = 0;
            }
            if (finalRange != null)
            {
                if (applyGostRules)
                {
                    ApplyGostHeadingStylesToRange(finalRange);
                }

                ApplyPostFormattingIfRequested(userPrompt, app.ActiveDocument, finalRange);

                _lastAiDoc = app.ActiveDocument;
                _lastAiStart = finalRange.Start;
                _lastAiEnd = finalRange.End;
                finalRange.SetRange(finalRange.End, finalRange.End);
                finalRange.Select();
            }
        }

        private async void RunPromptSmartInsertLive(string userPrompt)
        {
            var prompt = (userPrompt ?? "").Trim();
            if (prompt.Length == 0)
            {
                NoSelection("Введите запрос.");
                return;
            }

            if (TryHandleControlPrompt(prompt))
            {
                return;
            }

            if (!EnsureModel())
            {
                return;
            }

            var app = WordApp;
            if (app?.ActiveDocument == null || app.Selection == null)
            {
                return;
            }

            var lowerPrompt = prompt.ToLowerInvariant();
            var selectionHasText = app.Selection.Type == Word.WdSelectionType.wdSelectionNormal &&
                                   app.Selection.Range != null &&
                                   app.Selection.Range.Start != app.Selection.Range.End;

            if (IsFormattingOnlyRequest(prompt))
            {
                ApplyPostFormattingIfRequested(prompt, app.ActiveDocument, selectionHasText ? app.Selection.Range.Duplicate : null);
                return;
            }

            var plan = BuildPromptInsertionPlan(prompt);
            var modifyIntent = IsModifyRequest(prompt);
            bool replaceSelection = false;
            string locationInfo = "в текущую позицию курсора";
            Word.Range target = null;
            var targetResolved = false;

            if (modifyIntent)
            {
                if (WantsDocumentScope(lowerPrompt))
                {
                    target = app.ActiveDocument.Content.Duplicate;
                    replaceSelection = true;
                    locationInfo = "замена всего документа";
                    targetResolved = true;
                }
                else if (selectionHasText)
                {
                    plan.Target = PromptTargetKind.ReplaceSelection;
                }
                else
                {
                    NoSelection("Выделите текст для изменения или укажите \"весь документ\".");
                    return;
                }
            }

            if (!targetResolved)
            {
                target = ResolvePromptTargetRange(app.ActiveDocument, app.Selection, plan, out replaceSelection, out locationInfo);
            }
            if (target == null)
            {
                return;
            }

            if (plan.ApplyGostRules)
            {
                ApplyGostLayout(app.ActiveDocument);
            }

            var isTableRequest = IsTableRequest(prompt);
            var isChartRequest = IsChartRequest(prompt);
            var contextualPrompt = BuildContextAwarePrompt(
                prompt,
                app.ActiveDocument,
                target,
                replaceSelection,
                locationInfo,
                plan.ApplyGostRules,
                isTableRequest,
                isChartRequest);

            if (isTableRequest)
            {
                var expectedTables = GetExpectedTableCount(prompt);
                int expectedRows;
                int expectedCols;
                GetExpectedTableDimensions(prompt, out expectedRows, out expectedCols);
                await RunTablePromptValidated(
                    contextualPrompt,
                    prompt,
                    target.Duplicate,
                    replaceSelection,
                    plan.ApplyGostRules,
                    expectedTables,
                    expectedRows,
                    expectedCols);
                return;
            }

            Action<Word.Range> afterApply = finalRange =>
            {
                if (plan.ApplyGostRules)
                {
                    ApplyGostHeadingStylesToRange(finalRange);
                }

                ApplyPostFormattingIfRequested(prompt, app.ActiveDocument, finalRange);
            };

            await RunAILiveCore(
                "Генерирую...",
                (onChunk, ct) => _ollama.GenerateStreamAsync(
                    _selectedModel,
                    EnsureRussianPrompt(contextualPrompt),
                    _settings.SystemPrompt,
                    onChunk,
                    ct),
                replaceSelection,
                false,
                plan.ApplyGostRules ? (Func<string, string>)NormalizeGostDraftText : (isTableRequest ? (Func<string, string>)NormalizeTableOnly : NormalizeForWordSmart),
                afterApply,
                _ => target.Duplicate,
                true,
                true,
                isChartRequest);
        }

        private async void RunAgentLoop(string userPrompt)
        {
            var prompt = (userPrompt ?? "").Trim();
            if (prompt.Length == 0)
            {
                return;
            }

            if (!EnsureModel())
            {
                return;
            }

            var app = WordApp;
            if (app?.ActiveDocument == null || app.Selection == null)
            {
                return;
            }

            var selectionHasText = app.Selection.Type == Word.WdSelectionType.wdSelectionNormal &&
                                   app.Selection.Range != null &&
                                   app.Selection.Range.Start != app.Selection.Range.End;

            if (IsFormattingOnlyRequest(prompt))
            {
                ApplyPostFormattingIfRequested(prompt, app.ActiveDocument, selectionHasText ? app.Selection.Range.Duplicate : null);
                return;
            }

            try
            {
                RunOnUi(() =>
                {
                    app.StatusBar = "Агент выполняет...";
                    Application.DoEvents();
                });
                var controller = new WordAgentController(_ollama, _settings.SystemPrompt);
                await controller.RunAsync(app, _selectedModel, prompt, RunOnUi);
                RunOnUi(() => { app.StatusBar = ""; });
            }
            catch (Exception ex)
            {
                RunOnUi(() =>
                {
                    app.StatusBar = "";
                    MessageBox.Show("Ошибка агента: " + ex.Message, "Ollama AI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }

        private static bool IsAgentPrompt(string prompt)
        {
            var text = (prompt ?? "").Trim().ToLowerInvariant();
            return text.StartsWith("agent:") || text.StartsWith("агент:") || text.StartsWith("/agent") || text.StartsWith("@agent");
        }

        private static string StripAgentPrefix(string prompt)
        {
            var text = (prompt ?? "").Trim();
            var lower = text.ToLowerInvariant();
            if (lower.StartsWith("agent:") || lower.StartsWith("агент:"))
            {
                return text.Substring(text.IndexOf(':') + 1).Trim();
            }

            if (lower.StartsWith("/agent"))
            {
                return text.Substring(6).Trim();
            }

            if (lower.StartsWith("@agent"))
            {
                return text.Substring(6).Trim();
            }

            return text;
        }

        private PromptInsertionPlan BuildPromptInsertionPlan(string prompt)
        {
            var text = (prompt ?? "").Trim();
            var lower = text.ToLowerInvariant();
            var plan = new PromptInsertionPlan();
            plan.ApplyGostRules =
                lower.Contains("гост") ||
                lower.Contains("техническое задание") ||
                lower.Contains("тз по гост");

            if (ContainsAny(lower, "в начало документа", "в начале документа", "в начало файла", "вначале", "в начале"))
            {
                plan.Target = PromptTargetKind.DocumentStart;
                return plan;
            }

            if (ContainsAny(lower, "в конец документа", "в конце документа", "в конец файла", "добавь в конец"))
            {
                plan.Target = PromptTargetKind.DocumentEnd;
                return plan;
            }

            var after = Regex.Match(text, @"после\s+(?:раздела|пункта|заголовка)\s+(?<anchor>""[^""]+""|«[^»]+»|[^,.;\r\n]+)", RegexOptions.IgnoreCase);
            if (after.Success)
            {
                plan.Target = PromptTargetKind.AfterHeading;
                plan.Anchor = NormalizeAnchor(after.Groups["anchor"].Value);
                return plan;
            }

            var before = Regex.Match(text, @"перед\s+(?:разделом|пунктом|заголовком)\s+(?<anchor>""[^""]+""|«[^»]+»|[^,.;\r\n]+)", RegexOptions.IgnoreCase);
            if (before.Success)
            {
                plan.Target = PromptTargetKind.BeforeHeading;
                plan.Anchor = NormalizeAnchor(before.Groups["anchor"].Value);
                return plan;
            }

            var inSection = Regex.Match(text, @"\b(?:в|во)\s+(?:раздел|пункт)\s+(?<anchor>\d+(?:\.\d+){0,3})\b", RegexOptions.IgnoreCase);
            if (inSection.Success)
            {
                plan.Target = PromptTargetKind.AfterHeading;
                plan.Anchor = NormalizeAnchor(inSection.Groups["anchor"].Value);
                return plan;
            }

            if (ContainsAny(lower, "замени выдел", "перепиши выдел", "исправь выдел", "в выделенный текст", "в выделение"))
            {
                plan.Target = PromptTargetKind.ReplaceSelection;
                return plan;
            }

            return plan;
        }

        private Word.Range ResolvePromptTargetRange(
            Word.Document doc,
            Word.Selection selection,
            PromptInsertionPlan plan,
            out bool replaceSelection,
            out string locationInfo)
        {
            replaceSelection = false;
            locationInfo = "в текущую позицию курсора";
            if (doc == null || selection == null || plan == null)
            {
                return null;
            }

            if (plan.Target == PromptTargetKind.ReplaceSelection)
            {
                if (selection.Type == Word.WdSelectionType.wdSelectionNormal && selection.Range != null && selection.Range.Start != selection.Range.End)
                {
                    replaceSelection = true;
                    locationInfo = "замена выделенного фрагмента";
                    return selection.Range.Duplicate;
                }

                plan.Target = PromptTargetKind.Cursor;
                locationInfo = "выделения нет, вставка в текущую позицию курсора";
            }

            if (plan.Target == PromptTargetKind.DocumentStart)
            {
                locationInfo = "в начало документа";
                return doc.Range(0, 0);
            }

            if (plan.Target == PromptTargetKind.DocumentEnd)
            {
                var endRange = doc.Content.Duplicate;
                endRange.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
                locationInfo = "в конец документа";
                return endRange;
            }

            if (plan.Target == PromptTargetKind.AfterHeading || plan.Target == PromptTargetKind.BeforeHeading)
            {
                var heading = FindHeadingAnchorRange(doc, plan.Anchor);
                if (heading != null)
                {
                    var point = heading.Duplicate;
                    point.Collapse(plan.Target == PromptTargetKind.AfterHeading
                        ? Word.WdCollapseDirection.wdCollapseEnd
                        : Word.WdCollapseDirection.wdCollapseStart);
                    locationInfo = (plan.Target == PromptTargetKind.AfterHeading ? "после" : "перед") + " заголовком \"" + plan.Anchor + "\"";
                    return point;
                }

                locationInfo = "заголовок \"" + plan.Anchor + "\" не найден, вставка в текущую позицию курсора";
            }

            var fallback = selection.Range.Duplicate;
            fallback.Collapse(Word.WdCollapseDirection.wdCollapseEnd);
            return fallback;
        }

        private Word.Range FindHeadingAnchorRange(Word.Document doc, string anchor)
        {
            if (doc == null)
            {
                return null;
            }

            var target = NormalizeAnchor(anchor);
            if (target.Length == 0)
            {
                return null;
            }

            var isNumber = Regex.IsMatch(target, @"^\d+(?:\.\d+){0,3}$");
            foreach (Word.Paragraph paragraph in doc.Paragraphs)
            {
                var line = CollapseSpaces((paragraph.Range.Text ?? "").Replace("\r", "").Trim());
                if (line.Length == 0)
                {
                    continue;
                }

                var isHeading = IsHeadingLikeLine(line) || IsHeadingStyledParagraph(paragraph);
                if (!isHeading)
                {
                    continue;
                }

                if (isNumber)
                {
                    if (line.StartsWith(target + " ", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(line, target, StringComparison.OrdinalIgnoreCase))
                    {
                        return paragraph.Range.Duplicate;
                    }
                }
                else if (line.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return paragraph.Range.Duplicate;
                }
            }

            return null;
        }

        private string BuildContextAwarePrompt(
            string userPrompt,
            Word.Document doc,
            Word.Range target,
            bool replaceSelection,
            string locationInfo,
            bool applyGostRules,
            bool isTableRequest,
            bool isChartRequest)
        {
            var before = GetRangeTextSafe(doc, Math.Max(0, target.Start - 1200), target.Start);
            var after = GetRangeTextSafe(doc, target.End, Math.Min(doc.Content.End, target.End + 1200));
            var selected = replaceSelection ? (target.Text ?? "") : "";
            var headings = GetDocumentHeadings(doc, 14);
            var sb = new StringBuilder();
            sb.AppendLine("Запрос пользователя:");
            sb.AppendLine(userPrompt.Trim());
            sb.AppendLine();
            sb.AppendLine("Позиция вставки:");
            sb.AppendLine(locationInfo);

            if (replaceSelection)
            {
                sb.AppendLine();
                sb.AppendLine("Фрагмент для замены:");
                sb.AppendLine(Clip(NormalizeForWordSmart(selected), 1400));
            }

            sb.AppendLine();
            sb.AppendLine("Контекст перед позицией:");
            sb.AppendLine(Clip(NormalizeForWordSmart(before), 1400));
            sb.AppendLine();
            sb.AppendLine("Контекст после позиции:");
            sb.AppendLine(Clip(NormalizeForWordSmart(after), 1000));

            if (headings.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Структура заголовков:");
                foreach (var heading in headings)
                {
                    sb.AppendLine(heading);
                }
            }

            if (_selectedTemplate != null &&
                (applyGostRules || Regex.IsMatch(userPrompt ?? "", @"шаблон|гост|тз", RegexOptions.IgnoreCase)))
            {
                sb.AppendLine();
                sb.AppendLine("Активный шаблон базы знаний:");
                sb.AppendLine(_selectedTemplate.ToPromptBlock());
            }

            sb.AppendLine();
            sb.AppendLine("Правила ответа:");
            sb.AppendLine("1) Верни только готовый текст для вставки, без пояснений.");
            sb.AppendLine("2) Не склеивай абзацы в одну строку.");
            sb.AppendLine("3) Списки, нумерацию, таблицы и гиперссылки оформи корректно.");
            sb.AppendLine("4) Сохраняй терминологию и смысл существующего текста.");
            sb.AppendLine("5) Не переписывай и не дублируй существующий документ. Верни только новый фрагмент для точки вставки.");
            sb.AppendLine("6) Не используй Markdown-разметку (**, #, ``` и т.п.).");

            if (isTableRequest)
            {
                sb.AppendLine("7) Для таблицы верни только табличные строки (первая строка — заголовки, разделитель ';') ИЛИ команды @word:table без пояснений.");
                sb.AppendLine("   Если нужно несколько таблиц — верни несколько команд @word:table, каждая с новой строки.");
                int rows;
                int cols;
                GetExpectedTableDimensions(userPrompt, out rows, out cols);
                if (rows > 0 || cols > 0)
                {
                    var r = rows > 0 ? rows.ToString() : "?";
                    var c = cols > 0 ? cols.ToString() : "?";
                    sb.AppendLine("   Размер таблицы: " + c + " столбца(ов) и " + r + " строки(ок).");
                }
            }

            sb.AppendLine("8) При необходимости используй команды Word @word:... и не дублируй их обычным текстом.");
            sb.AppendLine("   Команду пиши в одну строку без переносов. Если параметры длинные, используй блок:");
            sb.AppendLine("   @word:table");
            sb.AppendLine("   headers=...");
            sb.AppendLine("   rows=...");
            sb.AppendLine("   @end");
            sb.AppendLine("   Если значение содержит пробелы, обрами его в кавычки.");
            sb.AppendLine("   Команды: @word:text value=...,");
            sb.AppendLine("   @word:font name=... size=... (или +2/-1) bold=... italic=... underline=... strike=... subscript=... superscript=... allcaps=... smallcaps=... color=#RRGGBB highlight=yellow scope=selection|paragraph|document,");
            sb.AppendLine("   @word:paragraph align=left|center|right|justify firstlinecm=... leftindentcm=... rightindentcm=... beforept=... afterpt=... linespacing=...,");
            sb.AppendLine("   @word:style name=Обычный|Без интервала|Заголовок 1|Заголовок 2|Заголовок 3 scope=...,");
            sb.AppendLine("   @word:case mode=upper|lower|title|sentence|toggle scope=..., @word:indent action=inc|dec stepcm=..., @word:clear scope=...,");
            sb.AppendLine("   @word:border type=box|top|bottom|left|right|none color=#RRGGBB scope=..., @word:shading color=#RRGGBB scope=...,");
            sb.AppendLine("   @word:find text=... scope=document|selection, @word:replace find=... replace=... scope=document|selection all=true, @word:select type=all|paragraph|sentence|word,");
            sb.AppendLine("   @word:heading level=1..3 text=..., @word:list type=bullet|number items=item1|item2,");
            sb.AppendLine("   @word:table headers=A|B rows=a1|b1; a2|b2, @word:chart type=column|bar|line|pie labels=... values=...,");
            sb.AppendLine("   @word:hyperlink text=... url=..., @word:page orientation=portrait|landscape paper=a4 marginscm=l,r,t,b, @word:break type=line|page, @word:image path=... widthcm=... heightcm=...");
            sb.AppendLine("   Если пользователь просит оформить ВЕСЬ документ, укажи scope=document (например, @word:font italic=true scope=document).");

            if (isChartRequest)
            {
                sb.AppendLine("9) Для диаграммы верни только одну команду @word:chart без пояснений.");
            }
            else
            {
                sb.AppendLine("9) Диаграммы добавляй только если пользователь явно просит.");
            }

            if (applyGostRules)
            {
                sb.AppendLine("10) Строго соблюдай ГОСТ: A4, поля 30/10/20/20 мм, Times New Roman 14, межстрочный 1.5, отступ 1.25 см, выравнивание по ширине.");
                sb.AppendLine("11) Заголовки и нумерация: 1 / 1.1 / 1.1.1 без точки в конце номера.");
            }

            return sb.ToString();
        }

        private static List<string> GetDocumentHeadings(Word.Document doc, int maxCount)
        {
            var headings = new List<string>();
            if (doc == null || maxCount <= 0)
            {
                return headings;
            }

            foreach (Word.Paragraph paragraph in doc.Paragraphs)
            {
                var line = CollapseSpaces((paragraph.Range.Text ?? "").Replace("\r", "").Trim());
                if (line.Length == 0)
                {
                    continue;
                }

                if (!IsHeadingLikeLine(line) && !IsHeadingStyledParagraph(paragraph))
                {
                    continue;
                }

                headings.Add(line);
                if (headings.Count >= maxCount)
                {
                    break;
                }
            }

            return headings;
        }

        private static string GetRangeTextSafe(Word.Document doc, int start, int end)
        {
            if (doc == null)
            {
                return "";
            }

            try
            {
                var docEnd = doc.Content.End;
                var safeStart = Math.Max(0, Math.Min(start, docEnd));
                var safeEnd = Math.Max(0, Math.Min(end, docEnd));
                if (safeEnd <= safeStart)
                {
                    return "";
                }

                return (doc.Range(safeStart, safeEnd).Text ?? "").Trim();
            }
            catch
            {
                return "";
            }
        }

        private static bool IsHeadingStyledParagraph(Word.Paragraph paragraph)
        {
            if (paragraph == null || paragraph.Range == null)
            {
                return false;
            }

            try
            {
                var styleName = Convert.ToString(paragraph.Range.get_Style()) ?? "";
                var lower = styleName.ToLowerInvariant();
                return lower.Contains("heading") || lower.Contains("заголовок");
            }
            catch
            {
                return false;
            }
        }

        private static string NormalizeAnchor(string text)
        {
            var value = (text ?? "").Trim();
            if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length >= 2)
            {
                value = value.Substring(1, value.Length - 2);
            }

            if (value.StartsWith("«") && value.EndsWith("»") && value.Length >= 2)
            {
                value = value.Substring(1, value.Length - 2);
            }

            return CollapseSpaces(value);
        }

        private static bool ContainsAny(string value, params string[] tokens)
        {
            if (string.IsNullOrWhiteSpace(value) || tokens == null || tokens.Length == 0)
            {
                return false;
            }

            foreach (var token in tokens)
            {
                if (!string.IsNullOrWhiteSpace(token) && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsTableRequest(string prompt)
        {
            var text = (prompt ?? "").ToLowerInvariant();
            return ContainsAny(text, "таблиц", "table");
        }

        private static bool IsChartRequest(string prompt)
        {
            var text = (prompt ?? "").ToLowerInvariant();
            if (text.Length == 0)
            {
                return false;
            }

            return ContainsAny(text, "график", "диаграм", "chart", "histogram", "plot");
        }

        private static bool IsWordCommandModeRequest(string prompt)
        {
            var text = (prompt ?? "").ToLowerInvariant();
            if (text.Length == 0)
            {
                return false;
            }

            return text.Contains("@word:") || ContainsAny(text, "режим команд", "выполни команды word", "word-команды");
        }

        private bool TryHandleControlPrompt(string prompt)
        {
            var text = (prompt ?? "").Trim().ToLowerInvariant();
            if (text.Length == 0)
            {
                return false;
            }

            var delete = ContainsAny(
                text,
                "удали то что пиш",
                "удали то что он пишет",
                "удали то что они пишет",
                "удали то что сейчас пиш",
                "удали что пиш",
                "удали то что написал",
                "удали что написал",
                "очисти ответ",
                "отмени и удали");

            var stop = delete || ContainsAny(
                text,
                "стоп",
                "останови",
                "прекрати",
                "хватит",
                "отмени генерац",
                "отмена генерац");

            if (!stop)
            {
                return false;
            }

            CancelActiveAi(delete, false);
            return true;
        }

        private void CancelActiveAi(bool deleteOutput, bool silent)
        {
            var cts = _activeAiCts;
            if (cts != null && !cts.IsCancellationRequested)
            {
                try
                {
                    cts.Cancel();
                }
                catch
                {
                }
            }

            if (deleteOutput)
            {
                RunOnUi(() =>
                {
                    var deleted = TryDeleteRange(_activeAiDoc, _activeAiStart, _activeAiEnd);
                    if (!deleted)
                    {
                        deleted = TryDeleteRange(_lastAiDoc, _lastAiStart, _lastAiEnd);
                    }

                    if (!silent)
                    {
                        var msg = deleted
                            ? "Генерация остановлена. Последний текст ИИ удален."
                            : "Генерация остановлена.";
                        MessageBox.Show(msg, "Ollama AI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                });
            }
            else if (!silent)
            {
                RunOnUi(() =>
                {
                    MessageBox.Show("Генерация остановлена.", "Ollama AI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            }
        }

        private static bool TryDeleteRange(Word.Document doc, int start, int end)
        {
            if (doc == null || start < 0 || end <= start)
            {
                return false;
            }

            try
            {
                var docEnd = doc.Content.End;
                var s = Math.Max(0, Math.Min(start, docEnd));
                var e = Math.Max(0, Math.Min(end, docEnd));
                if (e <= s)
                {
                    return false;
                }

                var range = doc.Range(s, e);
                range.Text = "";
                return true;
            }
            catch
            {
                return false;
            }
        }

        private Word.Range ReplaceRangeWithRichContent(Word.Range targetRange, string text, bool allowWordCommands, bool allowChartBlocks)
        {
            if (targetRange == null || targetRange.Document == null)
            {
                return targetRange;
            }

            var doc = targetRange.Document;
            var start = targetRange.Start;
            targetRange.Text = "";
            var end = InsertRichContent(doc, start, text, allowWordCommands, allowChartBlocks);
            return doc.Range(start, end);
        }

        private int InsertRichContent(Word.Document doc, int start, string text, bool allowWordCommands, bool allowChartBlocks)
        {
            var cursor = doc.Range(start, start);
            var source = (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n');
            if (source.Length == 0)
            {
                return cursor.End;
            }

            var rawLines = source.Split('\n');
            var lines = MergeWordCommandBlocks(rawLines).ToArray();
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i] ?? "";
                var trimmed = line.Trim();

                ChartSpec chart;
                if (allowChartBlocks && TryParseChartBlock(lines, ref i, out chart))
                {
                    BreakActiveListAtCursor(cursor);
                    InsertChartAtCursor(cursor, chart);
                    if (i < lines.Length - 1)
                    {
                        cursor.InsertParagraphAfter();
                        cursor.SetRange(cursor.End, cursor.End);
                    }

                    continue;
                }

                List<List<string>> tableRows;
                if (TryParseTabularBlock(lines, ref i, out tableRows))
                {
                    BreakActiveListAtCursor(cursor);
                    InsertWordTableAtCursor(doc, cursor, tableRows);

                    if (i < lines.Length - 1)
                    {
                        cursor.InsertParagraphAfter();
                        cursor.SetRange(cursor.End, cursor.End);
                    }

                    continue;
                }

                string command;
                Dictionary<string, string> args;
                string raw;
                if (allowWordCommands && TryParseWordCommand(trimmed, out command, out args, out raw))
                {
                    BreakActiveListAtCursor(cursor);
                    ExecuteWordCommand(doc, cursor, command, args, raw);
                    continue;
                }

                if (trimmed.Length == 0)
                {
                    BreakActiveListAtCursor(cursor);
                    cursor.InsertParagraphAfter();
                    cursor.SetRange(cursor.End, cursor.End);
                    continue;
                }

                var lineType = WriteStructuredLine(doc, cursor, line);
                if (i < lines.Length - 1)
                {
                    cursor.InsertParagraphAfter();
                    cursor.SetRange(cursor.End, cursor.End);
                    var next = (lines[i + 1] ?? "").Trim();
                    var keepList = (lineType == StructuredLineType.Bullet || lineType == StructuredLineType.Numbered) &&
                                   next.Length > 0 &&
                                   IsListLikeLine(next);
                    if (!keepList)
                    {
                        BreakActiveListAtCursor(cursor);
                    }
                }
            }

            return cursor.End;
        }

        private bool TryParseTabularBlock(string[] lines, ref int index, out List<List<string>> rows)
        {
            rows = new List<List<string>>();
            if (lines == null || index < 0 || index >= lines.Length)
            {
                return false;
            }

            var first = (lines[index] ?? "").Trim();
            if (first.Length == 0)
            {
                return false;
            }

            char delimiter;
            if (first.Contains("\t"))
            {
                delimiter = '\t';
            }
            else if (CountDelimiter(first, ';') >= 1)
            {
                delimiter = ';';
            }
            else if (CountDelimiter(first, '|') >= 1)
            {
                delimiter = '|';
            }
            else
            {
                return false;
            }

            var cursor = index;
            while (cursor < lines.Length)
            {
                var raw = (lines[cursor] ?? "").Trim();
                if (raw.Length == 0)
                {
                    break;
                }

                List<string> parsed;
                if (!TryParseTableRow(raw, delimiter, out parsed))
                {
                    break;
                }

                if (!IsMarkdownSeparatorRow(parsed))
                {
                    rows.Add(parsed);
                }

                cursor++;
            }

            if (rows.Count < 2 || rows.Max(r => r.Count) < 2)
            {
                if (_forcedTableRows > 0 || _forcedTableCols > 0)
                {
                    if (rows.Count == 0)
                    {
                        rows.Add(new List<string>());
                    }
                }
                else
                {
                    rows.Clear();
                    return false;
                }
            }

            index = cursor - 1;
            return true;
        }

        private static bool TryParseTableRow(string line, char delimiter, out List<string> cells)
        {
            cells = new List<string>();
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            var value = line.Trim();
            if (delimiter == '|')
            {
                if (!value.Contains("|"))
                {
                    return false;
                }

                if (value.StartsWith("|"))
                {
                    value = value.Substring(1);
                }

                if (value.EndsWith("|"))
                {
                    value = value.Substring(0, value.Length - 1);
                }
            }
            else if (!value.Contains(delimiter.ToString()))
            {
                return false;
            }

            foreach (var part in value.Split(new[] { delimiter }, StringSplitOptions.None))
            {
                cells.Add(part.Trim());
            }

            if (cells.Count < 2)
            {
                return false;
            }

            var nonEmpty = cells.Any(c => !string.IsNullOrWhiteSpace(c));
            return nonEmpty;
        }

        private static bool IsMarkdownSeparatorRow(List<string> cells)
        {
            if (cells == null || cells.Count == 0)
            {
                return false;
            }

            foreach (var cell in cells)
            {
                var v = (cell ?? "").Trim();
                if (v.Length == 0)
                {
                    return false;
                }

                if (!Regex.IsMatch(v, @"^:?-{2,}:?$"))
                {
                    return false;
                }
            }

            return true;
        }

        private void InsertWordTableAtCursor(Word.Document doc, Word.Range cursor, List<List<string>> rows)
        {
            if (doc == null || cursor == null || rows == null)
            {
                return;
            }

            ApplyTableOverrides(rows);
            if (rows.Count == 0 || rows.Max(r => r.Count) == 0)
            {
                return;
            }
            var rowCount = rows.Count;
            var colCount = Math.Max(1, rows.Max(r => r.Count));
            var table = doc.Tables.Add(cursor, rowCount, colCount);
            for (var r = 0; r < rowCount; r++)
            {
                for (var c = 0; c < colCount; c++)
                {
                    var value = c < rows[r].Count ? rows[r][c] : "";
                    table.Cell(r + 1, c + 1).Range.Text = value;
                }
            }

            try
            {
                table.Rows[1].Range.Bold = 1;
                table.AllowAutoFit = true;
                table.AutoFitBehavior(Word.WdAutoFitBehavior.wdAutoFitContent);
                table.Borders.Enable = 1;
                table.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                table.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
            }
            catch
            {
            }

            cursor.SetRange(table.Range.End, table.Range.End);
        }

        private StructuredLineType WriteStructuredLine(Word.Document doc, Word.Range cursor, string line)
        {
            if (doc == null || cursor == null)
            {
                return StructuredLineType.Plain;
            }

            var text = line ?? "";
            var headingMarkdown = Regex.Match(text, @"^\s*(#{1,3})\s+(.+)$");
            var headingNumeric = Regex.Match(text, @"^\s*(\d+(?:\.\d+){0,3})\s+(.+)$");
            var bullet = Regex.Match(text, @"^\s*[-*•–—]\s+(.+)$");
            var numbered = Regex.Match(text, @"^\s*\d+[\)\.]\s+(.+)$");

            var content = text.Trim();
            var headingLevel = 0;
            var applyBullet = false;
            var applyNumber = false;
            var resultType = StructuredLineType.Plain;

            if (headingMarkdown.Success)
            {
                headingLevel = Math.Min(3, headingMarkdown.Groups[1].Value.Length);
                content = headingMarkdown.Groups[2].Value.Trim();
                resultType = StructuredLineType.Heading;
            }
            else if (headingNumeric.Success)
            {
                headingLevel = Math.Min(3, headingNumeric.Groups[1].Value.Count(ch => ch == '.') + 1);
                content = headingNumeric.Groups[1].Value + " " + headingNumeric.Groups[2].Value.Trim();
                resultType = StructuredLineType.Heading;
            }
            else if (bullet.Success)
            {
                applyBullet = true;
                content = bullet.Groups[1].Value.Trim();
                resultType = StructuredLineType.Bullet;
            }
            else if (numbered.Success)
            {
                applyNumber = true;
                content = numbered.Groups[1].Value.Trim();
                resultType = StructuredLineType.Numbered;
            }

            if (!applyBullet && !applyNumber)
            {
                BreakActiveListAtCursor(cursor);
            }

            var start = cursor.End;
            WriteInlineRuns(doc, cursor, content);
            var end = cursor.End;
            var lineRange = doc.Range(start, end);

            if (headingLevel > 0)
            {
                if (headingLevel == 1)
                {
                    lineRange.set_Style(doc.Styles[Word.WdBuiltinStyle.wdStyleHeading1]);
                }
                else if (headingLevel == 2)
                {
                    lineRange.set_Style(doc.Styles[Word.WdBuiltinStyle.wdStyleHeading2]);
                }
                else
                {
                    lineRange.set_Style(doc.Styles[Word.WdBuiltinStyle.wdStyleHeading3]);
                }
            }
            else if (applyBullet)
            {
                lineRange.ListFormat.ApplyBulletDefault();
            }
            else if (applyNumber)
            {
                lineRange.ListFormat.ApplyNumberDefault();
            }

            return resultType;
        }

        private static void BreakActiveListAtCursor(Word.Range cursor)
        {
            if (cursor == null)
            {
                return;
            }

            try
            {
                var paragraph = cursor.Paragraphs.First.Range;
                if (paragraph != null && paragraph.ListFormat != null && paragraph.ListFormat.ListType != Word.WdListType.wdListNoNumbering)
                {
                    paragraph.ListFormat.RemoveNumbers(Word.WdNumberType.wdNumberParagraph);
                }
            }
            catch
            {
            }
        }

        private void WriteInlineRuns(Word.Document doc, Word.Range cursor, string line)
        {
            if (doc == null || cursor == null)
            {
                return;
            }

            foreach (var run in ParseInlineRuns(line))
            {
                if (string.IsNullOrEmpty(run.Text))
                {
                    continue;
                }

                var start = cursor.End;
                cursor.InsertAfter(run.Text);
                var end = start + run.Text.Length;
                var range = doc.Range(start, end);

                if (run.Bold)
                {
                    range.Font.Bold = 1;
                }

                if (run.Italic)
                {
                    range.Font.Italic = 1;
                }

                if (run.Underline)
                {
                    range.Font.Underline = Word.WdUnderline.wdUnderlineSingle;
                }

                if (!string.IsNullOrWhiteSpace(run.Link))
                {
                    object missing = Type.Missing;
                    doc.Hyperlinks.Add(range, run.Link, ref missing, ref missing, run.Text);
                }

                cursor.SetRange(end, end);
            }
        }

        private List<InlineRun> ParseInlineRuns(string line)
        {
            var result = new List<InlineRun>();
            var text = line ?? "";
            if (text.Length == 0)
            {
                return result;
            }

            var pattern = new Regex(@"(\[[^\]]+\]\((https?://[^\s\)]+)\)|\*\*\*.+?\*\*\*|\*\*.+?\*\*|__.+?__|\*[^*].+?\*|_[^_].+?_)", RegexOptions.Singleline);
            var pos = 0;
            foreach (Match m in pattern.Matches(text))
            {
                if (m.Index > pos)
                {
                    result.Add(new InlineRun { Text = text.Substring(pos, m.Index - pos) });
                }

                var token = m.Value;
                if (token.StartsWith("["))
                {
                    var closeBracket = token.IndexOf("](", StringComparison.Ordinal);
                    var closeParen = token.LastIndexOf(')');
                    if (closeBracket > 1 && closeParen > closeBracket + 2)
                    {
                        var label = token.Substring(1, closeBracket - 1);
                        var url = token.Substring(closeBracket + 2, closeParen - (closeBracket + 2));
                        result.Add(new InlineRun { Text = label, Link = url });
                    }
                    else
                    {
                        result.Add(new InlineRun { Text = token });
                    }
                }
                else if (token.StartsWith("***") && token.EndsWith("***") && token.Length > 6)
                {
                    result.Add(new InlineRun { Text = token.Substring(3, token.Length - 6), Bold = true, Italic = true });
                }
                else if (token.StartsWith("**") && token.EndsWith("**") && token.Length > 4)
                {
                    result.Add(new InlineRun { Text = token.Substring(2, token.Length - 4), Bold = true });
                }
                else if (token.StartsWith("__") && token.EndsWith("__") && token.Length > 4)
                {
                    result.Add(new InlineRun { Text = token.Substring(2, token.Length - 4), Underline = true });
                }
                else if ((token.StartsWith("*") && token.EndsWith("*") && token.Length > 2) ||
                         (token.StartsWith("_") && token.EndsWith("_") && token.Length > 2))
                {
                    result.Add(new InlineRun { Text = token.Substring(1, token.Length - 2), Italic = true });
                }
                else
                {
                    result.Add(new InlineRun { Text = token });
                }

                pos = m.Index + m.Length;
            }

            if (pos < text.Length)
            {
                result.Add(new InlineRun { Text = text.Substring(pos) });
            }

            return result;
        }

        private bool TryParseWordCommand(string line, out string command, out Dictionary<string, string> args, out string raw)
        {
            command = "";
            args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            raw = "";
            var text = (line ?? "").Trim();
            if (!text.StartsWith("@word:", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var payload = text.Substring(6).Trim();
            if (payload.Length == 0)
            {
                return false;
            }

            var space = payload.IndexOf(' ');
            if (space < 0)
            {
                command = payload.ToLowerInvariant();
                raw = "";
                return true;
            }

            command = payload.Substring(0, space).Trim().ToLowerInvariant();
            raw = payload.Substring(space + 1).Trim();

            foreach (Match m in Regex.Matches(raw, @"(?<k>[A-Za-z_][\w\-]*)=(?<v>\""[^\\""]*\""|'[^']*'|\S+)"))
            {
                var k = m.Groups["k"].Value.Trim();
                var v = m.Groups["v"].Value.Trim();
                if (v.StartsWith("\"") && v.EndsWith("\"") && v.Length >= 2)
                {
                    v = v.Substring(1, v.Length - 2);
                }
                else if (v.StartsWith("'") && v.EndsWith("'") && v.Length >= 2)
                {
                    v = v.Substring(1, v.Length - 2);
                }

                args[k] = v;
            }

            return true;
        }

        private void ExecuteWordCommand(Word.Document doc, Word.Range cursor, string command, Dictionary<string, string> args, string raw)
        {
            if (doc == null || cursor == null || string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            try
            {
                if (command == "text")
                {
                    var value = GetArg(args, "value", raw);
                    if (!string.IsNullOrWhiteSpace(raw) && raw.IndexOf("value=", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var looseValue = ExtractArg(raw, "value=");
                        if (!string.IsNullOrWhiteSpace(looseValue))
                        {
                            value = looseValue;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        WriteStructuredLine(doc, cursor, value);
                        cursor.InsertParagraphAfter();
                        cursor.SetRange(cursor.End, cursor.End);
                    }

                    return;
                }

                if (command == "font")
                {
                    var scope = GetArg(args, "scope", "selection");
                    var range = ResolveScopeRange(doc, cursor, scope);
                    if (range == null)
                    {
                        return;
                    }

                    var name = GetArg(args, "name", "");
                    if (!string.IsNullOrWhiteSpace(raw) && raw.IndexOf("name=", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var looseName = ExtractArg(raw, "name=", "size=", "bold=", "italic=", "underline=", "strike=", "doublestrike=", "subscript=", "superscript=", "allcaps=", "smallcaps=", "color=", "highlight=", "scope=");
                        if (!string.IsNullOrWhiteSpace(looseName))
                        {
                            name = looseName;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        range.Font.Name = name;
                    }

                    var sizeText = GetArg(args, "size", "");
                    float size;
                    if (TryParseRelativeFloat(sizeText, range.Font.Size, out size) && size > 0)
                    {
                        range.Font.Size = size;
                    }

                    bool flag;
                    if (TryParseBool(GetArg(args, "bold", ""), out flag))
                    {
                        range.Font.Bold = flag ? 1 : 0;
                    }

                    if (TryParseBool(GetArg(args, "italic", ""), out flag))
                    {
                        range.Font.Italic = flag ? 1 : 0;
                    }

                    if (TryParseBool(GetArg(args, "underline", ""), out flag))
                    {
                        range.Font.Underline = flag ? Word.WdUnderline.wdUnderlineSingle : Word.WdUnderline.wdUnderlineNone;
                    }

                    if (TryParseBool(GetArg(args, "strike", ""), out flag))
                    {
                        range.Font.StrikeThrough = flag ? 1 : 0;
                    }

                    if (TryParseBool(GetArg(args, "doublestrike", ""), out flag))
                    {
                        range.Font.DoubleStrikeThrough = flag ? 1 : 0;
                    }

                    if (TryParseBool(GetArg(args, "subscript", ""), out flag))
                    {
                        range.Font.Subscript = flag ? 1 : 0;
                    }

                    if (TryParseBool(GetArg(args, "superscript", ""), out flag))
                    {
                        range.Font.Superscript = flag ? 1 : 0;
                    }

                    if (TryParseBool(GetArg(args, "allcaps", ""), out flag))
                    {
                        range.Font.AllCaps = flag ? 1 : 0;
                    }

                    if (TryParseBool(GetArg(args, "smallcaps", ""), out flag))
                    {
                        range.Font.SmallCaps = flag ? 1 : 0;
                    }

                    var colorHex = GetArg(args, "color", "");
                    Color c;
                    if (TryParseColor(colorHex, out c))
                    {
                        range.Font.Color = (Word.WdColor)ColorTranslator.ToOle(c);
                    }

                    var highlightText = GetArg(args, "highlight", "");
                    Word.WdColorIndex hi;
                    if (TryParseHighlightColor(highlightText, out hi))
                    {
                        range.HighlightColorIndex = hi;
                    }

                    return;
                }

                if (command == "paragraph")
                {
                    var scope = GetArg(args, "scope", "selection");
                    var range = ResolveScopeRange(doc, cursor, scope);
                    if (range == null)
                    {
                        return;
                    }

                    var align = GetArg(args, "align", "").ToLowerInvariant();
                    if (align == "left")
                    {
                        range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    }
                    else if (align == "right")
                    {
                        range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                    }
                    else if (align == "center")
                    {
                        range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    }
                    else if (align == "justify")
                    {
                        range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                    }

                    float cm;
                    if (TryParseFloat(GetArg(args, "firstlinecm", ""), out cm))
                    {
                        range.ParagraphFormat.FirstLineIndent = WordApp != null ? WordApp.CentimetersToPoints(cm) : cm * 28.346f;
                    }

                    if (TryParseFloat(GetArg(args, "leftindentcm", GetArg(args, "indentcm", "")), out cm))
                    {
                        range.ParagraphFormat.LeftIndent = WordApp != null ? WordApp.CentimetersToPoints(cm) : cm * 28.346f;
                    }

                    if (TryParseFloat(GetArg(args, "rightindentcm", ""), out cm))
                    {
                        range.ParagraphFormat.RightIndent = WordApp != null ? WordApp.CentimetersToPoints(cm) : cm * 28.346f;
                    }

                    float pt;
                    if (TryParseFloat(GetArg(args, "beforept", ""), out pt))
                    {
                        range.ParagraphFormat.SpaceBefore = pt;
                    }

                    if (TryParseFloat(GetArg(args, "afterpt", ""), out pt))
                    {
                        range.ParagraphFormat.SpaceAfter = pt;
                    }

                    float mult;
                    if (TryParseFloat(GetArg(args, "linespacing", ""), out mult))
                    {
                        if (mult <= 1.01f)
                        {
                            range.ParagraphFormat.LineSpacingRule = Word.WdLineSpacing.wdLineSpaceSingle;
                        }
                        else if (mult <= 1.51f)
                        {
                            range.ParagraphFormat.LineSpacingRule = Word.WdLineSpacing.wdLineSpace1pt5;
                        }
                        else if (mult <= 2.01f)
                        {
                            range.ParagraphFormat.LineSpacingRule = Word.WdLineSpacing.wdLineSpaceDouble;
                        }
                        else
                        {
                            range.ParagraphFormat.LineSpacingRule = Word.WdLineSpacing.wdLineSpaceMultiple;
                            range.ParagraphFormat.LineSpacing = WordApp != null ? WordApp.LinesToPoints(mult) : mult * 12f;
                        }
                    }

                    return;
                }

                if (command == "style")
                {
                    var scope = GetArg(args, "scope", "selection");
                    var range = ResolveScopeRange(doc, cursor, scope);
                    if (range == null)
                    {
                        return;
                    }

                    var name = GetArg(args, "name", GetArg(args, "value", raw));
                    if (!string.IsNullOrWhiteSpace(raw) && raw.IndexOf("name=", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var looseName = ExtractArg(raw, "name=", "scope=");
                        if (!string.IsNullOrWhiteSpace(looseName))
                        {
                            name = looseName;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(raw) && raw.IndexOf("value=", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var looseValue = ExtractArg(raw, "value=", "scope=");
                        if (!string.IsNullOrWhiteSpace(looseValue))
                        {
                            name = looseValue;
                        }
                    }
                    ApplyStyle(doc, range, name);
                    return;
                }

                if (command == "case")
                {
                    var scope = GetArg(args, "scope", "selection");
                    var range = ResolveScopeRange(doc, cursor, scope);
                    if (range == null)
                    {
                        return;
                    }

                    var mode = GetArg(args, "mode", GetArg(args, "value", raw)).ToLowerInvariant();
                    Word.WdCharacterCase cc;
                    if (TryMapCharacterCase(mode, out cc))
                    {
                        range.Case = cc;
                    }

                    return;
                }

                if (command == "indent")
                {
                    var scope = GetArg(args, "scope", "selection");
                    var range = ResolveScopeRange(doc, cursor, scope);
                    if (range == null)
                    {
                        return;
                    }

                    var action = GetArg(args, "action", GetArg(args, "value", "")).ToLowerInvariant();
                    float stepCm = 0.63f;
                    float stepArg;
                    if (TryParseFloat(GetArg(args, "stepcm", ""), out stepArg) && stepArg > 0)
                    {
                        stepCm = stepArg;
                    }

                    var delta = WordApp != null ? WordApp.CentimetersToPoints(stepCm) : stepCm * 28.346f;
                    foreach (Word.Paragraph p in range.Paragraphs)
                    {
                        if (action == "dec" || action == "decrease" || action == "уменьш" || action == "влево")
                        {
                            p.Format.LeftIndent = Math.Max(0, p.Format.LeftIndent - delta);
                        }
                        else
                        {
                            p.Format.LeftIndent = p.Format.LeftIndent + delta;
                        }
                    }

                    return;
                }

                if (command == "clear")
                {
                    var scope = GetArg(args, "scope", "selection");
                    var range = ResolveScopeRange(doc, cursor, scope);
                    if (range == null)
                    {
                        return;
                    }

                    try
                    {
                        range.Font.Reset();
                        range.ParagraphFormat.Reset();
                        try
                        {
                            range.ListFormat.RemoveNumbers();
                        }
                        catch
                        {
                        }
                    }
                    catch
                    {
                    }

                    return;
                }

                if (command == "border")
                {
                    var scope = GetArg(args, "scope", "selection");
                    var range = ResolveScopeRange(doc, cursor, scope);
                    if (range == null)
                    {
                        return;
                    }

                    var type = GetArg(args, "type", GetArg(args, "value", "box")).ToLowerInvariant();
                    if (type == "none" || type == "нет" || type == "off")
                    {
                        range.Borders.Enable = 0;
                        return;
                    }

                    range.Borders.Enable = 1;
                    var colorText = GetArg(args, "color", "");
                    Color bc;
                    var hasColor = TryParseColor(colorText, out bc);

                    Action<Word.WdBorderType> apply = borderType =>
                    {
                        var border = range.Borders[borderType];
                        border.LineStyle = Word.WdLineStyle.wdLineStyleSingle;
                        if (hasColor)
                        {
                            border.Color = (Word.WdColor)ColorTranslator.ToOle(bc);
                        }
                    };

                    if (type == "top")
                    {
                        apply(Word.WdBorderType.wdBorderTop);
                    }
                    else if (type == "bottom")
                    {
                        apply(Word.WdBorderType.wdBorderBottom);
                    }
                    else if (type == "left")
                    {
                        apply(Word.WdBorderType.wdBorderLeft);
                    }
                    else if (type == "right")
                    {
                        apply(Word.WdBorderType.wdBorderRight);
                    }
                    else
                    {
                        apply(Word.WdBorderType.wdBorderTop);
                        apply(Word.WdBorderType.wdBorderBottom);
                        apply(Word.WdBorderType.wdBorderLeft);
                        apply(Word.WdBorderType.wdBorderRight);
                    }

                    return;
                }

                if (command == "shading")
                {
                    var scope = GetArg(args, "scope", "selection");
                    var range = ResolveScopeRange(doc, cursor, scope);
                    if (range == null)
                    {
                        return;
                    }

                    var colorText = GetArg(args, "color", GetArg(args, "value", ""));
                    Color sc;
                    if (TryParseColor(colorText, out sc))
                    {
                        range.Shading.BackgroundPatternColor = (Word.WdColor)ColorTranslator.ToOle(sc);
                    }

                    return;
                }

                if (command == "page")
                {
                    var orientation = GetArg(args, "orientation", "").ToLowerInvariant();
                    if (orientation == "landscape")
                    {
                        doc.PageSetup.Orientation = Word.WdOrientation.wdOrientLandscape;
                    }
                    else if (orientation == "portrait")
                    {
                        doc.PageSetup.Orientation = Word.WdOrientation.wdOrientPortrait;
                    }

                    var paper = GetArg(args, "paper", "").ToLowerInvariant();
                    if (paper == "a4")
                    {
                        doc.PageSetup.PaperSize = Word.WdPaperSize.wdPaperA4;
                    }

                    var margins = GetArg(args, "marginscm", "");
                    if (!string.IsNullOrWhiteSpace(margins))
                    {
                        var nums = margins.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (nums.Length >= 4)
                        {
                            float l;
                            float r;
                            float t;
                            float b;
                            if (TryParseFloat(nums[0], out l) && TryParseFloat(nums[1], out r) && TryParseFloat(nums[2], out t) && TryParseFloat(nums[3], out b))
                            {
                                doc.PageSetup.LeftMargin = WordApp != null ? WordApp.CentimetersToPoints(l) : l * 28.346f;
                                doc.PageSetup.RightMargin = WordApp != null ? WordApp.CentimetersToPoints(r) : r * 28.346f;
                                doc.PageSetup.TopMargin = WordApp != null ? WordApp.CentimetersToPoints(t) : t * 28.346f;
                                doc.PageSetup.BottomMargin = WordApp != null ? WordApp.CentimetersToPoints(b) : b * 28.346f;
                            }
                        }
                    }

                    return;
                }

                if (command == "find")
                {
                    var text = GetArg(args, "text", GetArg(args, "value", raw));
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        return;
                    }

                    var scope = GetArg(args, "scope", "document");
                    var range = ResolveScopeRange(doc, cursor, scope);
                    if (range == null)
                    {
                        return;
                    }

                    var find = range.Find;
                    find.ClearFormatting();
                    find.Text = text;
                    find.Forward = true;
                    find.Wrap = Word.WdFindWrap.wdFindStop;
                    if (find.Execute())
                    {
                        range.Select();
                    }

                    return;
                }

                if (command == "replace")
                {
                    var findText = GetArg(args, "find", "");
                    if (string.IsNullOrWhiteSpace(findText) && !string.IsNullOrWhiteSpace(raw))
                    {
                        findText = ExtractArg(raw, "find=", "replace=", "scope=");
                    }

                    var replaceText = GetArg(args, "replace", "");
                    if (string.IsNullOrWhiteSpace(replaceText) && !string.IsNullOrWhiteSpace(raw))
                    {
                        replaceText = ExtractArg(raw, "replace=", "scope=");
                    }

                    if (string.IsNullOrWhiteSpace(findText))
                    {
                        return;
                    }

                    var scope = GetArg(args, "scope", "document");
                    var range = ResolveScopeRange(doc, cursor, scope);
                    if (range == null)
                    {
                        return;
                    }

                    var all = true;
                    bool flag;
                    if (TryParseBool(GetArg(args, "all", ""), out flag))
                    {
                        all = flag;
                    }

                    var find = range.Find;
                    find.ClearFormatting();
                    find.Text = findText;
                    find.Replacement.ClearFormatting();
                    find.Replacement.Text = replaceText ?? "";
                    find.Forward = true;
                    find.Wrap = Word.WdFindWrap.wdFindStop;
                    find.Execute(Replace: all ? Word.WdReplace.wdReplaceAll : Word.WdReplace.wdReplaceOne);
                    return;
                }

                if (command == "select")
                {
                    var type = GetArg(args, "type", GetArg(args, "value", "all")).ToLowerInvariant();
                    if (type == "all" || type == "document" || type == "все")
                    {
                        doc.Content.Select();
                        return;
                    }

                    if (type == "paragraph" || type == "абзац")
                    {
                        try
                        {
                            cursor.Paragraphs.First.Range.Select();
                        }
                        catch
                        {
                        }

                        return;
                    }

                    if (type == "sentence" || type == "предложение")
                    {
                        try
                        {
                            var app = WordApp;
                            if (app?.Selection != null)
                            {
                                app.Selection.Sentences.First.Select();
                            }
                        }
                        catch
                        {
                        }

                        return;
                    }

                    if (type == "word" || type == "слово")
                    {
                        try
                        {
                            var app = WordApp;
                            if (app?.Selection != null)
                            {
                                app.Selection.Words.First.Select();
                            }
                        }
                        catch
                        {
                        }

                        return;
                    }
                }

                if (command == "hyperlink")
                {
                    var text = GetArg(args, "text", "");
                    var url = GetArg(args, "url", "");
                    if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(url))
                    {
                        return;
                    }

                    var s = cursor.End;
                    cursor.InsertAfter(text);
                    var e = s + text.Length;
                    var linkRange = doc.Range(s, e);
                    object missing = Type.Missing;
                    doc.Hyperlinks.Add(linkRange, url, ref missing, ref missing, text);
                    cursor.SetRange(e, e);
                    return;
                }

                if (command == "break")
                {
                    var type = GetArg(args, "type", GetArg(args, "value", "line")).ToLowerInvariant();
                    if (type == "page")
                    {
                        cursor.InsertBreak(Word.WdBreakType.wdPageBreak);
                    }
                    else
                    {
                        cursor.InsertParagraphAfter();
                        cursor.SetRange(cursor.End, cursor.End);
                    }

                    return;
                }

                if (command == "heading")
                {
                    var text = GetArg(args, "text", "");
                    int level;
                    if (!int.TryParse(GetArg(args, "level", "1"), out level))
                    {
                        level = 1;
                    }

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        return;
                    }

                    var s = cursor.End;
                    cursor.InsertAfter(text);
                    var e = s + text.Length;
                    var r = doc.Range(s, e);
                    if (level <= 1)
                    {
                        r.set_Style(doc.Styles[Word.WdBuiltinStyle.wdStyleHeading1]);
                    }
                    else if (level == 2)
                    {
                        r.set_Style(doc.Styles[Word.WdBuiltinStyle.wdStyleHeading2]);
                    }
                    else
                    {
                        r.set_Style(doc.Styles[Word.WdBuiltinStyle.wdStyleHeading3]);
                    }

                    cursor.SetRange(e, e);
                    cursor.InsertParagraphAfter();
                    cursor.SetRange(cursor.End, cursor.End);
                    return;
                }

                if (command == "list")
                {
                    var itemsRaw = GetArg(args, "items", "");
                    if (string.IsNullOrWhiteSpace(itemsRaw))
                    {
                        return;
                    }

                    var items = itemsRaw.Split(new[] { '|', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                    if (items.Count == 0)
                    {
                        return;
                    }

                    var s = cursor.End;
                    cursor.InsertAfter(string.Join("\r", items));
                    var e = cursor.End;
                    var r = doc.Range(s, e);
                    var type = GetArg(args, "type", "bullet").ToLowerInvariant();
                    if (type.StartsWith("num"))
                    {
                        r.ListFormat.ApplyNumberDefault();
                    }
                    else
                    {
                        r.ListFormat.ApplyBulletDefault();
                    }

                    cursor.SetRange(e, e);
                    cursor.InsertParagraphAfter();
                    cursor.SetRange(cursor.End, cursor.End);
                    return;
                }

                if (command == "table")
                {
                    var headers = GetArg(args, "headers", "");
                    var rowsRaw = GetArg(args, "rows", "");
                    if (string.IsNullOrWhiteSpace(headers))
                    {
                        headers = ExtractBetween(raw, "headers=", "rows=");
                    }

                    if (string.IsNullOrWhiteSpace(rowsRaw))
                    {
                        rowsRaw = ExtractBetween(raw, "rows=", "");
                    }

                    if (!string.IsNullOrWhiteSpace(headers) &&
                        headers.IndexOf('|') < 0 &&
                        headers.IndexOf(';') < 0)
                    {
                        headers = Regex.Replace(headers.Trim(), @"\s+", "|");
                    }

                    if (!string.IsNullOrWhiteSpace(rowsRaw) &&
                        rowsRaw.IndexOf('|') < 0 &&
                        rowsRaw.IndexOf(';') < 0)
                    {
                        rowsRaw = Regex.Replace(rowsRaw.Trim(), @"\s+", "|");
                    }

                    var headerList = headers.Split(new[] { '|', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                    var rows = new List<List<string>>();
                    if (!string.IsNullOrWhiteSpace(rowsRaw))
                    {
                        foreach (var rowRaw in rowsRaw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            rows.Add(rowRaw.Split(new[] { '|', ',' }, StringSplitOptions.None).Select(x => x.Trim()).ToList());
                        }
                    }

                    ApplyTableOverrides(ref headerList, ref rows);
                    var cols = headerList.Count > 0 ? headerList.Count : (rows.Count > 0 ? rows[0].Count : 0);
                    if (cols <= 0)
                    {
                        return;
                    }

                    var rowCount = Math.Max(1, rows.Count + (headerList.Count > 0 ? 1 : 0));
                    var table = doc.Tables.Add(cursor, rowCount, cols);
                    var rIndex = 1;
                    if (headerList.Count > 0)
                    {
                        for (var c = 0; c < cols; c++)
                        {
                            table.Cell(rIndex, c + 1).Range.Text = c < headerList.Count ? headerList[c] : "";
                        }

                        rIndex++;
                    }

                    foreach (var row in rows)
                    {
                        for (var c = 0; c < cols; c++)
                        {
                            table.Cell(rIndex, c + 1).Range.Text = c < row.Count ? row[c] : "";
                        }

                        rIndex++;
                    }

                    try
                    {
                        table.Rows[1].Range.Bold = 1;
                        table.AllowAutoFit = true;
                        table.AutoFitBehavior(Word.WdAutoFitBehavior.wdAutoFitContent);
                        table.Borders.Enable = 1;
                        table.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                        table.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    }
                    catch
                    {
                    }

                    cursor.SetRange(table.Range.End, table.Range.End);
                    cursor.InsertParagraphAfter();
                    cursor.SetRange(cursor.End, cursor.End);
                    return;
                }

                if (command == "chart")
                {
                    var spec = new ChartSpec();
                    spec.Type = GetArg(args, "type", "column");
                    spec.Title = GetArg(args, "title", "График");
                    var labelsRaw = GetArg(args, "labels", "");
                    var valuesRaw = GetArg(args, "values", "");
                    if (string.IsNullOrWhiteSpace(labelsRaw))
                    {
                        labelsRaw = ExtractBetween(raw, "labels=", "values=");
                    }

                    if (string.IsNullOrWhiteSpace(valuesRaw))
                    {
                        valuesRaw = ExtractBetween(raw, "values=", "");
                    }

                    if (!string.IsNullOrWhiteSpace(labelsRaw) &&
                        labelsRaw.IndexOf(',') < 0 &&
                        labelsRaw.IndexOf(';') < 0 &&
                        labelsRaw.IndexOf('|') < 0)
                    {
                        labelsRaw = Regex.Replace(labelsRaw.Trim(), @"\s+", ";");
                    }

                    if (!string.IsNullOrWhiteSpace(valuesRaw) &&
                        valuesRaw.IndexOf(',') < 0 &&
                        valuesRaw.IndexOf(';') < 0 &&
                        valuesRaw.IndexOf('|') < 0)
                    {
                        valuesRaw = Regex.Replace(valuesRaw.Trim(), @"\s+", ";");
                    }

                    spec.Labels = (labelsRaw ?? "").Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                    spec.Values = (valuesRaw ?? "").Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(v =>
                        {
                            double n;
                            var cleaned = v.Trim().Replace(" ", "").Replace(',', '.');
                            return double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out n) ? n : 0d;
                        })
                        .ToList();
                    InsertChartAtCursor(cursor, spec);
                    cursor.InsertParagraphAfter();
                    cursor.SetRange(cursor.End, cursor.End);
                    return;
                }

                if (command == "image")
                {
                    var path = GetArg(args, "path", "");
                    if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    {
                        return;
                    }

                    var pic = cursor.InlineShapes.AddPicture(path);
                    float w;
                    if (TryParseFloat(GetArg(args, "widthcm", ""), out w) && w > 0)
                    {
                        pic.Width = WordApp != null ? WordApp.CentimetersToPoints(w) : w * 28.346f;
                    }

                    float h;
                    if (TryParseFloat(GetArg(args, "heightcm", ""), out h) && h > 0)
                    {
                        pic.Height = WordApp != null ? WordApp.CentimetersToPoints(h) : h * 28.346f;
                    }

                    cursor.SetRange(pic.Range.End, pic.Range.End);
                    cursor.InsertParagraphAfter();
                    cursor.SetRange(cursor.End, cursor.End);
                }
            }
            catch
            {
            }
        }

        private Word.Range ResolveScopeRange(Word.Document doc, Word.Range cursor, string scope)
        {
            if (doc == null || cursor == null)
            {
                return null;
            }

            var mode = (scope ?? "").Trim().ToLowerInvariant();
            if (mode == "document")
            {
                return doc.Content;
            }

            var app = WordApp;
            if ((mode == "selection" || mode.Length == 0) && app?.Selection != null && app.Selection.Range != null)
            {
                var sel = app.Selection.Range;
                if (sel.Start != sel.End)
                {
                    return sel.Duplicate;
                }
            }

            if (mode == "paragraph")
            {
                try
                {
                    return cursor.Paragraphs.First.Range;
                }
                catch
                {
                }
            }

            return cursor.Duplicate;
        }

        private static string GetArg(Dictionary<string, string> args, string key, string fallback)
        {
            if (args != null && args.ContainsKey(key) && args[key] != null)
            {
                return args[key];
            }

            return fallback ?? "";
        }

        private static string ExtractBetween(string text, string startKey, string endKey)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(startKey))
            {
                return "";
            }

            var start = IndexOfKey(text, startKey, 0);
            if (start < 0)
            {
                return "";
            }

            start += startKey.Length;
            var end = string.IsNullOrWhiteSpace(endKey) ? -1 : IndexOfKey(text, endKey, start);
            if (end < 0)
            {
                end = text.Length;
            }

            if (end <= start)
            {
                return "";
            }

            return text.Substring(start, end - start).Trim();
        }

        private static string ExtractArg(string text, string key, params string[] endKeys)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(key))
            {
                return "";
            }

            var start = IndexOfKey(text, key, 0);
            if (start < 0)
            {
                return "";
            }

            start += key.Length;
            var end = text.Length;
            if (endKeys != null)
            {
                foreach (var endKey in endKeys)
                {
                    if (string.IsNullOrWhiteSpace(endKey))
                    {
                        continue;
                    }

                    var idx = IndexOfKey(text, endKey, start);
                    if (idx >= 0 && idx < end)
                    {
                        end = idx;
                    }
                }
            }

            if (end <= start)
            {
                return "";
            }

            return text.Substring(start, end - start).Trim().Trim('"');
        }

        private static int IndexOfKey(string text, string key, int startIndex)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(key))
            {
                return -1;
            }

            return text.IndexOf(key, startIndex, StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParseFloat(string text, out float value)
        {
            value = 0f;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var s = text.Trim().Replace(" ", "").Replace(',', '.');
            return float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryParseBool(string text, out bool value)
        {
            value = false;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var t = text.Trim().ToLowerInvariant();
            if (t == "1" || t == "true" || t == "yes" || t == "да" || t == "on")
            {
                value = true;
                return true;
            }

            if (t == "0" || t == "false" || t == "no" || t == "нет" || t == "off")
            {
                value = false;
                return true;
            }

            return bool.TryParse(t, out value);
        }

        private static bool TryParseHexColor(string text, out Color color)
        {
            color = Color.Black;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var v = text.Trim();
            if (!v.StartsWith("#"))
            {
                v = "#" + v;
            }

            try
            {
                color = ColorTranslator.FromHtml(v);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryParseColor(string text, out Color color)
        {
            color = Color.Black;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (TryParseHexColor(text, out color))
            {
                return true;
            }

            var key = text.Trim();
            var lower = key.ToLowerInvariant();
            switch (lower)
            {
                case "black":
                case "черный":
                    color = Color.Black;
                    return true;
                case "white":
                case "белый":
                    color = Color.White;
                    return true;
                case "red":
                case "красный":
                    color = Color.Red;
                    return true;
                case "blue":
                case "синий":
                    color = Color.Blue;
                    return true;
                case "green":
                case "зеленый":
                    color = Color.Green;
                    return true;
                case "gray":
                case "grey":
                case "серый":
                    color = Color.Gray;
                    return true;
                case "yellow":
                case "желтый":
                    color = Color.Yellow;
                    return true;
            }

            var named = Color.FromName(key);
            if (named.ToArgb() != 0)
            {
                color = named;
                return true;
            }

            return false;
        }

        private static bool TryParseRelativeFloat(string text, float current, out float value)
        {
            value = 0f;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var trimmed = text.Trim();
            if (trimmed.StartsWith("+") || trimmed.StartsWith("-"))
            {
                float delta;
                if (TryParseFloat(trimmed, out delta))
                {
                    value = current + delta;
                    return true;
                }

                return false;
            }

            return TryParseFloat(trimmed, out value);
        }

        private static bool TryParseHighlightColor(string text, out Word.WdColorIndex color)
        {
            color = Word.WdColorIndex.wdNoHighlight;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var v = text.Trim().ToLowerInvariant();
            switch (v)
            {
                case "none":
                case "no":
                case "нет":
                case "без":
                    color = Word.WdColorIndex.wdNoHighlight;
                    return true;
                case "yellow":
                case "желтый":
                    color = Word.WdColorIndex.wdYellow;
                    return true;
                case "green":
                case "brightgreen":
                case "ярко-зеленый":
                case "зеленый":
                    color = Word.WdColorIndex.wdBrightGreen;
                    return true;
                case "turquoise":
                case "бирюзовый":
                    color = Word.WdColorIndex.wdTurquoise;
                    return true;
                case "pink":
                case "розовый":
                    color = Word.WdColorIndex.wdPink;
                    return true;
                case "blue":
                case "синий":
                    color = Word.WdColorIndex.wdBlue;
                    return true;
                case "red":
                case "красный":
                    color = Word.WdColorIndex.wdRed;
                    return true;
                case "darkblue":
                case "темно-синий":
                    color = Word.WdColorIndex.wdDarkBlue;
                    return true;
                case "teal":
                case "темно-зеленый":
                    color = Word.WdColorIndex.wdTeal;
                    return true;
                case "violet":
                case "фиолетовый":
                    color = Word.WdColorIndex.wdViolet;
                    return true;
                case "darkred":
                case "темно-красный":
                    color = Word.WdColorIndex.wdDarkRed;
                    return true;
                case "darkyellow":
                case "темно-желтый":
                    color = Word.WdColorIndex.wdDarkYellow;
                    return true;
                case "gray50":
                case "серый":
                    color = Word.WdColorIndex.wdGray50;
                    return true;
                case "gray25":
                case "светло-серый":
                    color = Word.WdColorIndex.wdGray25;
                    return true;
                case "black":
                case "черный":
                    color = Word.WdColorIndex.wdBlack;
                    return true;
            }

            return false;
        }

        private static bool TryMapCharacterCase(string mode, out Word.WdCharacterCase cc)
        {
            cc = Word.WdCharacterCase.wdLowerCase;
            if (string.IsNullOrWhiteSpace(mode))
            {
                return false;
            }

            var v = mode.Trim().ToLowerInvariant();
            switch (v)
            {
                case "upper":
                case "uppercase":
                case "верхний":
                case "вверх":
                case "прописные":
                    cc = Word.WdCharacterCase.wdUpperCase;
                    return true;
                case "lower":
                case "lowercase":
                case "нижний":
                case "строчные":
                    cc = Word.WdCharacterCase.wdLowerCase;
                    return true;
                case "title":
                case "titlecase":
                case "каждое":
                case "каждое_слово":
                    cc = Word.WdCharacterCase.wdTitleWord;
                    return true;
                case "sentence":
                case "sentencecase":
                case "предложение":
                    cc = (Word.WdCharacterCase)3;
                    return true;
                case "toggle":
                case "togglecase":
                case "переключить":
                    cc = Word.WdCharacterCase.wdToggleCase;
                    return true;
            }

            return false;
        }

        private bool ApplyStyle(Word.Document doc, Word.Range range, string name)
        {
            if (doc == null || range == null || string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var key = name.Trim();
            var lower = key.ToLowerInvariant();
            Word.WdBuiltinStyle? builtin = null;
            if (lower == "normal" || lower == "обычный")
            {
                builtin = Word.WdBuiltinStyle.wdStyleNormal;
            }
            else if (lower.StartsWith("heading") || lower.StartsWith("заголовок"))
            {
                var m = Regex.Match(lower, @"(\d+)");
                int level;
                if (m.Success && int.TryParse(m.Groups[1].Value, out level))
                {
                    if (level <= 1)
                    {
                        builtin = Word.WdBuiltinStyle.wdStyleHeading1;
                    }
                    else if (level == 2)
                    {
                        builtin = Word.WdBuiltinStyle.wdStyleHeading2;
                    }
                    else
                    {
                        builtin = Word.WdBuiltinStyle.wdStyleHeading3;
                    }
                }
            }

            try
            {
                if (builtin.HasValue)
                {
                    range.set_Style(doc.Styles[builtin.Value]);
                    return true;
                }

                if (lower == "no spacing" || lower == "без интервала" || lower == "безинтервала")
                {
                    try
                    {
                        range.set_Style(doc.Styles["No Spacing"]);
                        return true;
                    }
                    catch
                    {
                        range.set_Style(doc.Styles["Без интервала"]);
                        return true;
                    }
                }

                range.set_Style(doc.Styles[key]);
                return true;
            }
            catch
            {
                try
                {
                    foreach (Word.Style style in doc.Styles)
                    {
                        var nameLocal = style.NameLocal ?? "";
                        if (string.Equals(nameLocal, key, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(nameLocal, key, StringComparison.OrdinalIgnoreCase))
                        {
                            range.set_Style(style);
                            return true;
                        }
                    }
                }
                catch
                {
                }
            }

            return false;
        }

        private bool TryParseChartBlock(string[] lines, ref int index, out ChartSpec chart)
        {
            chart = null;
            if (lines == null || index < 0 || index >= lines.Length)
            {
                return false;
            }

            var head = (lines[index] ?? "").Trim().ToLowerInvariant();
            if (head != "```chart" && head != "[chart]" && head != "chart:")
            {
                return false;
            }

            var spec = new ChartSpec { Type = "column", Title = "График" };
            var i = index + 1;
            for (; i < lines.Length; i++)
            {
                var line = (lines[i] ?? "").Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                var lower = line.ToLowerInvariant();
                if (lower == "```" || lower == "[/chart]" || lower == "endchart")
                {
                    break;
                }

                var colon = line.IndexOf(':');
                if (colon <= 0)
                {
                    continue;
                }

                var key = line.Substring(0, colon).Trim().ToLowerInvariant();
                var value = line.Substring(colon + 1).Trim();
                if (key == "type")
                {
                    spec.Type = value;
                }
                else if (key == "title")
                {
                    spec.Title = value;
                }
                else if (key == "labels")
                {
                    spec.Labels = value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                }
                else if (key == "values")
                {
                    spec.Values = value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(v =>
                        {
                            double num;
                            var cleaned = v.Trim().Replace(" ", "").Replace(',', '.');
                            return double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out num) ? num : 0d;
                        })
                        .ToList();
                }
            }

            index = i;
            if (spec.Labels.Count == 0 || spec.Values.Count == 0)
            {
                return true;
            }

            chart = spec;
            return true;
        }

        private void InsertChartAtCursor(Word.Range cursor, ChartSpec chart)
        {
            if (cursor == null || chart == null || chart.Labels.Count == 0 || chart.Values.Count == 0)
            {
                return;
            }

            try
            {
                dynamic inlineShapes = cursor.InlineShapes;
                dynamic inline = null;
                try
                {
                    inline = inlineShapes.AddChart2(-1, ResolveChartType(chart.Type), cursor);
                }
                catch
                {
                    inline = inlineShapes.AddChart(ResolveChartType(chart.Type), cursor);
                }

                if (inline == null)
                {
                    return;
                }

                dynamic chartObj = inline.Chart;
                chartObj.HasTitle = true;
                chartObj.ChartTitle.Text = string.IsNullOrWhiteSpace(chart.Title) ? "График" : chart.Title;

                dynamic wb = chartObj.ChartData.Workbook;
                dynamic ws = wb.Worksheets[1];
                ws.Cells[1, 1] = "Категория";
                ws.Cells[1, 2] = "Значение";

                var count = Math.Min(chart.Labels.Count, chart.Values.Count);
                for (var i = 0; i < count; i++)
                {
                    ws.Cells[i + 2, 1] = chart.Labels[i];
                    ws.Cells[i + 2, 2] = chart.Values[i];
                }

                dynamic dataRange = ws.Range[ws.Cells[1, 1], ws.Cells[count + 1, 2]];
                chartObj.SetSourceData(dataRange);
                cursor.SetRange((int)inline.Range.End, (int)inline.Range.End);
            }
            catch
            {
            }
        }

        private static Microsoft.Office.Core.XlChartType ResolveChartType(string type)
        {
            var value = (type ?? "").Trim().ToLowerInvariant();
            if (value.Contains("line") || value.Contains("линей"))
            {
                return Microsoft.Office.Core.XlChartType.xlLine;
            }

            if (value.Contains("pie") || value.Contains("круг"))
            {
                return Microsoft.Office.Core.XlChartType.xlPie;
            }

            if (value.Contains("bar") || value.Contains("полос"))
            {
                return Microsoft.Office.Core.XlChartType.xlBarClustered;
            }

            if (value.Contains("area") || value.Contains("област"))
            {
                return Microsoft.Office.Core.XlChartType.xlArea;
            }

            return Microsoft.Office.Core.XlChartType.xlColumnClustered;
        }

        private void ShowResult(string title, string original, string result, Action apply)
        {
            using (var dialog = new ResultDialog(title, original, result, apply))
            {
                dialog.ShowDialog();
            }
        }

        private string GetSelectedText()
        {
            var app = WordApp;
            if (app?.Selection == null)
            {
                return "";
            }

            if (app.Selection.Type != Word.WdSelectionType.wdSelectionNormal)
            {
                return "";
            }

            return (app.Selection.Text ?? "").Trim();
        }

        private string GetDocumentText()
        {
            try
            {
                var app = WordApp;
                if (app?.ActiveDocument == null)
                {
                    return "";
                }

                var text = app.ActiveDocument.Content.Text;
                return (text ?? "").Trim();
            }
            catch
            {
                return "";
            }
        }

        private static string Clip(string text, int max)
        {
            if (text == null)
            {
                return "";
            }

            return text.Length > max ? text.Substring(0, max) + "..." : text;
        }

        private static string NormalizeForWordText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            var raw = (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n');
            var lines = raw.Split('\n');
            var parts = new List<string>();
            var paragraph = new StringBuilder();

            Action flush = () =>
            {
                if (paragraph.Length == 0)
                {
                    return;
                }

                var value = paragraph.ToString().Trim();
                if (value.Length > 0)
                {
                    parts.Add(value);
                }

                paragraph.Clear();
            };

            foreach (var sourceLine in lines)
            {
                var line = CollapseSpaces((sourceLine ?? "").Trim());
                if (line.Length == 0)
                {
                    flush();
                    continue;
                }

                if (IsListLikeLine(line))
                {
                    flush();
                    parts.Add(line);
                    continue;
                }

                if (paragraph.Length > 0)
                {
                    paragraph.Append(' ');
                }

                paragraph.Append(line);
            }

            flush();
            return string.Join("\r", parts);
        }

        private static string NormalizeForWordSmart(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            var raw = (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n');
            var lines = raw.Split('\n');
            var normalized = new List<string>();
            var paragraph = new StringBuilder();
            var inFence = false;
            var previousWasList = false;

            Action flushParagraph = () =>
            {
                if (paragraph.Length == 0)
                {
                    return;
                }

                var value = CollapseSpaces(paragraph.ToString());
                if (value.Length > 0)
                {
                    normalized.Add(value);
                }

                paragraph.Clear();
            };

            foreach (var sourceLine in lines)
            {
                var rawLine = sourceLine ?? "";
                var trimmedRaw = rawLine.Trim();
                if (trimmedRaw.Length == 0)
                {
                    flushParagraph();
                    if (normalized.Count > 0 && normalized[normalized.Count - 1].Length > 0)
                    {
                        normalized.Add("");
                    }

                    previousWasList = false;
                    continue;
                }

                if (trimmedRaw.StartsWith("```"))
                {
                    flushParagraph();
                    normalized.Add(trimmedRaw);
                    inFence = !inFence;
                    previousWasList = false;
                    continue;
                }

                if (inFence)
                {
                    normalized.Add(trimmedRaw);
                    previousWasList = false;
                    continue;
                }

                if (IsVerbatimLine(trimmedRaw))
                {
                    flushParagraph();
                    normalized.Add(trimmedRaw);
                    previousWasList = false;
                    continue;
                }

                var line = CollapseSpaces(trimmedRaw);
                var isHeading = IsHeadingLikeLine(line);
                var isList = IsListLikeLine(line);
                if (isHeading || isList)
                {
                    flushParagraph();
                    normalized.Add(line);
                    previousWasList = isList;
                    continue;
                }

                if (previousWasList && normalized.Count > 0 && IsListLikeLine(normalized[normalized.Count - 1]))
                {
                    normalized[normalized.Count - 1] = normalized[normalized.Count - 1] + " " + line;
                    continue;
                }

                if (paragraph.Length > 0)
                {
                    paragraph.Append(' ');
                }

                paragraph.Append(line);
                previousWasList = false;
            }

            flushParagraph();
            return JoinNormalizedLines(normalized);
        }

        private static string NormalizeTableOnly(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            var rawLines = (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            var lines = MergeWordCommandBlocks(rawLines);
            var tableLines = new List<string>();
            var hasWordTableCommand = false;
            foreach (var sourceLine in lines)
            {
                var line = (sourceLine ?? "").Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                if (line.StartsWith("@word:", StringComparison.OrdinalIgnoreCase))
                {
                    tableLines.Add(line);
                    if (Regex.IsMatch(line, @"^@word:\s*table\b", RegexOptions.IgnoreCase))
                    {
                        hasWordTableCommand = true;
                    }
                    continue;
                }

                if (line.StartsWith("|") ||
                    line.Contains("\t") ||
                    CountDelimiter(line, ';') >= 1 ||
                    CountDelimiter(line, '|') >= 1)
                {
                    tableLines.Add(line);
                }
            }

            if (tableLines.Count >= 2 || hasWordTableCommand)
            {
                return string.Join("\r", tableLines);
            }

            return NormalizeForWordSmart(text);
        }

        private int CountTablesInText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            var rawLines = (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            var lines = MergeWordCommandBlocks(rawLines);
            var arr = lines.ToArray();
            var count = 0;
            for (var i = 0; i < arr.Length; i++)
            {
                var line = (arr[i] ?? "").Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                if (Regex.IsMatch(line, @"^@word:\s*table\b", RegexOptions.IgnoreCase))
                {
                    count++;
                    continue;
                }

                List<List<string>> rows;
                if (TryParseTabularBlock(arr, ref i, out rows))
                {
                    count++;
                }
            }

            return count;
        }

        private static int GetExpectedTableCount(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return 0;
            }

            var lower = prompt.ToLowerInvariant();
            var match = Regex.Match(lower, @"(\d+)\s*(табл|table)");
            if (match.Success)
            {
                int n;
                if (int.TryParse(match.Groups[1].Value, out n) && n > 0)
                {
                    return n;
                }
            }

            if (Regex.IsMatch(lower, @"\bодн(у|а)?\s+табл"))
            {
                return 1;
            }
            if (Regex.IsMatch(lower, @"\bдве\s+табл"))
            {
                return 2;
            }
            if (Regex.IsMatch(lower, @"\bтри\s+табл"))
            {
                return 3;
            }
            if (Regex.IsMatch(lower, @"\bчетыре\s+табл"))
            {
                return 4;
            }
            if (Regex.IsMatch(lower, @"\bпять\s+табл"))
            {
                return 5;
            }
            if (Regex.IsMatch(lower, @"\bпару\s+табл"))
            {
                return 2;
            }
            if (Regex.IsMatch(lower, @"\bнесколько\s+табл"))
            {
                return 2;
            }

            return 0;
        }

        private static void GetExpectedTableDimensions(string prompt, out int rows, out int cols)
        {
            rows = 0;
            cols = 0;
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return;
            }

            var lower = prompt.ToLowerInvariant();
            var colMatch = Regex.Match(lower, @"(\d+)\s*(столб|колонк|column|cols)");
            if (colMatch.Success)
            {
                int n;
                if (int.TryParse(colMatch.Groups[1].Value, out n) && n > 0)
                {
                    cols = n;
                }
            }

            var rowMatch = Regex.Match(lower, @"(\d+)\s*(строк|ряд|row)");
            if (rowMatch.Success)
            {
                int n;
                if (int.TryParse(rowMatch.Groups[1].Value, out n) && n > 0)
                {
                    rows = n;
                }
            }

            if (rows == 0 || cols == 0)
            {
                var sizeMatch = Regex.Match(lower, @"(\d+)\s*[xх*]\s*(\d+)");
                if (sizeMatch.Success)
                {
                    int a;
                    int b;
                    if (int.TryParse(sizeMatch.Groups[1].Value, out a) && int.TryParse(sizeMatch.Groups[2].Value, out b))
                    {
                        if (cols == 0 && a > 0)
                        {
                            cols = a;
                        }

                        if (rows == 0 && b > 0)
                        {
                            rows = b;
                        }
                    }
                }
            }
        }

        private static string BuildTableRetryPrompt(string basePrompt, int expectedTables, int expectedRows, int expectedCols)
        {
            var sb = new StringBuilder();
            sb.AppendLine(basePrompt ?? "");
            sb.AppendLine();
            sb.AppendLine("Повтор: предыдущий ответ не содержал нужное количество таблиц.");
            if (expectedTables > 0)
            {
                sb.AppendLine("Нужно ровно " + expectedTables + " таблиц.");
            }
            if (expectedRows > 0 || expectedCols > 0)
            {
                var rows = expectedRows > 0 ? expectedRows.ToString() : "?";
                var cols = expectedCols > 0 ? expectedCols.ToString() : "?";
                sb.AppendLine("Размер таблицы: " + cols + " столбца(ов) и " + rows + " строки(ок).");
            }

            sb.AppendLine("Верни ТОЛЬКО команды @word:table, каждая на отдельной строке. Никакого другого текста.");
            sb.AppendLine("Если параметры длинные, используй блок:");
            sb.AppendLine("@word:table");
            sb.AppendLine("headers=...");
            sb.AppendLine("rows=...");
            sb.AppendLine("@end");
            return sb.ToString();
        }

        private void ApplyPostFormattingIfRequested(string userPrompt, Word.Document doc, Word.Range insertedRange)
        {
            if (doc == null || string.IsNullOrWhiteSpace(userPrompt))
            {
                return;
            }

            var lower = userPrompt.ToLowerInvariant();
            var range = ResolveFormattingScope(lower, doc, insertedRange);
            if (range == null)
            {
                return;
            }

            bool? italic = ParseToggle(lower, "курсив", "italic");
            bool? bold = ParseToggle(lower, "жирн", "bold");
            bool? underline = ParseToggle(lower, "подчерк", "underline");
            bool? strike = ParseToggle(lower, "зачерк", "strike");

            if (italic.HasValue)
            {
                range.Font.Italic = italic.Value ? 1 : 0;
            }

            if (bold.HasValue)
            {
                range.Font.Bold = bold.Value ? 1 : 0;
            }

            if (underline.HasValue)
            {
                range.Font.Underline = underline.Value ? Word.WdUnderline.wdUnderlineSingle : Word.WdUnderline.wdUnderlineNone;
            }

            if (strike.HasValue)
            {
                range.Font.StrikeThrough = strike.Value ? 1 : 0;
            }

            Word.WdParagraphAlignment align;
            if (TryParseAlignment(lower, out align))
            {
                range.ParagraphFormat.Alignment = align;
            }

            float size;
            if (TryExtractFontSize(lower, out size) && size > 0)
            {
                range.Font.Size = size;
            }

            string fontName;
            if (TryExtractFontName(userPrompt, out fontName))
            {
                range.Font.Name = fontName;
            }

            string styleName;
            if (TryExtractStyleName(lower, out styleName))
            {
                ApplyStyle(doc, range, styleName);
            }
        }

        private Word.Range ResolveFormattingScope(string lowerPrompt, Word.Document doc, Word.Range insertedRange)
        {
            if (doc == null)
            {
                return null;
            }

            if (WantsDocumentScope(lowerPrompt))
            {
                return doc.Content;
            }

            if (ContainsAny(lowerPrompt, "выдел", "selection", "выделенный"))
            {
                var app = WordApp;
                if (app?.Selection != null && app.Selection.Range != null && app.Selection.Range.Start != app.Selection.Range.End)
                {
                    return app.Selection.Range.Duplicate;
                }
            }

            if (insertedRange != null)
            {
                return insertedRange.Duplicate;
            }

            return doc.Content;
        }

        private static bool WantsDocumentScope(string lowerPrompt)
        {
            if (string.IsNullOrWhiteSpace(lowerPrompt))
            {
                return false;
            }

            return ContainsAny(lowerPrompt,
                "весь документ",
                "весь текст",
                "ко всему документу",
                "по всему документу",
                "во всем документе",
                "все абзацы",
                "все страницы",
                "весь файл");
        }

        private static bool IsFormattingOnlyRequest(string prompt)
        {
            var lower = (prompt ?? "").ToLowerInvariant();
            if (lower.Length == 0)
            {
                return false;
            }

            if (ContainsAny(lower, "таблиц", "table", "диаграм", "chart", "график", "рисунок", "изображен", "картин", "фото"))
            {
                return false;
            }

            if (ContainsAny(lower,
                "сгенер", "напиши", "создай", "добавь", "продолж", "сформируй",
                "объясни", "опиши", "ответь", "расскажи", "перевед",
                "перепиш", "перефраз", "исправ", "измен", "отредакт",
                "улучш", "сократ", "расшир", "замени", "переработ"))
            {
                return false;
            }

            return ContainsAny(lower,
                "курсив", "italic", "жирн", "bold", "подчерк", "underline",
                "зачерк", "strike", "по ширине", "по центру", "вправо", "влево",
                "выравн", "шрифт", "размер", "пт", "стиль", "заголовок",
                "без интервала", "регистр", "uppercase", "lowercase");
        }

        private static bool IsModifyRequest(string prompt)
        {
            var lower = (prompt ?? "").ToLowerInvariant();
            if (lower.Length == 0)
            {
                return false;
            }

            return ContainsAny(lower,
                "исправ", "измен", "перепиш", "перефраз", "улучш",
                "сократ", "расшир", "подправ", "отредакт", "перевед",
                "передел", "преобраз", "замени", "переработ", "сделай иначе");
        }

        private static bool? ParseToggle(string lowerPrompt, string positiveToken, string altToken)
        {
            if (Regex.IsMatch(lowerPrompt, @"не\s+" + positiveToken))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(altToken) && Regex.IsMatch(lowerPrompt, @"не\s+" + altToken))
            {
                return false;
            }

            if (lowerPrompt.Contains(positiveToken) || (!string.IsNullOrWhiteSpace(altToken) && lowerPrompt.Contains(altToken)))
            {
                return true;
            }

            return null;
        }

        private static bool TryParseAlignment(string lowerPrompt, out Word.WdParagraphAlignment align)
        {
            align = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            if (ContainsAny(lowerPrompt, "по ширине", "по всей ширине", "justify"))
            {
                align = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                return true;
            }

            if (ContainsAny(lowerPrompt, "по центру", "центр", "center"))
            {
                align = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                return true;
            }

            if (ContainsAny(lowerPrompt, "по правому", "вправо", "right"))
            {
                align = Word.WdParagraphAlignment.wdAlignParagraphRight;
                return true;
            }

            if (ContainsAny(lowerPrompt, "по левому", "влево", "left"))
            {
                align = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                return true;
            }

            return false;
        }

        private static bool TryExtractFontSize(string lowerPrompt, out float size)
        {
            size = 0f;
            var m = Regex.Match(lowerPrompt, @"(размер|size)\s*(\d{1,2})");
            if (m.Success && float.TryParse(m.Groups[2].Value, out size))
            {
                return true;
            }

            var m2 = Regex.Match(lowerPrompt, @"\b(\d{1,2})\s*пт\b");
            if (m2.Success && float.TryParse(m2.Groups[1].Value, out size))
            {
                return true;
            }

            return false;
        }

        private static bool TryExtractFontName(string prompt, out string name)
        {
            name = "";
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return false;
            }

            var m = Regex.Match(prompt, "(?:шрифт|font)\\s*[\"'«](.+?)[\"'»]", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                name = m.Groups[1].Value.Trim();
                return name.Length > 0;
            }

            var known = new[]
            {
                "Times New Roman",
                "Calibri",
                "Arial",
                "Tahoma",
                "Courier New"
            };

            foreach (var font in known)
            {
                if (prompt.IndexOf(font, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    name = font;
                    return true;
                }
            }

            return false;
        }

        private static bool TryExtractStyleName(string lowerPrompt, out string styleName)
        {
            styleName = "";
            if (Regex.IsMatch(lowerPrompt, @"заголовок\\s*1") || Regex.IsMatch(lowerPrompt, @"heading\\s*1"))
            {
                styleName = "Заголовок 1";
                return true;
            }
            if (Regex.IsMatch(lowerPrompt, @"заголовок\\s*2") || Regex.IsMatch(lowerPrompt, @"heading\\s*2"))
            {
                styleName = "Заголовок 2";
                return true;
            }
            if (Regex.IsMatch(lowerPrompt, @"заголовок\\s*3") || Regex.IsMatch(lowerPrompt, @"heading\\s*3"))
            {
                styleName = "Заголовок 3";
                return true;
            }
            if (ContainsAny(lowerPrompt, "без интервала", "no spacing", "безинтервала"))
            {
                styleName = "Без интервала";
                return true;
            }
            if (ContainsAny(lowerPrompt, "обычный", "normal"))
            {
                styleName = "Обычный";
                return true;
            }

            return false;
        }

        private void ApplyTableOverrides(List<List<string>> rows)
        {
            if (rows == null)
            {
                return;
            }

            var targetCols = _forcedTableCols;
            var targetRows = _forcedTableRows;
            if (rows.Count == 0)
            {
                if (targetCols > 0 || targetRows > 0)
                {
                    var cols = targetCols > 0 ? targetCols : 1;
                    var count = targetRows > 0 ? targetRows : 1;
                    for (var i = 0; i < count; i++)
                    {
                        rows.Add(Enumerable.Repeat("", cols).ToList());
                    }
                }

                return;
            }

            var colCount = targetCols > 0 ? targetCols : Math.Max(1, rows.Max(r => r.Count));

            if (targetCols > 0)
            {
                for (var r = 0; r < rows.Count; r++)
                {
                    while (rows[r].Count < targetCols)
                    {
                        rows[r].Add("");
                    }

                    if (rows[r].Count > targetCols)
                    {
                        rows[r].RemoveRange(targetCols, rows[r].Count - targetCols);
                    }
                }
            }

            if (targetRows > 0)
            {
                if (rows.Count > targetRows)
                {
                    rows.RemoveRange(targetRows, rows.Count - targetRows);
                }
                else if (rows.Count < targetRows)
                {
                    while (rows.Count < targetRows)
                    {
                        rows.Add(Enumerable.Repeat("", colCount).ToList());
                    }
                }
            }
        }

        private void ApplyTableOverrides(ref List<string> headerList, ref List<List<string>> rows)
        {
            var combined = new List<List<string>>();
            var hasHeader = headerList != null && headerList.Count > 0;
            if (hasHeader)
            {
                combined.Add(new List<string>(headerList));
            }

            if (rows != null && rows.Count > 0)
            {
                foreach (var row in rows)
                {
                    combined.Add(new List<string>(row));
                }
            }

            ApplyTableOverrides(combined);

            if (hasHeader)
            {
                headerList = combined.Count > 0 ? combined[0] : new List<string>();
                rows = combined.Skip(1).ToList();
            }
            else
            {
                rows = combined;
            }
        }

        private static List<string> MergeWordCommandBlocks(string[] lines)
        {
            var result = new List<string>();
            if (lines == null || lines.Length == 0)
            {
                return result;
            }

            for (var i = 0; i < lines.Length; i++)
            {
                var raw = lines[i] ?? "";
                var line = raw.Trim();
                if (!line.StartsWith("@word:", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(raw);
                    continue;
                }

                var sb = new StringBuilder(line);
                var j = i + 1;
                for (; j < lines.Length; j++)
                {
                    var nextRaw = lines[j] ?? "";
                    var next = nextRaw.Trim();
                    if (next.Length == 0)
                    {
                        break;
                    }

                    if (next.Equals("@end", StringComparison.OrdinalIgnoreCase))
                    {
                        j++;
                        break;
                    }

                    if (next.StartsWith("@word:", StringComparison.OrdinalIgnoreCase) ||
                        next.Equals("```chart", StringComparison.OrdinalIgnoreCase) ||
                        next.Equals("[chart]", StringComparison.OrdinalIgnoreCase) ||
                        next.Equals("chart:", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    sb.Append(' ').Append(next);
                }

                result.Add(sb.ToString());
                i = j - 1;
            }

            return result;
        }

        private static string NormalizeForWordLiteral(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            return (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n').Trim().Replace("\n", "\r");
        }

        private static bool IsListLikeLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            return
                Regex.IsMatch(line, @"^([\-*•–—]|\d+[\.\)]|[A-Za-zА-Яа-я]\))\s+") ||
                Regex.IsMatch(line, @"^\d+(?:\.\d+){0,3}\s+[А-ЯA-Z]");
        }

        private static bool IsHeadingLikeLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            return
                Regex.IsMatch(line, @"^#{1,3}\s+\S+") ||
                Regex.IsMatch(line, @"^\d+(?:\.\d+){0,3}\s+\S+") ||
                line.StartsWith("Таблица ", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Рисунок ", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsVerbatimLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            return
                line.StartsWith("@word:", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("|") ||
                line.Contains("\t") ||
                CountDelimiter(line, ';') >= 1;
        }

        private static int CountDelimiter(string text, char delimiter)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            var count = 0;
            foreach (var ch in text)
            {
                if (ch == delimiter)
                {
                    count++;
                }
            }

            return count;
        }

        private static string JoinNormalizedLines(List<string> lines)
        {
            if (lines == null || lines.Count == 0)
            {
                return "";
            }

            var cleaned = new List<string>();
            foreach (var line in lines)
            {
                var value = line ?? "";
                if (value.Length == 0 && (cleaned.Count == 0 || cleaned[cleaned.Count - 1].Length == 0))
                {
                    continue;
                }

                cleaned.Add(value);
            }

            while (cleaned.Count > 0 && cleaned[cleaned.Count - 1].Length == 0)
            {
                cleaned.RemoveAt(cleaned.Count - 1);
            }

            return string.Join("\r", cleaned);
        }

        private static string CollapseSpaces(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        private static List<string> ParseListItems(string text)
        {
            var items = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
            {
                return items;
            }

            var raw = (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n');
            foreach (var line in raw.Split('\n'))
            {
                var item = (line ?? "").Trim();
                if (item.Length == 0)
                {
                    continue;
                }

                item = Regex.Replace(item, @"^([\-*•–—]|\d+[\.\)]|[A-Za-zА-Яа-я]\))\s*", "");
                item = CollapseSpaces(item);
                if (item.Length > 0)
                {
                    items.Add(item);
                }
            }

            return items;
        }

        private void ReplaceSelected(string text)
        {
            var app = WordApp;
            if (app?.Selection != null && app.Selection.Type == Word.WdSelectionType.wdSelectionNormal)
            {
                app.Selection.TypeText(NormalizeForWordText(text));
            }
        }

        private void ReplaceSelectedTracked(string text)
        {
            var app = WordApp;
            if (app?.ActiveDocument == null || app.Selection == null)
            {
                return;
            }

            var doc = app.ActiveDocument;
            var prevTrack = doc.TrackRevisions;
            doc.TrackRevisions = true;

            try
            {
                app.Selection.TypeText(NormalizeForWordText(text));
            }
            finally
            {
                doc.TrackRevisions = prevTrack;
            }
        }

        private void InsertAtCursor(string text)
        {
            var app = WordApp;
            if (app?.Selection != null)
            {
                app.Selection.TypeText(NormalizeForWordText(text));
            }
        }

        private void InsertAtDocumentTop(string heading, string text)
        {
            var app = WordApp;
            if (app?.ActiveDocument == null)
            {
                return;
            }

            var doc = app.ActiveDocument;
            var top = doc.Range(0, 0);
            var block = NormalizeForWordLiteral(heading) + "\r" + NormalizeForWordText(text) + "\r\r";
            top.InsertBefore(block);
        }

        private void InsertAsComment(string text)
        {
            var app = WordApp;
            if (app?.ActiveDocument == null || app.Selection == null)
            {
                return;
            }

            var range = app.Selection.Range;
            if (range == null)
            {
                return;
            }

            if (range.Start == range.End)
            {
                var end = Math.Min(range.Start + 1, app.ActiveDocument.Content.End);
                range = app.ActiveDocument.Range(range.Start, end);
            }

            app.ActiveDocument.Comments.Add(range, NormalizeForWordText(text));
        }

        private static void NoSelection(string msg)
        {
            MessageBox.Show(msg, "Ollama AI", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnImprove_Click(object sender, RibbonControlEventArgs e)
        {
            var text = GetSelectedText();
            if (text.Length == 0)
            {
                NoSelection("Сначала выделите текст.");
                return;
            }

            RunAIReplaceSelectedLive(
                "Улучшаю текст...",
                (onChunk, ct) => _ollama.GenerateStreamAsync(
                    _selectedModel,
                    EnsureRussianPrompt("Улучши стиль, грамматику и читаемость. Верни только улучшенный текст.\n\n" + text),
                    _settings.SystemPrompt,
                    onChunk,
                    ct),
                false);
        }

        private void btnRewriteTracked_Click(object sender, RibbonControlEventArgs e)
        {
            var text = GetSelectedText();
            if (text.Length == 0)
            {
                NoSelection("Сначала выделите текст.");
                return;
            }

            RunAIReplaceSelectedLive(
                "Переписываю с режимом правок...",
                (onChunk, ct) => _ollama.GenerateStreamAsync(
                    _selectedModel,
                    EnsureRussianPrompt("Перепиши текст ясно и профессионально, сохранив смысл. Верни только итоговый текст.\n\n" + text),
                    _settings.SystemPrompt,
                    onChunk,
                    ct),
                true);
        }

        private void btnTranslate_Click(object sender, RibbonControlEventArgs e)
        {
            var text = GetSelectedText();
            if (text.Length == 0)
            {
                NoSelection("Сначала выделите текст.");
                return;
            }

            using (var dialog = new TranslateDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var lang = dialog.Lang;
                RunAIReplaceSelectedLive(
                    "Перевожу...",
                    (onChunk, ct) => _ollama.GenerateStreamAsync(
                        _selectedModel,
                        "Переведи на язык: " + lang + ". Верни только перевод, без пояснений.\n\n" + text,
                        _settings.SystemPrompt,
                        onChunk,
                        ct),
                    false);
            }
        }

        private void btnTone_Click(object sender, RibbonControlEventArgs e)
        {
            var text = GetSelectedText();
            if (text.Length == 0)
            {
                NoSelection("Сначала выделите текст.");
                return;
            }

            using (var dialog = new ToneDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var tone = dialog.Tone;
                RunAIReplaceSelectedLive(
                    "Меняю тон...",
                    (onChunk, ct) => _ollama.GenerateStreamAsync(
                        _selectedModel,
                        EnsureRussianPrompt("Перепиши текст в тоне: " + tone + ". Верни только итоговый текст.\n\n" + text),
                        _settings.SystemPrompt,
                        onChunk,
                        ct),
                    false);
            }
        }

        private void btnGrammar_Click(object sender, RibbonControlEventArgs e)
        {
            var text = GetSelectedText();
            if (text.Length == 0)
            {
                NoSelection("Сначала выделите текст.");
                return;
            }

            RunAIReplaceSelectedLive(
                "Исправляю грамматику...",
                (onChunk, ct) => _ollama.GenerateStreamAsync(
                    _selectedModel,
                    EnsureRussianPrompt("Исправь орфографические и грамматические ошибки. Верни только исправленный текст.\n\n" + text),
                    _settings.SystemPrompt,
                    onChunk,
                    ct),
                false);
        }

        private void btnSummarize_Click(object sender, RibbonControlEventArgs e)
        {
            var text = GetSelectedText();
            if (text.Length == 0)
            {
                text = GetDocumentText();
            }

            if (text.Length == 0)
            {
                NoSelection("Документ пуст.");
                return;
            }

            var excerpt = Clip(text, 5000);
            RunAIInsertLive(
                "Делаю краткое содержание...",
                (onChunk, ct) => _ollama.GenerateStreamAsync(
                    _selectedModel,
                    EnsureRussianPrompt("Сделай краткое содержание в 5-7 пунктах. Верни только содержание.\n\n" + excerpt),
                    _settings.SystemPrompt,
                    onChunk,
                    ct));
        }

        private void btnExpand_Click(object sender, RibbonControlEventArgs e)
        {
            var text = GetSelectedText();
            if (text.Length == 0)
            {
                NoSelection("Сначала выделите текст.");
                return;
            }

            RunAIReplaceSelectedLive(
                "Расширяю текст...",
                (onChunk, ct) => _ollama.GenerateStreamAsync(
                    _selectedModel,
                    EnsureRussianPrompt("Раскрой текст подробнее, добавь детали и примеры. Верни только расширенный текст.\n\n" + text),
                    _settings.SystemPrompt,
                    onChunk,
                    ct),
                false);
        }

        private void btnShorten_Click(object sender, RibbonControlEventArgs e)
        {
            var text = GetSelectedText();
            if (text.Length == 0)
            {
                NoSelection("Сначала выделите текст.");
                return;
            }

            RunAIReplaceSelectedLive(
                "Сокращаю текст...",
                (onChunk, ct) => _ollama.GenerateStreamAsync(
                    _selectedModel,
                    EnsureRussianPrompt("Сократи текст примерно на 50%, сохранив смысл. Верни только сокращенный текст.\n\n" + text),
                    _settings.SystemPrompt,
                    onChunk,
                    ct),
                false);
        }

        private void btnPrompt_Click(object sender, RibbonControlEventArgs e)
        {
            using (var dialog = new PromptDialog("Генерация текста", "Опишите, что нужно сгенерировать:"))
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var prompt = dialog.Prompt;
                if (IsAgentPrompt(prompt))
                {
                    RunAgentLoop(StripAgentPrefix(prompt));
                    return;
                }

                RunPromptSmartInsertLive(prompt);
            }
        }

        private void btnAgent_Click(object sender, RibbonControlEventArgs e)
        {
            using (var dialog = new PromptDialog("Агент Word", "Опишите задачу для агента:"))
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                RunAgentLoop(dialog.Prompt);
            }
        }

        private void btnContinue_Click(object sender, RibbonControlEventArgs e)
        {
            var app = WordApp;
            if (app?.ActiveDocument == null || app.Selection == null)
            {
                return;
            }

            var start = Math.Max(0, app.Selection.Range.Start - 900);
            var context = (app.ActiveDocument.Range(start, app.Selection.Range.Start).Text ?? "").Trim();
            if (context.Length == 0)
            {
                NoSelection("Перед курсором нет текста.");
                return;
            }

            RunAIInsertLive(
                "Продолжаю текст...",
                (onChunk, ct) => _ollama.GenerateStreamAsync(
                    _selectedModel,
                    EnsureRussianPrompt("Естественно продолжи текст (1-3 абзаца). Верни только продолжение.\n\n" + context),
                    _settings.SystemPrompt,
                    onChunk,
                    ct));
        }

        private void btnTable_Click(object sender, RibbonControlEventArgs e)
        {
            using (var dialog = new PromptDialog("Создание таблицы", "Опишите, какую таблицу сгенерировать:"))
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var prompt = dialog.Prompt;
                RunAI(
                    "Генерирую таблицу...",
                    () => _ollama.GenerateAsync(
                        _selectedModel,
                        EnsureRussianPrompt(
                            "Сгенерируй таблицу по описанию: " + prompt + ". " +
                            "Верни только строки таблицы без пояснений. Первая строка — заголовки. Разделитель колонок ';'."),
                        _settings.SystemPrompt),
                    result =>
                    {
                        var app = WordApp;
                        if (app?.ActiveDocument == null || app.Selection == null)
                        {
                            return;
                        }

                        var target = app.Selection.Range.Duplicate;
                        var finalRange = ReplaceRangeWithRichContent(target, NormalizeTableOnly(result), false, false);
                        if (finalRange != null)
                        {
                            _lastAiDoc = app.ActiveDocument;
                            _lastAiStart = finalRange.Start;
                            _lastAiEnd = finalRange.End;
                            finalRange.SetRange(finalRange.End, finalRange.End);
                            finalRange.Select();
                        }
                    });
            }
        }

        private void btnList_Click(object sender, RibbonControlEventArgs e)
        {
            using (var dialog = new PromptDialog("Создание списка", "Что нужно перечислить?"))
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var prompt = dialog.Prompt;
                RunAI(
                    "Генерирую список...",
                    () => _ollama.GenerateAsync(_selectedModel, EnsureRussianPrompt("Сделай краткий маркированный список по теме: " + prompt + ". Верни пункты списком."), _settings.SystemPrompt),
                    result =>
                    {
                        var app = WordApp;
                        if (app?.ActiveDocument == null || app.Selection == null)
                        {
                            return;
                        }

                        var items = ParseListItems(result);
                        if (items.Count == 0)
                        {
                            return;
                        }

                        var start = app.Selection.Range.Start;
                        app.Selection.TypeText(string.Join("\r", items));
                        var end = app.Selection.Range.Start;
                        var listRange = app.ActiveDocument.Range(start, end);
                        listRange.ListFormat.ApplyBulletDefault();
                    });
            }
        }

        private void btnHeadings_Click(object sender, RibbonControlEventArgs e)
        {
            using (var dialog = new PromptDialog("Структура документа", "Тема документа:"))
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var prompt = dialog.Prompt;
                RunAI(
                    "Генерирую заголовки...",
                    () => _ollama.GenerateAsync(_selectedModel, EnsureRussianPrompt("Сформируй структуру заголовков документа. Верни по одному заголовку в строке.\n\nТема: " + prompt), _settings.SystemPrompt),
                    result =>
                    {
                        var app = WordApp;
                        if (app?.ActiveDocument == null || app.Selection == null)
                        {
                            return;
                        }

                        foreach (var line in result.Split('\n'))
                        {
                            var heading = line.Trim().TrimStart('#').Trim();
                            if (heading.Length == 0)
                            {
                                continue;
                            }

                            app.Selection.TypeText(heading);
                            app.Selection.set_Style(app.ActiveDocument.Styles[Word.WdBuiltinStyle.wdStyleHeading1]);
                            app.Selection.TypeParagraph();
                        }
                    });
            }
        }

        private void btnGostSpec_Click(object sender, RibbonControlEventArgs e)
        {
            var app = WordApp;
            if (app?.ActiveDocument == null || app.Selection == null)
            {
                return;
            }

            using (var dialog = new PromptDialog("ТЗ по ГОСТ", "Укажите название системы/проекта:"))
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var topic = (dialog.Prompt ?? "").Trim();
                if (topic.Length == 0)
                {
                    topic = "Новый проект";
                }

                var doc = app.ActiveDocument;
                ApplyGostLayout(doc);

                if (string.IsNullOrWhiteSpace(_selectedModel))
                {
                    InsertGostDraft(BuildGostFallback(topic));
                    MessageBox.Show(
                        "Режим ГОСТ применен.\nМодель не выбрана, поэтому вставлен базовый каркас ТЗ.",
                        "ТЗ по ГОСТ",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                var prompt = BuildGostPrompt(topic);
                RunAIGostInsertLive(
                    "Формирую ТЗ по ГОСТ...",
                    (onChunk, ct) => _ollama.GenerateStreamAsync(
                        _selectedModel,
                        EnsureRussianPrompt(prompt),
                        _settings.SystemPrompt,
                        onChunk,
                        ct));
            }
        }

        private void ApplyGostLayout(Word.Document doc)
        {
            if (doc == null || WordApp == null)
            {
                return;
            }

            var app = WordApp;
            doc.PageSetup.PaperSize = Word.WdPaperSize.wdPaperA4;
            doc.PageSetup.Orientation = Word.WdOrientation.wdOrientPortrait;
            doc.PageSetup.LeftMargin = app.CentimetersToPoints(3f);
            doc.PageSetup.RightMargin = app.CentimetersToPoints(1f);
            doc.PageSetup.TopMargin = app.CentimetersToPoints(2f);
            doc.PageSetup.BottomMargin = app.CentimetersToPoints(2f);
            doc.PageSetup.MirrorMargins = 0;
            doc.PageSetup.DifferentFirstPageHeaderFooter = -1;

            try
            {
                doc.AutoHyphenation = true;
                // 0 may throw COMException in some Word builds/locales.
                var zone = (int)Math.Round(app.CentimetersToPoints(0.2f));
                if (zone < 1)
                {
                    zone = 1;
                }

                doc.HyphenationZone = zone;
            }
            catch
            {
            }

            var normal = (Word.Style)doc.Styles[Word.WdBuiltinStyle.wdStyleNormal];
            normal.Font.Name = "Times New Roman";
            normal.Font.Size = 14;
            normal.Font.Color = Word.WdColor.wdColorBlack;
            normal.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
           normal.ParagraphFormat.FirstLineIndent = app.CentimetersToPoints(1.25f);
            normal.ParagraphFormat.SpaceBefore = 0;
            normal.ParagraphFormat.SpaceAfter = 0;
            normal.ParagraphFormat.LineSpacingRule = Word.WdLineSpacing.wdLineSpace1pt5;

            ConfigureGostHeadingStyle(doc, Word.WdBuiltinStyle.wdStyleHeading1);
            ConfigureGostHeadingStyle(doc, Word.WdBuiltinStyle.wdStyleHeading2);
            ConfigureGostHeadingStyle(doc, Word.WdBuiltinStyle.wdStyleHeading3);

            foreach (Word.Table table in doc.Tables)
            {
                table.Range.Font.Name = "Times New Roman";
                table.Range.Font.Size = 11;
                table.Range.ParagraphFormat.LineSpacingRule = Word.WdLineSpacing.wdLineSpaceSingle;
                table.Range.ParagraphFormat.SpaceBefore = 0;
                table.Range.ParagraphFormat.SpaceAfter = 0;
                table.Range.ParagraphFormat.FirstLineIndent = 0;
            }

            foreach (Word.Section section in doc.Sections)
            {
                section.PageSetup.DifferentFirstPageHeaderFooter = -1;
                var primary = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                primary.Range.Text = "";
                primary.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                primary.PageNumbers.RestartNumberingAtSection = false;
                primary.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter, false);

                var first = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterFirstPage];
                first.Range.Text = "";
            }
        }

        private void ConfigureGostHeadingStyle(Word.Document doc, Word.WdBuiltinStyle styleId)
        {
            if (doc == null)
            {
                return;
            }

            var style = (Word.Style)doc.Styles[styleId];
            style.Font.Name = "Times New Roman";
            style.Font.Size = 14;
            style.Font.Bold = 1;
            style.Font.Color = Word.WdColor.wdColorBlack;
            style.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            style.ParagraphFormat.FirstLineIndent = 0;
            style.ParagraphFormat.SpaceBefore = 12;
            style.ParagraphFormat.SpaceAfter = 6;
            style.ParagraphFormat.LineSpacingRule = Word.WdLineSpacing.wdLineSpaceSingle;
        }

        private static string BuildGostPrompt(string topic)
        {
            return
                "Составь техническое задание по ГОСТ 19 для темы: " + topic + ".\n\n" +
                "ЖЕСТКИЕ ТРЕБОВАНИЯ к структуре и оформлению текста:\n" +
                "1) Нумерация разделов: 1, 1.1, 1.1.1 (до 3-4 уровней), без точки в конце номера.\n" +
                "2) Заголовки: с заглавной буквы, без точки в конце, без переносов.\n" +
                "3) Формат абзацев: без markdown, без код-блоков, без пояснений; только готовый текст ТЗ.\n" +
                "4) Списки: допускаются маркированные (–) и нумерованные (1), 2), 3)).\n" +
                "5) Таблицы и рисунки: используй подписи вида \"Таблица 1 — ...\" и \"Рисунок 1 — ...\".\n" +
                "6) Текст только на русском, деловой и проверяемый стиль формулировок.\n" +
                "7) Соблюдай разделы ГОСТ 19: Общие сведения, Назначение разработки, Основание для разработки, " +
                "Требования к программе (функциональные/нефункциональные), Требования к интерфейсу, " +
                "Требования к надежности, Критерии приемки, Сроки и этапы.\n\n" +
                "Верни только финальный текст ТЗ.";
        }

        private static string BuildGostFallback(string topic)
        {
            return
                "ТЕХНИЧЕСКОЕ ЗАДАНИЕ\n" +
                "на разработку \"" + topic + "\"\n\n" +
                "1 Общие сведения\n" +
                "1.1 Назначение разработки\n" +
                "Укажите цель создаваемой системы и ожидаемый эффект от внедрения.\n\n" +
                "1.2 Основание для разработки\n" +
                "Укажите документ-основание, заказчика, дату и номер решения о разработке.\n\n" +
                "2 Требования к программе\n" +
                "2.1 Функциональные требования\n" +
                "Перечислите ключевые функции системы, сценарии пользователя и ограничения.\n\n" +
                "2.2 Нефункциональные требования\n" +
                "Укажите требования по производительности, безопасности, надежности, совместимости.\n\n" +
                "2.3 Требования к интерфейсу\n" +
                "Опишите состав экранных форм, элементы навигации и требования к доступности.\n\n" +
                "3 Состав и содержание работ\n" +
                "3.1 Этапы разработки\n" +
                "Опишите этапы проекта, результаты каждого этапа и сроки выполнения.\n\n" +
                "3.2 Порядок контроля и приемки\n" +
                "Опишите методику тестирования, критерии приемки и комплект сдаваемых материалов.\n\n" +
                "4 Требования к документации\n" +
                "4.1 Перечень эксплуатационных документов\n" +
                "Укажите обязательные документы пользователя и администратора.\n\n" +
                "5 Приложения\n" +
                "Таблица 1 — Матрица требований\n" +
                "Рисунок 1 — Архитектурная схема системы";
        }

        private static string NormalizeGostDraftText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            var normalized = NormalizeForWordSmart(text);
            var lines = normalized.Replace('\r', '\n').Split('\n');
            var result = new List<string>();
            foreach (var sourceLine in lines)
            {
                var line = CollapseSpaces((sourceLine ?? "").Trim());
                if (line.Length == 0)
                {
                    result.Add("");
                    continue;
                }

                line = StripInlineMarkdownFormatting(line);
                line = CollapseSpaces(line);
                if (Regex.IsMatch(line, @"^([\-*•–—]|\d+[\.\)])\s*$"))
                {
                    continue;
                }

                result.Add(line);
            }

            return JoinNormalizedLines(result);
        }

        private static string StripInlineMarkdownFormatting(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            var value = text ?? "";
            value = Regex.Replace(value, @"\[(.+?)\]\((https?://[^\s\)]+)\)", "$1 ($2)");
            value = Regex.Replace(value, @"\*\*\*(.+?)\*\*\*", "$1");
            value = Regex.Replace(value, @"\*\*(.+?)\*\*", "$1");
            value = Regex.Replace(value, @"__(.+?)__", "$1");
            value = Regex.Replace(value, @"\*(.+?)\*", "$1");
            value = Regex.Replace(value, @"_(.+?)_", "$1");
            value = value.Replace("`", "");
            return value;
        }

        private void InsertGostDraft(string text)
        {
            var app = WordApp;
            if (app?.ActiveDocument == null || app.Selection == null)
            {
                return;
            }

            var doc = app.ActiveDocument;
            var start = app.Selection.Range.Start;
            app.Selection.TypeText(NormalizeGostDraftText(text));
            var end = app.Selection.Range.Start;
            ApplyGostHeadingStylesToRange(doc.Range(start, end));
        }

        private void ApplyGostHeadingStylesToRange(Word.Range range)
        {
            if (range == null || range.Document == null)
            {
                return;
            }

            foreach (Word.Paragraph paragraph in range.Paragraphs)
            {
                var line = (paragraph.Range.Text ?? "").Replace("\r", "").Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                var m = Regex.Match(line, @"^(\d+(?:\.\d+){0,3})\s+");
                if (!m.Success)
                {
                    continue;
                }

                var level = m.Groups[1].Value.Count(ch => ch == '.') + 1;
                if (level <= 1)
                {
                    paragraph.Range.set_Style(range.Document.Styles[Word.WdBuiltinStyle.wdStyleHeading1]);
                }
                else if (level == 2)
                {
                    paragraph.Range.set_Style(range.Document.Styles[Word.WdBuiltinStyle.wdStyleHeading2]);
                }
                else
                {
                    paragraph.Range.set_Style(range.Document.Styles[Word.WdBuiltinStyle.wdStyleHeading3]);
                }
            }
        }

        private void btnExecutiveSummary_Click(object sender, RibbonControlEventArgs e)
        {
            var text = GetDocumentText();
            if (text.Length == 0)
            {
                NoSelection("Документ пуст.");
                return;
            }

            var excerpt = Clip(text, 7000);
            RunAI(
                "Формирую резюме для руководства...",
                () => _ollama.GenerateAsync(_selectedModel, EnsureRussianPrompt("Сформируй резюме для руководства в начале документа: 6-8 пунктов и 3 следующих шага.\n\n" + excerpt), _settings.SystemPrompt),
                result => InsertAtDocumentTop("Резюме для руководства", result));
        }

        private void btnAnalyze_Click(object sender, RibbonControlEventArgs e)
        {
            var text = GetDocumentText();
            if (text.Length == 0)
            {
                NoSelection("Документ пуст.");
                return;
            }

            var excerpt = Clip(text, 5000);
            RunAI(
                "Анализирую документ...",
                () => _ollama.GenerateAsync(_selectedModel, EnsureRussianPrompt("Проанализируй тему, аудиторию, сильные и слабые стороны, дай рекомендации:\n\n" + excerpt), _settings.SystemPrompt),
                result => ShowResult("Анализ документа", null, result, null));
        }

        private void btnKeyPoints_Click(object sender, RibbonControlEventArgs e)
        {
            var text = GetSelectedText();
            if (text.Length == 0)
            {
                text = GetDocumentText();
            }

            if (text.Length == 0)
            {
                NoSelection("Документ пуст.");
                return;
            }

            var excerpt = Clip(text, 5000);
            RunAIInsertLive(
                "Выделяю ключевые идеи...",
                (onChunk, ct) => _ollama.GenerateStreamAsync(
                    _selectedModel,
                    EnsureRussianPrompt("Выдели 8 ключевых идей. Каждый пункт с новой строки.\n\nИсточник:\n" + excerpt),
                    _settings.SystemPrompt,
                    onChunk,
                    ct));
        }

        private void btnErrors_Click(object sender, RibbonControlEventArgs e)
        {
            var text = GetSelectedText();
            if (text.Length == 0)
            {
                text = GetDocumentText();
            }

            if (text.Length == 0)
            {
                NoSelection("Документ пуст.");
                return;
            }

            var excerpt = Clip(text, 5000);
            RunAI(
                "Ищу ошибки и проблемы...",
                () => _ollama.GenerateAsync(_selectedModel, EnsureRussianPrompt("Найди логические, фактические и стилистические проблемы. Формат: цитата -> проблема -> исправление.\n\n" + excerpt), _settings.SystemPrompt),
                result => ShowResult("Найденные проблемы", null, result, null));
        }

        private void btnStats_Click(object sender, RibbonControlEventArgs e)
        {
            var app = WordApp;
            if (app?.ActiveDocument == null)
            {
                return;
            }

            var doc = app.ActiveDocument;
            var text = GetDocumentText();
            var words = doc.Words.Count;
            var charsCount = doc.Characters.Count;
            var paragraphs = doc.Paragraphs.Count;
            var readTime = Math.Max(1, (int)Math.Round(words / 200.0));
            var stats = "Слов: " + words.ToString("N0") + "\n" +
                        "Символов: " + charsCount.ToString("N0") + "\n" +
                        "Абзацев: " + paragraphs.ToString("N0") + "\n" +
                        "Время чтения: ~" + readTime + " мин";

            var sample = Clip(text, 1000);
            RunAI(
                "Оцениваю читаемость...",
                () => _ollama.GenerateAsync(_selectedModel, EnsureRussianPrompt("Оцени читаемость одним предложением и дай 2 улучшения:\n\n" + sample), _settings.SystemPrompt),
                result => MessageBox.Show(stats + "\n\nЧитаемость:\n" + result, "Статистика документа", MessageBoxButtons.OK, MessageBoxIcon.Information));
        }

        private void btnExplainComment_Click(object sender, RibbonControlEventArgs e)
        {
            var text = GetSelectedText();
            if (text.Length == 0)
            {
                NoSelection("Выделите текст для пояснения в комментарии.");
                return;
            }

            RunAI(
                "Генерирую комментарий...",
                () => _ollama.GenerateAsync(_selectedModel, EnsureRussianPrompt("Объясни этот фрагмент простым языком в 2-4 предложениях:\n\n" + text), _settings.SystemPrompt),
                InsertAsComment);
        }

        private void LoadKnowledgeBase()
        {
            _templates = KnowledgeBaseStore.Load();
            EnsureGostTemplateInKnowledgeBase();
            _selectedTemplate = _templates.FirstOrDefault(x => x.Id == _settings.LastTemplateId);
            if (_selectedTemplate == null)
            {
                _selectedTemplate = _templates.FirstOrDefault();
            }

            UpdateTemplateControls();
        }

        private void EnsureGostTemplateInKnowledgeBase()
        {
            if (_templates == null)
            {
                _templates = new List<KnowledgeTemplate>();
            }

            var gostTemplates = _templates.Where(t =>
                (!string.IsNullOrWhiteSpace(t.Name) && t.Name.IndexOf("гост 19", StringComparison.OrdinalIgnoreCase) >= 0) ||
                (!string.IsNullOrWhiteSpace(t.Name) && t.Name.IndexOf("тз по гост", StringComparison.OrdinalIgnoreCase) >= 0) ||
                (!string.IsNullOrWhiteSpace(t.Category) && t.Category.IndexOf("гост", StringComparison.OrdinalIgnoreCase) >= 0))
                .ToList();

            if (gostTemplates.Count == 0)
            {
                var template = KnowledgeBaseStore.Create(
                    "ГОСТ 19 (строгое ТЗ)",
                    "ГОСТ 19",
                    "Сформируй ТЗ строго по ГОСТ 19. Структура 1 / 1.1 / 1.1.1 без точки в конце номера. " +
                    "Заголовки с заглавной буквы, без точки. Формулировки конкретные и проверяемые.",
                    "Разделы: 1 Общие сведения; 1.1 Назначение разработки; 1.2 Основание для разработки; " +
                    "2 Требования к программе; 2.1 Функциональные требования; 2.2 Нефункциональные требования; " +
                    "3 Состав и содержание работ; 4 Требования к документации; 5 Приложения. " +
                    "Используй подписи: Таблица 1 — ... и Рисунок 1 — ...",
                    "1 Общие сведения\n1.1 Назначение разработки\n1.2 Основание для разработки");
                template.IsPinned = true;
                _templates.Add(template);
            }
            else
            {
                foreach (var t in gostTemplates)
                {
                    t.Category = "ГОСТ 19";
                    t.Instruction =
                        "Сформируй ТЗ строго по ГОСТ 19. Структура 1 / 1.1 / 1.1.1 без точки в конце номера. " +
                        "Заголовки с заглавной буквы, без точки. Формулировки конкретные и проверяемые.";
                    t.OutputFormat =
                        "Разделы: 1 Общие сведения; 1.1 Назначение разработки; 1.2 Основание для разработки; " +
                        "2 Требования к программе; 2.1 Функциональные требования; 2.2 Нефункциональные требования; " +
                        "3 Состав и содержание работ; 4 Требования к документации; 5 Приложения. " +
                        "Используй подписи: Таблица 1 — ... и Рисунок 1 — ...";
                    t.IsPinned = true;
                    t.UpdatedUtc = DateTime.UtcNow;
                }
            }

            KnowledgeBaseStore.Save(_templates);
            _templates = KnowledgeBaseStore.Load();
        }

        private void UpdateTemplateControls()
        {
            RunOnUi(() =>
            {
                _updatingTemplateUi = true;
                _templateLookup.Clear();
                cboTemplate.Items.Clear();

                foreach (var t in _templates)
                {
                    var label = t.ToString();
                    var item = Factory.CreateRibbonDropDownItem();
                    item.Label = label;
                    cboTemplate.Items.Add(item);
                    _templateLookup[label] = t.Id;
                    _templateLookup[t.Name ?? ""] = t.Id;
                }

                if (_selectedTemplate != null)
                {
                    cboTemplate.Text = _selectedTemplate.ToString();
                }
                else
                {
                    cboTemplate.Text = _templates.Count == 0 ? "Нет шаблонов" : _templates[0].ToString();
                }

                lblTemplateInfo.Label = "Шаблоны: " + _templates.Count;
                _updatingTemplateUi = false;
            });
        }

        private void cboTemplate_TextChanged(object sender, RibbonControlEventArgs e)
        {
            if (_updatingTemplateUi)
            {
                return;
            }

            var text = (cboTemplate.Text ?? "").Trim();
            if (text.Length == 0)
            {
                _selectedTemplate = null;
                _settings.LastTemplateId = "";
                PluginSettings.Save(_settings);
                return;
            }

            string id = null;
            if (_templateLookup.ContainsKey(text))
            {
                id = _templateLookup[text];
            }
            else
            {
                var candidate = _templates.FirstOrDefault(t =>
                    string.Equals(t.Name, text, StringComparison.OrdinalIgnoreCase) ||
                    t.ToString().StartsWith(text, StringComparison.OrdinalIgnoreCase));
                if (candidate != null)
                {
                    id = candidate.Id;
                }
            }

            _selectedTemplate = !string.IsNullOrWhiteSpace(id) ? _templates.FirstOrDefault(x => x.Id == id) : null;
            _settings.LastTemplateId = _selectedTemplate != null ? _selectedTemplate.Id : "";
            PluginSettings.Save(_settings);
        }

        private void btnKbManage_Click(object sender, RibbonControlEventArgs e)
        {
            using (var manager = new KnowledgeBaseManagerForm(_templates))
            {
                if (manager.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                _templates = manager.Templates;
                KnowledgeBaseStore.Save(_templates);
                if (_selectedTemplate == null || !_templates.Any(x => x.Id == _selectedTemplate.Id))
                {
                    _selectedTemplate = _templates.FirstOrDefault();
                }

                _settings.LastTemplateId = _selectedTemplate != null ? _selectedTemplate.Id : "";
                PluginSettings.Save(_settings);
                UpdateTemplateControls();
            }
        }

        private void btnKbFromSelection_Click(object sender, RibbonControlEventArgs e)
        {
            var selected = GetSelectedText();
            using (var editor = new TemplateEditForm(new KnowledgeTemplate
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "Новый шаблон",
                Category = "Пользовательский",
                Instruction = "Перепиши входной текст строго по этому шаблону.",
                OutputFormat = "Опишите здесь строгую структуру вывода.",
                Example = Clip(selected, 1200),
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            }))
            {
                if (editor.ShowDialog() != DialogResult.OK || editor.Template == null)
                {
                    return;
                }

                KnowledgeBaseStore.Upsert(editor.Template);
                LoadKnowledgeBase();
                _selectedTemplate = _templates.FirstOrDefault(x => x.Id == editor.Template.Id) ?? _selectedTemplate;
                _settings.LastTemplateId = _selectedTemplate != null ? _selectedTemplate.Id : "";
                PluginSettings.Save(_settings);
                UpdateTemplateControls();
            }
        }

        private void btnKbApply_Click(object sender, RibbonControlEventArgs e)
        {
            if (!EnsureTemplate())
            {
                return;
            }

            var selected = GetSelectedText();
            if (selected.Length == 0)
            {
                NoSelection("Выделите текст для применения шаблона.");
                return;
            }

            var prompt = BuildTemplatePrompt(_selectedTemplate, "Преобразуй выделенный текст по выбранному шаблону.", selected);
            RunAIReplaceSelectedLive(
                "Применяю шаблон...",
                (onChunk, ct) => _ollama.GenerateStreamAsync(
                    _selectedModel,
                    EnsureRussianPrompt(prompt),
                    _settings.SystemPrompt,
                    onChunk,
                    ct),
                false);
        }

        private void btnKbGenerate_Click(object sender, RibbonControlEventArgs e)
        {
            if (!EnsureTemplate())
            {
                return;
            }

            using (var dialog = new PromptDialog("Генерация по шаблону", "Опишите, что нужно сгенерировать:"))
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var prompt = BuildTemplatePrompt(_selectedTemplate, dialog.Prompt, "");
                RunAIInsertLive(
                    "Генерирую по шаблону...",
                    (onChunk, ct) => _ollama.GenerateStreamAsync(
                        _selectedModel,
                        EnsureRussianPrompt(prompt),
                        _settings.SystemPrompt,
                        onChunk,
                        ct));
            }
        }

        private static string BuildTemplatePrompt(KnowledgeTemplate template, string task, string input)
        {
            return
                "Строго следуй шаблону. Не добавляй пояснений.\n\n" +
                template.ToPromptBlock() + "\n\n" +
                "Задача:\n" + task + "\n\n" +
                "Входные данные:\n" + input + "\n\n" +
                "Сохраняй структуру абзацев и нумерацию. Не склеивай разделы в одну строку.\n" +
                "Верни только итоговый форматированный результат.";
        }

        private void ApplyRibbonFallbackIcons()
        {
            var info = IconToBmp(SystemIcons.Information, 32);
            var warn = IconToBmp(SystemIcons.Warning, 32);
            var ok = IconToBmp(SystemIcons.Shield, 32);
            var err = IconToBmp(SystemIcons.Error, 32);
            var app = IconToBmp(SystemIcons.Application, 32);
            var q = IconToBmp(SystemIcons.Question, 32);

            btnRefresh.Image = info;
            btnDiscoverLan.Image = q;
            btnAutoRefresh.Image = app;
            btnSettings.Image = app;
            btnImprove.Image = ok;
            btnRewriteTracked.Image = warn;
            btnTranslate.Image = app;
            btnTone.Image = info;
            btnGrammar.Image = ok;
            btnSummarize.Image = info;
            btnExpand.Image = IconToBmp(SystemIcons.Asterisk, 32);
            btnShorten.Image = IconToBmp(SystemIcons.Hand, 32);
            btnPrompt.Image = app;
            btnAgent.Image = ok;
            btnContinue.Image = info;
            btnTable.Image = app;
            btnList.Image = info;
            btnHeadings.Image = ok;
            btnGostSpec.Image = warn;
            btnExecutiveSummary.Image = info;
            btnAnalyze.Image = q;
            btnKeyPoints.Image = info;
            btnErrors.Image = err;
            btnStats.Image = app;
            btnExplainComment.Image = q;
            btnKbManage.Image = app;
            btnKbFromSelection.Image = IconToBmp(SystemIcons.Information, 32);
            btnKbApply.Image = ok;
            btnKbGenerate.Image = app;
            btnChat.Image = info;
            btnAskDoc.Image = q;
        }

        private static Bitmap IconToBmp(Icon icon, int size)
        {
            if (icon == null)
            {
                return null;
            }

            return new Bitmap(icon.ToBitmap(), new Size(size, size));
        }

        private static string EnsureRussianPrompt(string prompt)
        {
            var text = prompt ?? "";
            var isGost = Regex.IsMatch(text, @"\bгост\b|техническое задание|тз", RegexOptions.IgnoreCase);
            if (isGost)
            {
                return text +
                    "\n\nВАЖНО: Пиши ответ только на русском языке. Не используй английский, если явно не требуется перевод." +
                    "\nСохраняй структуру абзацев и разделов, не склеивай документ в одну строку." +
                    "\nЭто документ по ГОСТ: не используй markdown-разметку и не добавляй пояснений вне текста документа.";
            }

            var result = text +
                "\n\nВАЖНО: Пиши ответ только на русском языке. Не используй английский, если явно не требуется перевод." +
                "\nСохраняй структуру абзацев и разделов, не склеивай документ в одну строку." +
                "\nНе используй Markdown-разметку (**,#,``` и т.п.). Используй обычный текст и структуру Word." +
                "\nНе используй команды @word: если их явно не просят.";
            if (IsChartRequest(text))
            {
                result += "\nЕсли нужен график, верни команду вида: @word:chart type=column title=\"...\" labels=\"A|B|C\" values=\"10|20|30\"";
            }

            return result;
        }

        private void btnChat_Click(object sender, RibbonControlEventArgs e)
        {
            ToggleChat(btnChat.Checked);
        }

        private void ToggleChat(bool open)
        {
            _chatOpen = open;
            if (open)
            {
                if (_chatForm == null || _chatForm.IsDisposed)
                {
                    _chatForm = new ChatForm(_ollama, () => _selectedModel, GetDocumentText);
                    _chatForm.FormClosed += ChatForm_FormClosed;
                }

                _chatForm.Show();
            }
            else
            {
                _chatForm?.Close();
            }
        }

        private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _chatOpen = false;
            btnChat.Checked = false;
        }

        private void btnAskDoc_Click(object sender, RibbonControlEventArgs e)
        {
            var text = GetDocumentText();
            if (text.Length == 0)
            {
                NoSelection("Документ пуст.");
                return;
            }

            using (var dialog = new PromptDialog("Вопрос по документу", "Ваш вопрос:"))
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var excerpt = Clip(text, 6000);
                var prompt = "Документ:\n" + excerpt + "\n\nВопрос: " + dialog.Prompt + "\n\nОтветь точно и по делу.";
                RunAI(
                    "Задаю вопрос ИИ...",
                    () => _ollama.GenerateAsync(_selectedModel, EnsureRussianPrompt(prompt), _settings.SystemPrompt),
                    result => ShowResult("Ответ", null, result, null));
            }
        }

        private static string ShortUrl(string url)
        {
            var text = string.IsNullOrWhiteSpace(url) ? "" : url.Trim();
            if (text.Length <= 40)
            {
                return text;
            }

            return text.Substring(0, 40) + "...";
        }

        private void RunOnUi(Action action)
        {
            if (action == null)
            {
                return;
            }

            var invoker = _uiInvoker;
            if (invoker == null || invoker.IsDisposed)
            {
                action();
                return;
            }

            if (invoker.InvokeRequired)
            {
                invoker.Invoke((MethodInvoker)(() => action()));
            }
            else
            {
                action();
            }
        }
    }
}


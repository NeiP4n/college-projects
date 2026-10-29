using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsTimer = System.Windows.Forms.Timer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WordAIAgent.Models;
using WordAIAgent.Services;
using Word = Microsoft.Office.Interop.Word;

namespace WordAIAgent.Forms
{
    public partial class AIAgentForm : Form
    {
        private readonly List<ChatMessage> _history = new List<ChatMessage>();
        private readonly WinFormsTimer _modelsTimer = new WinFormsTimer();
        private readonly RoslynExecutor _roslyn = new RoslynExecutor();
        private ActionValidator _validator;
        private CursorGuard _cursorGuard;
        private WordRecoveryService _recovery;
        private TemplateProfileBuilder _templateBuilder;
        private TemplateProfile _templateProfile;
        private bool _templateModeEnabled;
        private bool _ignoreTemplateFlagChange;
        private CancellationTokenSource _cts;
        private bool _isProcessing;
        private bool _ignoreModelChange;
        private static readonly string PromptHeader = BuildPromptHeader();
        private static string _planPromptHeader;
        private static string PlanPromptHeader => _planPromptHeader ?? (_planPromptHeader = BuildPlanPromptHeader());
        private const bool AllowVba = false;
        private const int HistoryLimit = 8;
        private const int LlmRetries = 1;
        private const int DefaultMaxInsertLength = 15000;

        public AIAgentForm()
        {
            InitializeComponent();
            txtHost.Text = ThisAddIn.Settings.OllamaHost;
            _validator = new ActionValidator(ActionList);
            _cursorGuard = new CursorGuard(ThisAddIn.WordService);
            _recovery = new WordRecoveryService(ThisAddIn.WordService);
            _templateBuilder = new TemplateProfileBuilder(ThisAddIn.WordService);
            JsonLogger.Init(AppDomain.CurrentDomain.BaseDirectory);

            AppendSystem("Агент готов к работе. Введите команду на русском языке.");
            if (!RoslynExecutor.IsAvailable())
                AppendSystem("⚠ Roslyn недоступен — режим C#-кода отключён.");

            _modelsTimer.Interval = 20000;
            _modelsTimer.Tick += async (_, __) => await RefreshModelsAsync(false);
            _modelsTimer.Start();

            _ = RefreshModelsAsync(true);
            _ = CheckConnectionAsync();

            // Templates are disabled
            if (panelTemplates != null) panelTemplates.Visible = false;
            if (splitMain != null) splitMain.Panel2Collapsed = true;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _modelsTimer.Stop();
            _modelsTimer.Dispose();
            base.OnFormClosed(e);
        }

        // Sets splitter to 75% chat / 25% templates after form is visible and sized
        private void AIAgentForm_Shown(object sender, System.EventArgs e)
        {
            int w = splitMain.Width;
            if (w > 100)
                splitMain.SplitterDistance = Math.Max(splitMain.Panel1MinSize, (int)(w * 0.74));
        }

        private void UI(Action a)
        {
            if (IsDisposed || Disposing) return;
            if (InvokeRequired) { try { BeginInvoke(a); } catch { } }
            else a();
        }

        private async void BtnConnect_Click(object sender, EventArgs e)
        {
            var host = (txtHost.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(host)) { AppendSystem("Host пустой."); return; }
            ThisAddIn.Settings.OllamaHost = host;
            ThisAddIn.Settings.Normalize();
            ThisAddIn.SaveSettings();
            await CheckConnectionAsync();
            await RefreshModelsAsync(true);
        }

        private async Task RefreshModelsAsync(bool setCurrent)
        {
            try
            {
                var models = await ThisAddIn.OllamaService.GetModelsAsync().ConfigureAwait(false);
                if (models == null || models.Count == 0) return;
                var current = ThisAddIn.Settings.ModelName;
                string prev = null;
                UI(() => prev = cmbModels.SelectedItem?.ToString());

                UI(() =>
                {
                    _ignoreModelChange = true;
                    cmbModels.Items.Clear();
                    foreach (var m in models) cmbModels.Items.Add(m);
                    string select = setCurrent && !string.IsNullOrWhiteSpace(current) ? current : prev;
                    if (!string.IsNullOrWhiteSpace(select) && cmbModels.Items.Contains(select))
                        cmbModels.SelectedItem = select;
                    else if (cmbModels.Items.Count > 0)
                        cmbModels.SelectedIndex = 0;
                    _ignoreModelChange = false;
                });
            }
            catch { }
        }

        private void CmbModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreModelChange) return;
            var sel = cmbModels.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(sel)) return;
            ThisAddIn.Settings.ModelName = sel;
            ThisAddIn.SaveSettings();
        }

        private async Task CheckConnectionAsync()
        {
            var ok = await ThisAddIn.OllamaService.TestConnectionAsync().ConfigureAwait(false);
            UI(() =>
            {
                lblStatus.Text = ok ? "● Подключено" : "● Нет связи";
                lblStatus.ForeColor = ok ? Color.LimeGreen : Color.OrangeRed;
            });
        }

        private async void BtnSend_Click(object s, EventArgs e) => await SendAsync();

        private async void TxtInput_KeyDown(object s, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            { e.SuppressKeyPress = true; await SendAsync(); }
        }

        private async Task SendAsync()
        {
            var text = txtInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;
            if (IsCancelCommand(text))
            {
                _cts?.Cancel();
                AppendSystem("Отмена запрошена.");
                txtInput.Clear();
                return;
            }
            if (_isProcessing) return;
            if (TryHandleTemplateModeCommand(text))
            {
                txtInput.Clear();
                AppendUser(text);
                return;
            }
            if (TryHandleFileInsertCommand(text))
            {
                txtInput.Clear();
                AppendUser(text);
                return;
            }
            txtInput.Clear();
            AppendUser(text);
            await RunAgentAsync(text);
        }

        private async Task RunAgentAsync(string userText)
        {
            _isProcessing = true;
            SetBusy(true);
            var executed = new List<string>();
            var errors = new List<string>();
            var totalSw = Stopwatch.StartNew();
            _cts = new CancellationTokenSource();

            try
            {
                JsonLogger.Log("USER_INPUT", new { text = userText });

                var ctx = ThisAddIn.DocumentAnalyzer.BuildContext();
                _validator.MaxInsertLength = _templateModeEnabled ? 0 : DefaultMaxInsertLength;

                bool did = ExecuteByIntent(userText, executed, errors);
                string reply = "";

                if (!did)
                {
                    int loops = 0;
                    while (!did && loops++ < 3)
                    {
                        if (_cts.IsCancellationRequested) { AppendSystem("Отменено."); return; }
                        var llmSw = Stopwatch.StartNew();
                        var (token, raw) = await RequestPlanAsync(userText, ctx).ConfigureAwait(false);
                        llmSw.Stop();
                        JsonLogger.Log("LLM_RESPONSE", new { ms = llmSw.ElapsedMilliseconds, raw });
                        if (_cts.IsCancellationRequested) { AppendSystem("Отменено."); return; }

                        if (!IsDocumentCommand(userText))
                        {
                            var replyText = ExtractReply(token, raw);
                            if (!string.IsNullOrWhiteSpace(replyText))
                                reply = replyText;
                            break;
                        }

                        var plan = SelectPlan(token);
                        if (plan.Count == 0)
                        {
                            var replyText = ExtractReply(token, raw);
                            if (!string.IsNullOrWhiteSpace(replyText))
                                reply = replyText;
                            else
                                errors.Add("План не распознан.");
                            break;
                        }

                        JsonLogger.Log("PLAN", plan.Select(p => new { action = p.action, paramsJson = p.ps }));
                        var validation = _validator.Validate(plan, ctx);
                        if (!validation.IsValid)
                        {
                            foreach (var e in validation.Errors) errors.Add(e);
                            break;
                        }

                        if (validation.RequiresCursorGuard) _cursorGuard.EnsureSafeForInsert();
                        var execSw = Stopwatch.StartNew();
                        did = ExecutePlan(plan, userText, executed, errors);
                        execSw.Stop();
                        JsonLogger.Log("EXECUTION_TIME", new { ms = execSw.ElapsedMilliseconds });
                        foreach (var w in validation.Warnings) AppendSystem("⚠ " + w);

                        if (!did) break;

                        var reflect = await RequestReflectionAsync(userText, ctx, executed, errors).ConfigureAwait(false);
                        if (reflect.Decision == "continue" && reflect.Plan.Count > 0)
                        {
                            var v2 = _validator.Validate(reflect.Plan, ctx);
                            if (!v2.IsValid)
                            {
                                foreach (var e in v2.Errors) errors.Add(e);
                                break;
                            }
                            if (v2.RequiresCursorGuard) _cursorGuard.EnsureSafeForInsert();
                            did = ExecutePlan(reflect.Plan, userText, executed, errors);
                            if (!did) break;
                        }
                        else if (reflect.Decision == "fix" && reflect.Plan.Count > 0)
                        {
                            var v3 = _validator.Validate(reflect.Plan, ctx);
                            if (!v3.IsValid)
                            {
                                foreach (var e in v3.Errors) errors.Add(e);
                                break;
                            }
                            if (v3.RequiresCursorGuard) _cursorGuard.EnsureSafeForInsert();
                            did = ExecutePlan(reflect.Plan, userText, executed, errors);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(reply))
                    reply = !did
                        ? "Команда не распознана."
                        : "Выполнено: " + string.Join(", ", executed);

                AppendAssistant(reply);
                foreach (var err in errors.Distinct()) AppendSystem("⚠ " + err);

                _history.Add(new ChatMessage("user", userText));
                _history.Add(new ChatMessage("assistant", reply));
                while (_history.Count > HistoryLimit) _history.RemoveAt(0);
            }
            catch (Exception ex)
            {
                AppendSystem("Ошибка: " + ex.Message);
                if (_recovery.TryRecover(ex, out var msg) && !string.IsNullOrWhiteSpace(msg))
                    AppendSystem("⚠ " + msg);
                UI(() => { lblStatus.Text = "● Нет связи"; lblStatus.ForeColor = Color.OrangeRed; });
            }
            finally
            {
                totalSw.Stop();
                JsonLogger.Log("TOTAL_TIME", new { ms = totalSw.ElapsedMilliseconds });
                _cts?.Dispose();
                _cts = null;
                _isProcessing = false;
                SetBusy(false);
            }
        }

        private string BuildPrompt(string userText)
        {
            var sb = new StringBuilder();
            sb.AppendLine(PromptHeader);

            var ctx = ThisAddIn.DocumentAnalyzer.BuildExecutionContext(userText, 1500);
            if (!string.IsNullOrWhiteSpace(ctx))
            {
                sb.AppendLine("СОСТОЯНИЕ ДОКУМЕНТА:");
                sb.AppendLine(ctx);
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine($"ЗАПРОС: {userText}");

            return sb.ToString().Trim();
        }

        private string BuildPlanPrompt(string userText, DocumentContext ctx, string extra)
        {
            var sb = new StringBuilder();
            sb.AppendLine(PlanPromptHeader);

            if (!string.IsNullOrWhiteSpace(extra))
            {
                sb.AppendLine("ИНСТРУКЦИЯ:");
                sb.AppendLine(extra.Trim());
                sb.AppendLine();
            }

            if (_history.Count > 0)
            {
                sb.AppendLine("ИСТОРИЯ:");
                var recent = _history.Count > 6 ? _history.Skip(_history.Count - 6).ToList() : _history;
                foreach (var m in recent)
                    sb.AppendLine($"{(m.Role == "user" ? "Пользователь" : "Агент")}: {m.Content}");
                sb.AppendLine();
            }

            var ctxJson = ThisAddIn.DocumentAnalyzer.BuildExecutionContextJson(1500);
            if (!string.IsNullOrWhiteSpace(ctxJson))
            {
                sb.AppendLine("DOCUMENT_STATE_JSON:");
                sb.AppendLine(ctxJson);
                sb.AppendLine();
            }

            if (ctx != null)
            {
                if (ctx.Outline != null && ctx.Outline.Count > 0)
                {
                    sb.AppendLine("DOCUMENT_STRUCTURE:");
                    foreach (var h in ctx.Outline) sb.AppendLine(h);
                    sb.AppendLine();
                }
                if (ctx.Styles != null && ctx.Styles.Count > 0)
                {
                    sb.AppendLine("STYLES:");
                    sb.AppendLine(string.Join(", ", ctx.Styles));
                    sb.AppendLine();
                }
                if (ctx.TableInfo != null && ctx.TableInfo.Count > 0)
                {
                    sb.AppendLine("TABLES:");
                    foreach (var t in ctx.TableInfo) sb.AppendLine($"{t.Rows}x{t.Cols}");
                    sb.AppendLine();
                }
            }
            if (_templateModeEnabled && _templateProfile != null)
            {
                sb.AppendLine("TEMPLATE_PROFILE_JSON:");
                sb.AppendLine(JsonConvert.SerializeObject(_templateProfile, Formatting.None));
                sb.AppendLine();
                sb.AppendLine("ИНСТРУКЦИЯ_ШАБЛОН:");
                sb.AppendLine("Сохраняй структуру, стили, порядок разделов и общий объем шаблона. Полностью переформулируй содержание под тему пользователя.");
                sb.AppendLine("Если в шаблоне есть оглавление/разделы/таблицы — воспроизведи их в том же количестве и порядке.");
                sb.AppendLine("Ограничения по количеству страниц нет.");
                sb.AppendLine();
            }

            sb.AppendLine($"ЗАПРОС: {userText}");
            return sb.ToString().Trim();
        }

        private async Task<(JToken token, string raw)> RequestPlanAsync(string userText, DocumentContext ctx)
        {
            string raw = "";
            for (int attempt = 0; attempt <= LlmRetries; attempt++)
            {
                var extra = attempt == 0
                    ? null
                    : "Return valid JSON only. Schema: {\"analysis\":\"...\",\"goal\":\"...\",\"plan\":[{\"action\":\"Name\",\"params\":{}}]}";
                var prompt = BuildPlanPrompt(userText, ctx, extra);
                raw = await ThisAddIn.OllamaService.ChatAsync(_history, prompt).ConfigureAwait(false);
                var token = ParseJson(raw);
                if (token != null && (HasPlan(token) || HasCandidates(token))) return (token, raw);
            }
            return (null, raw);
        }

        private async Task<ReflectionResult> RequestReflectionAsync(string userText, DocumentContext ctx,
            List<string> executed, List<string> errors)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SYSTEM: Ты — инспектор результата действий в Word. Отвечай только JSON.");
            sb.AppendLine("ФОРМАТ: {\"decision\":\"continue|stop|fix\",\"plan\":[{\"action\":\"Name\",\"params\":{}}]}");
            sb.AppendLine();
            sb.AppendLine("DOCUMENT_STATE_JSON:");
            sb.AppendLine(ThisAddIn.DocumentAnalyzer.BuildExecutionContextJson(1500));
            sb.AppendLine();
            sb.AppendLine("EXECUTED:");
            sb.AppendLine(string.Join(", ", executed ?? new List<string>()));
            if (errors != null && errors.Count > 0)
            {
                sb.AppendLine("ERRORS:");
                foreach (var e in errors) sb.AppendLine(e);
            }
            sb.AppendLine();
            sb.AppendLine($"USER_REQUEST: {userText}");

            var raw = await ThisAddIn.OllamaService.ChatAsync(_history, sb.ToString()).ConfigureAwait(false);
            var token = ParseJson(raw);
            return ParseReflection(token);
        }

        private static string BuildPromptHeader()
        {
            var sb = new StringBuilder();
            sb.AppendLine("SYSTEM: Ты — точный исполнитель команд Microsoft Word. Отвечай только валидным JSON, без markdown и пояснений.");
            sb.AppendLine("Каждый ответ = одна строка JSON. Два варианта:");
            sb.AppendLine("  {\"reply\":\"…\",\"mode\":\"actions\",\"actions\":[{\"action\":\"Имя\",\"params\":{}}]}");
            sb.AppendLine("  {\"reply\":\"…\",\"mode\":\"none\"}  ← только если нет изменений документа");
            sb.AppendLine();
            sb.AppendLine("ПРАВИЛА (нарушение = ошибка):");
            sb.AppendLine("1. Любое изменение документа → mode=actions. Никогда не mode=none при изменениях.");
            sb.AppendLine("2. InsertText/AppendParagraph: текст пиши полностью внутри JSON прямо сейчас.");
            sb.AppendLine("3. ClearDocument — для удаления всего содержимого. FindReplace — для конкретных слов.");
            sb.AppendLine("4. Не вставляй в документ служебные метки: DOCUMENT_STATE, USER_REQUEST, CONTEXT, JSON и т.п.");
            sb.AppendLine("5. Если команда многошаговая — включи все нужные actions в один массив.");
            sb.AppendLine();
            sb.AppendLine("ДЕЙСТВИЯ (сокращённый набор):");
            sb.AppendLine("TEXT: InsertText(text), DeleteText(text), FindReplace(find,replace)");
            sb.AppendLine("TABLE: InsertTable(rows,cols)");
            sb.AppendLine("FORMAT: Bold(value), Italic(value), SetFontSize(size), AlignCenter");
            sb.AppendLine("PAGE: InsertPageBreak, InsertHeader(text), InsertFooter(text), SaveDocument, ExportPDF(path)");
            sb.AppendLine("OBJECTS: InsertPieChart, InsertImage(path)");
            sb.AppendLine("UNDO: Undo, Redo");
            return sb.ToString().Trim();
        }

        private static string BuildPlanPromptHeader()
        {
            var sb = new StringBuilder();
            sb.AppendLine("SYSTEM: Ты — планировщик действий для Microsoft Word. Отвечай только валидным JSON.");
            sb.AppendLine("ФОРМАТ: {\"analysis\":\"...\",\"goal\":\"...\",\"plan\":[{\"action\":\"Name\",\"params\":{}}]}");
            sb.AppendLine("АЛЬТЕРНАТИВА: {\"analysis\":\"...\",\"goal\":\"...\",\"candidates\":[{\"action\":\"Name\",\"score\":0.0,\"params\":{}}]}");
            sb.AppendLine("Правила:");
            sb.AppendLine("1. Только JSON. Без markdown и пояснений.");
            sb.AppendLine("2. Используй только действия из списка ниже.");
            sb.AppendLine("3. Если нужна вставка текста — текст должен быть внутри params.text.");
            sb.AppendLine("4. Если пользователь просит написать текст/стих/статью — используй InsertText.");
            sb.AppendLine();
            sb.AppendLine("ДЕЙСТВИЯ:");
            sb.AppendLine(string.Join(", ", ActionList));
            sb.AppendLine();
            sb.AppendLine("TOOLS (описание/пример):");
            sb.AppendLine("InsertText — вставить текст. Пример: {\"action\":\"InsertText\",\"params\":{\"text\":\"...\"}}");
            sb.AppendLine("InsertTable — создать таблицу. Пример: {\"action\":\"InsertTable\",\"params\":{\"rows\":3,\"cols\":4}}");
            sb.AppendLine("FindReplace — заменить текст. Пример: {\"action\":\"FindReplace\",\"params\":{\"find\":\"X\",\"replace\":\"\"}}");
            sb.AppendLine("InsertPieChart — вставить круговую диаграмму. Пример: {\"action\":\"InsertPieChart\"}");
            return sb.ToString().Trim();
        }

        private static JToken ParseJson(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var match = Regex.Match(text, @"\{.*\}", RegexOptions.Singleline);
            if (match.Success)
            {
                try { return JToken.Parse(match.Value); }
                catch { }
            }
            return null;
        }

        private static string ExtractReply(JToken t, string raw)
        {
            if (t is JObject o)
            {
                var r = o["reply"]?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(r)) return r;
            }
            var plain = (raw ?? "").Trim();
            return (!plain.StartsWith("{") && !plain.StartsWith("[") && !plain.StartsWith("```"))
                ? plain : "";
        }

        private static string GetMode(JToken t) =>
            (t as JObject)?["mode"]?.ToString()?.Trim().ToLowerInvariant() ?? "";

        private static bool HasPlan(JToken t) =>
            (t as JObject)?["plan"] is JArray arr && arr.Count > 0;

        private static bool HasCandidates(JToken t) =>
            (t as JObject)?["candidates"] is JArray arr && arr.Count > 0;

        private static List<(string action, JObject ps)> SelectPlan(JToken token)
        {
            var plan = ExtractPlanList(token);
            if (plan.Count > 0) return plan;
            var candidates = ExtractCandidateList(token);
            if (candidates.Count == 0) return plan;
            var best = candidates.OrderByDescending(c => c.score).First();
            if (string.IsNullOrWhiteSpace(best.action)) return plan;
            return new List<(string action, JObject ps)> { (NormalizeAction(best.action), best.ps ?? new JObject()) };
        }

        private bool ExecuteActions(JToken token, string userText, List<string> exec, List<string> errs)
        {
            var cmds = ExtractActionList(token);
            if (cmds.Count == 0) return false;

            bool ok = false;
            foreach (var (action, ps) in cmds)
            {
                if (IsClearAction(action) && !HasClearIntent(userText))
                { errs.Add("Очистка документа заблокирована — нет явной команды."); continue; }
                if (IsContextLeak(ps?["text"]?.ToString() ?? ps?["content"]?.ToString()))
                { errs.Add("Вставка служебного контекста заблокирована."); continue; }

                try
                {
                    if (DispatchAction(action, ps))
                    { ok = true; exec.Add(Humanize(action)); }
                    else
                        errs.Add($"Неизвестное действие: {action}");
                }
                catch (Exception ex) { errs.Add($"{action}: {ex.Message}"); }
            }
            return ok;
        }

        private bool ExecutePlan(List<(string action, JObject ps)> plan, string userText, List<string> exec, List<string> errs)
        {
            if (plan == null || plan.Count == 0) return false;
            bool ok = false;

            foreach (var (action, ps) in plan)
            {
                if (IsClearAction(action) && !HasClearIntent(userText))
                { errs.Add("Очистка документа заблокирована — нет явной команды."); continue; }
                if (IsContextLeak(ps?["text"]?.ToString() ?? ps?["content"]?.ToString()))
                { errs.Add("Вставка служебного контекста заблокирована."); continue; }

                try
                {
                    if (DispatchAction(action, ps))
                    {
                        ok = true;
                        exec.Add(Humanize(action));
                        JsonLogger.Log("EXECUTED_ACTION", new { action, ps });
                    }
                    else
                    {
                        errs.Add($"Неизвестное действие: {action}");
                    }
                }
                catch (Exception ex)
                {
                    errs.Add($"{action}: {ex.Message}");
                    JsonLogger.Log("WORD_EXCEPTION", new { action, error = ex.Message });
                }
            }
            return ok;
        }

        private static List<(string, JObject)> ExtractActionList(JToken token)
        {
            var result = new List<(string, JObject)>();
            if (token == null) return result;

            if (token is JObject obj && obj["actions"] is JArray arr)
            {
                foreach (var item in arr.OfType<JObject>())
                {
                    var name = GetActionName(item);
                    if (!string.IsNullOrWhiteSpace(name)) result.Add((name, GetParams(item)));
                }
                return result;
            }
            if (token is JObject single)
            {
                var name = GetActionName(single);
                if (!string.IsNullOrWhiteSpace(name)) result.Add((name, GetParams(single)));
            }
            return result;
        }

        private static List<(string action, JObject ps)> ExtractPlanList(JToken token)
        {
            var result = new List<(string, JObject)>();
            if (!(token is JObject obj)) return result;
            if (!(obj["plan"] is JArray arr)) return result;

            foreach (var item in arr.OfType<JObject>())
            {
                var name = GetActionName(item);
                if (string.IsNullOrWhiteSpace(name)) continue;
                result.Add((NormalizeAction(name), GetParams(item)));
            }
            return result;
        }

        private static List<(string action, double score, JObject ps)> ExtractCandidateList(JToken token)
        {
            var result = new List<(string, double, JObject)>();
            if (!(token is JObject obj)) return result;
            if (!(obj["candidates"] is JArray arr)) return result;

            foreach (var item in arr.OfType<JObject>())
            {
                var name = GetActionName(item);
                if (string.IsNullOrWhiteSpace(name)) continue;
                double score = 0;
                var sToken = item["score"];
                if (sToken != null && double.TryParse(sToken.ToString(), out var sc)) score = sc;
                result.Add((NormalizeAction(name), score, GetParams(item)));
            }
            return result;
        }

        private static string GetActionName(JObject o) =>
            o["action"]?.ToString() ?? o["name"]?.ToString() ?? o["command"]?.ToString();

        private static JObject GetParams(JObject o)
        {
            if (o["params"] is JObject jp) return jp;
            if (o["arguments"] is JObject ja) return ja;
            var flat = new JObject();
            var skip = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "action","name","command","reply","mode","code","actions","params","arguments" };
            foreach (var p in o.Properties())
                if (!skip.Contains(p.Name)) flat[p.Name] = p.Value.DeepClone();
            return flat;
        }

        private static ReflectionResult ParseReflection(JToken token)
        {
            var result = new ReflectionResult();
            if (!(token is JObject obj)) return result;
            result.Decision = (obj["decision"]?.ToString() ?? "").Trim().ToLowerInvariant();
            result.Plan = ExtractPlanList(obj);
            return result;
        }

        private class ReflectionResult
        {
            public string Decision { get; set; } = "stop";
            public List<(string action, JObject ps)> Plan { get; set; } = new List<(string, JObject)>();
        }

        private bool TryHandleTemplateModeCommand(string text)
        {
            var t = (text ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(t)) return false;

            if (Regex.IsMatch(t, @"\b(выключи|отключи|отмени|без)\b.*\bшаблон\b"))
            {
                DisableTemplateMode();
                return true;
            }

            if (Regex.IsMatch(t, @"\b(режим шаблона|шаблон|по шаблону|template)\b"))
            {
                EnableTemplateMode();
                return true;
            }

            return false;
        }

        private void ChkTemplateMode_CheckedChanged(object sender, EventArgs e)
        {
            if (_ignoreTemplateFlagChange) return;
            if (chkTemplateMode.Checked)
                EnableTemplateMode();
            else
                DisableTemplateMode();
            SyncTemplateFlag();
        }

        private void SyncTemplateFlag()
        {
            if (chkTemplateMode == null) return;
            _ignoreTemplateFlagChange = true;
            chkTemplateMode.Checked = _templateModeEnabled;
            _ignoreTemplateFlagChange = false;
        }

        private bool TryHandleFileInsertCommand(string text)
        {
            var t = NormalizeText(text);
            if (string.IsNullOrWhiteSpace(t)) return false;

            if (!Regex.IsMatch(t, @"\b(вставь|добавь|загрузи|импортируй)\b.*\b(файл|документ|txt|docx)\b"))
                return false;

            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Документы Word (*.docx)|*.docx|Текст (*.txt)|*.txt|Все файлы|*.*";
                dlg.Title = "Выберите файл";
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    AppendSystem("Загрузка файла отменена.");
                    return true;
                }

                try
                {
                    var ws = ThisAddIn.WordService;
                    ws.EnsureDocumentReady();
                    _cursorGuard?.EnsureSafeForInsert();

                    var path = dlg.FileName;
                    var ext = Path.GetExtension(path)?.ToLowerInvariant();
                    if (ext == ".txt")
                    {
                        var content = File.ReadAllText(path);
                        ws.InsertTextChunked(content);
                        AppendSystem("Текст из файла вставлен: " + Path.GetFileName(path));
                    }
                    else if (ext == ".docx")
                    {
                        ws.InsertFileContent(path);
                        AppendSystem("Файл вставлен: " + Path.GetFileName(path));
                    }
                    else
                    {
                        AppendSystem("Неподдерживаемый тип файла.");
                    }
                }
                catch (Exception ex)
                {
                    AppendSystem("Ошибка вставки файла: " + ex.Message);
                }
            }
            return true;
        }

        private void EnableTemplateMode()
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Word document (*.docx)|*.docx";
                dlg.Title = "Выберите шаблон .docx";
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    AppendSystem("Загрузка шаблона отменена.");
                    _templateProfile = null;
                    _templateModeEnabled = false;
                    _validator.MaxInsertLength = DefaultMaxInsertLength;
                    SyncTemplateFlag();
                    return;
                }

                try
                {
                    _templateProfile = _templateBuilder.Build(dlg.FileName);
                    _templateModeEnabled = true;
                    _validator.MaxInsertLength = 0;
                    AppendSystem("Режим шаблона включен: " + Path.GetFileName(dlg.FileName));
                    AppendSystem($"Страниц: {_templateProfile.Pages}, разделов: {_templateProfile.Outline.Count}, таблиц: {_templateProfile.Tables.Count}, стилей: {_templateProfile.Styles.Count}");
                    AppendSystem("Теперь опишите тему и задачу — я подстрою документ под шаблон.");
                }
                catch (Exception ex)
                {
                    _templateProfile = null;
                    _templateModeEnabled = false;
                    _validator.MaxInsertLength = DefaultMaxInsertLength;
                    AppendSystem("Ошибка загрузки шаблона: " + ex.Message);
                }
            }
            SyncTemplateFlag();
        }

        private void DisableTemplateMode()
        {
            _templateModeEnabled = false;
            _templateProfile = null;
            _validator.MaxInsertLength = DefaultMaxInsertLength;
            AppendSystem("Режим шаблона выключен.");
            SyncTemplateFlag();
        }

        private bool DispatchAction(string rawAction, JObject ps)
        {
            var action = NormalizeAction(rawAction);
            if (RequiresCursorGuard(action))
                _cursorGuard?.EnsureSafeForInsert();
            var ws = ThisAddIn.WordService;
            ws.EnsureDocumentReady();

            switch (action)
            {
                // Document
                case "NewDocument": ws.NewDocument(); return true;
                case "SaveDocument": ws.SaveDocument(); return true;
                case "SaveAsDocument": ws.SaveAsDocument(S(ps, "path") ?? DefaultDocPath()); return true;
                case "CloseDocument": ws.CloseDocument(B(ps, "save", true)); return true;
                case "ExportPDF": ws.ExportAsPDF(S(ps, "path") ?? DefaultPdfPath()); return true;
                case "PrintDocument": ws.PrintDocument(); return true;
                case "OpenDocument": ws.OpenDocument(S(ps, "path") ?? ""); return true;
                case "InsertFileContent": ws.InsertFileContent(S(ps, "path") ?? ""); return true;
                case "ApplyTemplate":
                    return false;

                // Security
                case "ProtectDocument": ws.ProtectDocument(S(ps, "password") ?? ""); return true;
                case "UnprotectDocument": ws.UnprotectDocument(S(ps, "password") ?? ""); return true;
                case "AddPassword": ws.AddPassword(S(ps, "password") ?? ""); return true;
                case "RemovePassword": ws.RemovePassword(); return true;
                case "RestrictEditing": ws.RestrictEditing(S(ps, "password") ?? ""); return true;

                // Text
                case "InsertText":
                    {
                        var text = S(ps, "text") ?? "";
                        if (text.Length > 5000)
                            ws.InsertTextChunked(text);
                        else
                            ws.InsertText(text);
                        return true;
                    }
                case "InsertTextAtEnd": ws.InsertTextAtEnd(S(ps, "text") ?? ""); return true;
                case "AppendParagraph": ws.AppendParagraph(S(ps, "text") ?? ""); return true;
                case "ReplaceSelection": ws.ReplaceSelection(S(ps, "text") ?? ""); return true;
                case "FindReplace": ws.FindAndReplace(S(ps, "find") ?? "", S(ps, "replace") ?? ""); return true;
                case "DeleteSelection":
                    {
                        if (ws.HasSelection())
                            ws.DeleteSelectedText();
                        else
                            ws.Undo();
                        return true;
                    }
                case "DeleteText":
                    {
                        var t = S(ps, "text");
                        if (!string.IsNullOrEmpty(t)) ws.FindAndReplace(t, "");
                        else ws.DeleteSelectedText();
                        return true;
                    }
                case "ClearDocument": ws.DeleteAllText(); return true;
                case "SelectAll": ws.SelectAll(); return true;
                case "CopySelection": ws.CopySelection(); return true;
                case "CutSelection": ws.CutSelection(); return true;
                case "PasteContent": ws.PasteContent(); return true;
                case "TypeParagraph":
                    {
                        if (ps?["text"] != null)
                            ws.AppendParagraph(S(ps, "text") ?? "");
                        else
                            ws.TypeParagraph();
                        return true;
                    }

                // Navigation
                case "Undo": ws.Undo(); return true;
                case "Redo": ws.Redo(); return true;
                case "GoToStart": ws.GoToStart(); return true;
                case "GoToEnd": ws.GoToEnd(); return true;
                case "GoToPage": ws.GoToPage(I(ps, "page", 1)); return true;

                // Font
                case "Bold": ws.SetBold(B(ps, "value", true)); return true;
                case "Italic": ws.SetItalic(B(ps, "value", true)); return true;
                case "Underline": ws.SetUnderline(B(ps, "value", true)); return true;
                case "StrikeThrough": ws.SetStrikeThrough(B(ps, "value", true)); return true;
                case "Subscript": ws.SetSubscript(B(ps, "value", true)); return true;
                case "Superscript": ws.SetSuperscript(B(ps, "value", true)); return true;
                case "AllCaps": ws.SetAllCaps(B(ps, "value", true)); return true;
                case "SetFontName": ws.SetFontName(S(ps, "name") ?? S(ps, "font") ?? "Times New Roman"); return true;
                case "SetFontSize": ws.SetFontSize(F(ps, "size", 14f)); return true;
                case "SetFontColor": ws.SetFontColor(ParseColor(ps["value"] ?? ps["color"])); return true;
                case "Highlight": ws.SetHighlight(ParseHighlight(ps["value"] ?? ps["color"])); return true;
                case "ClearFormatting": ws.ClearFormatting(); return true;

                // Paragraph
                case "AlignLeft": ws.AlignLeft(); return true;
                case "AlignCenter": ws.AlignCenter(); return true;
                case "AlignRight": ws.AlignRight(); return true;
                case "AlignJustify": ws.AlignJustify(); return true;
                case "SetJustifyAll": ws.SelectAll(); ws.AlignJustify(); return true;
                case "SetLineSpacing": ws.SetLineSpacing(F(ps, "value", 18f)); return true;
                case "SetLineSpacingMultiple": ws.SetLineSpacingMultiple(F(ps, "value", 1.5f)); return true;
                case "SetSpacingBefore": ws.SetSpacingBefore(F(ps, "value", 0f)); return true;
                case "SetSpacingAfter": ws.SetSpacingAfter(F(ps, "value", 0f)); return true;
                case "SetParagraphSpacing": ws.SetSpacingBefore(F(ps, "before", 0f)); ws.SetSpacingAfter(F(ps, "after", 0f)); return true;
                case "SetFirstLineIndentCm": ws.SetFirstLineIndentCm(F(ps, "value", 1.25f)); return true;
                case "IndentParagraph": ws.IndentParagraph(); return true;
                case "OutdentParagraph": ws.OutdentParagraph(); return true;
                case "FormatDocumentGOST": ws.ApplyGostFormatting(); return true;
                case "ApplyGost19Template": ws.ApplyGost19Template(); return true;
                case "RemoveDoubleSpaces": ws.RemoveDoubleSpacesInDocument(); return true;
                case "RemoveManualLineBreaks": ws.RemoveManualLineBreaks(); return true;

                // Styles
                case "ApplyHeading":
                case "SetHeadingLevel": ws.ApplyHeading(I(ps, "level", 1)); return true;
                case "ApplyStyle": ws.ApplyStyle(S(ps, "name") ?? S(ps, "style") ?? "Normal"); return true;
                case "ApplyNormalStyle": ws.ApplyNormalStyle(); return true;

                // TOC
                case "CreateTableOfContents": ws.InsertTableOfContents(); return true;
                case "UpdateTableOfContents": ws.UpdateTableOfContents(); return true;

                // Lists
                case "CreateBulletList": ws.CreateBulletList(); return true;
                case "CreateNumberedList": ws.CreateNumberedList(); return true;
                case "RemoveList": ws.RemoveList(); return true;
                case "IncreaseListLevel": ws.IncreaseListLevel(); return true;
                case "DecreaseListLevel": ws.DecreaseListLevel(); return true;

                // Tables
                case "InsertTable":
                    {
                        int rows = Math.Max(1, I(ps, "rows", 2));
                        int cols = Math.Max(1, I(ps, "cols", I(ps, "columns", 2)));
                        var pos = (S(ps, "position") ?? "").ToLowerInvariant();
                        if (pos == "start" || pos == "begin") ws.GoToStart();
                        ws.InsertTable(rows, cols);
                        return true;
                    }
                case "InsertPieChart": ws.InsertPieChart(); return true;
                case "InsertChart": ws.InsertChart(I(ps, "type", 5)); return true;
                case "AddTableRow": ws.AddTableRow(); return true;
                case "AddTableColumn": ws.AddTableColumn(); return true;
                case "DeleteTable": ws.DeleteCurrentOrLastTable(); return true;
                case "ApplyTableStyle": ws.ApplyTableStyle(S(ps, "style") ?? S(ps, "name") ?? "Table Grid"); return true;
                case "FillTableCell": ws.FillTableCell(I(ps, "table", 1), I(ps, "row", 1), I(ps, "col", 1), S(ps, "text") ?? ""); return true;

                // Images
                case "InsertImage": ws.InsertImage(S(ps, "path") ?? ""); return true;
                case "InsertImageFromClipboard": ws.InsertImageFromClipboard(); return true;
                case "InsertSmartArt": ws.InsertSmartArt(); return true;
                case "InsertEquation": ws.InsertEquation(); return true;
                case "InsertCaption": ws.InsertCaption(); return true;
                case "InsertCitation": ws.InsertCitation(); return true;
                case "InsertBibliography": ws.InsertBibliography(); return true;
                case "UpdateFields": ws.UpdateFields(); return true;

                // Links
                case "InsertHyperlink":
                    ws.InsertHyperlink(S(ps, "text") ?? S(ps, "displayText") ?? "", S(ps, "url") ?? "");
                    return true;
                case "InsertBookmark": ws.InsertBookmark(S(ps, "name") ?? "Bookmark1"); return true;
                case "GoToBookmark": ws.GoToBookmark(S(ps, "name") ?? ""); return true;

                // Footnotes / comments
                case "InsertFootnote": ws.InsertFootnote(S(ps, "text") ?? ""); return true;
                case "InsertEndnote": ws.InsertEndnote(S(ps, "text") ?? ""); return true;
                case "AddComment": ws.AddComment(S(ps, "text") ?? ""); return true;
                case "DeleteAllComments": ws.DeleteAllComments(); return true;
                case "AcceptAllComments": ws.AcceptAllComments(); return true;

                // Track changes
                case "EnableTrackChanges": ws.EnableTrackChanges(); return true;
                case "DisableTrackChanges": ws.DisableTrackChanges(); return true;
                case "AcceptAllChanges": ws.AcceptAllChanges(); return true;
                case "RejectAllChanges": ws.RejectAllChanges(); return true;

                // Page
                case "InsertPageBreak": ws.InsertPageBreak(); return true;
                case "InsertSectionBreak": ws.InsertSectionBreak(); return true;
                case "SetPageMarginsCm":
                    ws.SetPageMarginsCm(F(ps, "top", 2f), F(ps, "bottom", 2f), F(ps, "left", 2f), F(ps, "right", 2f));
                    return true;
                case "SetPageOrientation":
                    {
                        var v = (S(ps, "value") ?? "").ToLowerInvariant();
                        ws.SetPageOrientation(v == "landscape"
                            ? Word.WdOrientation.wdOrientLandscape
                            : Word.WdOrientation.wdOrientPortrait);
                        return true;
                    }
                case "InsertHeader": ws.InsertHeader(S(ps, "text") ?? ""); return true;
                case "InsertFooter": ws.InsertFooter(S(ps, "text") ?? ""); return true;
                case "RemoveHeader": ws.RemoveHeader(); return true;
                case "RemoveFooter": ws.RemoveFooter(); return true;
                case "InsertPageNumbers": ws.InsertPageNumbers(); return true;

                // Misc
                case "InsertTextBox": ws.InsertTextBox(); return true;
                case "InsertSymbol": ws.InsertSymbol(I(ps, "charCode", 183), S(ps, "font") ?? "Wingdings"); return true;
                case "InsertDateTime": ws.InsertDateTime(); return true;
                case "CheckSpelling": ws.CheckSpellingAndGrammar(); return true;
                case "SetViewDraft": ws.SetViewDraft(); return true;
                case "SetViewPrint": ws.SetViewPrint(); return true;
                case "SetViewWeb": ws.SetViewWeb(); return true;
                case "SetViewOutline": ws.SetViewOutline(); return true;
                case "SetZoom": ws.SetZoom(I(ps, "percent", 100)); return true;

                // Columns / layout
                case "SetColumns": ws.SetColumns(I(ps, "count", 1)); return true;
                case "SetColumnsWithSpacing": ws.SetColumnsWithSpacing(I(ps, "count", 2), F(ps, "spacingCm", 1.25f)); return true;
                case "InsertColumnBreak": ws.InsertColumnBreak(); return true;

                // Watermark
                case "InsertWatermark": ws.InsertTextWatermark(S(ps, "text") ?? "ЧЕРНОВИК"); return true;
                case "RemoveWatermark": ws.RemoveWatermark(); return true;

                // Borders
                case "SetParagraphBorder": ws.SetParagraphBorder(B(ps, "on", true)); return true;
                case "SetPageBorder": ws.SetPageBorder(); return true;
                case "RemovePageBorder": ws.RemovePageBorder(); return true;

                // Find & highlight
                case "FindAndHighlight": ws.FindAndHighlight(S(ps, "text") ?? ""); return true;

                // Character spacing
                case "SetCharacterSpacing": ws.SetCharacterSpacingPt(F(ps, "pts", 0)); return true;
                case "SetCharacterScaling": ws.SetCharacterScaling(I(ps, "percent", 100)); return true;

                // Tab stops
                case "ClearTabStops": ws.ClearTabStops(); return true;
                case "AddTabStop": ws.AddTabStop(F(ps, "positionCm", 1.25f)); return true;

                // Drop cap
                case "SetDropCap": ws.SetDropCap(I(ps, "lines", 3)); return true;
                case "RemoveDropCap": ws.RemoveDropCap(); return true;

                // Document stats
                case "GetStats":
                    {
                        var stats = ws.GetFullStats();
                        AppendSystem(stats);
                        return true;
                    }
                case "GetComments":
                    {
                        var comments = ws.GetAllComments();
                        AppendSystem(comments);
                        return true;
                    }
                case "GetTrackedChanges":
                    {
                        var changes = ws.GetTrackedChangeSummary();
                        AppendSystem(changes);
                        return true;
                    }

                // Selection expansion
                case "SelectWord": ws.SelectWord(); return true;
                case "SelectSentence": ws.SelectSentence(); return true;
                case "SelectParagraph": ws.SelectParagraph(); return true;
                case "MoveUp": ws.MoveUp(I(ps, "lines", 1)); return true;
                case "MoveDown": ws.MoveDown(I(ps, "lines", 1)); return true;
                case "MoveToLineStart": ws.MoveToLineStart(); return true;
                case "MoveToLineEnd": ws.MoveToLineEnd(); return true;

                // Table extended
                case "AutoFitTable": ws.AutoFitTable(I(ps, "tableIndex", 1)); return true;
                case "SetTableBorders": ws.SetTableBorders(I(ps, "tableIndex", 1)); return true;
                case "MergeTableCells":
                    ws.MergeTableCells(I(ps, "tableIndex", 1), I(ps, "row1", 1), I(ps, "col1", 1), I(ps, "row2", 1), I(ps, "col2", 2));
                    return true;
                case "SortTableByColumn":
                    ws.SortTableByColumn(I(ps, "tableIndex", 1), I(ps, "col", 1), B(ps, "ascending", true));
                    return true;

                // Footnotes/endnotes
                case "DeleteAllFootnotes": ws.DeleteAllFootnotes(); return true;
                case "DeleteAllEndnotes": ws.DeleteAllEndnotes(); return true;

                // Line numbers
                case "EnableLineNumbers": ws.EnableLineNumbers(B(ps, "restartEachPage", false)); return true;
                case "DisableLineNumbers": ws.DisableLineNumbers(); return true;

                // Sections
                case "GetSectionCount":
                    {
                        AppendSystem($"Разделов в документе: {ws.GetSectionCount()}");
                        return true;
                    }

                // Paste special
                case "PasteAsPlainText": ws.PasteAsPlainText(); return true;
                case "PasteAsRtf": ws.PasteAsRtf(); return true;

                // Smart quotes
                case "SetSmartQuotes": ws.SetSmartQuotes(B(ps, "on", true)); return true;

                // Cross-reference
                case "InsertCrossReference": ws.InsertCrossReference(S(ps, "bookmark") ?? ""); return true;

                // Styles management
                case "CreateCharacterStyle":
                    ws.CreateCharacterStyle(S(ps, "name") ?? "Мой стиль", S(ps, "font") ?? "Arial",
                        F(ps, "size", 12), B(ps, "bold", false));
                    return true;

                // Outline numbering
                case "ApplyOutlineNumbering": ws.ApplyOutlineNumbering(); return true;

                // Comments extended
                case "ReplyToLastComment": ws.ReplyToLastComment(S(ps, "text") ?? ""); return true;

                // Compare documents
                case "CompareWithDocument":
                    {
                        var r = ws.CompareWithDocument(S(ps, "path") ?? "");
                        AppendSystem(r);
                        return true;
                    }

                // VBA
                case "ExecuteVba":
                    if (!AllowVba)
                        throw new InvalidOperationException("ExecuteVba отключён по соображениям безопасности.");
                    ws.ExecuteTemporaryVba(S(ps, "code") ?? "");
                    return true;

                // Word internals
                case "ExecuteMso": ws.ExecuteMso(S(ps, "controlId") ?? S(ps, "name") ?? ""); return true;
                case "ExecuteWordBasic": ws.ExecuteWordBasic(S(ps, "command") ?? "", BuildArgs(ps["args"])); return true;
                case "RunMacro": ws.RunMacro(S(ps, "name") ?? "", BuildArgs(ps["args"])); return true;

                default: return false;
            }
        }

        private static bool RequiresCursorGuard(string action)
        {
            switch (NormalizeAction(action))
            {
                case "InsertText":
                case "InsertTextAtEnd":
                case "AppendParagraph":
                case "InsertTable":
                case "InsertPieChart":
                case "InsertChart":
                case "InsertImage":
                case "InsertImageFromClipboard":
                case "InsertTextBox":
                case "InsertSmartArt":
                case "InsertEquation":
                case "InsertCaption":
                case "InsertCitation":
                case "InsertBibliography":
                    return true;
                default:
                    return false;
            }
        }

        private static float F(JObject ps, string key, float def)
        {
            if (ps == null) return def;
            var v = ps[key];
            if (v == null) return def;
            return (float)(double.TryParse(v.ToString(), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double d) ? d : def);
        }

        private async Task<bool> RunCodeAsync(string code, List<string> exec, List<string> errs)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            if (!IsSafeCode(code, out var reason))
            {
                errs.Add("Roslyn: код заблокирован. " + reason);
                return false;
            }
            var result = _roslyn.Execute(code);
            if (result.Success)
            {
                exec.Add("выполнил код");
                if (!string.IsNullOrWhiteSpace(result.Output) &&
                    !result.Output.Equals("Код выполнен успешно.", StringComparison.OrdinalIgnoreCase))
                    AppendSystem(result.Output);
                return true;
            }

            // Auto-repair
            try
            {
                var errText = result.Error ?? string.Join("\n", result.CompileErrors);
                var fixPrompt =
                    "Исправь C# код для Word VSTO Interop.\n" +
                    "Доступно в Execute(): WordApp, ActiveDoc, WordSvc, Log(string), InsertText(string), ExecuteMso(string).\n" +
                    "Без using/class/method — только тело Execute().\n\n" +
                    $"Код:\n```csharp\n{code}\n```\n\nОшибка:\n{errText}\n\n" +
                    "Верни ТОЛЬКО исправленный код в ```csharp``` блоке.";

                var fixRaw = await ThisAddIn.AgentService
                    .ChatWithVisionAsync(new List<ChatMessage>(), fixPrompt).ConfigureAwait(false);
                var fixedCode = RoslynExecutor.ExtractCode(fixRaw);
                if (!string.IsNullOrWhiteSpace(fixedCode) && fixedCode.Trim() != code.Trim())
                {
                    var r2 = _roslyn.Execute(fixedCode);
                    if (r2.Success)
                    {
                        exec.Add("выполнил исправленный код");
                        AppendSystem("✓ Код исправлен автоматически.");
                        return true;
                    }
                    foreach (var ce in r2.CompileErrors) errs.Add(ce);
                }
            }
            catch { }

            if (!string.IsNullOrWhiteSpace(result.Error)) errs.Add(result.Error);
            foreach (var ce in result.CompileErrors) errs.Add(ce);
            return false;
        }

        private static bool IsSafeCode(string code, out string reason)
        {
            reason = null;
            var s = (code ?? "").ToLowerInvariant();
            var blocked = new[]
            {
                "system.io", "file.", "directory.", "process", "system.net", "socket",
                "registry", "environment.", "appdomain", "dllimport", "reflection",
                "thread", "task", "httpclient", "webrequest"
            };
            foreach (var b in blocked)
            {
                if (s.Contains(b))
                {
                    reason = $"недопустимое использование '{b}'.";
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> EscalateToCodeAsync(string userText, List<string> exec, List<string> errs)
        {
            try
            {
                var ctx = ThisAddIn.DocumentAnalyzer.BuildExecutionContext(userText, 6000);
                var raw = await ThisAddIn.AgentService
                    .RequestCodeAsync(userText, ctx).ConfigureAwait(false);
                var code = RoslynExecutor.ExtractCode(raw);
                return await RunCodeAsync(code, exec, errs).ConfigureAwait(false);
            }
            catch (Exception ex) { errs.Add("Эскалация: " + ex.Message); return false; }
        }

        private bool ExecuteByIntent(string text, List<string> exec, List<string> errs)
        {
            var t = NormalizeText(text);
            if (string.IsNullOrWhiteSpace(t)) return false;
            var ws = ThisAddIn.WordService;
            ws.EnsureDocumentReady();

            if (t == "привет" || t == "как дела" || t == "что с тобой")
                return false;

            // Undo
            if (t.Contains("отмени") || t == "назад") { ws.Undo(); exec.Add("отменил"); return true; }

            if (HasClearIntent(text))
            {
                ws.DeleteAllText();
                exec.Add("очистил документ");
                return true;
            }
            return false;
        }

        private static void ParseTableSize(string t, out int rows, out int cols)
        {
            rows = 2; cols = 2;
            var m = Regex.Match(t, @"(\d+)\s*[xх×]\s*(\d+)");
            if (m.Success) { int.TryParse(m.Groups[1].Value, out rows); int.TryParse(m.Groups[2].Value, out cols); return; }
            m = Regex.Match(t, @"(\d+)\s*на\s*(\d+)");
            if (m.Success) { int.TryParse(m.Groups[1].Value, out rows); int.TryParse(m.Groups[2].Value, out cols); }
            rows = Math.Max(1, rows); cols = Math.Max(1, cols);
        }

        private static bool ShouldEscalate(string text, string mode)
        {
            if (mode == "none") return false;
            var t = (text ?? "").ToLowerInvariant();
            if (t.EndsWith("?")) return false;
            if (t.Contains("убери") || t.Contains("удали") || t.Contains("очисти") ||
                t.Contains("стер") || t.Contains("убрать") || t.Contains("удалить")) return false;
            var editWords = new[] {
                "исправ","замен","поправ","сделай","создай","добав","встав",
                "перепиш","оформ","переформат","обнови","пронумер","оглавлен"
            };
            return editWords.Any(t.Contains);
        }

        private void RefreshTemplateList()
        {
            UI(() =>
            {
                lstTemplates.Items.Clear();
                foreach (var e in ThisAddIn.TemplateManager.All)
                    lstTemplates.Items.Add($"[{e.Category}] {e.Name} ({e.Type})");
            });
        }

        private void BtnAddTemplate_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Добавить шаблон / файл",
                Filter = "Документы Word (*.docx;*.dotx)|*.docx;*.dotx|Текст (*.txt;*.md)|*.txt;*.md|Все файлы|*.*",
                Multiselect = true
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            using var typeForm = new TemplateTypeDialog();
            if (typeForm.ShowDialog() != DialogResult.OK) return;

            foreach (var path in dlg.FileNames)
            {
                try
                {
                    ThisAddIn.TemplateManager.Add(path,
                        category: typeForm.Category,
                        type: typeForm.SelectedType,
                        description: typeForm.Description);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления {Path.GetFileName(path)}: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            RefreshTemplateList();
            AppendSystem($"Шаблоны добавлены. Всего: {ThisAddIn.TemplateManager.All.Count}");
        }

        private void BtnRemoveTemplate_Click(object sender, EventArgs e)
        {
            if (lstTemplates.SelectedIndex < 0) return;
            var entry = ThisAddIn.TemplateManager.All.ElementAtOrDefault(lstTemplates.SelectedIndex);
            if (entry == null) return;
            if (MessageBox.Show($"Удалить шаблон '{entry.Name}'?", "Подтверждение",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            ThisAddIn.TemplateManager.Remove(entry.Name);
            RefreshTemplateList();
        }

        private void BtnApplyTemplate_Click(object sender, EventArgs e)
        {
            if (lstTemplates.SelectedIndex < 0) { AppendSystem("Выберите шаблон в списке."); return; }
            var entry = ThisAddIn.TemplateManager.All.ElementAtOrDefault(lstTemplates.SelectedIndex);
            if (entry == null) return;
            try
            {
                ThisAddIn.TemplateManager.ApplyToDocument(entry);
                AppendSystem($"✓ Применён шаблон: {entry.Name}");
            }
            catch (Exception ex) { AppendSystem($"⚠ {ex.Message}"); }
        }

        private void AppendUser(string text) => AppendMessage("Вы:", text,
            Color.FromArgb(120, 200, 255), Color.FromArgb(220, 230, 255), bold: true);

        private void AppendAssistant(string text) => AppendMessage("AI:", text,
            Color.FromArgb(100, 220, 120), Color.FromArgb(210, 240, 210), bold: true);

        private void AppendSystem(string text) => AppendMessage("", text,
            Color.FromArgb(200, 170, 100), Color.FromArgb(200, 170, 100), bold: false, italic: true);

        private void AppendMessage(string header, string body, Color hColor, Color bColor,
            bool bold, bool italic = false)
        {
            UI(() =>
            {
                richBoxChat.SelectionStart = richBoxChat.TextLength;
                if (!string.IsNullOrEmpty(header))
                {
                    richBoxChat.SelectionColor = hColor;
                    richBoxChat.SelectionFont = new Font("Segoe UI", 9f,
                        bold ? FontStyle.Bold : FontStyle.Regular);
                    richBoxChat.AppendText($"\n{header}\n");
                }
                richBoxChat.SelectionColor = bColor;
                richBoxChat.SelectionFont = new Font("Segoe UI", 10f,
                    italic ? FontStyle.Italic : FontStyle.Regular);
                richBoxChat.AppendText((body ?? "").Trim() + "\n");
                richBoxChat.ScrollToCaret();
            });
        }

        private void SetBusy(bool busy)
        {
            UI(() =>
            {
                progressBar.Visible = busy;
                btnSend.Enabled = !busy;
                txtInput.Enabled = !busy;
                btnConnect.Enabled = !busy;
                cmbModels.Enabled = !busy;
                txtHost.Enabled = !busy;
                btnAddTemplate.Enabled = !busy;
                btnApplyTemplate.Enabled = !busy;
                if (chkTemplateMode != null) chkTemplateMode.Enabled = !busy;
            });
        }

        private static bool IsContextLeak(string text)
        {
            if (text == null) return false;
            var v = text.ToLowerInvariant();
            return v.Contains("document_state") || v.Contains("user_request")
                || v.Contains("selection_context") || v.Contains("document_context")
                || v.Contains("контекст документа") || v.Contains("json")
                || v.Contains("prompt") || v.Contains("system");
        }

        private static bool LooksLikeCode(string raw) =>
            !string.IsNullOrWhiteSpace(raw) &&
            (raw.Contains("```csharp") || raw.Contains("WordApp.") || raw.Contains("ActiveDoc."));

        private static bool IsCancelCommand(string text)
        {
            var t = NormalizeText(text);
            return t == "отмена" || t == "стоп" || t == "cancel";
        }

        private bool IsDocumentCommand(string text)
        {
            if (_templateModeEnabled) return true;
            var t = (text ?? "").ToLowerInvariant();
            string[] commands =
            {
                "вставь","создай","сделай","добавь","удали",
                "напиши","сформируй","таблица","диаграмма"
            };
            return commands.Any(t.Contains);
        }

        private static bool IsClearAction(string a)
        {
            var n = NormalizeAction(a);
            return n == "ClearDocument";
        }

        private static bool HasClearIntent(string text)
        {
            var t = NormalizeText(text);
            // Require explicit full-delete intent
            return t.Contains("удали весь текст") ||
                   t.Contains("удалить весь текст") ||
                   t.Contains("удали все") || t.Contains("удали всё") ||
                   t.Contains("удалить все") || t.Contains("удалить всё") ||
                   t.Contains("очисти документ") || t.Contains("очистить документ");
        }

        private static string Norm(string s) => NormalizeText(s);

        private static string NormalizeText(string s)
        {
            var t = (s ?? "")
                .Trim()
                .ToLowerInvariant()
                .Replace("ё", "е")
                .Replace("докуммент", "документ")
                .Replace("таблицаа", "таблица")
                .Replace("смарт арт", "смартарт");
            t = Regex.Replace(t, @"\bдиаграм\w*", "диаграмма");
            t = Regex.Replace(t, @"\bтаблиц\w*", "таблица");
            return t;
        }

        private static string GenerateText(int chars)
        {
            var sb = new StringBuilder();
            string paragraph =
                "Искусственный интеллект постепенно становится важной частью современной цифровой среды. " +
                "Он используется в системах анализа данных, автоматизации процессов и создании интеллектуальных интерфейсов. " +
                "Развитие технологий машинного обучения позволяет создавать программы, способные адаптироваться к новым условиям.";

            while (sb.Length < chars)
            {
                sb.Append(paragraph);
                sb.Append(" ");
            }

            return sb.ToString();
        }

        private static string NormalizeAction(string raw)
        {
            var a = (raw ?? "").Trim();
            var compact = Regex.Replace(a, @"[\s_\-]+", "").ToLowerInvariant();
            switch (compact)
            {
                case "formatgost": case "gost": return "FormatDocumentGOST";
                case "gost19": case "applygost19": return "ApplyGost19Template";
                case "insertpiechart": case "piechart": return "InsertPieChart";
                case "insertchart": case "createchart": case "addchart": return "InsertChart";
                case "createtable": case "addtable": return "InsertTable";
                case "removetable": case "deletetable": return "DeleteTable";
                case "delete": case "remove": return "DeleteSelection";
                case "undo": case "undolastaction": return "Undo";
                case "createheading": return "ApplyHeading";
                case "fontsize": return "SetFontSize";
                case "fontname": return "SetFontName";
                case "fontcolor": return "SetFontColor";
                case "marginscm": return "SetPageMarginsCm";
                case "inserttemplate": case "applytemplate": return "ApplyTemplate";
                case "insertfile": case "insertfilecontent": return "InsertFileContent";
                default: return a;
            }
        }

                private static string Humanize(string action)
        {
            switch (NormalizeAction(action))
            {
                case "InsertText": return "вставил текст";
                case "AppendParagraph": return "добавил абзац";
                case "FindReplace": return "выполнил замену";
                case "Bold": return "жирный";
                case "Italic": return "курсив";
                case "Underline": return "подчёркивание";
                case "SetFontName": return "изменил шрифт";
                case "SetFontSize": return "изменил размер";
                case "AlignJustify": return "выравнивание по ширине";
                case "AlignCenter": return "по центру";
                case "SetLineSpacingMultiple": return "межстрочный интервал";
                case "SetPageMarginsCm": return "поля страницы";
                case "InsertTable": return "создал таблицу";
                case "DeleteTable": return "удалил таблицу";
                case "InsertPieChart": return "круговая диаграмма";
                case "InsertChart": return "диаграмма";
                case "InsertSmartArt": return "SmartArt";
                case "InsertEquation": return "формула";
                case "InsertCaption": return "подпись";
                case "InsertCitation": return "цитата";
                case "InsertBibliography": return "библиография";
                case "UpdateFields": return "обновил поля";
                case "ApplyGost19Template": return "ГОСТ 19";
                case "FormatDocumentGOST": return "форматирование ГОСТ";
                case "CreateTableOfContents": return "оглавление";
                case "InsertPageNumbers": return "номера страниц";
                case "SaveDocument": return "сохранил";
                case "ExportPDF": return "экспорт PDF";
                case "ApplyTemplate": return "применил шаблон";
                case "InsertFileContent": return "вставил файл";
                default: return NormalizeAction(action);
            }
        }

        private static string S(JObject ps, string key) => ps?[key]?.ToString();
        private static bool B(JObject ps, string key, bool def)
        {
            var t = ps?[key];
            if (t == null) return def;
            if (t.Type == JTokenType.Boolean) return t.Value<bool>();
            var v = t.ToString().Trim().ToLowerInvariant();
            return v == "true" || v == "1" || v == "yes" ? true
                : v == "false" || v == "0" || v == "no" ? false : def;
        }
        private static int I(JObject ps, string key, int def) =>
            ps?[key]?.Value<int?>() ?? def;

        private static object[] BuildArgs(JToken token)
        {
            if (!(token is JArray arr)) return Array.Empty<object>();
            return arr.Select(t =>
            {
                switch (t?.Type)
                {
                    case JTokenType.Integer: return (object)t.Value<int>();
                    case JTokenType.Float: return (object)t.Value<double>();
                    case JTokenType.Boolean: return (object)t.Value<bool>();
                    default: return (object)t?.ToString();
                }
            }).ToArray();
        }

        private static Color ParseColor(JToken token)
        {
            var v = (token?.ToString() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(v)) return Color.Black;
            try { return System.Drawing.ColorTranslator.FromHtml(v); } catch { }
            switch (v.ToLowerInvariant())
            {
                case "red": case "красный": return Color.Red;
                case "blue": case "синий": return Color.Blue;
                case "green": case "зелёный": case "зеленый": return Color.Green;
                case "yellow": case "жёлтый": case "желтый": return Color.Yellow;
                case "black": case "чёрный": case "черный": return Color.Black;
                case "white": case "белый": return Color.White;
                default: return Color.Black;
            }
        }

        private static Word.WdColorIndex ParseHighlight(JToken token)
        {
            switch ((token?.ToString() ?? "").Trim().ToLowerInvariant())
            {
                case "yellow": case "жёлтый": case "желтый": return Word.WdColorIndex.wdYellow;
                case "green": case "зелёный": case "зеленый": return Word.WdColorIndex.wdBrightGreen;
                case "blue": case "синий": return Word.WdColorIndex.wdBlue;
                case "red": case "красный": return Word.WdColorIndex.wdRed;
                case "pink": case "розовый": return Word.WdColorIndex.wdPink;
                case "none": case "off": return Word.WdColorIndex.wdNoHighlight;
                default: return Word.WdColorIndex.wdYellow;
            }
        }

        private static string DefaultPdfPath() =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                "Document_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf");

        private static string DefaultDocPath() =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                "Document_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".docx");

        private static readonly string[] ActionList =
        {
            "InsertText","InsertTable","DeleteText","FindReplace",
            "Bold","Italic","SetFontSize","AlignCenter",
            "InsertPieChart","InsertImage","InsertPageBreak",
            "InsertHeader","InsertFooter","SaveDocument","ExportPDF",
            "Undo","Redo"
        };
    }
}





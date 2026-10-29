using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using MathTrainer.Models;
using MathTrainer.Services;

namespace MathTrainer.Forms;

public partial class WebAppForm : Form
{
    private readonly AuthService  _auth;
    private readonly StatsService _stats;
    private User? _user;

    public WebAppForm(AuthService auth, StatsService stats, QuestionGen _)
    {
        _auth  = auth;
        _stats = stats;
        InitializeComponent();
        Load += async (_, _) => await InitAsync();
    }

    // ── WebView init ──────────────────────────────────────────────
    private async Task InitAsync()
    {
        await Task.Run(DbInitializer.EnsureCreated);
        await webView.EnsureCoreWebView2Async();
        var s = webView.CoreWebView2.Settings;
        s.AreDefaultContextMenusEnabled = false;
        s.AreDevToolsEnabled            = false;
        s.IsStatusBarEnabled            = false;
        webView.CoreWebView2.WebMessageReceived += OnMessage;

        var wwwroot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "app.local", wwwroot, CoreWebView2HostResourceAccessKind.Allow);
        webView.CoreWebView2.Navigate("https://app.local/index.html");
    }

    // ── Message router ────────────────────────────────────────────
    private void OnMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            var raw    = JsonSerializer.Deserialize<string>(e.WebMessageAsJson) ?? e.WebMessageAsJson;
            var msg    = JsonSerializer.Deserialize<JsonElement>(raw);
            var action = msg.GetProperty("action").GetString() ?? "";
            switch (action)
            {
                case "login":      HandleLogin(msg);      break;
                case "register":   HandleRegister(msg);   break;
                case "getBests":   SendBests();           break;
                case "saveResult": HandleSaveResult(msg); break;
                case "getStats":   SendStats();           break;
            }
        }
        catch { }
    }

    // ── Login / Register ──────────────────────────────────────────
    private void HandleLogin(JsonElement msg)
    {
        var u = msg.GetProperty("username").GetString() ?? "";
        var p = msg.GetProperty("password").GetString() ?? "";
        var (ok, message, user) = _auth.Login(u, p);
        if (ok && user != null) { _user = user; PostOk(user); }
        else Post("loginError", new { message });
    }

    private void HandleRegister(JsonElement msg)
    {
        var u = msg.GetProperty("username").GetString() ?? "";
        var p = msg.GetProperty("password").GetString() ?? "";
        var (ok, message, user) = _auth.Register(u, p);
        if (ok && user != null) { _user = user; PostOk(user); }
        else Post("loginError", new { message });
    }

    private void PostOk(User u) =>
        Post("loginSuccess", new { user = new { username = u.Username, createdAt = u.CreatedAt } });

    // ── Stats ─────────────────────────────────────────────────────
    private void SendStats()
    {
        if (_user == null) return;
        var results = _stats.Load(_user.Id);
        var topicStats = _stats.GetTopicStats(_user.Id);

        var recent = results.Take(20).Select(r => new {
            topicId   = r.TopicId,
            topicName = r.TopicName,
            correct   = r.CorrectCount,
            total     = r.TotalCount,
            pct       = r.TotalCount > 0 ? (int)Math.Round((double)r.CorrectCount / r.TotalCount * 100) : 0,
            date      = r.CompletedAt.ToString("dd.MM HH:mm"),
        }).ToList();

        var byTopic = topicStats.Select(s => new {
            name    = s.TopicName,
            avg     = (int)Math.Round(s.AvgPct),
            best    = (int)Math.Round(s.BestPct),
            count   = s.Count,
        }).ToList();

        var totalTests  = results.Count;
        var avgPct      = results.Count > 0 ? (int)Math.Round(results.Average(r => r.TotalCount > 0 ? (double)r.CorrectCount / r.TotalCount * 100 : 0)) : 0;
        var topicsCount = topicStats.Count;

        Post("stats", new { recent, byTopic, totalTests, avgPct, topicsCount });
    }

    // ── Bests ──────────────────────────────────────────────────────
    private void SendBests()
    {
        if (_user == null) return;
        var results = _stats.Load(_user.Id);
        var bests   = results
            .GroupBy(r => r.TopicId)
            .ToDictionary(
                g => g.Key,
                g => g.Max(r => r.Percentage >= 90 ? 5 :
                                r.Percentage >= 75 ? 4 :
                                r.Percentage >= 60 ? 3 : 2));
        Post("bests", new { bests });
    }

    // ── Save result ────────────────────────────────────────────────
    private void HandleSaveResult(JsonElement msg)
    {
        if (_user == null) return;
        var topicId   = msg.GetProperty("topicId").GetString()   ?? "";
        var topicName = msg.GetProperty("topicName").GetString()  ?? "";
        var correct   = msg.GetProperty("correctCount").GetInt32();
        var total     = msg.GetProperty("totalCount").GetInt32();
        var answers   = msg.GetProperty("answers").EnumerateArray()
            .Select(a => new AnswerRecord
            {
                QuestionText  = a.GetProperty("questionText").GetString()  ?? "",
                UserAnswer    = a.GetProperty("userAnswer").GetString()    ?? "",
                CorrectAnswer = a.GetProperty("correctAnswer").GetString() ?? "",
                IsCorrect     = a.GetProperty("isCorrect").GetBoolean(),
                Explanation   = a.GetProperty("explanation").GetString()   ?? "",
            }).ToList();

        _stats.Save(new TestResult
        {
            UserId = _user.Id, Username = _user.Username,
            TopicId = topicId, TopicName = topicName,
            CorrectCount = correct, TotalCount = total,
            Answers = answers,
        });
    }

    // ── Post helper ────────────────────────────────────────────────
    private void Post(string type, object data)
    {
        var json = JsonSerializer.Serialize(new { type, data });
        if (InvokeRequired) Invoke(() => webView.CoreWebView2.PostWebMessageAsString(json));
        else webView.CoreWebView2.PostWebMessageAsString(json);
    }
}

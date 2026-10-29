using MathTrainer.Helpers;
using MathTrainer.Models;
using MathTrainer.Services;

namespace MathTrainer.Forms;

public class ProfileForm : Form
{
    private readonly StatsService _stats;
    private readonly User         _user;

    public ProfileForm(StatsService stats, User user)
    {
        _stats = stats;
        _user  = user;
        Build();
    }

    private void Build()
    {
        Text            = $"Профиль — {_user.Username}";
        Size            = new Size(660, 620);
        MinimumSize     = new Size(620, 560);
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = Color.FromArgb(245, 246, 250);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;

        // ── Header ───────────────────────────────────────────────────
        var header = new DoubleBufferedPanel { Dock = DockStyle.Top, Height = 100 };
        header.Paint += (_, e) =>
        {
            var g = e.Graphics;
            using var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                header.ClientRectangle,
                Color.FromArgb(67, 56, 202),
                Color.FromArgb(17, 24, 39),
                System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal);
            g.FillRectangle(br, header.ClientRectangle);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using var circleBr = new SolidBrush(Color.FromArgb(50, 255, 255, 255));
            g.FillEllipse(circleBr, 16, 12, 64, 64);
            using var ef = new Font("Segoe UI Emoji", 28);
            using var eb = new SolidBrush(Color.White);
            g.DrawString("👤", ef, eb, 18f, 14f);

            using var nf = new Font("Segoe UI", 16, FontStyle.Bold);
            using var nb = new SolidBrush(Color.White);
            g.DrawString(_user.Username, nf, nb, 92f, 18f);

            using var df = new Font("Segoe UI", 9);
            using var db = new SolidBrush(Color.FromArgb(170, 200, 210, 255));
            g.DrawString($"Зарегистрирован: {_user.CreatedAt:dd MMMM yyyy}", df, db, 92f, 52f);
        };

        // ── Stats ─────────────────────────────────────────────────────
        var allResults  = _stats.Load(_user.Id);
        var topicStats  = _stats.GetTopicStats(_user.Id);
        int totalTests  = allResults.Count;
        double avgScore = totalTests > 0 ? allResults.Average(r => r.Percentage) : 0;
        int best        = totalTests > 0 ? (int)allResults.Max(r => r.Percentage) : 0;

        // Summary cards
        var summaryPanel = new Panel
        {
            Location  = new Point(16, 112),
            Size      = new Size(628, 80),
            BackColor = Color.Transparent
        };

        var statCards = new (string val, string lbl, Color color)[]
        {
            ($"{totalTests}", "Тестов", Color.FromArgb(99, 102, 241)),
            ($"{avgScore:F0}%", "Среднее", avgScore >= 75 ? Color.FromArgb(22, 163, 74) : avgScore >= 50 ? Color.FromArgb(217, 119, 6) : Color.FromArgb(220, 38, 38)),
            ($"{best}%", "Лучший результат", Color.FromArgb(37, 99, 235)),
        };

        for (int i = 0; i < statCards.Length; i++)
        {
            int ci = i;
            var sc = new DoubleBufferedPanel
            {
                Location = new Point(i * 210, 0),
                Size     = new Size(198, 72)
            };
            sc.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var path = ModernButton.BuildRoundedPath(new Rectangle(0, 0, sc.Width - 1, sc.Height - 1), 12);
                using var bgBr = new SolidBrush(Color.White);
                g.FillPath(bgBr, path);
                using var pen = new Pen(Color.FromArgb(229, 231, 235));
                g.DrawPath(pen, path);

                using var vf = new Font("Segoe UI", 20, FontStyle.Bold);
                using var vb = new SolidBrush(statCards[ci].color);
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(statCards[ci].val, vf, vb, new RectangleF(0, 10, sc.Width, 30), sf);

                using var lf = new Font("Segoe UI", 8);
                using var lb = new SolidBrush(Color.FromArgb(107, 114, 128));
                g.DrawString(statCards[ci].lbl, lf, lb, new RectangleF(0, 44, sc.Width, 20), sf);
            };
            summaryPanel.Controls.Add(sc);
        }

        // ── Topic progress ────────────────────────────────────────────
        var lblTopics = new Label
        {
            Text      = "Прогресс по темам",
            Font      = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            AutoSize  = true,
            Location  = new Point(16, 202),
            BackColor = Color.Transparent
        };

        var topicScroll = new Panel
        {
            Location   = new Point(16, 228),
            Size       = new Size(628, 260),
            AutoScroll = true,
            BackColor  = Color.Transparent
        };

        int ty = 0;
        if (topicStats.Count == 0)
        {
            var empty = new Label
            {
                Text      = "Вы ещё не проходили тесты.",
                Font      = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.FromArgb(107, 114, 128),
                AutoSize  = true,
                Location  = new Point(0, 8),
                BackColor = Color.Transparent
            };
            topicScroll.Controls.Add(empty);
        }

        foreach (var ts in topicStats)
        {
            var row = new TopicStatRow(ts) { Location = new Point(0, ty), Size = new Size(608, 52) };
            topicScroll.Controls.Add(row);
            ty += 60;
        }

        // ── Recent history ────────────────────────────────────────────
        var lblHist = new Label
        {
            Text      = "Последние тесты",
            Font      = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            AutoSize  = true,
            Location  = new Point(16, 498),
            BackColor = Color.Transparent
        };

        var histPanel = new Panel
        {
            Location  = new Point(16, 522),
            Size      = new Size(628, 24 * Math.Min(allResults.Count, 3) + 4),
            BackColor = Color.Transparent
        };

        int hy = 0;
        foreach (var r in allResults.Take(3))
        {
            Color rc = r.Grade == "2" ? Color.FromArgb(220, 38, 38) :
                       r.Grade == "5" ? Color.FromArgb(22, 163, 74) :
                       Color.FromArgb(107, 114, 128);
            var lbl = new Label
            {
                Text      = $"{r.CompletedAt:dd.MM HH:mm}   {r.TopicName,-26}   {r.CorrectCount}/{r.TotalCount}  —  оценка {r.Grade}",
                Font      = new Font("Consolas", 8),
                ForeColor = rc,
                AutoSize  = true,
                Location  = new Point(0, hy),
                BackColor = Color.Transparent
            };
            histPanel.Controls.Add(lbl);
            hy += 22;
        }

        var btnClose = new ModernButton
        {
            Text      = "Закрыть",
            Size      = new Size(150, 40),
            Location  = new Point(240, 570),
            NormalBg  = Color.FromArgb(99, 102, 241),
            HoverBg   = Color.FromArgb(79, 70, 229),
            PressedBg = Color.FromArgb(55, 48, 163),
            Font      = new Font("Segoe UI", 11, FontStyle.Bold)
        };
        btnClose.Click += (_, _) => Close();

        Controls.AddRange([header, summaryPanel, lblTopics, topicScroll, lblHist, histPanel, btnClose]);
    }
}

internal class TopicStatRow : Control
{
    private readonly StatsService.TopicStat _ts;

    public TopicStatRow(StatsService.TopicStat ts)
    {
        _ts = ts;
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var path = ModernButton.BuildRoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), 10);
        using var bg   = new SolidBrush(Color.White);
        g.FillPath(bg, path);
        using var pen  = new Pen(Color.FromArgb(229, 231, 235));
        g.DrawPath(pen, path);

        using var nf = new Font("Segoe UI", 9, FontStyle.Bold);
        using var nb = new SolidBrush(Color.FromArgb(17, 24, 39));
        g.DrawString(_ts.TopicName, nf, nb, 12f, 8f);

        using var sf2 = new Font("Segoe UI", 7);
        using var sb2 = new SolidBrush(Color.FromArgb(107, 114, 128));
        g.DrawString($"Попыток: {_ts.Count}   Лучшее: {_ts.BestPct:F0}%", sf2, sb2, 12f, 28f);

        // Bar track
        int barX = 240, barY = 20, barW = 280, barH = 10;
        using var trackBr = new SolidBrush(Color.FromArgb(229, 231, 235));
        using var tp      = ModernButton.BuildRoundedPath(new Rectangle(barX, barY, barW, barH), 5);
        g.FillPath(trackBr, tp);

        int fillW = Math.Max(10, (int)(_ts.AvgPct / 100.0 * barW));
        Color barColor = _ts.AvgPct >= 75 ? Color.FromArgb(34, 197, 94) :
                         _ts.AvgPct >= 50 ? Color.FromArgb(245, 158, 11) :
                         Color.FromArgb(239, 68, 68);
        using var fillBr = new SolidBrush(barColor);
        using var fp     = ModernButton.BuildRoundedPath(new Rectangle(barX, barY, fillW, barH), 5);
        g.FillPath(fillBr, fp);

        using var pctF = new Font("Segoe UI", 8, FontStyle.Bold);
        using var pctB = new SolidBrush(barColor);
        g.DrawString($"{_ts.AvgPct:F0}%", pctF, pctB, barX + barW + 8, barY - 2);
    }
}

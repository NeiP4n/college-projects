using MathTrainer.Helpers;
using MathTrainer.Models;

namespace MathTrainer.Forms;

public class ResultForm : Form
{
    private readonly TestResult _result;

    public ResultForm(TestResult result)
    {
        _result = result;
        Build();
    }

    private void Build()
    {
        Text            = "Результат теста";
        Size            = new Size(580, 640);
        MinimumSize     = new Size(540, 580);
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = Color.FromArgb(245, 246, 250);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;

        // ── Banner ────────────────────────────────────────────────────
        Color bannerColor = _result.Grade switch
        {
            "5" => Color.FromArgb(22, 163, 74),
            "4" => Color.FromArgb(37, 99, 235),
            "3" => Color.FromArgb(217, 119, 6),
            _   => Color.FromArgb(220, 38, 38)
        };
        string emoji = _result.Grade switch { "5" => "🏆", "4" => "🎯", "3" => "👍", _ => "📖" };
        string msg   = _result.Grade switch
        {
            "5" => "Отлично! Превосходный результат!",
            "4" => "Хорошо! Так держать!",
            "3" => "Удовлетворительно. Попробуй ещё раз.",
            _   => "Нужно больше практики. Не сдавайся!"
        };

        var banner = new DoubleBufferedPanel { Dock = DockStyle.Top, Height = 140 };
        banner.Paint += (_, e) =>
        {
            var g = e.Graphics;
            using var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                banner.ClientRectangle, bannerColor,
                ControlPaint.Dark(bannerColor, 0.15f),
                System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal);
            g.FillRectangle(br, banner.ClientRectangle);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using var ef = new Font("Segoe UI Emoji", 36);
            using var eb = new SolidBrush(Color.White);
            g.DrawString(emoji, ef, eb, 16f, 20f);

            using var gf = new Font("Segoe UI", 40, FontStyle.Bold);
            using var gb = new SolidBrush(Color.White);
            g.DrawString(_result.Grade, gf, gb, 78f, 16f);

            using var pf = new Font("Segoe UI", 14, FontStyle.Bold);
            using var pb = new SolidBrush(Color.White);
            g.DrawString($"{_result.Percentage:F0}%  —  {_result.CorrectCount} из {_result.TotalCount}", pf, pb, 130f, 28f);

            using var mf = new Font("Segoe UI", 10);
            using var mb = new SolidBrush(Color.FromArgb(220, 255, 255, 255));
            g.DrawString(msg, mf, mb, 130f, 62f);

            using var tf = new Font("Segoe UI", 9);
            using var tb = new SolidBrush(Color.FromArgb(160, 255, 255, 255));
            g.DrawString(_result.TopicName, tf, tb, 130f, 88f);
        };

        // ── Scroll area ───────────────────────────────────────────────
        var scroll = new Panel
        {
            Location   = new Point(16, 148),
            Size       = new Size(548, 420),
            AutoScroll = true,
            BackColor  = Color.Transparent
        };

        int y = 4;
        foreach (var a in _result.Answers)
        {
            var row = new AnswerRow(a) { Location = new Point(0, y), Size = new Size(528, 72) };
            scroll.Controls.Add(row);
            y += 80;
        }

        // ── Close button ──────────────────────────────────────────────
        var btnClose = new ModernButton
        {
            Text      = "Закрыть",
            Size      = new Size(160, 42),
            Location  = new Point(196, 574),
            NormalBg  = Color.FromArgb(99, 102, 241),
            HoverBg   = Color.FromArgb(79, 70, 229),
            PressedBg = Color.FromArgb(55, 48, 163),
            Font      = new Font("Segoe UI", 11, FontStyle.Bold)
        };
        btnClose.Click += (_, _) => Close();

        Controls.AddRange([banner, scroll, btnClose]);
    }
}

internal class AnswerRow : Control
{
    private readonly AnswerRecord _a;

    public AnswerRow(AnswerRecord a)
    {
        _a = a;
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var bg     = _a.IsCorrect ? Color.FromArgb(240, 253, 244) : Color.FromArgb(254, 242, 242);
        var border = _a.IsCorrect ? Color.FromArgb(187, 247, 208) : Color.FromArgb(254, 202, 202);
        var fgMain = _a.IsCorrect ? Color.FromArgb(22, 101, 52)   : Color.FromArgb(153, 27, 27);

        using var path = ModernButton.BuildRoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), 10);
        using var bgBr = new SolidBrush(bg);
        g.FillPath(bgBr, path);
        using var pen = new Pen(border);
        g.DrawPath(pen, path);

        // Icon
        using var iconF = new Font("Segoe UI Emoji", 16);
        using var iconB = new SolidBrush(Color.Black);
        g.DrawString(_a.IsCorrect ? "✅" : "❌", iconF, iconB, 10f, 14f);

        // Question
        using var qf = new Font("Segoe UI", 9, FontStyle.Bold);
        using var qb = new SolidBrush(Color.FromArgb(17, 24, 39));
        g.DrawString(_a.QuestionText, qf, qb, new RectangleF(48, 8, Width - 56, 20));

        // Answer
        string ansText = _a.IsCorrect
            ? $"Верно: {_a.CorrectAnswer}"
            : $"Ваш: {_a.UserAnswer}   •   Правильно: {_a.CorrectAnswer}";
        using var af = new Font("Segoe UI", 8, FontStyle.Bold);
        using var ab = new SolidBrush(fgMain);
        g.DrawString(ansText, af, ab, new RectangleF(48, 28, Width - 56, 20));

        // Explanation
        using var ef = new Font("Segoe UI", 7, FontStyle.Italic);
        using var eb = new SolidBrush(Color.FromArgb(107, 114, 128));
        g.DrawString(_a.Explanation, ef, eb, new RectangleF(48, 48, Width - 56, 18));
    }
}

using MathTrainer.Helpers;
using MathTrainer.Models;

namespace MathTrainer.Forms;

public class QuizForm : Form
{
    private readonly Topic              _topic;
    private readonly List<Question>     _questions;
    private int                         _current;
    private int                         _correct;
    private bool                        _answered;
    private readonly List<AnswerRecord> _answers = new();

    private DoubleBufferedPanel _header  = null!;
    private Label               _lblQ    = null!;
    private OptionButton[]      _opts    = null!;
    private Label               _lblHint = null!;
    private ModernButton        _btnNext = null!;

    public TestResult? Result { get; private set; }

    public QuizForm(Topic topic, List<Question> questions)
    {
        _topic     = topic;
        _questions = questions;
        Build();
        ShowQ();
    }

    private void Build()
    {
        Text            = $"{_topic.Icon} {_topic.Name}";
        Size            = new Size(640, 580);
        MinimumSize     = new Size(600, 540);
        MaximumSize     = new Size(700, 620);
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = Color.FromArgb(245, 246, 250);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;

        // ── Header ───────────────────────────────────────────────────
        _header = new DoubleBufferedPanel { Dock = DockStyle.Top, Height = 72 };
        _header.Paint += PaintHeader;

        // ── Question card ─────────────────────────────────────────────
        var qCard = new DoubleBufferedPanel
        {
            Location  = new Point(24, 84),
            Size      = new Size(592, 100),
            BackColor = Color.White
        };
        qCard.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var path = ModernButton.BuildRoundedPath(new Rectangle(0, 0, qCard.Width - 1, qCard.Height - 1), 12);
            using var bg = new SolidBrush(Color.White);
            g.FillPath(bg, path);
            using var pen = new Pen(Color.FromArgb(229, 231, 235));
            g.DrawPath(pen, path);
        };

        _lblQ = new Label
        {
            Dock      = DockStyle.Fill,
            Font      = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding   = new Padding(12)
        };
        qCard.Controls.Add(_lblQ);

        // ── Option buttons ────────────────────────────────────────────
        _opts = new OptionButton[4];
        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            _opts[i] = new OptionButton(i)
            {
                Location = new Point(24, 196 + i * 66),
                Size     = new Size(592, 54)
            };
            _opts[i].Click += (_, _) => OnAnswer(idx);
            Controls.Add(_opts[i]);
        }

        // ── Hint ──────────────────────────────────────────────────────
        _lblHint = new Label
        {
            Location  = new Point(24, 466),
            Size      = new Size(592, 36),
            Font      = new Font("Segoe UI", 9, FontStyle.Italic),
            ForeColor = Color.FromArgb(107, 114, 128),
            BackColor = Color.Transparent,
            Visible   = false
        };

        // ── Next button ───────────────────────────────────────────────
        _btnNext = new ModernButton
        {
            Text      = "Следующий →",
            Size      = new Size(180, 42),
            Location  = new Point(436, 510),
            Visible   = false,
            NormalBg  = Color.FromArgb(99, 102, 241),
            HoverBg   = Color.FromArgb(79, 70, 229),
            PressedBg = Color.FromArgb(55, 48, 163),
            Font      = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        _btnNext.Click += (_, _) => Next();

        Controls.AddRange([_header, qCard, _lblHint, _btnNext]);
    }

    private void PaintHeader(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        using var br = new System.Drawing.Drawing2D.LinearGradientBrush(
            _header.ClientRectangle,
            Color.FromArgb(99, 102, 241),
            Color.FromArgb(79, 70, 229),
            System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
        g.FillRectangle(br, _header.ClientRectangle);

        using var titleFont = new Font("Segoe UI Emoji", 14, FontStyle.Bold);
        using var titleBr   = new SolidBrush(Color.White);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.DrawString($"{_topic.Icon}  {_topic.Name}", titleFont, titleBr, 16f, 12f);

        // Progress bar
        string prog = $"{_current + 1} / {_questions.Count}";
        using var pFont = new Font("Segoe UI", 9, FontStyle.Bold);
        using var pBr   = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
        var pSize = g.MeasureString(prog, pFont);
        g.DrawString(prog, pFont, pBr, _header.Width - pSize.Width - 16, 14f);

        // Bar
        int barW = _header.Width - 32;
        int fillW = _questions.Count > 0 ? (int)((double)_current / _questions.Count * barW) : 0;
        using var trackBr = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
        using var trackPath = ModernButton.BuildRoundedPath(new Rectangle(16, 52, barW, 8), 4);
        g.FillPath(trackBr, trackPath);
        if (fillW > 8)
        {
            using var fillBr   = new SolidBrush(Color.White);
            using var fillPath = ModernButton.BuildRoundedPath(new Rectangle(16, 52, fillW, 8), 4);
            g.FillPath(fillBr, fillPath);
        }
    }

    private void ShowQ()
    {
        _answered = false;
        _lblHint.Visible  = false;
        _btnNext.Visible  = false;

        var q = _questions[_current];
        _lblQ.Text = q.Text;
        _header.Invalidate();

        for (int i = 0; i < 4; i++)
        {
            _opts[i].SetState(q.Options[i], OptionState.Normal);
            _opts[i].Enabled = true;
        }
    }

    private void OnAnswer(int idx)
    {
        if (_answered) return;
        _answered = true;

        var q = _questions[_current];
        bool correct = idx == q.CorrectIndex;
        if (correct) _correct++;

        _answers.Add(new AnswerRecord
        {
            QuestionText  = q.Text,
            UserAnswer    = q.Options[idx],
            CorrectAnswer = q.Options[q.CorrectIndex],
            IsCorrect     = correct,
            Explanation   = q.Explanation
        });

        for (int i = 0; i < 4; i++)
        {
            _opts[i].Enabled = false;
            if (i == q.CorrectIndex)
                _opts[i].SetState(q.Options[i], OptionState.Correct);
            else if (i == idx)
                _opts[i].SetState(q.Options[i], OptionState.Wrong);
        }

        _lblHint.Text    = $"💡  {q.Explanation}";
        _lblHint.Visible = true;

        bool last = _current == _questions.Count - 1;
        _btnNext.Text    = last ? "Завершить тест ✓" : "Следующий →";
        _btnNext.Visible = true;
    }

    private void Next()
    {
        _current++;
        if (_current >= _questions.Count)
        {
            Result = new TestResult
            {
                TopicId      = _topic.Id,
                TopicName    = _topic.Name,
                CorrectCount = _correct,
                TotalCount   = _questions.Count,
                Answers      = _answers
            };
            DialogResult = DialogResult.OK;
            Close();
            return;
        }
        ShowQ();
    }
}

internal enum OptionState { Normal, Correct, Wrong }

internal class OptionButton : Control
{
    private string      _text  = "";
    private OptionState _state = OptionState.Normal;
    private bool        _hov;
    private readonly int _index;

    public OptionButton(int index)
    {
        _index = index;
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);
    }

    public void SetState(string text, OptionState state)
    {
        _text  = text;
        _state = state;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e) { _hov = true;  Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { _hov = false; Invalidate(); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = ModernButton.BuildRoundedPath(rect, 12);

        Color bg, border, fg;
        string prefix;
        switch (_state)
        {
            case OptionState.Correct:
                bg = Color.FromArgb(240, 253, 244); border = Color.FromArgb(34, 197, 94); fg = Color.FromArgb(22, 101, 52); prefix = "✅ ";
                break;
            case OptionState.Wrong:
                bg = Color.FromArgb(254, 242, 242); border = Color.FromArgb(239, 68, 68); fg = Color.FromArgb(153, 27, 27); prefix = "❌ ";
                break;
            default:
                bg = _hov ? Color.FromArgb(238, 242, 255) : Color.White;
                border = _hov ? Color.FromArgb(165, 180, 252) : Color.FromArgb(229, 231, 235);
                fg = Color.FromArgb(17, 24, 39);
                prefix = $"{(char)('A' + _index)}. ";
                break;
        }

        using var bgBr  = new SolidBrush(bg);
        g.FillPath(bgBr, path);
        using var pen = new Pen(border, _state != OptionState.Normal ? 2f : 1f);
        g.DrawPath(pen, path);

        using var font  = new Font("Segoe UI Emoji", 11, _state != OptionState.Normal ? FontStyle.Bold : FontStyle.Regular);
        using var fgBr  = new SolidBrush(fg);
        var sf = new StringFormat { LineAlignment = StringAlignment.Center };
        g.DrawString(prefix + _text, font, fgBr, new RectangleF(16, 0, Width - 32, Height), sf);
    }
}

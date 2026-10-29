using MathTrainer.Helpers;
using MathTrainer.Models;
using MathTrainer.Services;

namespace MathTrainer.Forms;

public class MainForm : Form
{
    private readonly StatsService _stats;
    private readonly QuestionGen  _gen;
    private readonly User         _user;
    private Panel _content = null!;

    public MainForm(StatsService stats, QuestionGen gen, User user)
    {
        _stats = stats;
        _gen   = gen;
        _user  = user;
        Build();
    }

    private void Build()
    {
        Text          = "MathTrainer";
        Size          = new Size(920, 680);
        MinimumSize   = new Size(720, 500);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor     = Color.FromArgb(245, 246, 250);

        // ── Sidebar ──────────────────────────────────────────────────
        var sidebar = MakeSidebar();

        // ── Content area (scrollable) ────────────────────────────────
        _content = new Panel
        {
            Dock       = DockStyle.Fill,
            AutoScroll = true,
            BackColor  = Color.FromArgb(245, 246, 250),
            Padding    = new Padding(28, 24, 16, 24)
        };

        RebuildContent();

        Controls.Add(_content);
        Controls.Add(sidebar);
    }

    // ═══════════════════════════════════════════════════════════════
    //  Sidebar  — TableLayoutPanel, 3 строки: лого / nav / user
    // ═══════════════════════════════════════════════════════════════
    private Panel MakeSidebar()
    {
        const int SBW       = 210;
        const int ROW_NAV   = 44;
        var       darkColor = Color.FromArgb(30, 27, 75);

        var sb = new Panel { Dock = DockStyle.Left, Width = SBW, BackColor = darkColor };
        sb.Paint += (_, e) =>
        {
            using var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                sb.ClientRectangle,
                Color.FromArgb(55, 48, 163),
                Color.FromArgb(17, 24, 39),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(br, sb.ClientRectangle);
        };

        // ── Logo block ─────────────────────────────────────────────
        var logoPanel = new TableLayoutPanel
        {
            Dock        = DockStyle.Top,
            Height      = 84,
            ColumnCount = 2,
            RowCount    = 2,
            BackColor   = Color.Transparent,
            Padding     = new Padding(12, 14, 8, 8)
        };
        logoPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 46));
        logoPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        logoPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        logoPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

        var iconLbl = new Label
        {
            Text      = "📐",
            Font      = new Font("Segoe UI Emoji", 22),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        logoPanel.Controls.Add(iconLbl, 0, 0);
        logoPanel.SetRowSpan(iconLbl, 2);

        logoPanel.Controls.Add(new Label
        {
            Text      = "MathTrainer",
            Font      = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft
        }, 1, 0);

        logoPanel.Controls.Add(new Label
        {
            Text      = "Тренажёр математики",
            Font      = new Font("Segoe UI", 7),
            ForeColor = Color.FromArgb(150, 190, 210, 255),
            BackColor = Color.Transparent,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.TopLeft
        }, 1, 1);

        sb.Controls.Add(logoPanel);

        // ── Divider ────────────────────────────────────────────────
        var div = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 1,
            BackColor = Color.FromArgb(45, 255, 255, 255)
        };
        sb.Controls.Add(div);

        // ── Nav buttons ────────────────────────────────────────────
        var navItems = new (string icon, string label, Action click)[]
        {
            ("🏠", "Темы",        RebuildContent),
            ("📊", "Профиль",     OpenProfile),
            ("🌙", "Тёмная тема", ToggleTheme),
        };

        // Add in reverse so DockStyle.Top stacks correctly
        foreach (var (icon, label, click) in navItems.Reverse())
        {
            var btn = new SidebarBtn(icon, label)
            {
                Dock   = DockStyle.Top,
                Height = ROW_NAV
            };
            btn.Click += (_, _) => click();
            sb.Controls.Add(btn);
        }

        // ── User info (bottom) ─────────────────────────────────────
        var userTbl = new TableLayoutPanel
        {
            Dock        = DockStyle.Bottom,
            Height      = 60,
            ColumnCount = 1,
            RowCount    = 2,
            BackColor   = Color.Transparent,
            Padding     = new Padding(12, 8, 8, 8)
        };
        userTbl.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(45, 255, 255, 255));
            e.Graphics.DrawLine(pen, 0, 0, userTbl.Width, 0);
        };
        userTbl.Controls.Add(new Label
        {
            Text      = $"👤  {_user.Username}",
            Font      = new Font("Segoe UI Emoji", 9, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft
        });
        userTbl.Controls.Add(new Label
        {
            Text      = $"с {_user.CreatedAt:dd.MM.yyyy}",
            Font      = new Font("Segoe UI", 7),
            ForeColor = Color.FromArgb(110, 180, 200, 255),
            BackColor = Color.Transparent,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.TopLeft
        });
        sb.Controls.Add(userTbl);

        return sb;
    }

    // ═══════════════════════════════════════════════════════════════
    //  Content
    // ═══════════════════════════════════════════════════════════════
    private void RebuildContent()
    {
        _content.SuspendLayout();
        _content.Controls.Clear();

        // Greeting
        var hi = new Label
        {
            Text      = $"Привет, {_user.Username}! 👋",
            Font      = new Font("Segoe UI Emoji", 20, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            AutoSize  = true,
            Margin    = new Padding(0, 0, 0, 2),
            BackColor = Color.Transparent
        };
        var sub = new Label
        {
            Text      = "Выбери тему и начни тренировку",
            Font      = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(107, 114, 128),
            AutoSize  = true,
            Margin    = new Padding(0, 0, 0, 16),
            BackColor = Color.Transparent
        };

        var topics = Topic.GetAll();

        var flow = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            AutoScroll    = false,
            FlowDirection = FlowDirection.TopDown,
            WrapContents  = false,
            BackColor     = Color.Transparent
        };

        flow.Controls.Add(hi);
        flow.Controls.Add(sub);
        flow.Controls.Add(SectionLabel("Начальная школа"));
        flow.Controls.Add(TopicRow(topics.Where(t => t.Level == SchoolLevel.Elementary).ToList()));
        flow.Controls.Add(SectionLabel("Средняя школа"));
        flow.Controls.Add(TopicRow(topics.Where(t => t.Level == SchoolLevel.Middle).ToList()));

        _content.Controls.Add(flow);
        _content.ResumeLayout();
    }

    private static Label SectionLabel(string text) => new()
    {
        Text      = text,
        Font      = new Font("Segoe UI", 12, FontStyle.Bold),
        ForeColor = Color.FromArgb(55, 65, 81),
        AutoSize  = true,
        Margin    = new Padding(0, 12, 0, 8),
        BackColor = Color.Transparent
    };

    private FlowLayoutPanel TopicRow(List<Topic> topics)
    {
        var row = new FlowLayoutPanel
        {
            AutoSize      = true,
            AutoSizeMode  = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents  = true,
            BackColor     = Color.Transparent,
            Margin        = new Padding(0, 0, 0, 4),
            MaximumSize   = new Size(10000, 0)   // allow any width
        };

        foreach (var t in topics)
            row.Controls.Add(TopicCard(t));

        return row;
    }

    private Panel TopicCard(Topic topic)
    {
        const int W = 190;
        bool hovered = false;

        var card = new DoubleBufferedPanel
        {
            Size   = new Size(W, 168),
            Margin = new Padding(0, 0, 12, 12),
            Cursor = Cursors.Hand
        };
        card.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
            using var path = ModernButton.BuildRoundedPath(r, 14);
            using var bg   = new SolidBrush(hovered ? Color.FromArgb(238, 242, 255) : Color.White);
            g.FillPath(bg, path);
            using var pen  = new Pen(hovered ? Color.FromArgb(165, 180, 252) : Color.FromArgb(229, 231, 235),
                                     hovered ? 2f : 1f);
            g.DrawPath(pen, path);
        };

        // Emoji label — system renderer = color
        var iconLbl = new Label
        {
            Text      = topic.Icon,
            Font      = new Font("Segoe UI Emoji", 28),
            BackColor = Color.Transparent,
            Location  = new Point(0, 10),
            Size      = new Size(W, 48),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var nameLbl = new Label
        {
            Text      = topic.Name,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            BackColor = Color.Transparent,
            Size      = new Size(W - 16, 48),
            Location  = new Point(8, 64),
            TextAlign = ContentAlignment.TopCenter
        };

        var descLbl = new Label
        {
            Text      = topic.Description,
            Font      = new Font("Segoe UI", 7),
            ForeColor = Color.FromArgb(107, 114, 128),
            BackColor = Color.Transparent,
            Size      = new Size(W - 16, 30),
            Location  = new Point(8, 130),
            TextAlign = ContentAlignment.TopCenter
        };

        card.Controls.AddRange([iconLbl, nameLbl, descLbl]);

        void HoverOn()  { hovered = true;  card.Invalidate(); }
        void HoverOff() { hovered = false; card.Invalidate(); }

        card.MouseEnter += (_, _) => HoverOn();
        card.MouseLeave += (_, _) => HoverOff();
        card.Click      += (_, _) => StartQuiz(topic);
        foreach (Control c in card.Controls)
        {
            c.MouseEnter += (_, _) => HoverOn();
            c.MouseLeave += (_, _) => HoverOff();
            c.Click      += (_, _) => StartQuiz(topic);
            c.Cursor      = Cursors.Hand;
        }

        return card;
    }

    // ═══════════════════════════════════════════════════════════════
    //  Actions
    // ═══════════════════════════════════════════════════════════════
    private void StartQuiz(Topic topic)
    {
        var questions = _gen.Generate(topic.Id, 10);
        using var quiz = new QuizForm(topic, questions);
        if (quiz.ShowDialog(this) == DialogResult.OK && quiz.Result != null)
        {
            quiz.Result.UserId   = _user.Id;
            quiz.Result.Username = _user.Username;
            _stats.Save(quiz.Result);
            using var rf = new ResultForm(quiz.Result);
            rf.ShowDialog(this);
        }
    }

    private void OpenProfile()
    {
        using var pf = new ProfileForm(_stats, _user);
        pf.ShowDialog(this);
    }

    private void ToggleTheme()
    {
        Theme.ApplyTheme(!Theme.IsDark);
        BackColor         = Theme.Background;
        _content.BackColor = Theme.Background;
        RebuildContent();
    }
}

// ── Sidebar button ────────────────────────────────────────────────
internal class SidebarBtn : Control
{
    private readonly string _icon;
    private readonly string _label;
    private bool _hov;

    public SidebarBtn(string icon, string label)
    {
        _icon  = icon;
        _label = label;
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);
    }

    protected override void OnMouseEnter(EventArgs e) { _hov = true;  Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { _hov = false; Invalidate(); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        if (_hov)
        {
            using var hBr   = new SolidBrush(Color.FromArgb(30, 255, 255, 255));
            using var hPath = ModernButton.BuildRoundedPath(new Rectangle(6, 2, Width - 12, Height - 4), 8);
            g.FillPath(hBr, hPath);
        }
        using var ef = new Font("Segoe UI Emoji", 12);
        using var eb = new SolidBrush(Color.White);
        g.DrawString(_icon, ef, eb, 14f, 11f);
        using var lf = new Font("Segoe UI", 9);
        using var lb = new SolidBrush(Color.FromArgb(210, 255, 255, 255));
        g.DrawString(_label, lf, lb, 42f, 13f);
    }
}

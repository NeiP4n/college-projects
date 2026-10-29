using MathTrainer.Helpers;
using MathTrainer.Models;
using MathTrainer.Services;

namespace MathTrainer.Forms;

public class LoginForm : Form
{
    private readonly AuthService _auth;
    private TextBox      _txtLogin   = null!;
    private TextBox      _txtPass    = null!;
    private TextBox      _txtConfirm = null!;
    private Label        _lblError   = null!;
    private Panel        _pConfirm   = null!;
    private ModernButton _btnSubmit  = null!;
    private ModernButton _btnToggle  = null!;
    private bool         _isRegister;

    public User? CurrentUser { get; private set; }

    public LoginForm(AuthService auth)
    {
        _auth = auth;
        Build();
    }

    private void Build()
    {
        Text            = "MathTrainer";
        StartPosition   = FormStartPosition.CenterScreen;
        BackColor       = Color.FromArgb(245, 246, 250);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        AutoSize        = true;
        AutoSizeMode    = AutoSizeMode.GrowAndShrink;

        // ── Gradient header (owner-drawn, no child labels) ────────────
        var header = new PaintPanel(PaintHeader) { Height = 160, Dock = DockStyle.Top };

        // ── Card (plain Panel — no UserPaint, children render fine) ──
        var card = new Panel
        {
            Location  = new Point(28, 172),
            Size      = new Size(404, 0),      // height set at end
            BackColor = Color.White
        };
        card.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var p = new Pen(Color.FromArgb(229, 231, 235));
            g.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
        };

        // ── Fields ────────────────────────────────────────────────────
        int x = 24, y = 24;

        card.Controls.Add(FieldLabel("Логин",          x, y));          y += 22;
        _txtLogin   = FieldBox(x, y, card, password: false);            y += 50;
        card.Controls.Add(FieldLabel("Пароль",         x, y));          y += 22;
        _txtPass    = FieldBox(x, y, card, password: true);             y += 50;

        // Confirm row (hidden in login mode)
        _pConfirm = new Panel { Location = new Point(x, y), Size = new Size(356, 72), BackColor = Color.White, Visible = false };
        _pConfirm.Controls.Add(FieldLabel("Подтвердите пароль", 0, 0));
        _txtConfirm = FieldBox(0, 22, _pConfirm, password: true);
        card.Controls.Add(_pConfirm);
        y += 0; // only grows when visible

        _lblError = new Label
        {
            Location  = new Point(x, y + 78),
            Size      = new Size(356, 20),
            Font      = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(220, 38, 38),
            BackColor = Color.White
        };
        card.Controls.Add(_lblError);

        _btnSubmit = new ModernButton
        {
            Text      = "Войти",
            Location  = new Point(x, y + 106),
            Size      = new Size(356, 46),
            Font      = new Font("Segoe UI", 12, FontStyle.Bold),
            NormalBg  = Color.FromArgb(99, 102, 241),
            HoverBg   = Color.FromArgb(79, 70, 229),
            PressedBg = Color.FromArgb(55, 48, 163)
        };
        _btnSubmit.Click += (_, _) => OnSubmit();
        card.Controls.Add(_btnSubmit);

        _btnToggle = new ModernButton
        {
            Text         = "Нет аккаунта? Зарегистрироваться",
            Location     = new Point(x, y + 160),
            Size         = new Size(356, 38),
            Font         = new Font("Segoe UI", 9),
            NormalBg     = Color.Transparent,
            HoverBg      = Color.FromArgb(238, 242, 255),
            PressedBg    = Color.FromArgb(224, 231, 255),
            NormalText   = Color.FromArgb(99, 102, 241),
            IsOutline    = false,
            CornerRadius = 8
        };
        _btnToggle.Click += (_, _) => Toggle();
        card.Controls.Add(_btnToggle);

        card.Height = y + 210;     // login-mode height

        // Tab order / enter key
        _txtLogin.KeyDown   += (_, e) => { if (e.KeyCode == Keys.Enter) _txtPass.Focus(); e.SuppressKeyPress = e.KeyCode == Keys.Enter; };
        _txtPass.KeyDown    += (_, e) => { if (e.KeyCode == Keys.Enter) { if (_isRegister) _txtConfirm.Focus(); else OnSubmit(); e.SuppressKeyPress = true; } };
        _txtConfirm.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { OnSubmit(); e.SuppressKeyPress = true; } };

        Controls.Add(card);
        Controls.Add(header);   // last = on top
    }

    // ── Header paint ─────────────────────────────────────────────────
    private void PaintHeader(PaintEventArgs e, Control ctrl)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var br = new System.Drawing.Drawing2D.LinearGradientBrush(
            ctrl.ClientRectangle,
            Color.FromArgb(99, 102, 241),
            Color.FromArgb(67, 56, 202),
            System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal);
        g.FillRectangle(br, ctrl.ClientRectangle);

        // Icon circle
        using var circleBr = new SolidBrush(Color.FromArgb(40, 255, 255, 255));
        g.FillEllipse(circleBr, 20, 24, 62, 62);

        // Icon
        using var iFont = new Font("Segoe UI Emoji", 26);
        g.DrawString("📐", iFont, Brushes.White, 22f, 26f);

        // Title
        using var tFont = new Font("Segoe UI", 20, FontStyle.Bold);
        g.DrawString("MathTrainer", tFont, Brushes.White, 94f, 30f);

        // Subtitle
        using var sFont = new Font("Segoe UI", 10);
        using var sBr   = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
        g.DrawString("Учись. Тренируйся. Развивайся.", sFont, sBr, 94f, 68f);

        // Mode label
        using var mFont = new Font("Segoe UI", 9, FontStyle.Bold);
        using var mBr   = new SolidBrush(Color.FromArgb(180, 255, 255, 255));
        g.DrawString(_isRegister ? "Создание аккаунта" : "Вход в систему", mFont, mBr, 20f, 130f);
    }

    // ── Helpers ───────────────────────────────────────────────────────
    private static Label FieldLabel(string text, int x, int y) => new()
    {
        Text      = text,
        Location  = new Point(x, y),
        Font      = new Font("Segoe UI", 9, FontStyle.Bold),
        ForeColor = Color.FromArgb(75, 85, 99),
        AutoSize  = true,
        BackColor = Color.White
    };

    private static TextBox FieldBox(int x, int y, Control parent, bool password)
    {
        // Wrapper panel gives a coloured border via Paint
        var wrap = new Panel
        {
            Location  = new Point(x, y),
            Size      = new Size(356, 38),
            BackColor = Color.White
        };
        bool focused = false;
        wrap.Paint += (_, e) =>
        {
            using var p = new Pen(focused ? Color.FromArgb(99, 102, 241) : Color.FromArgb(209, 213, 219),
                                  focused ? 2f : 1f);
            e.Graphics.DrawRectangle(p, 0, 0, wrap.Width - 1, wrap.Height - 1);
        };

        var tb = new TextBox
        {
            Location              = new Point(8, 7),
            Size                  = new Size(340, 22),
            BorderStyle           = BorderStyle.None,
            Font                  = new Font("Segoe UI", 11),
            UseSystemPasswordChar = password,
            BackColor             = Color.White
        };
        tb.GotFocus  += (_, _) => { focused = true;  wrap.Invalidate(); };
        tb.LostFocus += (_, _) => { focused = false; wrap.Invalidate(); };

        wrap.Controls.Add(tb);
        parent.Controls.Add(wrap);
        return tb;
    }

    // ── Toggle login / register ───────────────────────────────────────
    private void Toggle()
    {
        _isRegister = !_isRegister;
        _pConfirm.Visible = _isRegister;

        // Shift controls below confirm row
        int delta = _isRegister ? 72 : -72;
        foreach (Control c in _btnSubmit.Parent!.Controls)
        {
            if (c == _pConfirm) continue;
            if (c.Location.Y >= _pConfirm.Location.Y + 1)
                c.Location = new Point(c.Location.X, c.Location.Y + delta);
        }
        _btnSubmit.Parent.Height += delta;
        _lblError.Text = "";

        // Redraw header mode label
        Controls.OfType<PaintPanel>().FirstOrDefault()?.Invalidate();
        Text = _isRegister ? "MathTrainer — Регистрация" : "MathTrainer — Вход";

        _btnSubmit.Text = _isRegister ? "Создать аккаунт" : "Войти";
        _btnToggle.Text = _isRegister ? "Уже есть аккаунт? Войти" : "Нет аккаунта? Зарегистрироваться";

        // Resize form to fit updated card
        var card = _btnSubmit.Parent!;
        int needed = card.Bottom + 16;
        if (ClientSize.Height != needed)
            ClientSize = new Size(ClientSize.Width, needed);
    }

    // ── Submit ────────────────────────────────────────────────────────
    private void OnSubmit()
    {
        _lblError.Text = "";
        var login = _txtLogin.Text.Trim();
        var pass  = _txtPass.Text;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(pass))
        { _lblError.Text = "⚠  Заполните все поля"; return; }

        if (_isRegister)
        {
            if (pass != _txtConfirm.Text) { _lblError.Text = "⚠  Пароли не совпадают"; return; }
            var (ok, msg, user) = _auth.Register(login, pass);
            if (!ok) { _lblError.Text = $"⚠  {msg}"; return; }
            CurrentUser = user;
        }
        else
        {
            var (ok, msg, user) = _auth.Login(login, pass);
            if (!ok) { _lblError.Text = $"⚠  {msg}"; return; }
            CurrentUser = user;
        }
        DialogResult = DialogResult.OK;
        Close();
    }
}


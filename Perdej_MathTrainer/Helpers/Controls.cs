namespace MathTrainer.Helpers;

public class ModernButton : Button
{
    private bool _hovered;
    private bool _pressed;

    [System.ComponentModel.DesignerSerializationVisibility(
        System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Color NormalBg  { get; set; } = Theme.Primary;

    [System.ComponentModel.DesignerSerializationVisibility(
        System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Color HoverBg   { get; set; } = Theme.PrimaryDark;

    [System.ComponentModel.DesignerSerializationVisibility(
        System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Color PressedBg { get; set; } = Color.FromArgb(55, 48, 163);

    [System.ComponentModel.DesignerSerializationVisibility(
        System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Color NormalText { get; set; } = Color.White;

    [System.ComponentModel.DesignerSerializationVisibility(
        System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int CornerRadius { get; set; } = 10;

    [System.ComponentModel.DesignerSerializationVisibility(
        System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool IsOutline { get; set; }

    public ModernButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Font   = new Font("Segoe UI", 10, FontStyle.Bold);
        Cursor = Cursors.Hand;
        BackColor = Color.Transparent;
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint |
                 ControlStyles.SupportsTransparentBackColor, true);
    }

    protected override void OnMouseEnter(EventArgs e) { _hovered = true;  Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { _hovered = false; _pressed = false; Invalidate(); }
    protected override void OnMouseDown(MouseEventArgs e) { _pressed = true;  Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp  (MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e);   }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Color.White);

        var bg   = _pressed ? PressedBg : _hovered ? HoverBg : NormalBg;
        var rect = new Rectangle(0, 0, Width, Height);

        using var path = BuildRoundedPath(rect, CornerRadius);

        if (IsOutline)
        {
            using var fillBrush = new SolidBrush(_hovered || _pressed ? bg : NormalBg);
            g.FillPath(fillBrush, path);
            using var pen = new Pen(NormalText, 2);
            g.DrawPath(pen, path);
        }
        else
        {
            using var fillBrush = new SolidBrush(bg);
            g.FillPath(fillBrush, path);
        }

        var textRect = new Rectangle(rect.X + 4, rect.Y, rect.Width - 8, rect.Height);
        using var sf = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };

        switch (TextAlign)
        {
            case ContentAlignment.MiddleLeft:
                sf.Alignment = StringAlignment.Near;
                textRect = new Rectangle(rect.X + 12, rect.Y, rect.Width - 16, rect.Height);
                break;
            case ContentAlignment.MiddleRight:
                sf.Alignment = StringAlignment.Far;
                textRect = new Rectangle(rect.X, rect.Y, rect.Width - 12, rect.Height);
                break;
            default:
                sf.Alignment = StringAlignment.Center;
                break;
        }

        using var tb = new SolidBrush(NormalText);
        g.DrawString(Text, Font, tb, textRect, sf);
    }

    internal static System.Drawing.Drawing2D.GraphicsPath BuildRoundedPath(Rectangle r, int radius)
    {
        var p = new System.Drawing.Drawing2D.GraphicsPath();
        if (radius <= 0) { p.AddRectangle(r); return p; }
        int d = radius * 2;
        p.AddArc(r.X,           r.Y,            d, d, 180, 90);
        p.AddArc(r.Right - d,   r.Y,            d, d, 270, 90);
        p.AddArc(r.Right - d,   r.Bottom - d,   d, d,   0, 90);
        p.AddArc(r.X,           r.Bottom - d,   d, d,  90, 90);
        p.CloseFigure();
        return p;
    }
}

public class ModernCard : Panel
{
    [System.ComponentModel.DesignerSerializationVisibility(
        System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int CornerRadius { get; set; } = 14;

    public ModernCard()
    {
        BackColor = Theme.CardBg;
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g    = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var rect = new Rectangle(1, 1, Width - 2, Height - 2);

        using var path = ModernButton.BuildRoundedPath(rect, CornerRadius);
        using var bg   = new SolidBrush(BackColor);
        g.FillPath(bg, path);

        using var pen = new Pen(Color.FromArgb(30, Theme.Border));
        g.DrawPath(pen, path);
    }
}

public class ModernProgressBar : Control
{
    private int   _value;
    private int   _maximum = 100;
    private Color _barColor = Color.Empty;

    public int Maximum
    {
        get => _maximum;
        set { _maximum = value; Invalidate(); }
    }

    public int Value
    {
        get => _value;
        set { _value = Math.Min(value, _maximum); Invalidate(); }
    }

    [System.ComponentModel.DesignerSerializationVisibility(
        System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Color BarColor
    {
        get => _barColor == Color.Empty ? Theme.Primary : _barColor;
        set { _barColor = value; Invalidate(); }
    }

    public int CornerRadius { get; set; } = 4;

    public ModernProgressBar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);
        Height = 8;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var bg = new Rectangle(0, 0, Width, Height);

        using var bgBrush = new SolidBrush(Color.FromArgb(229, 231, 235));
        using var bgPath  = ModernButton.BuildRoundedPath(bg, CornerRadius);
        g.FillPath(bgBrush, bgPath);

        if (_maximum > 0 && _value > 0)
        {
            int fillW = Math.Max(CornerRadius * 2, (int)((double)_value / _maximum * Width));
            var fg = new Rectangle(0, 0, fillW, Height);
            using var fgPath  = ModernButton.BuildRoundedPath(fg, CornerRadius);
            using var fgBrush = new SolidBrush(BarColor);
            g.FillPath(fgBrush, fgPath);
        }
    }
}

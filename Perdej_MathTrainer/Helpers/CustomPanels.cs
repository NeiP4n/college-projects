namespace MathTrainer.Helpers;

public class DoubleBufferedPanel : Panel
{
    public DoubleBufferedPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);
    }
}

internal class PaintPanel : Panel
{
    private readonly Action<PaintEventArgs, Control> _paint;
    public PaintPanel(Action<PaintEventArgs, Control> paint)
    {
        _paint         = paint;
        DoubleBuffered = true;
    }
    protected override void OnPaint(PaintEventArgs e) => _paint(e, this);
}

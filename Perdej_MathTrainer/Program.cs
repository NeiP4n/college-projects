using MathTrainer.Forms;
using MathTrainer.Services;

namespace MathTrainer;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.SetHighDpiMode(HighDpiMode.SystemAware);

        Application.Run(new WebAppForm(new AuthService(), new StatsService(), new QuestionGen()));
    }
}

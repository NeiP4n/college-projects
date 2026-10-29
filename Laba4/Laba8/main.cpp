#include "MyForm8.h"

using namespace System;
using namespace System::Windows::Forms;
using namespace Laba8;

[STAThread]
int main()
{
	Application::EnableVisualStyles();
	Application::SetCompatibleTextRenderingDefault(false);
	Application::Run(gcnew MyForm());
	return 0;
}

#include "MyForm.h"//эта строка у вас уже может быть в файле

using namespace System;
using namespace System::Windows::Forms;

[STAThread]
void main(array<String^>^ arg) {
    Application::EnableVisualStyles();
    Application::SetCompatibleTextRenderingDefault(false);

    Laba4::MyForm form; //Laba4 - имя вашего проекта
    Application::Run(% form);
}

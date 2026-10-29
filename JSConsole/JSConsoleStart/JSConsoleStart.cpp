#include <windows.h>
#include <iostream> 

int main()
{
    setlocale(LC_ALL, "Rus");
    STARTUPINFO si;
    PROCESS_INFORMATION pi;
    ZeroMemory(&si, sizeof(si));
    si.cb = sizeof(si);
    ZeroMemory(&pi, sizeof(pi));

    if (!CreateProcess(
        L"D:\\Учеба\\VsProjects\\JSConsole\\JSConsoleStart\\JSConsole.exe", NULL, NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi))
    {
        printf("CreateProcess failed (%d).\n", GetLastError());
        return 1;
    }
    return 0;
}

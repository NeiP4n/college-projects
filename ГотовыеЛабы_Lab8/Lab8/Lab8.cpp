#include <windows.h>

LRESULT CALLBACK WndProc(HWND, UINT, WPARAM, LPARAM);

HWND hWnd;
BOOL isStuck = FALSE;
POINT lastCursorPos = { 0 };

int APIENTRY WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance,
    LPSTR lpCmdLine, int nCmdShow)
{
    WNDCLASS wc = { 0 };
    wc.lpfnWndProc = WndProc;
    wc.hInstance = hInstance;
    wc.lpszClassName = TEXT("Window");
    wc.hCursor = LoadCursor(NULL, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
    wc.style = CS_HREDRAW | CS_VREDRAW | CS_DBLCLKS;

    RegisterClass(&wc);

    hWnd = CreateWindow(wc.lpszClassName, TEXT("Вариант 10 — Прилипание окна"),
        WS_OVERLAPPEDWINDOW | WS_VISIBLE | WS_HSCROLL | WS_VSCROLL,
        CW_USEDEFAULT, CW_USEDEFAULT, 500, 400,
        NULL, NULL, hInstance, NULL);

    MSG msg;
    while (GetMessage(&msg, NULL, 0, 0))
    {
        TranslateMessage(&msg);
        DispatchMessage(&msg);

        if (isStuck)
        {
            POINT p;
            GetCursorPos(&p);
            int dx = p.x - lastCursorPos.x;
            int dy = p.y - lastCursorPos.y;

            RECT r;
            GetWindowRect(hWnd, &r);
            MoveWindow(hWnd, r.left + dx, r.top + dy,
                r.right - r.left, r.bottom - r.top, TRUE);

            lastCursorPos = p;
        }
    }

    return (int)msg.wParam;
}

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch (msg)
    {
    case WM_NCHITTEST:
    {
        LRESULT hit = DefWindowProc(hwnd, msg, wParam, lParam);

        if ((hit == HTVSCROLL || hit == HTHSCROLL) && (GetAsyncKeyState(VK_LBUTTON) & 0x8000))
        {
            if (!isStuck)
            {
                isStuck = TRUE;
                GetCursorPos(&lastCursorPos);
                SetCapture(hwnd);
            }
        }

        return hit;
    }

    case WM_LBUTTONDOWN:
    case WM_NCLBUTTONDOWN:
    {
        if (isStuck)
        {
            isStuck = FALSE;
            ReleaseCapture();
        }
        break;
    }

    case WM_LBUTTONUP:
    {
        break;
    }
    DestroyWindow(hWnd); return 0;
    case WM_NCLBUTTONDBLCLK:
        if (wParam == HTCLOSE) PostQuitMessage(0);

    default:
        return DefWindowProc(hwnd, msg, wParam, lParam);
    }

    return 0;
}

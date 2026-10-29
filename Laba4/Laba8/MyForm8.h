#pragma once
#include <windows.h>

// Подключаем необходимые пространства имен
using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;

// Пространство имен Laba8
namespace Laba8 {

    // Объявление класса формы
    public ref class MyForm : public System::Windows::Forms::Form
    {
    public:
        // Конструктор
        MyForm(void)
        {
            InitializeComponent();
            this->AutoScroll = true; // Включаем полосы прокрутки
            this->ClientSize = System::Drawing::Size(400, 300);
            this->Text = L"Lab 8 Variant 10";
            isSticky = false;
            titleClickCount = 0;
            // Устанавливаем размер контента, чтобы полосы прокрутки были видны
            this->AutoScrollMinSize = System::Drawing::Size(500, 500);
        }

    protected:
        // Деструктор
        ~MyForm()
        {
            if (components)
            {
                delete components;
            }
        }

        // Переопределяем WndProc для обработки неклиентских событий
        virtual void WndProc(Message% m) override
        {
            // Вызываем базовую реализацию
            Form::WndProc(m);

            // Обрабатываем щелчок левой кнопкой мыши в неклиентской области
            if (m.Msg == WM_NCLBUTTONDOWN)
            {
                // Проверяем, что щелчок был по заголовку
                if (m.WParam.ToInt32() == HTCAPTION)
                {
                    titleClickCount++;
                    if (titleClickCount >= 3 && isSticky)
                    {
                        isSticky = false; // Отключаем прилипание
                        titleClickCount = 0; // Сбрасываем счетчик
                    }
                }
            }
        }

    private:
        System::ComponentModel::Container^ components;
        bool isSticky; // Флаг прилипания
        int titleClickCount; // Счетчик щелчков по заголовку

        // Инициализация компонентов формы
#pragma region Windows Form Designer generated code
        void InitializeComponent(void)
        {
            this->components = gcnew System::ComponentModel::Container();
            this->Size = System::Drawing::Size(400, 300);
            this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
            this->Text = L"Lab8Var10";
            // Подписываемся на событие щелчка мыши
            this->MouseDown += gcnew System::Windows::Forms::MouseEventHandler(this, &MyForm::MyForm_MouseDown);
        }
#pragma endregion

        // Обработчик щелчка мыши по форме
        void MyForm_MouseDown(System::Object^ sender, System::Windows::Forms::MouseEventArgs^ e)
        {
            // Проверяем, щелкнули ли по полосе прокрутки
            if (IsOverScrollBar(e->Location))
            {
                if (e->Button == System::Windows::Forms::MouseButtons::Left)
                {
                    isSticky = true; // Активируем прилипание
                    StartStickyMovement();
                }
            }
        }

        // Проверка, находится ли точка над полосой прокрутки
        bool IsOverScrollBar(System::Drawing::Point point)
        {
            // Получаем размеры клиентской области и полос прокрутки
            int vScrollWidth = SystemInformation::VerticalScrollBarWidth;
            int hScrollHeight = SystemInformation::HorizontalScrollBarHeight;
            int clientWidth = this->ClientSize.Width;
            int clientHeight = this->ClientSize.Height;

            // Проверяем вертикальную полосу прокрутки
            bool overVScroll = (point.X >= clientWidth - vScrollWidth && point.X <= clientWidth && point.Y >= 0 && point.Y <= clientHeight);
            // Проверяем горизонтальную полосу прокрутки
            bool overHScroll = (point.Y >= clientHeight - hScrollHeight && point.Y <= clientHeight && point.X >= 0 && point.X <= clientWidth);

            return overVScroll || overHScroll;
        }

        // Метод для перемещения окна за курсором
        void StartStickyMovement()
        {
            while (isSticky)
            {
                // Получаем позицию курсора
                POINT p;
                GetCursorPos(&p);
                // Перемещаем окно
                this->Location = System::Drawing::Point(p.x - this->Width / 2, p.y - this->Height / 2);
                // Обрабатываем события
                System::Windows::Forms::Application::DoEvents();
                // Небольшая задержка
                System::Threading::Thread::Sleep(10);
            }
        }

        // Константы для сообщений Windows
        static const int WM_NCLBUTTONDOWN = 0x00A1;
        static const int HTCAPTION = 2;
    };
}
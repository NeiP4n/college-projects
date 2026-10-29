#pragma once

using namespace System;
using namespace System::Drawing;
using namespace System::Windows::Forms;

namespace MyForm8 {

	public ref class Form8 : public Form
	{
	public:
		Form8()
		{
			InitializeComponent();
		}

	private:
		bool isStuck = false;
		Point offset;
		int titleClicks = 0;
		Timer^ clickTimer;

		void InitializeComponent(void)
		{
			this->Text = L"Вариант 10";
			this->Width = 600;
			this->Height = 400;
			this->AutoScroll = true;
			this->AutoScrollMinSize = Drawing::Size(2000, 2000);

			// События
			this->MouseDown += gcnew MouseEventHandler(this, &Form8::OnMouseDown);
			this->MouseMove += gcnew MouseEventHandler(this, &Form8::OnMouseMove);
			this->MouseClick += gcnew MouseEventHandler(this, &Form8::OnMouseClick);
			this->MouseDoubleClick += gcnew MouseEventHandler(this, &Form8::OnMouseDoubleClick);

			// Таймер для отслеживания тройного щелчка
			clickTimer = gcnew Timer();
			clickTimer->Interval = 600; // 600 мс на тройной клик
			clickTimer->Tick += gcnew EventHandler(this, &Form8::ResetClicks);
		}

		void OnMouseDown(Object^ sender, MouseEventArgs^ e)
		{
			if (IsOnScrollBar(e->Location))
			{
				isStuck = true;
				Point screenPoint = PointToScreen(e->Location);
				offset = Point(screenPoint.X - this->Location.X, screenPoint.Y - this->Location.Y);
			}
		}


		void OnMouseMove(Object^ sender, MouseEventArgs^ e)
		{
			if (isStuck)
			{
				Point cursorPos = Cursor->Position;
				this->Location = Point(cursorPos.X - offset.X, cursorPos.Y - offset.Y);
			}
		}

		void OnMouseClick(Object^ sender, MouseEventArgs^ e)
		{
			if (e->Y < SystemInformation::CaptionHeight)
			{
				titleClicks++;
				if (titleClicks == 3)
				{
					isStuck = false;
					titleClicks = 0;
				}
				clickTimer->Stop();
				clickTimer->Start();
			}
		}

		void OnMouseDoubleClick(Object^ sender, MouseEventArgs^ e)
		{
			clickTimer->Stop(); // чтобы таймер не сбрасывал между двойным и тройным кликом
		}

		void ResetClicks(Object^ sender, EventArgs^ e)
		{
			titleClicks = 0;
			clickTimer->Stop();
		}

		bool IsOnScrollBar(Point p)
		{
			return (p.X >= this->ClientSize.Width - SystemInformation::VerticalScrollBarWidth ||
				p.Y >= this->ClientSize.Height - SystemInformation::HorizontalScrollBarHeight);
		}
	};

}

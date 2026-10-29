#include <thread>
#include <chrono>
#include <windows.h>

#pragma once

namespace Laba4 {

	using namespace System;
	using namespace System::Threading;
	using namespace System::ComponentModel;
	using namespace System::Collections;
	using namespace System::Windows::Forms;
	using namespace System::Data;
	using namespace System::Drawing;

	typedef struct _CONTEXT {
		DWORD ContextFlags;
		DWORD Dr0; DWORD Dr1;
		DWORD Dr2; DWORD Dr3;
		DWORD Dr6; DWORD Dr7;
		DWORD SegGs; DWORD SegFs;
		DWORD SegEs; DWORD SegDs;
		DWORD Edi; DWORD Esi;
		DWORD Ebx; DWORD Edx;
		DWORD Ecx; DWORD Eax;
		DWORD Ebp; DWORD EFlags;
		DWORD Esp; DWORD SegSs;
	} CONTEXT;


	public ref class MyForm : public System::Windows::Forms::Form
	{
	public:
		MyForm(void)
		{
			InitializeComponent();
		}

	protected:
		~MyForm()
		{
			if (components)
			{
				delete components;
			}
		}

	private: System::Windows::Forms::Button^ Create;
	private: System::Windows::Forms::ProgressBar^ progressBar1;
	private: System::Windows::Forms::ProgressBar^ progressBar2;
	private: System::Windows::Forms::ProgressBar^ progressBar3;
	private: System::Windows::Forms::Label^ label1;
	private: System::Windows::Forms::Label^ label2;
	private: System::Windows::Forms::Label^ label3;
	private: System::Windows::Forms::Button^ Start;
	private: System::Windows::Forms::Button^ Stop;
	private: System::Windows::Forms::Button^ Terminate;
	private: System::Windows::Forms::TextBox^ ContextBox;
	protected:

	private:
		System::ComponentModel::Container^ components;

#pragma region Windows Form Designer generated code
		void InitializeComponent(void)
		{
			this->Create = (gcnew System::Windows::Forms::Button());
			this->progressBar1 = (gcnew System::Windows::Forms::ProgressBar());
			this->progressBar2 = (gcnew System::Windows::Forms::ProgressBar());
			this->progressBar3 = (gcnew System::Windows::Forms::ProgressBar());
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->label2 = (gcnew System::Windows::Forms::Label());
			this->label3 = (gcnew System::Windows::Forms::Label());
			this->Start = (gcnew System::Windows::Forms::Button());
			this->Stop = (gcnew System::Windows::Forms::Button());
			this->Terminate = (gcnew System::Windows::Forms::Button());
			this->ContextBox = (gcnew System::Windows::Forms::TextBox());
			this->SuspendLayout();
			// 
			// Create
			// 
			this->Create->Location = System::Drawing::Point(482, 32);
			this->Create->Name = L"Create";
			this->Create->Size = System::Drawing::Size(179, 60);
			this->Create->TabIndex = 0;
			this->Create->Text = L"Создать";
			this->Create->UseVisualStyleBackColor = true;
			this->Create->Click += gcnew System::EventHandler(this, &MyForm::Create_Click);
			// 
			// progressBar1
			// 
			this->progressBar1->Location = System::Drawing::Point(12, 32);
			this->progressBar1->Name = L"progressBar1";
			this->progressBar1->Size = System::Drawing::Size(317, 40);
			this->progressBar1->TabIndex = 1;
			// 
			// progressBar2
			// 
			this->progressBar2->Location = System::Drawing::Point(12, 104);
			this->progressBar2->Name = L"progressBar2";
			this->progressBar2->Size = System::Drawing::Size(317, 40);
			this->progressBar2->TabIndex = 2;
			// 
			// progressBar3
			// 
			this->progressBar3->Location = System::Drawing::Point(12, 176);
			this->progressBar3->Name = L"progressBar3";
			this->progressBar3->Size = System::Drawing::Size(317, 40);
			this->progressBar3->TabIndex = 3;
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(345, 44);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(57, 16);
			this->label1->TabIndex = 4;
			this->label1->Text = L"Поток 1";
			// 
			// label2
			// 
			this->label2->AutoSize = true;
			this->label2->Location = System::Drawing::Point(345, 113);
			this->label2->Name = L"label2";
			this->label2->Size = System::Drawing::Size(57, 16);
			this->label2->TabIndex = 5;
			this->label2->Text = L"Поток 2";
			// 
			// label3
			// 
			this->label3->AutoSize = true;
			this->label3->Location = System::Drawing::Point(345, 190);
			this->label3->Name = L"label3";
			this->label3->Size = System::Drawing::Size(57, 16);
			this->label3->TabIndex = 6;
			this->label3->Text = L"Поток 3";
			// 
			// Start
			// 
			this->Start->Location = System::Drawing::Point(482, 118);
			this->Start->Name = L"Start";
			this->Start->Size = System::Drawing::Size(179, 56);
			this->Start->TabIndex = 7;
			this->Start->Text = L"Старт";
			this->Start->UseVisualStyleBackColor = true;
			this->Start->Click += gcnew System::EventHandler(this, &MyForm::Start_Click);
			// 
			// Stop
			// 
			this->Stop->Location = System::Drawing::Point(482, 209);
			this->Stop->Name = L"Stop";
			this->Stop->Size = System::Drawing::Size(179, 52);
			this->Stop->TabIndex = 8;
			this->Stop->Text = L"Отключить";
			this->Stop->UseVisualStyleBackColor = true;
			this->Stop->Click += gcnew System::EventHandler(this, &MyForm::Stop_Click);
			// 
			// Terminate
			// 
			this->Terminate->Location = System::Drawing::Point(482, 294);
			this->Terminate->Name = L"Terminate";
			this->Terminate->Size = System::Drawing::Size(179, 55);
			this->Terminate->TabIndex = 9;
			this->Terminate->Text = L"Принудительно отключить";
			this->Terminate->UseVisualStyleBackColor = true;
			this->Terminate->Click += gcnew System::EventHandler(this, &MyForm::Terminate_Click);
			// 
			// ContextBox
			// 
			this->ContextBox->Location = System::Drawing::Point(675, 19);
			this->ContextBox->Multiline = true;
			this->ContextBox->Name = L"ContextBox";
			this->ContextBox->Size = System::Drawing::Size(353, 355);
			this->ContextBox->TabIndex = 10;
			// 
			// MyForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(8, 16);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(1040, 386);
			this->Controls->Add(this->ContextBox);
			this->Controls->Add(this->Terminate);
			this->Controls->Add(this->Stop);
			this->Controls->Add(this->Start);
			this->Controls->Add(this->label3);
			this->Controls->Add(this->label2);
			this->Controls->Add(this->label1);
			this->Controls->Add(this->progressBar3);
			this->Controls->Add(this->progressBar2);
			this->Controls->Add(this->progressBar1);
			this->Controls->Add(this->Create);
			this->Name = L"MyForm";
			this->Text = L"MyForm";
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion

private: System::Threading::Thread^ taskThread1;
private: System::Threading::Thread^ taskThread2;
private: System::Threading::Thread^ taskThread3;
private: bool running = false;
private: bool stopRequested = false;

private: System::Void Create_Click(System::Object^ sender, System::EventArgs^ e) {
	progressBar1->Value = 0;
	progressBar2->Value = 0;
	progressBar3->Value = 0;

	taskThread1 = gcnew Thread(gcnew ThreadStart(this, &MyForm::RunTask));
	taskThread2 = gcnew Thread(gcnew ThreadStart(this, &MyForm::RunTask2));
	taskThread3 = gcnew Thread(gcnew ThreadStart(this, &MyForm::RunTask3));
}

private: System::Void Start_Click(System::Object^ sender, System::EventArgs^ e) {
	if (!running) {
		running = true;
		stopRequested = false;
		taskThread1->Start();
		taskThread2->Start();
		taskThread3->Start();
	}
}

private: System::Void Stop_Click(System::Object^ sender, System::EventArgs^ e) {
	stopRequested = true;
}

private: System::Void Terminate_Click(System::Object^ sender, System::EventArgs^ e) {
	if (taskThread1 && taskThread1->IsAlive) taskThread1->Abort();
	if (taskThread2 && taskThread2->IsAlive) taskThread2->Abort();
	if (taskThread3 && taskThread3->IsAlive) taskThread3->Abort();
	running = false;
}

private: void RunTask() {
	RunLongTask(progressBar1, label1, 6409);
}

private: void RunTask2() {
	RunLongTask(progressBar2, label2, 1289);
}

private: void RunTask3() {
	RunLongTask(progressBar3, label3, 3000);
}

private: void UpdateContextBox(TextBox^ contextBox, String^ text) {
	if (contextBox->InvokeRequired) {
		contextBox->Invoke(gcnew Action<TextBox^, String^>(this, &MyForm::UpdateContextBox), contextBox, text);
	}
	else {
		contextBox->AppendText(text + Environment::NewLine);
	}
}

private: void RunLongTask(ProgressBar^ progressBar, Label^ label, int sleep) {
	System::Threading::Thread^ currentThread = System::Threading::Thread::CurrentThread;
	int threadId = currentThread->ManagedThreadId;
	PrintContextInfo(currentThread);
	UpdateContextBox(ContextBox, "Поток запущен: " + threadId.ToString());

	for (int i = 0; i <= 100 && !stopRequested; i++) {
		Thread::Sleep(sleep);
		UpdateProgressBar(progressBar, i);
		UpdateLabel(label, "Поток: " + (i + 1).ToString() + "%");
	}

	running = false;
}
private: void PrintContextInfo(System::Threading::Thread^ currentThread) {
	CONTEXT context; 
	UpdateContextBox(ContextBox, "Context Flags: " + context.ContextFlags.ToString());
	UpdateContextBox(ContextBox, "Dr0: " + context.Dr0.ToString() + ", Dr1: " + context.Dr1.ToString() +
		", Dr2: " + context.Dr2.ToString() + ", Dr3: " + context.Dr3.ToString());
	UpdateContextBox(ContextBox, "Edi: " + context.Edi.ToString() + ", Esi: " + context.Esi.ToString() +
		", Ebx: " + context.Ebx.ToString() + ", Edx: " + context.Edx.ToString());
	UpdateContextBox(ContextBox, "Ecx: " + context.Ecx.ToString() + ", Eax: " + context.Eax.ToString() +
		", Ebp: " + context.Ebp.ToString() + ", EFlags: " + context.EFlags.ToString());
	UpdateContextBox(ContextBox, "Esp: " + context.Esp.ToString() + ", SegSs: " + context.SegSs.ToString());
}



private: void UpdateProgressBar(ProgressBar^ progressBar, int value) {
	if (progressBar->InvokeRequired) {
		progressBar->Invoke(gcnew Action<ProgressBar^, int>(this, &MyForm::UpdateProgressBar), progressBar, value);
	}
	else {
		progressBar->Value = value;
	}
}

private: void UpdateLabel(Label^ label, String^ text) {
	if (label->InvokeRequired) {
		label->Invoke(gcnew Action<Label^, String^>(this, &MyForm::UpdateLabel), label, text);
	}
	else {
		label->Text = text;
	}
}

};
}

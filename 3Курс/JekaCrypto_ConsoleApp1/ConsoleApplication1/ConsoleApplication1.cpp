#include <iostream>
#include <string>
#include <locale>
#include <windows.h>
using namespace std;

wstring Cesar(const wstring& text, int shift) {
    wstring result = L"";
    const int size_rus = 32; 
    const int size_eng = 26; 

    int shift_rus = shift % size_rus;
    int shift_eng = shift % size_eng;
    
    for (wchar_t c : text) {
        if (c >= L'А' && c <= L'Я') {
            wchar_t base = L'А';
            result += (c - base + shift_rus + size_rus) % size_rus + base;
        }
        else if (c >= L'а' && c <= L'я') {
            wchar_t base = L'а';
            result += (c - base + shift_rus + size_rus) % size_rus + base;
        }
        else if (c >= L'A' && c <= L'Z') {
            wchar_t base = L'A';
            result += (c - base + shift_eng + size_eng) % size_eng + base;
        }
        else if (c >= L'a' && c <= L'z') {
            wchar_t base = L'a';
            result += (c - base + shift_eng + size_eng) % size_eng + base;
        }
        else {
            result += c;
        }
    }

    return result;
}


int main() {
    setlocale(LC_ALL, "RUS");
    SetConsoleOutputCP(1251);
    SetConsoleCP(1251);

    int shift;
    wstring text;
    wstring search;
    int choice;


    wcout << L"Выберите способ:  \n";
    wcout << L"1) Ввести ключ \n";
    wcout << L"2) С циклом \n";
    wcout << L"Ввести: ";
    cin >> choice;
    wcin.ignore();

    if (choice == 1) {
        wcout << L"Введите текст: ";
        getline(wcin, text);

        wcout << L"Введите сдвиг: ";
        wcin >> shift;

        wcout << L"Зашифрованный текст: " << Cesar(text, shift) << endl;
        wcout << L"Дешифрованный текст: " << Cesar(text, -shift) << endl;
    }
    else if (choice == 2) {
        wcout << L"Введите текст: ";
        getline(wcin, text);

        wcout << L"Введите количество сдвигов: ";
        wcin >> shift;

        for (int i = 0; i <= shift; i++) {
            wcout << i << L") Зашифрованный текст: " << Cesar(text, i) << endl;
            wcout << -i << L") Дешифрованный текст: " << Cesar(text, -i) << endl;
            wcout << endl;
        }

        wcout << L"Введите текст для поиска: ";
        wcin.ignore(); 
        getline(wcin, search);

        bool found = false;
        for (int i = 0; i <= shift; i++) {
            wstring texts = Cesar(text, i);
            if (texts == search) {
                wcout << L"Дешифрованный текст \"" << search << L"\" найден при сдвиге " << -i << L".\n";
                found = true;
                break;
            }
        }
        return 0;
    }
}

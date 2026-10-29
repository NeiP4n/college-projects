#include <iostream>
#include <fstream>
#include <ctime>
#include <Windows.h>
using namespace std;

	int main() {
		SetConsoleOutputCP(1251);
		int s = rand() % 21 + 10;

		int choice;
		cout << "Выберите способ создания последовательности:" << endl;
		cout << "1. Сгенерировать случайную последовательность" << endl;
		cout << "2. Ввести последовательность вручную" << endl;
		cout << "3. Из файла" << endl;

		cin >> choice;
		if (choice == 1) {
			int n = rand() % 21 + 10;
			cout << "Длина последовательности: " << s << endl;
			ofstream outfile("Input.txt");
			if (!outfile.is_open()) {
				cout << "Не удалось открыть файл для записи!" << endl;
				return 1;
			}
			outfile << n << endl;

			for (int i = 0; i < n; i++) {
				int randomNumber = rand() % 100;
				outfile << randomNumber << endl;
			}
			outfile.close();
			cout << "Случайная последовательность успешно записана в файл Input.txt" << endl;
		}
		else if (choice == 2) {
			int n = rand() % 21 + 10;
			cout << "Длина последовательности: " << s << endl;
			ofstream outfile("Input.txt");
			if (!outfile.is_open()) {
				cout << "Не удалось открыть файл для записи!" << endl;
				return 1;
			}
			outfile << n << endl;

			cout << "Введите " << n << " целых чисел:" << endl;
			for (int i = 0; i < n; i++) {
				int number;
				cin >> number;
				outfile << number << endl;
			}
			outfile.close();
			cout << "Последновательность успешно записана в файл Input.txt." << endl;
		}
		else if (choice == 3) {
			cout << "Длина последовательности: " << s << endl;
			int n = rand() % 21 + 10;
			ifstream inFile("Input.txt");
			if (!inFile.is_open()) {
				cout << "Не удалось открыть файл для чтения!" << endl;
				return 1;
			}
			inFile >> n;

			for (int i = 0; i < n; i++) {
				cout << n << endl;
			}
			cout << "Последовательность успешно выведена в консоль" << endl;
		}
		else {
			cout << "Ничего не выбрано" << endl;
		}
		return 0;
	}
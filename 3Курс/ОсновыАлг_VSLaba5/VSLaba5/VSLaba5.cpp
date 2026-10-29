#include <iostream>
#include <fstream>
#include <cstdlib>
#include <ctime>
#include <string>
#include <limits>
#include <sstream>
#include <cmath>
#include <locale>
#include <random>

using namespace std;

void createInputFile(const string& filename, int n, float min, float max) {
    ofstream outFile(filename, ios::binary);
    if (!outFile) {
        wcerr << L"Ошибка открытия файла для записи!" << endl;
        return;
    }

    std::mt19937 gen;
    gen.seed(time(0));

    std::uniform_real_distribution<float> dist(min, max);

    for (int i = 0; i < n; ++i) {
        float num = dist(gen);
        outFile.write(reinterpret_cast<char*>(&num), sizeof(num));  
    }

    outFile.close();
    wcout << L"Файл " << filename.c_str() << L" успешно создан." << endl;
}

void printBinaryFile(const string& inputFilename, const string& outputFilename) {
    ifstream inFile(inputFilename, ios::binary);
    ofstream outFile(outputFilename);

    if (!inFile) {
        wcerr << L"Ошибка открытия файла для чтения!" << endl;
        return;
    }

    float num;
    wcout << L"Содержимое файла " << inputFilename.c_str() << L": ";
    while (inFile.read(reinterpret_cast<char*>(&num), sizeof(num))) {
        wcout << num << L" ";
        outFile << num << " ";
    }
    wcout << endl;

    inFile.close();
    outFile.close();
}

void createOutputFile(const string& inputFilename, const string& outputFilename, float limit) {
    ifstream inFile(inputFilename, ios::binary);
    ofstream outFile(outputFilename, ios::binary);

    if (!inFile || !outFile) {
        wcerr << L"Ошибка открытия файла!" << endl;
        return;
    }

    // Определение минимального и максимального значений в input.bin
    float num, minValue = numeric_limits<float>::max(), maxValue = numeric_limits<float>::lowest();
    while (inFile.read(reinterpret_cast<char*>(&num), sizeof(num))) {
        if (num < minValue) minValue = num;
        if (num > maxValue) maxValue = num;
    }

    // Вычисление разности максимального и минимального значений
    float diff = maxValue - minValue;

    // Перемещение потока в начало файла для второй проходки
    inFile.clear();  // Сброс состояния потока
    inFile.seekg(0, ios::beg);

    // Обработка и запись данных в output.bin
    while (inFile.read(reinterpret_cast<char*>(&num), sizeof(num))) {
        if (fabs(num) <= limit) {
            num = diff;
        }
        outFile.write(reinterpret_cast<char*>(&num), sizeof(num));
    }

    inFile.close();
    outFile.close();
    wcout << L"Файл " << outputFilename.c_str() << L" успешно создан с учетом условия варианта 14." << endl;
}

void appendToInputFile(const string& inputFilename, const string& outputFilename) {
    ifstream outputFile(outputFilename, ios::binary);
    ofstream appendedFile(inputFilename, ios::binary | ios::app);

    if (!outputFile || !appendedFile) {
        wcerr << L"Ошибка открытия файла!" << endl;
        return;
    }

    float num;
    while (outputFile.read(reinterpret_cast<char*>(&num), sizeof(num))) {
        appendedFile.write(reinterpret_cast<char*>(&num), sizeof(num));
    }

    outputFile.close();
    appendedFile.close();
    wcout << L"Содержимое " << outputFilename.c_str() << L" добавлено в " << inputFilename.c_str() << L"." << endl;
}

void overwriteInputFile(const string& filename, float limit) {
    ifstream inputFile(filename, ios::binary);
    ofstream tempFile("temp.bin", ios::binary);

    if (!inputFile || !tempFile) {
        wcerr << L"Ошибка открытия файла!" << endl;
        return;
    }

    float num;
    while (inputFile.read(reinterpret_cast<char*>(&num), sizeof(num))) {
        if (abs(num) >= limit) {
            tempFile.write(reinterpret_cast<char*>(&num), sizeof(num));
        }
    }

    inputFile.close();
    tempFile.close();

    remove(filename.c_str());
    rename("temp.bin", filename.c_str());

    wcout << L"Файл " << filename.c_str() << L" успешно перезаписан с лимитом " << limit << L"." << endl;
}

void writeOut2File(const string& inputTextFilename, const string& outputBinaryFilename) {
    ifstream inFile(inputTextFilename);
    ofstream outFile(outputBinaryFilename, ios::binary);

    if (!inFile || !outFile) {
        wcerr << L"Ошибка открытия файла!" << endl;
        return;
    }

    string line;
    float num;
    while (getline(inFile, line)) {
        stringstream ss(line);
        while (ss >> num) {
            wcout << L"Записываю число: " << num << endl;
            outFile.write(reinterpret_cast<char*>(&num), sizeof(num));
        }
    }

    inFile.close();
    outFile.close();
    wcout << L"Содержимое " << inputTextFilename.c_str() << L" записано в " << outputBinaryFilename.c_str() << L"." << endl;
}

bool compareFiles(const string& file1, const string& file2) {
    ifstream f1(file1, ios::binary);
    ifstream f2(file2, ios::binary);

    if (!f1.is_open() || !f2.is_open()) {
        wcerr << L"Не удалось открыть файлы." << endl;
        return false;
    }

    f1.seekg(0, ios::end);
    f2.seekg(0, ios::end);
    size_t size1 = f1.tellg();
    size_t size2 = f2.tellg();

    if (size1 != size2) {
        wcout << L"Файлы различаются по длине." << endl;
        return false;
    }

    f1.seekg(0, ios::beg);
    f2.seekg(0, ios::beg);

    float num1, num2;
    size_t index = 0;
    const float tolerance = 0.0001f; // Установите желаемую точность

    while (f1.read(reinterpret_cast<char*>(&num1), sizeof(num1)) &&
        f2.read(reinterpret_cast<char*>(&num2), sizeof(num2))) {
        // Сравнение с учетом заданной точности
        if (fabs(num1 - num2) > tolerance) {
            wcout << L"Различие на позиции " << index << L": "
                << L"file1 = " << num1 << L", file2 = " << num2 << endl;
            return false;
        }
        index++;
    }

    wcout << L"Файлы совпадают." << endl;
    return true;
}


int main() {
    setlocale(LC_ALL, "ru_RU.UTF-8");
    system("chcp 65001");

    int n;
    float minRange, maxRange, limit;

    wcout << L"Введите количество чисел (n): ";
    cin >> n;
    wcout << L"Введите минимальное значение диапазона: ";
    cin >> minRange;
    wcout << L"Введите максимальное значение диапазона: ";
    cin >> maxRange;

    createInputFile("input.bin", n, minRange, maxRange);
    system("pause");

    printBinaryFile("input.bin", "text1.txt");
    system("pause");

    wcout << L"Введите значение limit для записи выходного файла: ";
    cin >> limit;
    createOutputFile("input.bin", "output.bin", limit);
    system("pause");

    printBinaryFile("output.bin", "text2.txt");
    system("pause");

    appendToInputFile("input.bin", "output.bin");
    system("pause");

    printBinaryFile("input.bin", "text3.txt");
    system("pause");

    wcout << L"Введите новое значение limit для перезаписи: ";
    cin >> limit;
    overwriteInputFile("input.bin", limit);
    printBinaryFile("input.bin", "final_text.txt");
    system("pause");

    writeOut2File("final_text.txt", "out2.bin");
    printBinaryFile("out2.bin", "text5.txt");
    system("pause");

    wcout << L"Содержимое input.bin:" << endl;
    printBinaryFile("input.bin", "text6.txt");
    wcout << L"Содержимое out2.bin:" << endl;
    printBinaryFile("out2.bin", "text7.txt");

    compareFiles("input.bin", "out2.bin");

    system("pause");
    return 0;
}

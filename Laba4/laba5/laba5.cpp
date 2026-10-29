#include <iostream>
#include <vector>
#include <thread>
#include <windows.h>

using namespace std;
using namespace chrono;

int matrixSize;
vector<vector<int>> matrix;
int mainDiagSum = 0;
int secDiagonalSum = 0;

void MainDiagonal() {
    for (int i = 0; i < matrixSize; i++) {
        mainDiagSum += matrix[i][i];
    }
}

void SecondaryDiagonal() {
    for (int i = 0; i < matrixSize; i++) {
        secDiagonalSum += matrix[i][matrixSize - i - 1];
    }
}

void RunThread(int priority) {
    mainDiagSum = 0;
    secDiagonalSum = 0;

    auto start = high_resolution_clock::now();

    thread mainDiagThread(MainDiagonal);
    thread secDiagThread(SecondaryDiagonal);

    SetThreadPriority(GetCurrentThread(), priority);

    mainDiagThread.join();
    secDiagThread.join();

    auto stop = high_resolution_clock::now();

    auto duration = duration_cast<milliseconds>(stop - start);

    cout << "Сумма главной диагонали: " << mainDiagSum
        << " Сумма побочной диагонали: " << secDiagonalSum
        << " Время выполнения: " << duration.count() << " мс" << endl;
}

int main() {
    setlocale(LC_ALL, "RUS");
    cout << "Введите размер квадратной матрицы: ";
    cin >> matrixSize;

    matrix.resize(matrixSize, vector<int>(matrixSize));
    cout << "Введите элементы матрицы (построчно):\n";
    for (int i = 0; i < matrixSize; i++) {
        for (int j = 0; j < matrixSize; j++) {
            cin >> matrix[i][j];
        }
    }

    cout << "Приоритет: THREAD_PRIORITY_NORMAL: ";
    RunThread(THREAD_PRIORITY_NORMAL);
    cout << "Приоритет: THREAD_PRIORITY_LOWEST: ";
    RunThread(THREAD_PRIORITY_LOWEST);

    return 0;
}

#include <iostream>
#include <vector>
#include <thread>

using namespace std;

int main() {
    setlocale(LC_ALL, "RUS");
    int size;
    int mainDiagSum = 0;
    int secDiagonalSum = 0;

    cout << "Введите размер квадратной матрицы: ";
    cin >> size;

    vector<vector<int>> matrix(size, vector<int>(size));

    cout << "Введите элементы матрицы (построчно):\n";
    for (int i = 0; i < size; i++) {
        for (int j = 0; j < size; j++) {
            cin >> matrix[i][j];
        }
    }

    thread mainDiagThread([&]() {
        for (int i = 0; i < size; i++) {
            mainDiagSum += matrix[i][i];
        }
        });

    thread secDiagThread([&]() {
        for (int i = 0; i < size; i++) {
            secDiagonalSum += matrix[i][size - i - 1];
        }
        });

    mainDiagThread.join();
    secDiagThread.join();

    cout << "Сумма главной диагонали: " << mainDiagSum << endl;
    cout << "Сумма побочной диагонали: " << secDiagonalSum << endl;

    return 0;
}

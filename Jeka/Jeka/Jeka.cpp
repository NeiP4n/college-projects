#include <iostream>
#include <vector>
#include <fstream>
#include <cstdlib>
#include <ctime>
using namespace std;

// Функция для проверки наличия цифры в числе
bool DigitNum(unsigned short number, int digit) {
    while (number > 0) {
        if (number % 10 == digit) {
            return true;
        }
        number /= 10;
    }
    return false;
}

// Функция для нахождения суммы цифр числа
int sumDigits(unsigned short number) {
    int sum = 0;
    while (number > 0) {
        sum += number % 10;
        number /= 10;
    }
    return sum;
}

// Функция для поиска первого числа, содержащего заданную цифру
int findFirstDigit(const vector<unsigned short>& vec, int digit) {
    for (int i = 0; i < vec.size(); i++) {
        if (DigitNum(vec[i], digit)) {
            return i; // Возвращаем индекс найденного числа
        }
    }
    return -1; // Если не найдено
}

// Функция для нахождения всех чисел, не содержащих заданную цифру
vector<unsigned short> findNumbersDigit(const vector<unsigned short>& vec, int digit) {
    vector<unsigned short> result;
    for (unsigned short num : vec) {
        if (!DigitNum(num, digit)) {
            result.push_back(num);
        }
    }
    return result;
}

// Функция для удаления чисел, кратных сумме своих цифр
void removeDivisibleSumDigits(vector<unsigned short>& vec) {
    for (int i = 0; i < vec.size(); ) {
        int sum = sumDigits(vec[i]);
        if (sum != 0 && vec[i] % sum == 0) {
            vec.erase(vec.begin() + i);
        }
        else {
            i++;
        }
    }
}

// Функция для добавления элемента перед первым
void addBeforeFirst(vector<unsigned short>& vec, unsigned short element) {
    if (!vec.empty()) {
        vec.insert(vec.begin(), element);
    }
    else {
        cout << "Массив пуст, нельзя добавить перед первым элементом.\n";
    }
}

// Функция для удаления первого элемента
void removeFirst(vector<unsigned short>& vec) {
    if (!vec.empty()) {
        vec.erase(vec.begin());
    }
    else {
        cout << "Массив пуст, нечего удалять.\n";
    }
}

// Функция для проверки, пуст ли массив
void checkIfEmpty(const vector<unsigned short>& vec) {
    if (vec.empty()) {
        cout << "Массив пуст.\n";
    }
    else {
        cout << "Массив не пуст.\n";
    }
}

// Функция для поиска элемента по ключу
int findElement(const vector<unsigned short>& vec, unsigned short key) {
    for (int i = 0; i < vec.size(); i++) {
        if (vec[i] == key) {
            return i; // Возвращаем индекс найденного элемента
        }
    }
    return -1; // Если не найдено
}

// Функция для добавления элемента после указанного
void addAfter(vector<unsigned short>& vec, int position, unsigned short element) {
    if (position >= 0 && position < vec.size()) {
        vec.insert(vec.begin() + position + 1, element);
    }
    else {
        cout << "Неверная позиция.\n";
    }
}

// Функция для удаления элемента по ключу
void removeElement(vector<unsigned short>& vec, unsigned short key) {
    int position = findElement(vec, key);
    if (position != -1) {
        vec.erase(vec.begin() + position);
    }
    else {
        cout << "Элемент с ключом " << key << " не найден.\n";
    }
}

// Функция для вывода элементов массива
void printVector(const vector<unsigned short>& vec) {
    if (vec.empty()) {
        cout << "Массив пуст.\n";
    }
    else {
        for (unsigned short elem : vec) {
            cout << elem << " ";
        }
        cout << endl;
    }
}

// Функция для заполнения массива случайными числами
void fillVectorRandom(vector<unsigned short>& vec, int size) {
    srand(static_cast<unsigned>(time(0))); // Инициализация генератора случайных чисел
    for (int i = 0; i < size; ++i) {
        vec.push_back(rand() % 100); // Заполнение случайными числами от 0 до 99
    }
}

// Функция для заполнения массива из файла
void fillVectorFromFile(vector<unsigned short>& vec, const string& filename) {
    ifstream file(filename);
    if (file.is_open()) {
        unsigned short num;
        while (file >> num) {
            vec.push_back(num);
        }
        file.close();
    }
    else {
        cout << "Не удалось открыть файл.\n";
    }
}

int main() {
    setlocale(LC_ALL, "RUS");
    vector<unsigned short> myVector;
    bool filled = false;

    while (true) {
        if (!filled) {
            // Выбор источника данных
            cout << "Выберите способ заполнения массива:\n";
            cout << "1. Ввести элементы с клавиатуры\n";
            cout << "2. Заполнить случайными числами\n";
            cout << "3. Заполнить из файла\n";
            cout << "Введите номер варианта: ";

            int choice;
            cin >> choice;

            if (choice == 1) {
                // Заполнение массива с клавиатуры
                cout << "Введите элементы массива (введите +1 для завершения ввода):\n";
                while (true) {
                    unsigned short num;
                    cin >> num;
                    if (num == +1) break;
                    myVector.push_back(num);
                }
            }
            else if (choice == 2) {
                // Заполнение массива случайными числами
                int size;
                cout << "Введите количество случайных чисел: ";
                cin >> size;
                fillVectorRandom(myVector, size);
            }
            else if (choice == 3) {
                // Заполнение массива из файла
                string filename;
                cout << "Введите имя файла: ";
                cin >> filename;
                fillVectorFromFile(myVector, filename);
            }
            else {
                cout << "Неверный выбор. Попробуйте снова.\n";
                continue; // Возвращаемся к выбору способа заполнения
            }
            filled = true;
        }

        // Меню действий
        cout << "\nМеню:\n";
        cout << "1. Добавить элемент перед первым\n";
        cout << "2. Удалить первый элемент\n";
        cout << "3. Проверить массив на пустоту\n";
        cout << "4. Найти элемент по ключу\n";
        cout << "5. Добавить элемент после указанного\n";
        cout << "6. Удалить элемент по ключу\n";
        cout << "7. Удалить элементы, кратные сумме своих цифр\n";
        cout << "8. Найти первое число, содержащее заданную цифру\n";
        cout << "9. Найти все числа, не содержащие заданную цифру\n";
        cout << "10. Показать массив\n";
        cout << "11. Завершить программу\n";
        cout << "Выберите действие: ";

        int action;
        cin >> action;

        if (action == 1) {
            unsigned short element;
            cout << "Введите элемент для добавления перед первым: ";
            cin >> element;
            addBeforeFirst(myVector, element);
        }
        else if (action == 2) {
            removeFirst(myVector);
        }
        else if (action == 3) {
            checkIfEmpty(myVector);
        }
        else if (action == 4) {
            unsigned short key;
            cout << "Введите ключ для поиска: ";
            cin >> key;
            int position = findElement(myVector, key);
            if (position != -1) {
                cout << "Элемент с ключом " << key << " найден на позиции " << position << endl;
            }
            else {
                cout << "Элемент с ключом " << key << " не найден.\n";
            }
        }
        else if (action == 5) {
            unsigned short key, element;
            cout << "Введите ключевой элемент для поиска: ";
            cin >> key;
            int position = findElement(myVector, key);
            if (position != -1) {
                cout << "Введите элемент для добавления после ключевого элемента: ";
                cin >> element;
                addAfter(myVector, position, element);
            }
            else {
                cout << "Элемент с ключом " << key << " не найден.\n";
            }
        }
        else if (action == 6) {
            unsigned short key;
            cout << "Введите ключ для удаления: ";
            cin >> key;
            removeElement(myVector, key);
        }
        else if (action == 7) {
            removeDivisibleSumDigits(myVector);
            cout << "Элементы, кратные сумме своих цифр, удалены.\n";
        }
        else if (action == 8) {
            int digit;
            cout << "Введите цифру для поиска первого числа, содержащего её: ";
            cin >> digit;
            int index = findFirstDigit(myVector, digit);
            if (index != -1) {
                cout << "Первое число, содержащее цифру " << digit << ": " << myVector[index] << endl;
            }
            else {
                cout << "Число, содержащее цифру " << digit << ", не найдено.\n";
            }
        }
        else if (action == 9) {
            int digit;
            cout << "Введите цифру для поиска чисел, не содержащих её: ";
            cin >> digit;
            vector<unsigned short> result = findNumbersDigit(myVector, digit);
            cout << "Числа, не содержащие цифру " << digit << ": ";
            for (unsigned short num : result) {
                cout << num << " ";
            }
            cout << endl;
        }
        else if (action == 10) {
            printVector(myVector);
        }
        else if (action == 11) {
            cout << "Завершение программы.\n";
            break; // Завершение программы
        }
        else {
            cout << "Неверный выбор. Попробуйте снова.\n";
        }
    }
    return 0;
}

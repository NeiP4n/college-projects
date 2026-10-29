#include <windows.h>
#include <iostream>

using namespace std;

const SIZE_T ELEMENT_SIZE = 10 * 1024;

struct ListNode {
    ListNode* next;
    char data[1];
};

ListNode* head = nullptr;
ListNode* cursor = nullptr;

bool isEmpty() {
    return head == nullptr;
}

void moveToStart() {
    cursor = head;
}

void viewCurrent() {
    if (cursor && cursor->next) {
        cout << "Текущий элемент: " << cursor->next << endl;
    }
    else {
        cout << "Элемент не найден.\n";
    }
}

void moveCursorRight() {
    if (cursor && cursor->next) {
        cursor = cursor->next;
    }
    else {
        cout << "Невозможно переместить указатель вправо.\n";
    }
}

void addAfterCursor() {
    ListNode* newNode = (ListNode*)VirtualAlloc(nullptr, ELEMENT_SIZE, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
    if (!newNode) {
        cout << "Ошибка выделения памяти.\n";
        return;
    }

    newNode->next = nullptr;

    if (isEmpty()) {
        head = newNode;
        cursor = head;
    }
    else {
        newNode->next = cursor->next;
        cursor->next = newNode;
    }

    cout << "Элемент добавлен за указателем: " << newNode << endl;
}

void deleteAfterCursor() {
    if (cursor && cursor->next) {
        ListNode* temp = cursor->next;
        cursor->next = temp->next;

        if (!VirtualFree(temp, 0, MEM_RELEASE)) {
            cout << "Ошибка освобождения памяти.\n";
        }
        else {
            cout << "Элемент удален.\n";
        }
    }
    else {
        cout << "Нет элемента для удаления.\n";
    }
}

void printList() {
    ListNode* temp = head;
    int index = 0;
    while (temp) {
        cout << "[" << index++ << "] Элемент: " << temp << endl;
        temp = temp->next;
    }
}

void clearList() {
    while (head) {
        ListNode* temp = head;
        head = head->next;
        VirtualFree(temp, 0, MEM_RELEASE);
    }
    cursor = nullptr;
    cout << "Список очищен.\n";
}

int main() {
    setlocale(LC_ALL, "Russian");

    int choice;
    do {
        cout << "\n--- Меню ---\n";
        cout << "1. Проверить, список пуст/не пуст\n";
        cout << "2. Установить указатель в начало списка\n";
        cout << "3. Добавить элемент за указателем\n";
        cout << "4. Удалить элемент за указателем\n";
        cout << "5. Просмотреть элемент за указателем\n";
        cout << "6. Переместить указатель вправо\n";
        cout << "7. Печать всего списка\n";
        cout << "0. Выход\n";
        cout << "Выбор: ";
        cin >> choice;

        switch (choice) {
        case 1:
            cout << (isEmpty() ? "Список пуст.\n" : "Список не пуст.\n");
            break;
        case 2:
            moveToStart();
            cout << "Указатель установлен на начало.\n";
            break;
        case 3:
            addAfterCursor();
            break;
        case 4:
            deleteAfterCursor();
            break;
        case 5:
            viewCurrent();
            break;
        case 6:
            moveCursorRight();
            break;
        case 7:
            printList();
            break;
        case 0:
            clearList();
            break;
        default:
            cout << "Неверный выбор.\n";
            break;
        }

    } while (choice != 0);

    return 0;
}

#include <iostream>
#include <random>
#include "Game.h"

using namespace std;

class Game {
public:
    Game() {
        initGame();  // Инициализируем игру при создании объекта
    }

    void startGame() {
        int choice;
        wstring nickName;

        func.SetColor(10);
        cout << "                           << ДОБРО ПОЖАЛОВАТЬ В ИГРУ >> \n";
        cout << "Придумайте ник: ";

        func.SetColor(7);
        wcin >> nickName;

        func.SetColor(13);
        func.health = 10;
        func.money = 0;

        // Основной цикл игры
        while (true) {
            func.MainMenu(nickName); // Показать главное меню
            cin >> choice;
            system("cls");

            if (choice == 1) {
                cout << "Ваше здоровье: " << func.health << endl;
            }
            else if (choice == 2) {
  
                handleRandomEvent(nickName);
            }
            else if (choice == 3) {
                func.SetColor(14);
                cout << "Ваши деньги: " << func.money << endl;
            }

            // Проверка на перезапуск при потере здоровья
            if (func.health <= 0) {
                cout << "Вы проиграли. Игра завершена." << endl;
                system("pause");
                break;  // Завершаем игру, если здоровье игрока стало 0
            }
        }
    }

private:
    Functions func;
    Png png;
    Fraza fraza;
    random_device rd;
    mt19937 gen;
    uniform_int_distribution<> dis;

    void initGame() {
        gen = mt19937(rd());
        dis = uniform_int_distribution<>(1, 100);
        func.health = 10;
        func.money = 0;
    }


    void handleRandomEvent(wstring& nickName) {
        int random = dis(gen);

        if (random >= 1 && random <= 5) {
            system("cls");
            fraza.Battle(func.enemyNick, func.enemyHealth, func.enemyDamage);
            fraza.playerBattle(nickName);

        }
        else if (random >= 6 && random <= 15) {
            func.Lavka();
        }
        else if (random >= 16 && random <= 30) {
            func.MoneyUp(func.money);
        }
        else if (random >= 41 && random <= 70) {
            func.HealthDown(func.health);
        }
        else {
           func.HealthUp(func.health);

        }
    }

};

// Главная функция
int main() {
    SetConsoleCP(65001);
    SetConsoleOutputCP(65001);
    setlocale(LC_ALL, "ru_RU.UTF-8");

    Game game;
    game.startGame();  // Запускаем игру
    return 0;
}

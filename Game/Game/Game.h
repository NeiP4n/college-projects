#include <iostream>
#include <windows.h>
#include <algorithm>
#include <string>
#include <iomanip>
#include <winnt.h>
#include <clocale>
#include <conio.h>
#include <cstdlib>
#include <random>
#include <sstream>
using namespace std;

#ifndef GAME_H
#define GAME_H

class Functions {
public:
    // Объявление статических переменных
    static int health;
    static int damage;
    static int money;

    static int WIN;

    static bool isLevel;
    static bool isLevel2;
    static bool isBattle;
    static bool isDef;
    

    // Враг
    static int enemyHealth;
    static int enemyDamage;
    static wstring enemyNick;
    static wstring selectedAvatar;

    // Методы
    static void SetColor(int color);
    static void HealthUp(int& health);
    static void HealthDown(int& health);
    static void MoneyUp(int& money);
    static void EnemyUp();

    static void MainMenu(std::wstring nickName);
    static void Lavka();
};

// Png.cpp
class Png {
public:
    wstring enemyAvatar;
    wstring enemyAvatar2;
    wstring enemyAvatar3;

    Png() : enemyAvatar(L"⠄⠄⠄⠄⣠⠞⠉⢉⠩⢍⡙⠛⠋⣉⠉⠍⢉⣉⣉⣉⠩⢉⠉⠛⠲⣄⠄⠄⠄⠄\n"
                        L"⠄⠄⠄⡴⠁⠄⠂⡠⠑⠄⠄⠄⠂⠄⠄⠄⠄⠠⠄⠄⠐⠁⢊⠄⠄⠈⢦⠄⠄⠄\n"
                        L"⠄⣠⡾⠁⠄⠄⠄⣴⡪⠽⣿⡓⢦⠄⠄⡀⠄⣠⢖⣻⣿⣒⣦⠄⡀⢀⣈⢦⡀⠄\n"
                        L"⣰⠑⢰⠋⢩⡙⠒⠦⠖⠋⠄⠈⠁⠄⠄⠄⠄⠈⠉⠄⠘⠦⠤⠴⠒⡟⠲⡌⠛⣆\n"
                        L"⢹⡰⡸⠈⢻⣈⠓⡦⢤⣀⡀⢾⠩⠤⠄⠄⠤⠌⡳⠐⣒⣠⣤⠖⢋⡟⠒⡏⡄⡟\n"
                        L"⠄⠙⢆⠄⠄⠻⡙⡿⢦⣄⣹⠙⠒⢲⠦⠴⡖⠒⠚⣏⣁⣤⣾⢚⡝⠁⠄⣨⠞⠄\n"
                        L"⠄⠄⠈⢧⠄⠄⠙⢧⡀⠈⡟⠛⠷⡾⣶⣾⣷⠾⠛⢻⠉⢀⡽⠋⠄⠄⣰⠃⠄⠄\n"
                        L"⠄⠄⠄⠄⠑⢤⡠⢂⠌⡛⠦⠤⣄⣇⣀⣀⣸⣀⡤⠼⠚⡉⢄⠠⣠⠞⠁⠄⠄⠄\n"
                        L"⠄⠄⠄⠄⠄⠄⠉⠓⠮⣔⡁⠦⠄⣤⠤⠤⣤⠄⠰⠌⣂⡬⠖⠋⠄⠄⠄⠄⠄⠄\n"
                        L"⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠉⠒⠤⢤⣀⣀⡤⠴⠒⠉⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄"),
        enemyAvatar2(L"⣿⣿⣿⣿⣿⣿⣿⡿⠿⠿⠿⠿⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿\n"
                     L"⣿⣿⣿⣿⣿⠟⠋⠁⠀⠀⠀⠀⠀⠀⠀⠀⠉⠻⣿⣿⣿⣿⣿⣿⣿\n"
                     L"⣿⣿⣿⣿⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢺⣿⣿⣿⣿⣿⣿⣿\n"
                     L"⣿⣿⣿⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠆⠜⣿⣿⣿⣿⣿⣿⣿\n"
                     L"⣿⠿⠿⠛⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠻⣿⣿⣿\n"
                     L"⡏⠁⠀⠀⠀⠀⠀⣀⣠⣤⣤⣶⣶⣶⣶⣶⣦⣤⡄⠀⠀⠀⠀⢀⣴\n"
                     L"⣄⠀⠀⠀⢠⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢿⡧⠇⢀⣤⣶⣿⣿\n"
                     L"⣿⣿⣿⣾⣮⣭⣿⡻⣽⣒⠀⣤⣜⣭⠐⢐⣒⠢⢰⢸⣿⣿⣿⣿\n"
                     L"⣿⣿⣿⣿⣏⣿⣿⣿⣿⣿⣿⡟⣾⣿⠂⢈⢿⣷⣞⣸⣿⣿⣿⣿\n"
                     L"⣿⣿⣿⣿⣿⣽⣿⣿⣷⣶⣾⡿⠿⣿⠗⠈⢻⣿⣿⣿⣿⣿⣿⣿\n"
                     L"⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠻⠋⠉⠑⠀⠀⢘⢻⣿⣿⣿⣿⣿⣿\n"
                     L"⣿⣿⣿⣿⡿⠟⢹⣿⣿⡇⢀⣶⣶⠴⠶⠀⠀⢽⣿⣿⣿⣿⣿⣿\n"
                     L"⣿⣿⣿⣿⡿⠀⠀⢸⣿⣿⠀⠀⠣⠀⠀⠀⠀⠀⡇⢿⣿⣿⣿⣿⣿\n"
                     L"⣿⡿⠟⠋⠀⠀⠀⠀⠹⣿⣧⣀⠀⠀⠀⠀⡀⣴⠁⢘⡙⢿⣿⣿⣿\n"
                     L"⠉⠉⠁⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⢿⠗⠂⠄⠀⣴⡟⠀⠀⡃⣿⣿"),
        enemyAvatar3  (L"⣿⣿⠋⠡⢔⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣶⡌⢙⣿⣿\n"
                       L"⣿⠃⠄⢰⣧⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠄⠘⣿\n"
                       L"⣿⠄⠄⢐⣿⣻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠄⠄⣿\n"
                       L"⡏⠄⠄⠈⠉⠙⠛⢿⣿⣿⣿⣿⣿⣿⡿⠟⠟⠛⠗⠄⠄⣸\n"
                       L"⡇⠄⠄⠐⠷⣻⡶⢌⠛⠿⣿⣿⢿⠿⣻⠶⠶⢶⣦⠄⠄⢐\n"
                       L"⡇⠄⢰⡆⢤⣦⣄⣧⣬⡦⣼⣷⣤⣈⣬⣄⣠⣅⣢⡗⠄⠸\n"
                       L"⡇⠄⠰⢿⣷⣤⣤⣾⣿⡃⣿⣿⡟⣿⣯⣯⣭⣷⢿⠅⠠⣿\n"
                       L"⡇⠄⠄⢾⣿⣿⠿⣿⢿⢠⣿⣿⣷⢼⣟⠿⣿⣿⣿⣻⢠⣺\n"
                       L"⣿⡇⠄⢈⠉⠱⣿⣿⣄⠈⠻⠻⠋⢠⣿⣷⠌⠙⠛⠁⢸⣿\n"
                       L"⣿⣇⠄⢻⣷⣦⣄⠉⢛⡒⠄⠄⢀⣛⠋⣀⣤⣾⡗⡀⣾⣿\n"
                       L"⣿⣿⣦⢂⠻⣿⣿⠷⣮⣟⣻⣛⣿⣿⣲⣿⣿⡏⠔⢼⣿⣿\n"
                       L"⡿⠿⠛⠁⠁⠙⣿⣥⣈⣉⣉⣋⣉⣨⣼⣿⢟⠌⠄⠄⠈⠉\n"
                       L"⠄⠄⠄⠄⠄⠄⠄⠙⣿⣿⣿⣿⣿⣿⠿⠉⠄⠄⠄⠄⠄⠄\n"
                       L"⠄⠄⠄⠄⠄⠄⠄⠄⠈⠉⠛⠟⠝⠁⠄⠄⠄⠄⠄⠄⠄⠄")
    {}
};

class Fraza {
public:
    static int playerDamage;   // Урон игрока
    static wstring damageSlova[11];

    static void UpdateBattleStatus(const std::wstring& enemyNick, int enemyHealth, int enemyDamage, int playerHealth, int playerDamage);
    static void playerBattle(std::wstring& nickName);
    static void Battle(std::wstring& enemyNick, int& enemyHealth, int& enemyDamage);
};


#endif

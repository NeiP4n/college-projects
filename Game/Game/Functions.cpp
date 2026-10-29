#include "Game.h"

Functions func;
Png png;
Fraza fraza;


// Functions переменнные
int Functions::health = 10;
int Functions::damage = 2;
int Functions::money = 0;
int Functions::WIN = 0;
bool Functions::isBattle = true;
bool Functions::isDef = true;
bool Functions::isLevel = true;
bool Functions::isLevel2 = false;



// Functions переменные Врага
int Functions::enemyHealth;
int Functions::enemyDamage;
wstring Functions::enemyNick = L"";
wstring Functions::selectedAvatar;
wstring names[4] = { L"Ексон", L"Андрей", L"Балбес", L"Рубен" };

int playerDamage = 2;
wstring Fraza::damageSlova[11] = {
    L"Он еще не знает.",
    L"Ты попадешь, но так ли это поможет?",
    L"Это не остановит меня!",
    L"Ух, ты крепкий!",
    L"Мне не страшны твои удары!",
    L"Тебе не победить меня так легко!",
    L"Я не отступлю, как бы больно мне не было!",
    L"Кажется, ты только разозлил меня!",
    L"Ты думаешь, что это может меня остановить?",
    L"Это лишь малая часть того, что я могу вынести!"
};
const int width = 100;

// RANDOM
random_device rd;
mt19937 gen(rd());

// Fraza переменные
int Fraza::playerDamage = 2;

void Functions::SetColor(int color) {
    HANDLE hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
    SetConsoleTextAttribute(hConsole, color);
}

void Functions::HealthUp(int& health) {
    health += 1;
    SetColor(2);
    wcout << L"Здоровье увеличено. \n";
}

void Functions::HealthDown(int& health) {
    health -= 1;
    SetColor(4);
    wcout << L"Здоровье уменьшено" << endl;
}

void Functions::MoneyUp(int& money) {
    int mone = rand() % 10 + 1;
    money += mone;
    SetColor(14);
    wcout << L"Деньги увеличены на +" << mone << endl;
}

void Functions::Lavka() {
    bool isPriceDef = true;
    bool isBuy = false;
    int random = rand() % 100;

    if (random <= 40) {
        isBuy = true;
    }

    int choice2;

    while (true) {

        SetColor(13);
        wcout << L"==================================================== \n";
        SetColor(14);
        wcout << L" " << endl;
        wcout << L"   Лавка(предлагает товары :) ) \n" << endl;

        // Лечебное средство
        SetColor(15);
        wcout << L"1.     Захилиться(+1)     ";
        SetColor(6);
        wcout << L"       Цена: 10 монет\n   " << endl;

        // Улучшение
        SetColor(15);
        wcout << L"2.     Монета(+1)      ";
        SetColor(6);
        wcout << L"        Цена: 10 здоровья\n" << endl;

        // Магазин
        SetColor(15);
        wcout << L"3.     Выкуп(Победа)   ";
        SetColor(6);
        wcout << L"        Цена: 100 монет\n" << endl;

        // Особое предложение
        if (isBuy) {
            SetColor(12);
            wcout << L"4.    < Ядерка > ";
            SetColor(6);
            wcout << L"       Цена: 200 монет \n" << endl;
        }

        // Добавим пункт для выхода из лавки в главное меню

        wcout << L"5.     Восстановить Защиту";
        SetColor(6);
        wcout << L"       Цена: 40 монет \n" << endl;

        SetColor(15);
        wcout << L"6.     Выход в Главное меню\n";


        SetColor(13);
        wcout << L"==================================================== \n" << endl;

        cin >> choice2;
        system("cls");

        if (choice2 == 1) {
            if (func.money >= 10) {
                func.money -= 10;
                func.health += 1;
                wcout << L"Вы купили жизнь \n";
            }
            else {
                wcout << L"< Недостаточно монет >\n" << endl;
                wcout << L"У вас есть монеты: " << money << endl;
            }
        }
        if (choice2 == 2) {
            if (func.health >= 10) {
                func.money += 1;
                func.health -= 10;
                wcout << L"Вы купили монету \n";
            }
            else {
                wcout << L"< Недостаточно жизни > \n";
                wcout << L"У вас недостаточно жизни " << func.health << endl;
            }
        }
        if (choice2 == 3) {
            if (func.money >= 100) {
                wcout << L"Поздравляю, вы выкупились из рабства \n";
                std::wcout << L"\nВы решили заплатить цену и выкупиться.\n"
                    L"Ваши цепи падают, и вы получаете свободу. Но с этой свободой приходит тяжесть осознания,\n"
                    L"что вы отдали больше, чем могли бы себе представить. Жизнь, которую вы знали, больше не существует.\n"
                    L"Вы стали свободным человеком, но с каждым шагом в новый мир ощущаете, как теряете часть себя.\n"
                    L"Но, возможно, это и есть настоящая цена свободы — готовность отказаться от прошлого ради будущего.\n"
                    L"Вы приняли свою жертву, и теперь перед вами лежит пустая дорога, полная возможностей.\n";
  
                system("pause");
                exit(0);
                break;
            }
            else {
            wcout << L"Вас пнули в живот...\n";
            }
        }

        if (choice2 == 4 && isBuy) {
            int Da;
           

            SetColor(4);
            wcout << L"<<<<<   Вы хотите купить ЯДЕРКУ?   >>>>>  \n";
            wcout << L"1.   Купить ЯДЕРКУ \n" << endl;
            wcout << L"2.   Отказаться от ЯДЕРКИ \n" << endl;

            while (true) {
                wcout << L"Выбор: ";
                cin >> Da;
                wcout << endl;

                if (Da == 1 && func.money >= 200) {
                    wstring temp;
                    SetColor(6);
                    wcout << L"Вы купили ЯДЕРКУ!     ";
                    SetColor(4);
                    wcout << L"    <<<  Сделка завершена  >>> \n";

                    wcout << L"Напишите код чтобы продолжить... \n \n";
                    wcout << L"Ввести: ";
                    wcin >> temp;
                    if (temp == temp) {

                        wcout << L"Вы стоите перед ядерной бомбой с пультом в руках. На экране мигает таймер — последний шанс ввести код и предотвратить взрыв. С трудом набираете цифры, но на последней секунде палец дрогнул. \n \n"

                            L"Экран краснеет, и раздаётся сигнал ошибки. \n \n"

                            L"Всё вокруг ослепляет яркая вспышка.Вы не успеваете осознать произошедшее... \n \n";

                        system("pause");
                    }
                    
                    exit(0);
                    break;
                }
                else if (Da == 2) {
                    wcout << L" <<  Отказ от ЯДЕРКИ  >> \n" << endl;
                                    system("cls");
                    break;
                }
                else if (func.money < 200) {
                    wcout << L"У вас недостаточно монет. \n";
                }
                else {
                    wcout << L"! Неверный выбор! \n";
                }
            }
        }


        if (choice2 == 5 && isPriceDef == true) {
            if (func.money >= 40) {
                func.money -= 0;

                wcout << L"Вы восстановили защиту \n";

                func.isDef = true;
                isPriceDef = false;
            }
            else {
                wcout << L"У вас недостаточно монет. \n";
            }
        }
        if (choice2 == 6) {
            system("cls");
            break;  // Выход из цикла лавки возвращаемся в главное меню
        }
    }
}

void Fraza::Battle(wstring& enemyNick, int& enemyHealth, int& enemyDamage) {
    system("cls");

    // Аватары врагов
    wstring avatars[3] = { png.enemyAvatar, png.enemyAvatar2, png.enemyAvatar3 };

    // Генератор случайных значений для параметров врага
    uniform_int_distribution<> damage(1, 20);
    uniform_int_distribution<> avatar(0, 2);
    uniform_int_distribution<> hp(5, 140);
    uniform_int_distribution<> slova(0, 9);
    uniform_int_distribution<> dis(0, 3);

    // Генерация случайных значений для врага
    enemyHealth = hp(gen);
    enemyDamage = damage(gen);
    enemyNick = names[dis(gen)];

    // Сохранение значений для EnemyUp
    func.enemyHealth = enemyHealth;
    func.enemyDamage = enemyDamage;

    // Логика критического урона
    uniform_int_distribution<> critChance(1, 100);  // 20% шанс на критический удар
    bool isCriticalHit = critChance(gen) <= 20;
    int effectiveDamage = isCriticalHit ? enemyDamage * 2 : enemyDamage;

    // Проверка состояния перед усилением врагов
    wcout << L"Проверка состояния перед EnemyUp: " << L"HP игрока: " << func.health
        << L", isLevel: " << func.isLevel << L", isLevel2: " << func.isLevel2 << endl;


    // Вывод информации о враге
    func.SetColor(4);
    wcout << L"\n===== Информация о противнике =====\n";
    wcout << L"Имя: " << enemyNick << endl;
    wcout << L"Здоровье: " << func.enemyHealth << endl;
    wcout << L"Урон: " << effectiveDamage << (isCriticalHit ? L" (Критический удар!)" : L"") << endl;

    // Вывод аватара
    func.selectedAvatar = avatars[avatar(gen)];
    wstringstream ss(func.selectedAvatar);
    wstring line;
    while (getline(ss, line, L'\n')) {
        wcout << setw(60) << line << endl;
    }

    // Вывод случайной фразы врага
    wstring frazochka = damageSlova[slova(gen)];
    wcout << L"Фраза: " << frazochka << endl;

    wcout << L"===================================\n";
}



void Fraza::playerBattle(wstring& nickName) {
    int choice;

    // Генератор случайных значений для функции сбежать
    random_device rd;
    mt19937 gen(rd());
    uniform_int_distribution<> fleeChance(1, 100);  // Шанс на сбегание (10%)

    // Фразы проигрыша
    wstring Loses[6] = {
        L"В последний момент, когда я уже почти схватил его, враг резко повернулся и, улыбаясь, нанёс смертельный удар. Я упал на землю, не успев даже пошевелиться. \n",
        L"Всё, что мне оставалось — это смотреть, как он уходит, оставляя меня побеждённым, раздавленным, с последним, слабым вздохом, исходящим из груди.\n",
        L"Мои силы иссякли, а его смех звучал, как последний приговор. Он оказался сильнее, умнее и не оставил мне ни единого шанса. \n",
        L"Я пытался бороться до последнего, но его темные силы были неизмеримо сильнее. В какой-то момент мир стал тускнеть, и я понял, что не смогу выбраться. \n",
        L"Он смотрел на меня с холодной насмешкой, и я знал, что всё окончено. Враг победил, и я оказался бессильным перед его властью.",
        L"Вы опускаете оружие и падаете на колени.Враг приближается, но вы больше не можете бороться. Вы сдаетесь, осознавая, что потеряли всё, что было дорого. Это конец, но в вашей душе всё еще живет огонёк сопротивления.\n" 
    };

    // Случайная фраза проигрыша
    uniform_int_distribution<> randomLose(0, 5);  // Случайное число от 0 до 5
    int randomLoseIndex = randomLose(gen);  // Генерация случайного индекса

    wstring consovka = Loses[randomLoseIndex];  // Выбор фразы по индексу


   Functions::isBattle = true;

    while (func.isBattle) {
        // Обновление статуса боя
        UpdateBattleStatus(func.enemyNick, func.enemyHealth, func.enemyDamage, func.health, fraza.playerDamage);

        // Система выбора действий

        wcout << L"1. Атака       2. Защита        3. Попытка сбежать        4. Сдаться\n";
        wcout << L"Ввести: ";
        wcin >> choice;

        switch (choice) {
        case 1: {
            // Атака
            wcout << L"Вы атаковали врага!\n";
            // Уменьшаем здоровье врага на урон игрока
            func.enemyHealth -= fraza.playerDamage;
            func.health -= func.enemyDamage;
            if (func.enemyHealth <= 0) {
                wcout << L"Вы победили врага!\n";
                func.WIN += 1;
                func.isBattle = false;  // Завершаем бой, так как враг побежден
            }
            break;
        }
        case 2: {
            // Защита
            if (func.isDef) {
                wcout << L"Вы защищаетесь!\n";

                func.enemyHealth -= func.damage;
                func.isDef = false;
            }
            else {
                wcout << L"Защита сломалась...";
            }


            if (func.enemyHealth <= 0) {
                wcout << L"Вы победили врага!\n";
                func.WIN += 1;
                func.isBattle = false;  // Завершаем бой, так как враг побежден
            }

            if (func.health <= 0) {
                wcout << L"Вы проиграли бой.\n";
                wcout << consovka;  // Фраза проигрыша
                func.isBattle = false;  // Завершаем бой так как игрок проиграл
            }
            break;
        }
        case 3: {
            // Попытка сбежать
            if (fleeChance(gen) <= 10) { 
                wcout << L"Вы успешно сбежали от врага!\n";
                func.isBattle = false;  
            }
            else {

                wcout << L"Попытка сбежать не удалась!\n";
            }
            break;
        }
        case 4: {
            wcout << consovka;  // Проигрыш
            func.isBattle = false;
            system("pause");
            exit(0);
            break;
        }
        default: {
            wcout << L"Неверный выбор!\n";
            break;
            }
        }

        // Проверка на окончание боя после всех действий
        if (func.enemyHealth <= 0) {
            wcout << L"Поздравляю, вы победили врага!\n";
            func.money += 60;
            playerDamage += 1;
            func.SetColor(14);
            wcout << L"Деньги увеличены на +60 \n";
            wcout << L"Урон увеличен на +1 \n";
            func.MoneyUp(func.money);
            system("pause");
            func.isBattle = false;  // Завершаем бой
        }
        else if (func.health <= 0) {
            wcout << L"Вы проиграли бой.\n" << consovka;
            system("pause");
            func.isBattle = false;  // Завершаем бой, так как игрок проиграл
        }
    }
}



void Fraza::UpdateBattleStatus(const wstring& enemyNick, int enemyHealth, int enemyDamage, int playerHealth, int playerDamage) {

    system("cls");  // Очистка экрана для обновления состояния


    

    uniform_int_distribution<> slova(0, 9);
    int randomFraza = slova(gen);
    wstring frazochka = damageSlova[randomFraza];

    // Информация о противнике
    func.SetColor(4);  // Цвет для противника
    wcout << L"===== Информация о противнике =====\n";
    wcout << L"Имя: " << enemyNick << endl;
    wcout << L"Здоровье: " << enemyHealth << endl;
    wcout << L"Урон: " << enemyDamage << endl;



    wstringstream ss(func.selectedAvatar);
    wstring line;
    while (getline(ss, line, L'\n')) {
        wcout << setw(width) << right << line << endl;
    }

    // Вывод фразы противника
    wcout << L"Фраза: " << frazochka << endl;
    wcout << L"=======================================================================================================================\n\n";

    // Информация о игроке
    func.SetColor(2);  // Цвет для игрока
    wcout << L"\n=== Ваши характеристики ===\n";
    wcout << L"Здоровье: " << playerHealth << endl;
    wcout << L"Урон: " << playerDamage << endl;
    wcout << L"============================\n";
}

void Functions::MainMenu(wstring nickName) {
  SetColor(8);
    wcout << L" " << endl;
    wcout << L"      <  Главное меню  > \n" << setw(50) << L"" << L"Ваш Ник: " << nickName << endl;
    wcout << setw(63) << L"Ваши Победы: " << func.WIN << endl;
  SetColor(7);
    wcout << L"==========================\n";
    wcout << L"1.      Показать здоровье \n";
    wcout << L"2.      Открыть Лутбокс \n";
    wcout << L"3.      Показать деньги \n";
    wcout << L"==========================\n" << endl;

    wcout << L"Выбор: ";
}





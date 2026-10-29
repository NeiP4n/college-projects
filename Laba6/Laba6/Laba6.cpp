#include <iostream>
#include <queue>
#include <windows.h>
#include <thread>

using namespace std;

const int NUM_CHAIRS = 3;
bool barberSleeping = true;
queue<int> waitingClients;
CRITICAL_SECTION barberCriticalSection; 
void barber() {
    while (true) {
        EnterCriticalSection(&barberCriticalSection); 
        if (!waitingClients.empty()) {
            int client = waitingClients.front();
            waitingClients.pop();
            LeaveCriticalSection(&barberCriticalSection); 
            cout << "Парикмахер стрижет клиента " << client << endl;
            this_thread::sleep_for(chrono::seconds(2));
            cout << "Клиент " << client << " подстрижен" << endl;
        }
        else {
            barberSleeping = true;
            LeaveCriticalSection(&barberCriticalSection); 
            cout << "Парикмахер спит\n" << endl;
            this_thread::sleep_for(chrono::seconds(1));
        }
    }
}

void client(int id) {
    EnterCriticalSection(&barberCriticalSection); 
    if (barberSleeping) {
        barberSleeping = false;
        LeaveCriticalSection(&barberCriticalSection); 
        cout << "Клиент " << id << " разбудил парикмахера" << endl;
    }
    else if (waitingClients.size() < NUM_CHAIRS) {
        waitingClients.push(id);
        LeaveCriticalSection(&barberCriticalSection); 
        cout << "Клиент " << id << " ожидает своей очереди\n" << endl;
    }
    else {
        LeaveCriticalSection(&barberCriticalSection); 
        cout << "Клиент " << id << " ушел, так как нет свободных мест\n" << endl;
    }
}

int main() {
    setlocale(LC_ALL, "Rus");
    InitializeCriticalSection(&barberCriticalSection); 
    thread barberThread(barber);

    for (int i = 1; i <= 10; i++) {
        this_thread::sleep_for(chrono::seconds(1));
        thread clientThread(client, i);
        clientThread.detach();
    }

    barberThread.join();
    DeleteCriticalSection(&barberCriticalSection); 
    return 0;
}

namespace TourAgents.Data 
{
    public class Тур
    {
        public int Идентификатор { get; set; } 
        public string Название { get; set; }
        public string Направление { get; set; }
        public DateTime Дата_начала { get; set; }
        public DateTime Дата_окончания { get; set; }
        public decimal Цена { get; set; }
        public string Описание { get; set; }
        public int Доступные_места { get; set; }
        public List<Бронирование> Бронирования { get; set; }
    }

    public class Клиент
    {
        public int Идентификатор { get; set; } 
        public string Имя { get; set; }
        public string Фамилия { get; set; }
        public string Телефон { get; set; }
        public string Электронная_почта { get; set; }
        public List<Бронирование> Бронирования { get; set; }
    }

    public class Бронирование
    {
        public int Идентификатор { get; set; } 
        public int Идентификатор_тура { get; set; }
        public Тур Тур { get; set; }
        public int Идентификатор_клиента { get; set; } 
        public Клиент Клиент { get; set; } 
        public DateTime Дата_бронирования { get; set; }
        public string Статус { get; set; }
    }
}
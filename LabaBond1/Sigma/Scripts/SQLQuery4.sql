CREATE TABLE Поставщики (
    ПоставщикID INT PRIMARY KEY NOT NULL,
    Название NVARCHAR(255) NOT NULL,
    Адрес NVARCHAR(255),
    КонтактныйТелефон NVARCHAR(20)
);

CREATE TABLE КнижныйФонд (
    Идентификатор INT PRIMARY KEY NOT NULL,
    ИнвентарныйНомер NVARCHAR(50) NOT NULL,
    БиблиотечныйКод NVARCHAR(50) NOT NULL,
    ОтметкаВыдачи NVARCHAR(15) NOT NULL
);

CREATE TABLE Каталог (
    КаталогID INT PRIMARY KEY NOT NULL,
    БиблиотечныйКод NVARCHAR(50) NOT NULL UNIQUE,
    Авторы NVARCHAR(255),
    Название NVARCHAR(255),
    Издательство NVARCHAR(255),
    ГодИздания INT,
    КоличествоСтраниц INT,
    Тема NVARCHAR(100),
    Цена MONEY,
    ПоставщикID INT,
    FOREIGN KEY (ПоставщикID) REFERENCES Поставщики(ПоставщикID)
);

CREATE TABLE Книги (
    КнигаID INT PRIMARY KEY NOT NULL,
    КнижныйФондID INT NOT NULL,
    КаталогID INT NOT NULL,
    FOREIGN KEY (КнижныйФондID) REFERENCES КнижныйФонд(Идентификатор),
    FOREIGN KEY (КаталогID) REFERENCES Каталог(КаталогID)
);

CREATE TABLE Читатели (
    ЧитательID INT PRIMARY KEY NOT NULL,
    НомерБилета NVARCHAR(50) NOT NULL,
    Фамилия NVARCHAR(50),
    Имя NVARCHAR(50),
    Отчество NVARCHAR(50),
    Адрес NVARCHAR(255),
    ДомашнийТелефон NVARCHAR(20),
    РабочийТелефон NVARCHAR(20)
);

CREATE TABLE ВыдачаКниг (
    ВыдачаID INT PRIMARY KEY NOT NULL,
    ЧитательID INT NOT NULL,
    КнигаID INT NOT NULL,
    ДатаВыдачи DATE NOT NULL,
    ДатаВозврата DATE,
    FOREIGN KEY (ЧитательID) REFERENCES Читатели(ЧитательID),
    FOREIGN KEY (КнигаID) REFERENCES Книги(КнигаID)
);

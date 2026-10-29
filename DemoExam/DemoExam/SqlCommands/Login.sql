SELECT Логин, Пароль, [Роль сотрудника] FROM [Пользователи]
WHERE Логин = @login AND 
Пароль = @password AND
[Роль сотрудника] = @role
# EnazaTestTask

Ссылка на задание:
https://docs.google.com/document/d/1vU2ip9hbYG2oO6-DsCPGAgovbays2t0h/edit
- Не реализованы unit-тесты
- Не совсем понятен тезис "После успешной регистрации" в данном пункте: "После успешной регистрации нового пользователя ему должен быть выставлен статус "Active"". Принял решение сделать как считал правильным. В коде прокомментировал. 
- Реализовано получение токена в отдельном контроллере, токен необходимо получить и авторизоваться в интерфейсе сваггера с помощью кнопки "Authorize".
- Код вынесен в отдельную ветку и MR, для удобства комментирования и дальнейших вопросов/ответов. 

# Задание 2.
##### Шаг 1
`CREATE TEMP TABLE tmp_a ( VALUE INT);`
`CREATE TEMP TABLE tmp_b ( VALUE INT);`
`INSERT INTO tmp_a (VALUE) VALUES (10);`
`INSERT INTO tmp_b (VALUE) VALUES (100);`

``SELECT * from tmp_a 
UNION ALL 
SELECT * from tmp_b 
LIMIT 1;``
###### Результат - 10

##### Шаг 2

``delete from tmp_a;``
``SELECT * from tmp_a 
UNION ALL 
SELECT * from tmp_b 
LIMIT 1;``

###### Результат - 100

##### Шаг 3

``INSERT INTO tmp_a (VALUE) VALUES (10);``
``delete from tmp_b;``
``SELECT * from tmp_a 
UNION ALL 
SELECT * from tmp_b 
LIMIT 1;``

###### Результат - 10

##### Шаг 4

``delete from tmp_a;``
``SELECT * from tmp_a 
UNION ALL 
SELECT * from tmp_b 
LIMIT 1;``

##### Результат - null

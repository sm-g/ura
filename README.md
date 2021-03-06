# Модуль авторизации пользователей #

![2014-09-04_18-53-07.png](https://bitbucket.org/repo/4X9Mnb/images/2070350536-2014-09-04_18-53-07.png)

### Требования ###
Требуется способ гибко назначать права доступа на выполнение определённых действий различным категориям пользователей — юристам, бухгалтерам, врачам кардиологического отделения — и каждому пользователю индивидуально.

Решение — ролевая модель управления доступом (RBAC). 

Расширим подход, чтобы можно было назначать права доступа пользователю напрямую, не создавая предварительно роль. Возможности, доступ к которым есть у конкретного пользователя, — это сумма всех возможностей ролей пользователя, за исключением специально убранных (исключённые), плюс специально добавленные пользователю возможности (дополнительные). 

![Рисунок1.png](https://bitbucket.org/repo/4X9Mnb/images/2018761439-%D0%A0%D0%B8%D1%81%D1%83%D0%BD%D0%BE%D0%BA1.png)

Роль может соответствовать как должности (у роли «медсестра» есть возможность «заполнять медицинскую карту»), так и группе пользователей (у роли «юристы» есть возможность «печатать документы»).

### Реализация ###

Действия — добавление/удаление: 

* возможностей в роль, 
* ролей пользователю,
* возможностей пользователю



 действие | пользователь — роль | роль — возможность | пользователь — возможность
-|--------------------|--------------------|------------------------------------
+ | убираем дополнительные возможности, которые есть в роли, исключённые возможности остаются | все пользователи с ролью получают возможность, если она не была запрещена | проверяем, можно ли заменить дополнительные возможности пользователя на некую роль
− | убираем исключённые возможности, бывшие только в этой роли | пользователи с ролью теряют возможность, если нет других ролей с ней | добавляем возможность в исключённые либо убираем дополнительную возможность


При добавлении пользователю роли или возможности в роль запрещённые возможности пользователей не меняются.

При добавлении пользователю возможности проверяется, можно ли заменить его дополнительные возможности на некую роль.

При удалении у пользователя роли запрет возможностей, которые были только в этой роли, снимается.

Роль и пользователь автоматически запрещаются, если у них не остаётся действующих возможностей.


### Использование

```
#!c#

Ability print = new Ability() { Description = "print" }; // возможность печатать
Role admin    = new Role(new[] { print }) { Description = "admin" }; // роль "админ"
User alex     = new User("Alex"); // пользователь Алекс
var printer   = new Printer();

bool canPrint = controller.Can(alex, print); // Алекс хочет печатать
Assert.IsFalse(canPrint); // но не может
 
user.Roles.Add(admin); // добавляем ему роль "админ", у которой такая возможность есть
 
if (controller.Can(alex, print))
  printer.Print("document"); // ok

```
# PickupPoint

Навчальний проєкт з дисципліни «Конструювання програмного забезпечення».
Варіант № 18: пункт видачі замовлень.

## Склад рішення

- PickupPoint.Core — логіка предметної області (бібліотека класів)
- PickupPoint.App — консольний застосунок (введення, виведення)

## Вимоги

.NET SDK 10.0 або новіший, Git.

## Складання та запуск

```bash
dotnet build
dotnet run --project PickupPoint.App
```

Демонстраційне замовлення має меню: додати позицію, позначити прибуття,
видати, скасувати, показати звіт і зберегти звіт у `report.csv`. Файл
`audit.log` дописується класом `FileAuditLog`; обидва файли ігноруються Git.

## Обробка помилок і логування

- Барикада проходить у застосунку: `PickupOrderLineParser` розбирає рядки
  консолі та повертає `Result<PickupOrderLine>`, а ядро приймає вже
  типізовану позицію в `PickupOrderService.AddLine`.
- Очікувані помилки користувача повертають `Result<T>` і повторний запит;
  порушення правил предметної області повідомляють `DomainRuleException`
  з полем `Rule` і, за потреби, `innerException`.
- Сервіс пише структурований журнал через `ILogger<PickupOrderService>`:
  рівні Debug, Information, Warning, Error, іменовані заповнювачі та
  `BeginScope("Order:{OrderId}", id)`.
- `FileAuditLog` реалізує `IDisposable`; `using` гарантує закриття файлу
  навіть після винятку. Перевірка: тест
  `AfterExceptionInsideUsing_FileHandleIsReleased`.
- Ціна вводиться з крапкою (`InvariantCulture`): `250.50`, а не `250,50`.

Перевірка відсутності розбору рядків і консольного виведення в ядрі:

```bash
git grep -n -E "int\.Parse|decimal\.Parse" -- PickupPoint.Core
git grep -n "Console.WriteLine" -- PickupPoint.Core
```

Обидві команди не мають нічого виводити.

## Стандарт кодування

Правила описано у файлі CODING_STANDARD.md і закріплено в .editorconfig.
Перевірка: `dotnet format --verify-no-changes`

## Автор

Черних Назар, група 471
# IMoRS

**IMoRS** (Interactive Map Object Rendering System) — десктопное приложение для Windows
для создания и управления пользовательскими метками на интерактивной карте.
Добавляйте точки на карту OpenStreetMap, назначайте им иконки (встроенные или
свои изображения), задавайте описание и размер, а все данные храните локально
в SQLite. Поддерживается экспорт и импорт меток.

Приложение написано на **C#** с использованием **Avalonia UI** и архитектуры **MVVM**,
карта построена на **Mapsui + BruTile**, хранение — **SQLite + Entity Framework Core**.

> **Ключевые слова:** карта, метки на карте, OpenStreetMap, Mapsui, Avalonia,
> .NET, C#, desktop, SQLite, GIS, markers, map editor.

# Скриншоты
<img width="2560" height="1440" alt="image" src="https://github.com/user-attachments/assets/00693620-8acf-4d97-9421-422e07c74983" />
<img width="2560" height="1440" alt="image" src="https://github.com/user-attachments/assets/d7ac5ae9-6a0f-480d-a2ad-50ea790c87ca" />
<img width="2560" height="1440" alt="image" src="https://github.com/user-attachments/assets/3100f7a2-1954-4161-a0b1-b1a4540b48fe" />

## Возможности

- **Карта** на базе Mapsui с подложкой OpenStreetMap.
- **Метки**:
  - добавление по клику на карте;
  - редактирование описания;
  - изменение размера;
  - удаление;
  - экспорт/импорт.
- **Иконки**:
  - встроенный набор иконок;
  - добавление собственных изображений (JPG, JPEG, PNG, BMP, GIF);
- **Хранение данных**: локальная база SQLite. Метки сохраняются между запусками.
- **Кастомный интерфейс**: окно без системной рамки, собственные кнопки управления.
- **Анимации**: плавное появление при запуске, выдвижение панелей, открытие списка иконок.

## Технологии

| Компонент | Технология |
|---|---|
| Платформа | .NET 10 |
| Язык | C# |
| UI-фреймворк | Avalonia 12 |
| Архитектура | MVVM (CommunityToolkit.Mvvm) |
| Карта | Mapsui 5 + BruTile |
| База данных | SQLite + Entity Framework Core |
| Обработка изображений | SkiaSharp |

## Требования

- Windows 10+

## Установка

1. Скачайте последний релиз из раздела [Releases](../../releases).
2. Установите и запустите `IMoRS.exe`.

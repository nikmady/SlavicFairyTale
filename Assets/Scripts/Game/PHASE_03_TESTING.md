# Phase 3 — MetaContext, Save/Load, UI

## Цель

Проверить сохранение и загрузку MetaContext, отображение и изменение мета-данных через UI.

---

## 1) Вход в Play Mode

Запустите сцену (Play). Убедитесь, что Phase 1 и Phase 2 работают (панель, состояние Boot, кнопки смены состояний).

---

## 2) Переход в MetaHubState

В UI нажмите **Go Hub**. Состояние должно смениться на MetaHub. В разделе **META STATE** отображаются:

- Player Level  
- Total XP  
- Selected Class  
- Currency GOLD  

(Значения берутся из загруженного при Boot или нового MetaContext.)

---

## 3) Изменение мета-данных

- **Add XP (+100)** — добавляет 100 XP в `progression.totalXP`. Текст «Total XP» обновляется.
- **Add Gold (+50)** — добавляет 50 к валюте «GOLD». Текст «Currency GOLD» обновляется.

Проверьте, что числа на экране меняются сразу после нажатия.

---

## 4) Сохранение

Нажмите **Save Meta**. В консоли должно появиться сообщение о сохранении (например, `[Game] Meta saved to ...`).

---

## 5) Выход из Play Mode

Остановите воспроизведение (Stop).

---

## 6) Повторный вход в Play Mode

Снова нажмите Play. После входа в состояние Boot мета-данные загружаются с диска. Перейдите в **Go Hub** и проверьте:

- Total XP и Currency GOLD совпадают с теми, что были до остановки (если перед этим нажимали Add XP / Add Gold и **Save Meta**).

---

## 7) Проверка сохранения

- При первом запуске (когда файла ещё нет) в консоли: создание нового MetaContext (нет сохранённого файла).
- После **Save Meta** и повторного Play — сообщение о загрузке мета с диска и восстановленные значения в UI.

---

## Путь к файлу сохранения

Файл мета-сохранения:

- **Имя файла:** `meta_save.json`
- **Папка:** `Application.persistentDataPath`

В редакторе Unity это обычно:

- **Windows:** `%userprofile%\AppData\LocalLow\<CompanyName>\<ProductName>\`
- **macOS:** `~/Library/Application Support/<CompanyName>/<ProductName>/`

В консоли при сохранении выводится полный путь к файлу.

---

## Как сбросить сохранение вручную

1. Выйти из Play Mode.
2. Найти папку `Application.persistentDataPath` (см. выше; Company Name и Product Name — из Player Settings).
3. Удалить файл `meta_save.json` в этой папке.
4. При следующем Play будет создан новый MetaContext (как при первом запуске).

Либо в коде/инструментах редактора можно вызывать `File.Delete(Application.persistentDataPath + "/meta_save.json")` до следующего запуска.

---

## Кнопка Reload Meta

**Reload Meta** перечитывает `meta_save.json` с диска и обновляет `GameRoot.Meta`. UI обновит значения в следующем кадре. Используйте после изменения файла снаружи или для проверки загрузки без перезапуска Play Mode.

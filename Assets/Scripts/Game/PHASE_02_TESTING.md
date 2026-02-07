# Phase 2 — GameStateMachine, проверка

## Предварительно

- Phase 1 должна быть настроена (префабы, сцена с GameRootCanvas, Play работает).
- Для отображения кнопок смены состояний обновите UI-префаб по инструкции ниже.

---

## Обновление UI-префаба (Phase 2)

Не обязательно пересоздавать префаб с нуля.

1. Откройте префаб **GameRootCanvas** (`Assets/Prefabs/UI/GameRootCanvas.prefab`).
2. Выберите объект **Panel** (корень панели с VerticalLayoutGroup).
3. Под существующими элементами добавьте:
   - **Text** — имя объекта например `CurrentState`, текст по умолчанию: `Current State: Boot`. В Inspector у компонента GameRootPanel в поле **Current State Text** перетащите этот Text.
   - **Button** с подписью **Go Boot** → в GameRootPanel перетащите в **Go Boot Button**.
   - **Button** «Go Hub» → **Go Hub Button**.
   - **Button** «Go WorldMap» → **Go WorldMap Button**.
   - **Button** «Go LoadLocation» → **Go Load Location Button**.
   - **Button** «Go Run» → **Go Run Button**.
   - **Button** «Go RunEnd» → **Go Run End Button**.
4. Сохраните префаб (Ctrl+S или Apply в Inspector).

Либо выполните **Game → Create Phase 1 Prefabs** ещё раз — скрипт создаёт префаб уже с Phase 2 элементами (текущая версия может перезаписать существующий префаб).

---

## 1) Вход в Play Mode

Запустите сцену (Play). Должны быть видны панель Phase 1 и, если вы добавили элементы Phase 2, строка «Current State» и кнопки смены состояний.

---

## 2) Что проверить

- **Начальное состояние = Boot**  
  В UI: «Current State: Boot». В консоли при старте: `[Game] Enter Boot`.

- **Логи переходов**  
  При нажатии любой кнопки смены состояния (например «Go Hub»):
  - `[Game] Exit Boot`
  - `[Game] Enter MetaHub`
  - `[Game] State change: Boot -> MetaHub`  
  Состояние на экране меняется на «Current State: MetaHub».

- **Поведение UI**  
  - Текст текущего состояния обновляется при каждом переходе.
  - Кнопка текущего состояния неактивна (нельзя «перейти» в то же состояние).

- **Цикл Tick и heartbeat**  
  Heartbeat по-прежнему пишет в консоль раз в секунду; ошибок и дублирования Enter/Exit при переключении состояний нет.

---

## 3) Нажатие кнопок

- **Go Boot** → состояние Boot.
- **Go Hub** → MetaHub.
- **Go WorldMap** → WorldMap.
- **Go LoadLocation** → LoadLocation.
- **Go Run** → RunLocation.
- **Go RunEnd** → RunEnd.

После каждого перехода в консоли: Exit предыдущего, Enter нового, одна строка «State change: … -> …».

---

## 4) Итоговая проверка

- Ошибок в консоли нет.
- Enter/Exit каждого состояния вызываются по одному разу при переходе (без дублей).
- Tick продолжает вызываться (heartbeat и счётчики работают).
- Сцен не загружаем, Addressables и контексты (MetaContext/RunContext) не используем — только логика состояний и UI.

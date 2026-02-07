# Phase 6 — WorldRuntime: как проверить

В Phase 6 добавлен слой WorldRuntime (контейнер мира). Новых кнопок и полей в UI **нет** — префаб и меню **Create Phase 1 Prefabs** менять не нужно. Проверка делается через уже существующие кнопки и консоль.

---

## Что появилось в Phase 6

- **WorldRuntimeContext** — данные текущей локации (locationId, biomeId, worldOrigin, isInitialized).
- **WorldRuntime** — владеет контекстом, жизненный цикл: Initialize → Tick → Dispose.
- **WorldBootstrap** — создаёт WorldRuntime по locationId и biomeId.
- **GameRoot** — методы `CreateWorldRuntime(locationId, biomeId)` и `DestroyWorldRuntime()`; свойство `CurrentWorldRuntime`.
- **LoadLocationState** — в BeginLoadLocation создаёт мир через GameRoot, в CompleteLoadLocation переключает в RunLocation.
- **RunLocationState** — при входе проверяет WorldRuntime и логирует "WorldRuntime active for location &lt;id&gt;", в Tick прокидывает вызов в WorldRuntime.
- **RunEndState** — при входе вызывает DestroyWorldRuntime и логирует "WorldRuntime disposed".

Мир создаётся **только** в LoadLocationState и уничтожается **только** в RunEndState.

---

## Нужно ли обновлять префаб

**Нет.** В Phase 6 не добавлялись новые UI-элементы и не менялись компоненты на префабах. Если у вас уже стоит GameRootCanvas из предыдущих фаз — пересоздавать его не нужно. Если префабов ещё нет — один раз выполните **Game → Create Phase 1 Prefabs** (как в PHASE_01_TESTING.md).

---

## Как проверить по шагам

### 1. Запуск

1. Откройте сцену, в которой есть **GameRootCanvas** (и при необходимости объект с GameRootInstaller, если GameRoot создаётся из префаба).
2. Нажмите **Play**.

### 2. Переход в World Map

1. В панели нажмите **Go WorldMap**.
2. Должны отобразиться узлы (Village и при разблокировке — Forest и др.).
3. При необходимости нажмите **Unlock Forest**, чтобы разблокировать следующий узел.

### 3. Выбор узла и создание WorldRuntime

1. Нажмите на **разблокированный** узел (например Village или Forest).
2. Должен произойти переход в состояние **LoadLocation**.
3. В консоли ожидаются строки:
   - `[Game] Enter LoadLocation`
   - `[Game] Preparing location: <nodeId>`
   - затем переход в RunLocation (см. ниже).

В этот момент внутри LoadLocationState вызывается `CreateWorldRuntime(nodeId, biomeId)` — мир создаётся, но визуально ничего не меняется (чанков/сцен нет).

### 4. RunLocation — мир активен

1. После LoadLocation автоматически переключается состояние **RunLocation**.
2. В консоли должно быть:
   - `[Game] Enter RunLocation`
   - `[Game] WorldRuntime active for location <nodeId>` (например `village` или `forest`).
3. Пока вы остаётесь в RunLocation, каждый кадр вызывается `WorldRuntime.Tick(dt)` (сейчас внутри пусто, но объект живёт).

### 5. RunEnd — уничтожение мира

1. В панели нажмите **Go RunEnd**.
2. В консоли должно быть:
   - `[Game] Enter RunEnd`
   - `[Game] WorldRuntime disposed`.

После этого WorldRuntime у GameRoot больше нет; при следующем заходе в локацию он создаётся заново в LoadLocationState.

### 6. Повторный заход в локацию

1. Снова нажмите **Go WorldMap**, выберите узел.
2. Снова пройдите цепочку: LoadLocation → RunLocation (лог "WorldRuntime active for location ...") → при необходимости Go RunEnd (лог "WorldRuntime disposed").

Так вы убеждаетесь, что мир каждый раз создаётся при входе в локацию и уничтожается при RunEnd.

---

## Краткий чеклист по консоли

| Действие              | Ожидаемый лог |
|-----------------------|----------------|
| Выбор узла в WorldMap | Enter LoadLocation, Preparing location: &lt;id&gt; |
| После LoadLocation    | Enter RunLocation, WorldRuntime active for location &lt;id&gt; |
| Нажатие Go RunEnd     | Enter RunEnd, WorldRuntime disposed |

---

## Если что-то не так

- **Нет лога "WorldRuntime active for location"** — проверьте, что вы действительно перешли в RunLocation (по кнопке состояния или после выбора узла). Убедитесь, что выбран разблокированный и достижимый узел.
- **Нет "WorldRuntime disposed"** — нажимайте именно **Go RunEnd** после RunLocation.
- **Ошибки в консоли** — убедитесь, что все скрипты Phase 6 на месте (WorldRuntimeContext, WorldRuntime, WorldBootstrap) и проект пересобран.

---

## Итог

- Префаб и **Game → Create Phase 1 Prefabs** для Phase 6 **не меняются**.
- Проверка: Play → Go WorldMap → выбор узла → смотрите консоль (LoadLocation → RunLocation → "WorldRuntime active") → Go RunEnd → "WorldRuntime disposed".
- Этого достаточно, чтобы убедиться, что WorldRuntime создаётся и уничтожается в нужных местах.

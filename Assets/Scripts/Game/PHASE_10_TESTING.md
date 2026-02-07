# Phase 10 — PlayerInput + PlayerMovement: как проверить

Phase 10 вводит **ввод** (WASD / стрелки) и **движение** игрока через отдельные классы, связывает их в WorldRuntime и убирает TEMP-движение. PlayerAnchor остаётся единственным источником позиции.

---

## Что добавлено

- **PlayerInput** (не MonoBehaviour) — читает ввод через `UnityEngine.Input` (Horizontal/Vertical = WASD и стрелки), возвращает нормализованное направление или Vector2.zero. Метод: `ReadMoveInput()`.
- **PlayerMovement** (не MonoBehaviour) — хранит ссылку на PlayerAnchor и `moveSpeed`; метод `ApplyMovement(Vector2 direction, float deltaTime)` сдвигает `anchor.Position`.
- **WorldRuntime** — TEMP-движение и лог позиции удалены. Добавлены `_playerInput` и `_playerMovement`; в Initialize() создаются PlayerInput и PlayerMovement(anchor, 5f); в Tick(): `input = _playerInput.ReadMoveInput()`, `_playerMovement.ApplyMovement(input, dt)`.

PlayerRuntime по-прежнему только читает anchor и обновляет view и камеру; ввод и изменение anchor не трогает.

---

## Как проверить по шагам

### 1. Управление в RunLocation

1. **Play** → **Go WorldMap** → выберите узел (например Village).
2. В **RunLocation** управление: **WASD** или **стрелки**.
3. Игрок и камера двигаются только при нажатых клавишах; без ввода персонаж стоит на месте.
4. Диагональ (W+D и т.п.) даёт нормализованное направление — движение без ускорения по диагонали.

### 2. Стриминг чанков

- При движении в сторону чанков они подгружаются (логи `[Game] Load chunk ...` при наличии LocationChunkSet в Resources).
- При уходе из зоны — выгрузка (`[Game] Unload chunk ...`).

### 3. Консоль

- Логов вида `[Game] TEMP player position` больше нет.
- Ожидаются только стандартные логи состояний (Enter/Exit LoadLocation, RunLocation, WorldRuntime active и при необходимости Load/Unload chunk).

---

## Краткий чеклист

| Действие | Ожидание |
|----------|----------|
| RunLocation, без ввода | Игрок не двигается |
| RunLocation, WASD/стрелки | Игрок и камера двигаются по направлению |
| RunLocation, диагональ | Движение по диагонали, без «ускорения» |
| Движение по миру | ChunkStreamer подгружает/выгружает чанки по позиции |

---

## Итог

- **Протестить Phase 10** = убедиться, что в RunLocation игрок управляется только с клавиатуры (WASD/стрелки), камера следует за ним, чанки стримятся по позиции, временного кода в консоли нет.

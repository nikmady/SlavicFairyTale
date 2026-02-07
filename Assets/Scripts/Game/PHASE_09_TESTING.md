# Phase 9 — PlayerRuntime + Camera: как проверить

Phase 9 вводит **PlayerRuntime** как runtime-сущность игрока, связывает её с **PlayerAnchor**, добавляет визуал (**PlayerView**) и камеру (**PlayerCameraRig**), следующую за игроком. View отделён от логики; управление — через WorldRuntime.

---

## Что добавлено

- **PlayerRuntime** (не MonoBehaviour) — владеет жизненным циклом игрока:
  - Поля: `anchor`, `root`, `view`, `cameraRig`
  - `Initialize()` — создаёт GameObject "Player" с PlayerView, GameObject "PlayerCameraRig" с Camera и PlayerCameraRig
  - `Tick(dt)` — синхронизирует позицию view с anchor, вызывает камеру Follow(anchor.Position)
  - `Dispose()` — уничтожает оба GameObject
- **PlayerView** (MonoBehaviour) — только визуал: SpriteRenderer (placeholder), без логики и без Update()
- **PlayerCameraRig** (MonoBehaviour) — держит Camera, метод `Follow(Vector2)` для следования за позицией; без Update(), вызов из PlayerRuntime.Tick
- **WorldRuntime** — поле `_playerRuntime`; в Initialize() создаётся PlayerRuntime с передачей PlayerAnchor и вызывается Initialize(); в Tick() — `_playerRuntime.Tick(dt)`; в Dispose() — `_playerRuntime.Dispose()`

Player создаётся только внутри WorldRuntime; GameRoot и состояния (LoadLocationState, RunEndState) не создают PlayerRuntime.

---

## Как проверить по шагам

### 1. Запуск и переход в локацию

1. **Play** → сцена с GameRootCanvas.
2. **Go WorldMap** → выберите узел (например Village).
3. После перехода в **RunLocation** в сцене должны появиться:
   - Объект **Player** (с компонентом PlayerView и SpriteRenderer).
   - Объект **PlayerCameraRig** (с Camera и PlayerCameraRig).
4. Камера должна показывать игрока (или пустое место, если спрайт не назначен — это нормально для placeholder).
5. Игрок и камера двигаются вправо (TEMP-движение по X из WorldRuntime).
6. Раз в секунду в консоли: `[Game] TEMP player position: (x, 0)`.

### 2. Камера следует за игроком

- Пока вы в RunLocation, позиция камеры должна совпадать с позицией Player (камера в той же XY, z = -10).
- Движение без сглаживания — камера каждый кадр ставится в позицию игрока.

### 3. Выход из локации

1. Нажмите **Go RunEnd**.
2. В консоли: `[Game] WorldRuntime disposed`.
3. Объекты **Player** и **PlayerCameraRig** должны исчезнуть из иерархии (Dispose уничтожает их).

### 4. Повторный заход

1. Снова **Go WorldMap** → выберите узел.
2. Player и камера создаются заново при входе в RunLocation.

---

## Краткий чеклист

| Действие | Ожидание |
|----------|----------|
| Вход в RunLocation | В иерархии: Player, PlayerCameraRig; камера смотрит на игрока |
| В RunLocation | Игрок и камера двигаются вправо; раз в секунду лог TEMP position |
| Go RunEnd | Player и PlayerCameraRig уничтожены |
| Повторный вход в локацию | Player и камера созданы снова |

---

## Качество архитектуры

- PlayerRuntime не знает про StateMachine и MetaContext.
- PlayerView не знает про WorldRuntime.
- PlayerCameraRig не знает про GameRoot; обновление только через вызов Follow() из PlayerRuntime.
- В PlayerView и PlayerCameraRig нет Update() — всё управляется из WorldRuntime → PlayerRuntime.Tick.

---

## Итог

- **Протестить Phase 9** = убедиться, что при входе в локацию появляются визуальный игрок и камера, камера следует за игроком, при RunEnd всё уничтожается и при повторном входе создаётся снова.
- Движение по-прежнему TEMP (без ввода); замена на ввод — в следующих фазах.

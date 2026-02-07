# Phase 11B — Enemy Runtime + Spawn: как проверить

Phase 11B вводит **врагов как runtime-сущности**: EnemyRuntime, EnemyView, EnemySystem, спавн по чанкам через EnemySpawnPoint. Без боя, AI и урона.

---

## Что добавлено

- **ICombatant** — заглушка интерфейса (Position, IsAlive); реализуется EnemyRuntime.
- **EnemyRuntime** (не MonoBehaviour) — enemyId, position, view, isAlive, spawnChunkId; Initialize() создаёт GameObject с EnemyView, Tick() синхронизирует позицию view, Dispose() уничтожает объект.
- **EnemyView** (MonoBehaviour) — только визуал (SpriteRenderer), без логики.
- **EnemySystem** — список EnemyRuntime; SpawnEnemy(enemyId, position, chunkId), DespawnEnemy(enemy), DespawnEnemiesInChunk(chunkId), Tick(dt).
- **EnemySpawnPoint** (MonoBehaviour) — data-marker, поле enemyId; не спавнит сам.
- **ChunkIdentity** (MonoBehaviour) — на корне чанка, поле chunkId; выставляется ChunkStreamer при создании корня.
- **ChunkEnemySpawner** (MonoBehaviour) — на корне чанка; OnEnable находит EnemySpawnPoint в детях и вызывает EnemySystem.SpawnEnemy(..., chunkId); OnDisable вызывает DespawnEnemiesInChunk(chunkId).
- **EnemySystemRegistry** — статический Current; WorldRuntime выставляет/очищает.
- **WorldRuntime** — _enemySystem, инициализация и EnemySystemRegistry.Set в Initialize(), _enemySystem.Tick(dt) в Tick(), Clear и null в Dispose().
- **ChunkStreamer.LoadChunk** — на корень вешаются ChunkIdentity (chunkId), ChunkInteractionRegistrar, ChunkEnemySpawner.

---

## Как проверить

### 1. Без спавн-поинтов в чанках

- Play → WorldMap → выбор узла → RunLocation. Чанки подгружаются, врагов нет (в текущей реализации чанки создаются пустыми, без детей EnemySpawnPoint). Консоль без ошибок.

### 2. Враги из чанка (когда есть спавн-поинты)

Чтобы враги появились, в подгруженном чанке должны быть объекты с **EnemySpawnPoint** под корнем чанка. Сейчас корень чанка создаётся пустым в коде; для теста можно:

- Временно вручную добавить в сцену объект с ChunkIdentity (chunkId = id одного из чанков из LocationChunkSet) и ChunkEnemySpawner, и дочерний объект с EnemySpawnPoint (enemyId задан). Либо позже подключать префабы чанков с детьми-EnemySpawnPoint.
- Ожидание: при активации такого «чанка» в консоли/игре появляются враги (Enemy_&lt;enemyId&gt; в иерархии), при деактивации — исчезают.

### 3. Жизненный цикл

- При выгрузке чанка (игрок ушёл из радиуса стриминга) вызывается UnloadChunk → Destroy(root) → OnDisable у ChunkEnemySpawner → DespawnEnemiesInChunk → враги этого чанка удаляются из мира и из списка.

---

## Краткий чеклист

| Условие | Ожидание |
|--------|----------|
| Чанки без детей EnemySpawnPoint | Врагов нет, ошибок нет |
| Чанк с детьми EnemySpawnPoint | Появление врагов при загрузке чанка, исчезновение при выгрузке |
| RunLocation | EnemySystem.Tick вызывается каждый кадр |

---

## Итог

- **Протестить Phase 11B** = убедиться, что игра входит в RunLocation без ошибок, при наличии спавн-поинтов в чанке враги появляются и пропадают вместе с чанком. Визуально враги просто стоят на месте (без боя и AI).

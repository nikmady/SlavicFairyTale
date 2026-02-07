# Phase 8 — ChunkStreamer (runtime): как проверить

Phase 8 добавляет **runtime-стриминг чанков** по позиции «игрока»: загрузка и выгрузка чанков через ChunkStreamer без Addressables и сцен. Поведение в игре меняется: в RunLocation чанки создаются/удаляются по движению заглушки.

---

## Что добавлено

- **PlayerAnchor** — заглушка (не MonoBehaviour) с полем `Vector2 Position`; позже будет заменена на PlayerRuntime.
- **ChunkRuntime** — представление загруженного чанка в рантайме: `ChunkDescriptor` + `GameObject root`.
- **ChunkStreamer** — владеет `IChunkSource` и словарём активных чанков; конфиг `streamingRadius`. Методы:
  - `UpdateStreaming(Vector2 playerPosition)` — строит bounds, запрашивает чанки через источник, подгружает недостающие, выгружает лишние (с учётом `isAlwaysLoaded`).
  - `LoadChunk(ChunkDescriptor)` — создаёт пустой GameObject `"Chunk_<id>"`, лог `[Game] Load chunk <id>`.
  - `UnloadChunk(string chunkId)` — уничтожает GameObject, лог `[Game] Unload chunk <id>`.
- **WorldRuntime** — поля `_chunkStreamer`, `_playerAnchor`; в `Initialize()` создаются PlayerAnchor (Vector2.zero) и ChunkStreamer (радиус 30); в `Tick(dt)` вызывается `UpdateStreaming(_playerAnchor.Position)` и **временное** движение по X для теста (раз в секунду лог позиции).
- **LoadLocationState** — после создания WorldRuntime резолвится LocationChunkSet по `locationId` (Resources.LoadAll + поиск по locationId), вызывается `worldRuntime.SetChunkSource(chunkSet)`. Если набора нет — предупреждение в консоль; стриминг работает с нулём чанков.

Addressables, EditorWindow, сцены, боёвка и PlayerController **не добавлялись**.

---

## Как проверить по шагам

### 1. Без LocationChunkSet (ноль чанков)

1. **Play** → **Go WorldMap** → выберите узел (например Village).
2. Должны быть логи: Enter LoadLocation → Preparing location → Enter RunLocation → WorldRuntime active.
3. В RunLocation раз в секунду в консоли: `[Game] TEMP player position: (x, 0)` — координата X растёт (временное движение).
4. Если LocationChunkSet для этой локации нет или не в Resources: лог `[Game] No LocationChunkSet found for locationId '...'` — стриминг с нулём чанков, без Load/Unload.
5. **Go RunEnd** → WorldRuntime disposed.

### 2. С LocationChunkSet (стриминг чанков)

1. В Unity: создайте LocationChunkSet (**Create → Game → Location Chunk Set**).
2. Задайте **Location Id** так же, как nodeId локации (например `village` для узла Village).
3. В **Chunks** добавьте хотя бы один элемент; у ChunkDescriptor задайте:
   - **Chunk Id** (например `chunk_1`);
   - **Bounds** — Center и Size так, чтобы чанк пересекал зону вокруг (0,0) при радиусе 30 (например Center (0,0,0), Size (20,20,1)).
4. Сохраните ассет в папку **Resources** (или в подпапку, например `Resources/LocationChunkSets`), чтобы сработал `Resources.LoadAll<LocationChunkSet>("")`.
5. **Play** → **Go WorldMap** → выберите узел с тем же locationId (village).
6. В консоли ожидаются:
   - `[Game] Load chunk chunk_1` (или ваш chunkId) — когда «игрок» попадает в зону чанка.
   - Раз в секунду: `[Game] TEMP player position: (x, 0)`.
7. Если оставить игру в RunLocation, X растёт; при выходе чанка из радиуса стриминга: `[Game] Unload chunk chunk_1`.
8. **Go RunEnd** → WorldRuntime disposed; чанки выгружаются.

### 3. Чеклист по консоли

| Действие | Ожидаемый лог |
|----------|----------------|
| Выбор узла | Enter LoadLocation, Preparing location: &lt;id&gt; |
| Нет ChunkSet для локации | No LocationChunkSet found for locationId '...' |
| RunLocation (есть ChunkSet, чанк в зоне) | Load chunk &lt;chunkId&gt; |
| RunLocation, раз в секунду | TEMP player position: (x, 0) |
| Игрок ушёл из зоны чанка | Unload chunk &lt;chunkId&gt; |
| Go RunEnd | WorldRuntime disposed |

---

## Проверка без дубликатов и утечек

- Один и тот же chunkId не должен загружаться дважды (в логе не должно быть двух подряд «Load chunk X» без «Unload chunk X»).
- После **Go RunEnd** не должно оставаться объектов `Chunk_*` в сцене (Dispose/уничтожение мира очищает чанки через ChunkStreamer).

---

## Итог

- **Протестить Phase 8** = тот же сценарий, что и после Phase 7 (Play → WorldMap → узел → RunLocation → RunEnd), плюс при наличии LocationChunkSet в Resources — логи Load/Unload чанков и TEMP позиции.
- Временное движение и лог позиции помечены в коде как TEMP и будут убраны при появлении PlayerRuntime.
- Подробности Phase 7 — в **PHASE_07_TESTING.md**.

# Phase 7 — Chunk Descriptor (DATA): как проверить

Phase 7 — это **только слой данных**: добавлены типы для описания чанков и реестр по локациям. Стриминга, загрузки и нового геймплея **нет**, поведение в игре не менялось.

---

## Что добавлено

- **ChunkDescriptor** — структура данных одного чанка (chunkId, addressableKey, bounds, biomeId, isAlwaysLoaded).
- **LocationChunkSet** — ScriptableObject-реестр чанков одной локации; методы `GetAllChunks()`, `GetChunksIntersecting(Bounds)`; реализует **IChunkSource**.
- **IChunkSource** — интерфейс для источника чанков (для будущего использования в WorldRuntime).
- **WorldRuntime** — добавлены поле и метод `SetChunkSource(IChunkSource)`; пока нигде не вызываются.
- **LoadLocationState** — добавлен TODO: в будущем здесь резолвить LocationChunkSet по locationId.

Никакой логики загрузки чанков, Addressables или Editor-окон не добавлялось.

---

## Как проверить, что ничего не сломалось

Проверка та же, что и после Phase 6: убедиться, что старый сценарий работает.

1. **Play** → сцена с GameRootCanvas.
2. **Go WorldMap** → видны узлы (Village и т.д.).
3. Выбрать разблокированный узел (например **Village**) → переход в LoadLocation, затем в RunLocation.
4. В консоли:
   - `[Game] Enter LoadLocation`
   - `[Game] Preparing location: village` (или другой nodeId)
   - `[Game] Enter RunLocation`
   - `[Game] WorldRuntime active for location village`
5. **Go RunEnd** → в консоли:
   - `[Game] Enter RunEnd`
   - `[Game] WorldRuntime disposed`

Если всё так и есть — Phase 7 не сломала текущий поток. Дополнительных кнопок или действий для Phase 7 не требуется.

---

## Проверка данных (по желанию)

Убедиться, что новые типы работают:

1. В Unity: правый клик в папке Project → **Create → Game → Location Chunk Set**.
2. Должен создаться ассет с полями **Location Id** и **Chunks** (список).
3. Заполните, например, `locationId = "village"`, оставьте список чанков пустым — этого достаточно, чтобы проверить, что ScriptableObject и данные на месте.
4. Сохраните ассет. Для Phase 7 его никуда подключать не нужно — это задел на будущее (резолв по locationId в LoadLocationState).

---

## Итог

- **Протестить Phase 7** = убедиться, что игра ведёт себя как после Phase 6 (шаги выше).
- Новых действий в Play Mode для Phase 7 нет; проверка — компиляция + тот же сценарий без ошибок.
- Подробная проверка Phase 6 описана в **PHASE_06_TESTING.md**.

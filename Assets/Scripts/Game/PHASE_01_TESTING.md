# Phase 1 — настройка сцены и проверка

## Подготовка префабов (один раз)

В Unity: меню **Game → Create Phase 1 Prefabs**. Создаются `Assets/Prefabs/GameRoot.prefab`, `Assets/Resources/GameRoot.prefab`, `Assets/Prefabs/UI/GameRootCanvas.prefab`.

---

## Настройка сцены (точно по шагам)

1. **Создайте или откройте любую сцену.**

2. **Проверьте наличие EventSystem.**  
   Если в сцене нет EventSystem, добавьте его (например, через GameObject → UI → Event System). В префабе GameRootCanvas уже есть EventSystem — при добавлении канваса из префаба он попадёт в сцену.

3. **Поместите префаб GameRootCanvas в сцену.**  
   Перетащите `Assets/Prefabs/UI/GameRootCanvas.prefab` в Hierarchy. В префабе уже есть объект GameBootstrap с GameRootInstaller — GameRoot будет создаваться автоматически при Play.

4. **Нажмите Play.**

5. **Ожидаемое поведение:**
   - Видна UI-панель «Game Debug Panel» (слева сверху).
   - Elapsed time увеличивается.
   - Tick count увеличивается.
   - В консоли примерно раз в секунду: `[Game] Heartbeat | Time: X.Xs | Ticks: N`.

6. **Поведение кнопок:**
   - **Reset Heartbeat** — счётчики времени и тиков сбрасываются (и на панели, и в heartbeat-логах).
   - **Print Status** — в консоль выводится одна строка статуса, например: `[Game] Elapsed: X.XXs, Ticks: N`.

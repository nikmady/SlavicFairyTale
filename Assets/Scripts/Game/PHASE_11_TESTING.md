# Phase 11 — Interaction System: как проверить

Phase 11 вводит **универсальную систему взаимодействий**: игрок нажимает **E**, система ищет ближайший объект в радиусе и вызывает его `Interact()`. Без боёвки, UI, инвентаря и квестов.

---

## Что добавлено

- **IInteractable** — интерфейс: `WorldPosition`, `InteractionRadius`, `CanInteract()`, `Interact()`.
- **InteractionRequest** — структура запроса: `origin`, `radius` (поиск вокруг игрока).
- **InteractionSystem** — список зарегистрированных IInteractable; `Register`/`Unregister`; `TryInteract(request)` находит ближайший в радиусе (с учётом `InteractionRadius` и `CanInteract()`), вызывает `Interact()`.
- **WorldRuntime** — поле `InteractionSystem`, инициализация в `Initialize()`, публичный геттер для регистрации объектов. В `Tick()` при нажатии **E** формируется `InteractionRequest` от `PlayerAnchor.Position` (радиус 5) и вызывается `TryInteract()`.
- **Заглушки** (MonoBehaviour): **ResourceInteractable**, **RuneInteractable**, **NpcInteractable** — реализуют IInteractable, публичное поле `interactionRadius = 2f`, в `Interact()` лог `[Game] Interacted with Resource/Rune/NPC`; в OnEnable регистрируются в `GameRoot.CurrentWorldRuntime.InteractionSystem`, в OnDisable снимаются с регистрации.

---

## Как проверить по шагам

### 1. Без объектов в сцене

1. **Play** → **Go WorldMap** → выберите узел → **RunLocation**.
2. Нажмите **E** — в консоль ничего не выводится (нет зарегистрированных объектов).
3. Управление WASD и камера работают как раньше.

### 2. С одним объектом

1. В сцене или в префабе чанка создайте пустой GameObject (например «TestResource»).
2. Добавьте компонент **ResourceInteractable** (или RuneInteractable / NpcInteractable).
3. Задайте позицию в мире так, чтобы игрок мог подойти (в зоне стриминга и в пределах 5 единиц от старта, или подойдите к объекту WASD).
4. **Play** → зайдите в RunLocation, подойдите к объекту (расстояние до объекта ≤ его `interactionRadius`, по умолчанию 2).
5. Нажмите **E** — в консоли: `[Game] Interacted with Resource` (или Rune/NPC в зависимости от компонента).

### 3. Несколько объектов

1. Разместите несколько объектов с разными типами (Resource, Rune, NPC) так, чтобы один был ближе к игроку.
2. Подойдите так, чтобы в радиус запроса (5) попадали несколько объектов.
3. Нажмите **E** — должен сработать **ближайший** (один лог в консоль).

### 4. Регистрация при появлении в мире

- Объекты с компонентами *Resource* / *Rune* / *Npc* Interactable регистрируются при **OnEnable** и снимаются при **OnDisable**.
- Если объекты появляются через стриминг чанков (GameObject с таким компонентом создаётся при LoadChunk), они автоматически регистрируются при включении.

---

## Краткий чеклист

| Действие | Ожидание |
|----------|----------|
| E без объектов в радиусе | Ничего |
| E рядом с ResourceInteractable | `[Game] Interacted with Resource` |
| E рядом с RuneInteractable | `[Game] Interacted with Rune` |
| E рядом с NpcInteractable | `[Game] Interacted with NPC` |
| Несколько в радиусе | Срабатывает ближайший |

---

## Итог

- **Протестить Phase 11** = зайти в RunLocation, поставить объект с одним из *Interactable*, подойти и нажать **E** — в консоли соответствующий лог. Система готова к расширению (ресурсы, NPC, квесты позже).

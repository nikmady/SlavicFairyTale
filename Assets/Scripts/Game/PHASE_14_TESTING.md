# Phase 14 — подробная настройка и проверка

Дополнительные скрипты **не нужны**. Всё уже есть в проекте. Ниже — что именно используется и как всё настроить по шагам.

---

## Какие скрипты уже есть (ничего не дописывать)

- **AbilityConfig.cs** — конфиг способности (ScriptableObject). В Inspector появляется пункт меню **Create → Game → Ability Config**.
- **AbilityEffect.cs**, **DealDamageEffect.cs** — эффект «нанести урон».
- **AbilityRuntime.cs**, **AbilitySystem.cs** — кулдауны и активация способностей.
- Загрузка конфигов в коде: `Resources.LoadAll<AbilityConfig>("")` — подхватывает все Ability Config из любой папки **Resources**.

Если пункта **Create → Game → Ability Config** в меню нет — убедись, что скрипт **AbilityConfig.cs** висит в проекте и нет ошибок компиляции (Console без красного).

---

## Часть 1. Настройка способности (только для Phase 14)

### 1.1. Папка Resources

- В окне **Project** найди папку **Assets**.
- Если папки **Resources** нет: ПКМ по **Assets** → **Create** → **Folder** → назови папку **Resources** (имя именно так, с большой R).
- Зайди внутрь **Assets/Resources**.

### 1.2. Создать конфиг способности

- ПКМ по папке **Resources** (или по любой папке внутри Assets, куда потом положишь конфиг; для загрузки он **обязательно** должен лежать в **Resources**).
- В меню выбери: **Create** → **Game** → **Ability Config**.
- Появится файл вида `AbilityConfig.asset`. Переименуй при желании (например `BasicStrike`).
- **Важно:** для загрузки конфиг должен лежать **внутри папки Resources**. Если создал в другой папке — перетащи файл `.asset` в **Assets/Resources**.

### 1.3. Заполнить конфиг в Inspector

- Кликни по созданному ассету (`.asset`). В **Inspector** заполни поля:

| Поле | Что вписать | Пример |
|------|-------------|--------|
| **Ability Id** | Любой короткий id способности | `basic_strike` |
| **Cooldown** | Кулдаун в секундах | `2` |
| **Range** | Дистанция до цели (враг должен быть ближе) | `10` |
| **Power** | Урон способности | `25` |
| **Effect Type** | Выпадающий список | **Deal Damage** |

- Сохрани сцену / проект (Ctrl+S). Дополнительные скрипты для способности **не нужны**.

---

## Часть 2. Чтобы проверять бой (враги и мир)

Чтобы в игре появлялись враги и срабатывали логи `[Combat]` и `[Ability]`, должны быть настроены мир и враги из **более ранних фаз**. Кратко, что должно быть.

### 2.1. Враг (Phase 11C)

1. **EnemyConfig**  
   ПКМ в Project → **Create** → **Game** → **Enemy Config**.  
   Заполни: **Enemy Id** (например `enemy_skeleton`), **Max Health** (например `50`), **Enemy View Prefab** (префаб с компонентом **EnemyView** — см. ниже).

2. **Префаб врага (EnemyView)**  
   - Создай пустой GameObject (или с спрайтом).  
   - Добавь на него компонент **EnemyView** (скрипт из `Scripts/Game/Runtime/Enemy/EnemyView.cs`).  
   - Сохрани как префаб (перетащи в папку в Project).  
   - Этот префаб укажи в поле **Enemy View Prefab** в EnemyConfig.

3. **EnemyConfigDatabase**  
   ПКМ в Project → **Create** → **Game** → **Enemy Config Database**.  
   В Inspector в список **Configs** перетащи свой **EnemyConfig**.  
   Сохрани ассет **в папку Resources** с именем **EnemyConfigDatabase** (без лишних символов), чтобы код смог загрузить: `Resources.Load<EnemyConfigDatabase>("EnemyConfigDatabase")`.

### 2.2. Чанк с точкой спавна врага

- У тебя есть префаб чанка локации (или сцена чанка). На его корень должен висеть скрипт **ChunkEnemyRegistrar**.
- Внутри чанка создай дочерний пустой GameObject. Добавь на него скрипт **EnemySpawnPoint**. В Inspector у **EnemySpawnPoint** поле **Enemy Id** должно совпадать с **Enemy Id** из EnemyConfig (например `enemy_skeleton`).
- Позиция этого объекта — место появления врага в мире.

### 2.3. Локация и стриминг

- Должен быть **LocationChunkSet** (Create → Game → Location Chunk Set), в нём список чанков этой локации. Один из чанков — тот, где есть EnemySpawnPoint.
- LocationChunkSet должен лежать в **Resources** (или загружаться так же, как в Phase 08), чтобы при заходе в локацию подгружался нужный чанк с врагами.

Если что-то из этого ещё не настроено — сначала сделай минимальный вариант по Phase 11C (один EnemyConfig, одна база, один чанк с одной точкой спавна), потом возвращайся к проверке Phase 14.

---

## Часть 3. Как запустить и что проверить

1. Запусти игру (Play).
2. Перейди в режим локации с врагами (как у тебя принято: например World Map → выбор узла → загрузка локации и переход в RunLocation).
3. Подойди к месту, где спавнятся враги (ближе чем **Range** способности, например до 10 единиц).
4. Смотри **Console**:
   - Должно появиться: `[Combat] Player sees X enemies`.
   - Примерно раз в секунду: либо `[Ability] Activated basic_strike` и `[Ability] Cooldown started`, либо `[Combat] Player hits Enemy for 10` (обычный удар, если способность на кулдауне).
   - После смерти врага: `[Combat] Enemy died`.

---

## Часть 4. Если что-то не работает

| Проблема | Что проверить |
|----------|----------------|
| В меню нет **Create → Game → Ability Config** | Ошибки компиляции в Console; наличие **AbilityConfig.cs** в проекте. |
| Способность не срабатывает, только «Player hits Enemy for 10» | Конфиг способности лежит **в папке Resources**; у конфига заполнены Ability Id, Range, Power, Effect Type = Deal Damage. Враг ближе чем **Range**. |
| Нет сообщения «Player sees X enemies» | Есть ли враги на чанке (EnemySpawnPoint + ChunkEnemyRegistrar), подходишь ли достаточно близко; загружается ли **EnemyConfigDatabase** из Resources и есть ли в нём конфиг с нужным **enemyId**. |
| Ошибки при загрузке EnemyConfigDatabase | В Resources лежит ассет с **именем** `EnemyConfigDatabase` (именно так). |

---

## Итог

- **Скрипты:** все нужные для Phase 14 уже в проекте, доп. скрипты не нужны.
- **Способность:** один раз создать **Ability Config** через **Create → Game → Ability Config**, заполнить поля, положить ассет в **Resources**.
- **Бой:** для полной проверки нужны настроенные враги и чанки (Phase 11C); тогда в RunLocation появятся логи способности и комбата.

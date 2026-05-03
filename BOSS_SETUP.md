# Настройка босса в Unity

## Архитектурные изменения

Боссы теперь используют **единую State Machine** вместе с обычными врагами. Логика атаки босса вынесена в `BossAttackSO` (наследник `EnemyAttackSOBase`), что полностью унифицирует архитектуру.

**Ключевые изменения:**
- Удален класс `BossAttackState`
- `BossEnemy` теперь использует стандартный `EnemyAttackState` с кастомным `BossAttackSO`
- `BossAttackControllerSO` отвечает только за выбор ability и параметры
- `BossAttackSO` отвечает за исполнение атак (MeleeSlam, Ranged, QuickShot)

---

## Структура компонентов босса

```
BossEnemy (MonoBehaviour)
├── EnemyAttackBase (ScriptableObject) → BossAttackSO
├── AttackController (ScriptableObject) → BossAttackControllerSO
├── EnemyIdleBase → BossIdleBase.asset
├── EnemyChaseBase → BossChaseSO.asset
├── EnemyInvestigateBase → BossInvestigateBase.asset
├── EnemyNavMeshAgent2D
├── EnemyHealth
├── EnemyAggroCheck
└── Animator
```

---

## Пошаговая настройка

### 1. Создание ScriptableObject для атаки босса

1. В Project окне перейдите в папку `Assets/_Scripts/Enemy/Boss/`
2. Правой кнопкой → **Create** → **Enemy Logic** → **Boss** → **Boss Attack**
3. Назовите ассет: `BossAttack_[BossName].asset` (например, `BossAttack_Tank.asset`)

### 2. Назначение ассетов на префаб босса

Откройте префаб босса (Tank, Agile или Summoner) и в компоненте **BossEnemy** настройте:

#### Поля ScriptableObject:
- **Enemy Attack Base** → перетащите созданный `BossAttack_[BossName].asset`
- **Attack Controller** → перетащите соответствующий `BossAttackControllerSO`:
  - Tank: `Boss1TankController.asset`
  - Agile: `Boss2AgileController.asset`
  - Summoner: `Boss3SummonerController.asset`

#### Остальные компоненты (уже должны быть):
- **Enemy Idle Base** → `BossIdleBase.asset`
- **Enemy Chase Base** → `BossChaseSO.asset` (или `BossChaseDirect.asset`)
- **Enemy Investigate Base** → `BossInvestigateBase.asset`
- **Enemy Health** → настройте MaxHealth
- **NavMesh Agent** → скорость и радиус
- **EnemyNavMeshAgent2D** → параметры навигации

### 3. Настройка BossAttackControllerSO

Откройте `BossAttackControllerSO` ассет босса и заполните список **Abilities**:

#### Общие параметры Ability:
- **Type** — тип способности (MeleeSlam, Ranged, QuickShot, Charge, Dash, Summon)
- **Cooldown** — время перезарядки
- **ChanceWeight** — вес шанса выбора (если несколько доступных)
- **RequireLineOfSight** — требуется ли обзор игрока
- **MinRange / MaxRange** — дистанция для использования

#### Параметры для каждого типа:

**MeleeSlam:**
- Damage — урон
- Radius — радиус поражения
- Windup — задержка перед ударом
- AttackDuration — длительность атаки
- AnimationTrigger — триггер анимации (например, "MeleeSlam")

**Ranged:**
- Damage — урон снаряда
- ProjectileSpeed — скорость
- ProjectileLifetime — время жизни
- ProjectilePrefab — префаб снаряда
- FireRate — выстрелов в секунду
- BurstCount — количество снарядов за одну активацию
- AnimationTrigger — "RangedAttack"

**QuickShot:**
- VolleySize — количество снарядов в залпе
- SpreadAngle — угол разброса (градусы)
- Damage — урон
- ProjectileSpeed — скорость
- ProjectileLifetime — время жизни
- ProjectilePrefab — префаб
- VolleyDelay — задержка между выстрелами
- AnimationTrigger — "QuickShot"

**Charge** (special):
- ChargeSpeed — скорость рывка
- ChargeDuration — длительность
- ChargeDamage — урон от столкновения
- AoeRadius — радиус AoE удара
- Windup — задержка перед рывком
- VulnerableDuration — время уязвимости после способности
- AnimationTrigger — "Charge"

**Dash** (special):
- DashSpeed — скорость
- DashDuration — длительность
- DashAwayFromPlayer — true=отбегает, false=к игроку/перпендикулярно
- VulnerableDuration — время уязвимости
- AnimationTrigger — "Dash"

**Summon** (special):
- MinionPrefabs — массив префабов миньонов
- WaveSize — количество за одну волну
- SummonDuration — длительность призыва
- WaitForMinionsDeath — true=ждать смерти миньонов, false=по таймеру
- AnimationTrigger — "Summon"

### 4. Настройка уязвимостей

В компоненте **BossEnemy**:
- **Use Ranged Vulnerability** — активно для Tank босса (уязвим к ranged после Charge)
- **Use Hook Vulnerability** — активно для Agile босса (уязвим к хуку после Dash)

Для босса-призывателя (Summoner) оба флага должны быть выключены.

### 5. Проверка State Machine

**States** (автоматически):
- **Idle** → ожидание,使用 `BossIdleBase.asset`
- **Chase** → преследование,使用 `BossChaseSO.asset`
- **Attack** → атака,使用 `BossAttackSO.asset`
- **Investigate** → расследование шума,使用 `BossInvestigateBase.asset`
- **Special** (временные): Charge/Dash/Summon → соответствующие Boss*State

**Transition логика** (встроена в SO):
- Idle → при Aggro → Chase
- Chase → в пределах дистанции атаки → Attack
- Chase → потерял aggro → Investigate
- Attack → завершил → Chase
- Любое состояние → special ability → Special State → Chase

### 6. Настройка Animator

У босса должен быть Animator Controller с параметрами:
- **State** — integer, соответствующий `EnemyAnimState` (Idle=0, Alert=1, Chase=2, Attack=3, Death=4)
- Триггеры: "MeleeSlam", "RangedAttack", "QuickShot", "Charge", "Dash", "Summon"

Анимации должны содержать события или длительность, синхронизированную с параметрами в BossAttackControllerSO (windup, attackDuration и т.д.).

---

## Пример: Настройка Tank босса

1. Создайте `BossAttack_Tank.asset` (Create → Enemy Logic/Boss/Boss Attack)
2. На префабе `Tank.prefab`:
   - Enemy Attack Base = `BossAttack_Tank.asset`
   - Attack Controller = `Boss1TankController.asset`
   - Use Ranged Vulnerability = `true`
   - Use Hook Vulnerability = `false`
3. В `Boss1TankController.asset` добавьте abilities:
   - ability 0: Type=MeleeSlam, Cooldown=2, MinRange=0, MaxRange=2, MeleeSlamParams (damage=15, radius=1.2, windup=0.4, attackDuration=0.6)
   - ability 1: Type=Ranged, Cooldown=3, MinRange=3, MaxRange=10, RangedParams (damage=10, projectileSpeed=8, burstCount=3, fireRate=2)
   - ability 2: Type=Charge, Cooldown=8, MinRange=2, MaxRange=8, ChargeParams (chargeSpeed=10, chargeDuration=0.8, chargeDamage=25, aoeRadius=1.5, windup=0.3, vulnerableDuration=2)
4. Сохраните префаб.

---

## Совместимость с существующей системой

- **EnemyAggroCheck** — работает без изменений
- **EnemyNavMeshAgent2D** — используется для движения
- **EnemyHealth** — здоровье босса
- **Player abilities** — взаимодействуют через `IDamageable`
- **Hook vulnerability** — активируется `HookProjectile` при попадании в босса с `UsesHookVulnerability`
- **Ranged vulnerability** — активируется после Charge способности

---

## Debug

В консоли включены Debug.Log сообщения:
- `[BossEnemy]` — инициализация, обновление
- `[BossAttackController]` — выбор способностей
- `[BossAttackSO]` — исполнение атак
- `[BossChargeState]`, `[BossDashState]`, `[BossSummonState]` — special abilities

Для отключения найдите и закомментируйте Debug.Log в скриптах.

---

## Добавление нового босса

1. Создайте префаб с компонентами:
   - `BossEnemy`
   - `EnemyNavMeshAgent2D`
   - `EnemyHealth`
   - `EnemyAggroCheck` (с триггером Collider2D)
   - `Animator`
2. Создайте ассеты:
   - `BossAttackControllerSO` (с abilities)
   - `BossAttackSO` (пустой, логика в коде)
   - `BossChaseSO` (или унаследуйте от `EnemyChaseDirectToPlayer`)
   - `BossIdleBase` (или используйте стандартный)
   - `BossInvestigateBase` (или стандартный)
3. Назначьте ассеты в инспекторе префаба
4. Настройте параметры здоровья, навигации, уязвимости

---

**Важно:** Architecture теперь полностью соответствует паттерну обычных врагов. Босс — это просто враг с расширенными special states и кастомным `BossAttackSO`.

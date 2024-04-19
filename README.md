# Airball-Quantum

### Тестовая сетевая игра с использованием ECS фреймворка [Photon Quantum 2](https://www.photonengine.com/quantum)
<br/>
<br/>

[![Photon Quantum ECS game](https://img.youtube.com/vi/RdnWkFfr25o/0.jpg)](https://www.youtube.com/watch?v=RdnWkFfr25o)
<br/>
<br/>
## Установка
Нужно скачать весь репозиторий и распаковать его [airball-quantum.zip](https://github.com/unitydev17/Airball-Quantum/archive/refs/heads/master.zip).
Репозиторий содержит в себе unity проект [quantum_unity](https://github.com/unitydev17/Airball-Quantum/tree/master/quantum_unity) и solution симуляции в папке [quantum_code](https://github.com/unitydev17/Airball-Quantum/tree/master/quantum_code). Подробнее можно посмотреть на старнице установки SDK
<br><br>
[https://doc.photonengine.com/quantum/current/getting-started/initial-setup](https://doc.photonengine.com/quantum/current/getting-started/initial-setup)
<br><br>
Для запуска нужно открыть quantum-unity, и в конфигурации PhotonServerSetting.asset прописать AppId, полученное при создании новой игры в dashboard Quantum - необходима регистрация
<br><br>
[https://id.photonengine.com/account/signin](https://id.photonengine.com/account/signin)
<br><br>
## Код
Код разделен на два проекта, проект Unity и проект симуляции. При внесении изменений в проект симуляции его необходимо скомпилировать (!). При этом будут обновлены и сгенерированы общие для симуляции и Unity классы.

## [Quantum_code](https://github.com/unitydev17/Airball-Quantum/tree/master/quantum_code)

[Quantum_code](https://github.com/unitydev17/Airball-Quantum/tree/master/quantum_code) представляет собой ECS проект. 
Создание систем происходит в SystemSetup:
<br><br>
```C#
public static class SystemSetup
{
    public static SystemBase[] CreateSystems(RuntimeConfig gameConfig, SimulationConfig simulationConfig)
    {
        return new[]
        {
            // pre-defined core systems
            new Core.CullingSystem2D(),
            //new Core.CullingSystem3D(),

            new Core.PhysicsSystem2D(),
            //new Core.PhysicsSystem3D(),

            Core.DebugCommand.CreateSystem(),

            // new Core.NavigationSystem(),
            new Core.EntityPrototypeSystem(),
            new Core.PlayerConnectedSystem(),

            // user systems go here

            new PlayerSpawnSystem(),
            new PlayerMovementSystem(),
            new ScoreSystem(), 

            new BallSpawnSystem(),
            new BallMoveSystem(),
            new BallCollisionSystem(),
            new GoalSystem(),

            new PlayerCommandSystem(),

            new RestartSystem(),
            new SystemManagementSystem()
        };
    }
}
```
<br><br>
Пример системы создания мяча BallSpawnSystem.cs:
<br><br>
```C#
public unsafe class BallSpawnSystem : SystemSignalsOnly, ISignalOnPlayerDataSet
{
    public void OnPlayerDataSet(Frame f, PlayerRef player)
    {
        var ball = f.Filter<BallComponent>();    // фильтр получает все сущности (entity) содержащие компонент BallComponent
        if (ball.Next(out _, out _)) return;     // если есть хоть одна сущность с компонентом BallComponent, значит мяч создавать не нужно 

        var prototype = f.FindAsset<EntityPrototype>("Resources/DB/Ball|EntityPrototype");    // путь на ассет мяча
        var entity = f.Create(prototype);                                                     // создание сущности из ассета

        if (!f.Unsafe.TryGetPointer<Transform2D>(entity, out var transform)) return;          // получаем указатель на компонент Transform2D созданной игровой сущности

        var spawnPlaces = f.Filter<Spawn, Transform2D>();                                     // на сцене также есть объекты указывающие на места появления игроков и мяча, они представлены компонентами Spawn 
        while (spawnPlaces.Next(out _, out var spawn, out var spawnTr))                       // пробегаем по всем Spawn сущностям в цикле
        {
            if (spawn.index != 0) continue;                                                   // для мяча предусмотрен индекс 0
            transform->Position = spawnTr.Position;                                           // устанавливаем позицию мяча в точку со spawn.index = 0
            break;
        }
    }
}
```
<br><br>
Сообщения между системами внутри симуляции реализованы с помощью сигналов. 
Сигналы описываются с помощью DSL (Domain specific language) языка и расположены в *.qtn файлах.
Пример описания сигналов в Signals.qtn:
<br><br>
```C#
signal OnReadyToPlay();
signal OnGoalScored(int gateIndex);
signal DisableSystems();

```

<br><br>
Отправление сигнала из системы регистрации гола: 
<br><br>
```C#
public class GoalSystem : SystemSignalsOnly, ISignalOnTriggerEnter2D
{
    public void OnTriggerEnter2D(Frame f, TriggerInfo2D info)
    {
        var gateIndex = f.Get<Gate>(info.Other).index;            // получаем из фрейма сущность содержащую компонент Gate (ворота)
        f.Signals.OnGoalScored(gateIndex);                        // отправляем сигнал о том, что был забит гол и индекс ворот
    }
}
```
<br><br>
Получение сигнала в системе управления всеми системами: 
<br><br>
```C#
public class SystemManagementSystem : SystemSignalsOnly, ISignalOnGoalScored
{
    public void OnGoalScored(Frame f, int gateIndex)    // получаем сигнал о забитом голе - интерфейс ISignalOnGoalScored был сгенерирован ранее из Signals.qtn путем запуска проекта на компиляцию,
    {                                                   // в этой системе gateIndex не понадобился, в системе регистрации очков будет нужен
        DisableMainSystems(f);                          
    }

    private static void DisableMainSystems(Frame f)
    {
        f.SystemDisable<PlayerMovementSystem>();    // выключаем систему управления битами
        f.SystemDisable<Core.PhysicsSystem2D>();    // выключаем физическую систему
    }
}

```
<br><br>
Симуляция представляе собой модель (Model) игры, в то время как в Unity создается представление (View). Для передачи данных из модели в представление используются события (events).
Пример описания события гола в Goal.qtn: 
<br><br>
```C#
struct GoalStruct
{
    int value_1;
    int value_2;
}

event Goal
{
    GoalStruct goalStruct;
}
```
<br><br>
Отправление события гола из системы подсчета очков: 
<br><br>
```C#
public unsafe class ScoreSystem : SystemSignalsOnly, ISignalOnGoalScored
{
    public void OnGoalScored(Frame f, int gateIndex)
    {
        var players = f.Filter<PlayerLink, Score>();                            // достаем все сущности игроков, на каждом из которых присутствует компонент PlayerLink и Score
        var goalStruct = new GoalStruct();                                      // создаем структуру для события

        while (players.NextUnsafe(out _, out var playerLink, out var score))    // пробегаем по всем игрокам
        {
            var actorId = f.PlayerToActorId(playerLink->Player);                // actorId - это номер игрока, заданный фреймворком, нужен чтобы понять в чьи ворота забит гол
            if (actorId != gateIndex) score->value++;                           // увеличиваем очки игроку с другим индексом - тому, кто забил, а не тому чьи ворота

            if (playerLink->isMaster)                                            // первое значение счета, количество голов - для мастер-клиента (player1)
            {
                goalStruct.value_1 = score->value;
            }
            else
            {
                goalStruct.value_2 = score->value;                               // второе значение - для зависимого-клиента (player2)
            }                                                                    // в дальнейшем в unity части мы используем это для правильной последовательсти отображения счета
        }

        f.Events.Goal(goalStruct);                                               // отправляем сигнал в unity view
    }
}
```
<br><br>
Отправление события гола из системы подсчета очков: 
<br><br>
```C#

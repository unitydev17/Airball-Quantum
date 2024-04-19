# Airball-Quantum

### Тестовая сетевая игра с использованием ECS фреймворка [Photon Quantum 2](https://www.photonengine.com/quantum)
<br/>
<br/>

[![Photon Quantum ECS game](https://img.youtube.com/vi/RdnWkFfr25o/0.jpg)](https://www.youtube.com/watch?v=RdnWkFfr25o)
<br/>
<br/>
## Установка

Репозиторий содержит в себе unity проект quantum-unity и solution симуляции в папке quantum-code. Подробнее можно посмотреть на старнице установки SDK
<br><br>
[https://doc.photonengine.com/quantum/current/getting-started/initial-setup](https://doc.photonengine.com/quantum/current/getting-started/initial-setup)
<br><br>
Для запуска нужно открыть quantum-unity, и в конфигурации PhotonServerSetting.asset прописать AppId, полученное при создании новой игры в dashboard Quantum - необходима регистрация
<br><br>
[https://id.photonengine.com/account/signin](https://id.photonengine.com/account/signin)
<br><br>
## Код
Код разделен на два проекта, проект Unity и проект симуляции. При внесении изменений в проект симуляции его необходимо скомпилировать (!). При этом будут обновлены и сгенерированы общие для симуляции и Unity классы.


## Памятки по коду

quantum_code представляет собой ECS проект. Создание систем происходит в SytemSetup:


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

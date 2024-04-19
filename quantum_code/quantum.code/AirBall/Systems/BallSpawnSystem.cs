namespace Quantum.AirBall
{
    public unsafe class BallSpawnSystem : SystemSignalsOnly, ISignalOnPlayerDataSet
    {
        public void OnPlayerDataSet(Frame f, PlayerRef player)
        {
            var ball = f.Filter<BallComponent>();
            if (ball.Next(out _, out _)) return;

            var prototype = f.FindAsset<EntityPrototype>("Resources/DB/Ball|EntityPrototype");
            var entity = f.Create(prototype);

            if (!f.Unsafe.TryGetPointer<Transform2D>(entity, out var transform)) return;

            var spawnPlaces = f.Filter<Spawn, Transform2D>();
            while (spawnPlaces.Next(out _, out var spawn, out var spawnTr))
            {
                if (spawn.index != 0) continue;
                transform->Position = spawnTr.Position;
                break;
            }
        }
    }
}
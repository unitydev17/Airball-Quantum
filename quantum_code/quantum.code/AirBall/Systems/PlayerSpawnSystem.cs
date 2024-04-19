namespace Quantum.AirBall
{
    public unsafe class PlayerSpawnSystem : SystemSignalsOnly, ISignalOnPlayerDataSet
    {
        public void OnPlayerDataSet(Frame f, PlayerRef player)
        {
            // Add entity

            var data = f.GetPlayerData(player);

            var prototype = f.FindAsset<EntityPrototype>(data.CharacterPrototype.Id);
            var entity = f.Create(prototype);
            var playerLink = new PlayerLink
            {
                Player = player,
                isMaster = data.isMaster
            };
            f.Add(entity, playerLink);


            // Set entity transform position

            var actorId = f.PlayerToActorId(playerLink.Player);
            if (!f.Unsafe.TryGetPointer<Transform2D>(entity, out var transform)) return;
            var spawnPlaces = f.Filter<Spawn, Transform2D>();

            while (spawnPlaces.Next(out _, out var spawn, out var spawnTr))
            {
                if (spawn.index != actorId) continue;
                transform->Position = spawnTr.Position;
                break;
            }
        }
    }
}
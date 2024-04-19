namespace Quantum.AirBall
{
    public unsafe class PlayerMovementSystem : SystemMainThreadFilter<PlayerMovementSystem.Filter>, ISignalOnComponentAdded<TopDownKCC>
    {
        public struct Filter
        {
            public EntityRef entity;
            public TopDownKCC* kcc;
        }

        public void OnAdded(Frame f, EntityRef entity, TopDownKCC* component)
        {
            var settings = f.FindAsset<TopDownKCCSettings>(component->Settings.Id);
            settings.Init(ref *component);
        }

        public override void Update(Frame f, ref Filter filter)
        {
            Input input = default;

            if (f.Unsafe.TryGetPointer(filter.entity, out PlayerLink* playerLink))
            {
                input = *f.GetPlayerInput(playerLink->Player);
            }

            filter.kcc->Move(f, filter.entity, input.MouseDirection.XY);

            f.AddOrGet<Velocity>(filter.entity, out var velocity);
            velocity->value = input.MouseDirection;
        }
    }
}
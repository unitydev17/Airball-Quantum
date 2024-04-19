namespace Quantum.AirBall
{
    public unsafe class BallCollisionSystem : SystemMainThreadFilter<BallCollisionSystem.Filter>, ISignalOnCollisionEnter2D
    {
        public struct Filter
        {
            public EntityRef entity;
            public HitComponent* hit;
            public PhysicsBody2D* pb;
        }

        public void OnCollisionEnter2D(Frame f, CollisionInfo2D info)
        {
            var ballEntity = info.Entity;
            var puddleEntity = info.Other;

            if (!f.Has<Velocity>(puddleEntity)) return;

            var puddleVelocity = f.Get<Velocity>(puddleEntity);
            var hit = new HitComponent
            {
                direction = puddleVelocity.value
            };

            f.Add(ballEntity, hit);
        }


        public override void Update(Frame f, ref Filter filter)
        {
            f.Remove<HitComponent>(filter.entity);
            filter.pb->AddLinearImpulse(filter.hit->direction);
        }
    }
}
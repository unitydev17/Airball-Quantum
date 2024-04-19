using Photon.Deterministic;

namespace Quantum.AirBall
{
    public unsafe class BallMoveSystem : SystemMainThreadFilter<BallMoveSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef entity;
            public BallComponent* ballComponent;
            public PhysicsBody2D* pb;
        }


        public override void Update(Frame f, ref Filter filter)
        {
            var velocity = filter.pb->Velocity;
            velocity = FPVector2.ClampMagnitude(velocity, filter.ballComponent->MaxSpeed);
            filter.pb->Velocity = velocity;
        }
    }
}
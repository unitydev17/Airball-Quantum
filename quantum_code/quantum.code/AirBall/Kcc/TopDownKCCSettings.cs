using System;
using Photon.Deterministic;
using Quantum.Core;

namespace Quantum
{
    public enum TopDownKCCMovementType
    {
        None,
        Free,
        Tangent
    }

    public struct TopDownKCCMovementData
    {
        public TopDownKCCMovementType Type;
        public FPVector2 Correction;
        public FPVector2 Direction;
        public FP MaxPenetration;
    }

    public partial struct TopDownKCC
    {
        public void Move(FrameBase f, EntityRef entity, FPVector2 direction, int layerMask = -1, QueryOptions queryOptions = QueryOptions.HitAll)
        {
            var settings = f.FindAsset<TopDownKCCSettings>(Settings.Id);
            var movement = settings.ComputeRawMovement(f, entity, direction, layerMask, queryOptions);
            settings.SteerAndMove(f, entity, movement);
        }
    }

    public unsafe partial class TopDownKCCSettings
    {
        // This is the KCC actual radius (non penetrable)
        public FP Radius = FP._0_50;
        public Int32 MaxContacts = 2;
        public FP AllowedPenetration = FP._0_10;
        public FP CorrectionSpeed = FP._10;
        public FP BaseSpeed = FP._2;
        public FP Acceleration = FP._10;
        public Boolean Debug = false;
        public FP Brake = 1;

        public void Init(ref TopDownKCC kcc)
        {
            kcc.Settings = this;
            kcc.MaxSpeed = BaseSpeed;
            kcc.Acceleration = Acceleration;
        }

        public void SteerAndMove(FrameBase f, EntityRef entity, in TopDownKCCMovementData movementData)
        {
            TopDownKCC* kcc = null;
            if (f.Unsafe.TryGetPointer(entity, out kcc) == false)
            {
                return;
            }

            Transform2D* transform = null;
            if (f.Unsafe.TryGetPointer(entity, out transform) == false)
            {
                return;
            }

            Assert.Check((kcc->Acceleration == 0 && kcc->MaxSpeed == 0) == false, $"Acceleration and MaxSpeed equal 0. Did you forget to call Init on the TopDownKCC?");

            if (movementData.Type != TopDownKCCMovementType.None)
            {
                kcc->Velocity += kcc->Acceleration * f.DeltaTime * movementData.Direction;
                if (kcc->Velocity.SqrMagnitude > kcc->MaxSpeed * kcc->MaxSpeed)
                {
                    kcc->Velocity = kcc->Velocity.Normalized * kcc->MaxSpeed;
                }
                //transform->Rotation = FPVector2.RadiansSigned(FPVector2.Up, movementData.Direction);// FPMath.Atan2(kcc->Velocity.Y, kcc->Velocity.X);
            }
            else
            {
                // brake instead?
                kcc->Velocity = FPVector2.MoveTowards(kcc->Velocity, FPVector2.Zero, f.DeltaTime * Brake);
            }

            if (movementData.MaxPenetration > AllowedPenetration)
            {
                if (movementData.MaxPenetration > AllowedPenetration * 2)
                {
                    transform->Position += movementData.Correction;
                }
                else
                {
                    transform->Position += movementData.Correction * f.DeltaTime * CorrectionSpeed;
                }

            }


            transform->Position += (kcc->Velocity + kcc->Force) * f.DeltaTime;


#if DEBUG
            if (Debug)
            {
                Draw.Circle(transform->Position, Radius, ColorRGBA.ColliderBlue);
                Draw.Ray(transform->Position, kcc->Velocity, ColorRGBA.Blue);
                Draw.Ray(transform->Position, kcc->Force, ColorRGBA.Red);
            }
#endif
            // reset force every tick
            kcc->Force = default;

        }

        public TopDownKCCMovementData ComputeRawMovement(FrameBase f, EntityRef entity, FPVector2 direction, int layerMask = -1, QueryOptions queryOptions = QueryOptions.HitAll)
        {
            TopDownKCC* kcc = null;
            if (f.Unsafe.TryGetPointer(entity, out kcc) == false)
            {
                return default;
            }

            Transform2D* transform = null;
            if (f.Exists(entity) == false || f.Unsafe.TryGetPointer(entity, out transform) == false)
            {
                return default;
            }

            TopDownKCCMovementData movementPack = default;


            movementPack.Type = direction != default ? TopDownKCCMovementType.Free : TopDownKCCMovementType.None;
            movementPack.Direction = direction;
            Shape2D shape = Shape2D.CreateCircle(Radius);

            var hits = f.Physics2D.OverlapShape(transform->Position, FP._0, shape, layerMask, options: queryOptions | QueryOptions.ComputeDetailedInfo);
            int count = Math.Min(MaxContacts, hits.Count);

            if (hits.Count > 0)
            {
                Boolean initialized = false;
                hits.Sort(transform->Position);
                for (int i = 0; i < hits.Count && count > 0; i++)
                {
                    // ignore triggers
                    if (hits[i].IsTrigger)
                    {
                        // callback here...
                        continue;
                    }

                    // ignoring "self" contact
                    if (hits[i].Entity == entity)
                    {
                        continue;
                    }

                    var contactPoint = hits[i].Point;
                    var contactToCenter = transform->Position - contactPoint;
                    var localDiff = contactToCenter.Magnitude - Radius;
                    var localNormal = contactToCenter.Normalized;

                    var other = hits[i].Entity;

                    if (other != default && f.Exists(other) == true && f.Has<TopDownKCC>(other) && f.TryGet<PhysicsCollider2D>(other, out var otherCollider))
                    {
                        var otherTransform = f.Get<Transform2D>(other);
                        var centerToCenter = otherTransform.Position - transform->Position;
                        var maxRadius = FPMath.Max(Radius, otherCollider.Shape.Circle.Radius);
                        if (centerToCenter.Magnitude <= maxRadius)
                        {
                            localDiff = -maxRadius;
                            localNormal = entity.Index > other.Index ? FPVector2.Right : FPVector2.Left;
                        }
                    }

#if DEBUG
                    if (Debug)
                    {
                        Draw.Circle(contactPoint, FP._0_10, ColorRGBA.Red);
                    }
#endif

                    count--;

                    // define movement type
                    if (!initialized)
                    {
                        initialized = true;

                        if (direction != default)
                        {
                            var angle = FPVector2.RadiansSkipNormalize(direction.Normalized, localNormal);
                            if (angle >= FP.Rad_90)
                            {
                                var d = FPVector2.Dot(direction, localNormal);
                                var tangentVelocity = direction - localNormal * d;
                                if (tangentVelocity.SqrMagnitude > FP.EN4)
                                {
                                    movementPack.Direction = tangentVelocity.Normalized;
                                    movementPack.Type = TopDownKCCMovementType.Tangent;
                                }
                                else
                                {
                                    movementPack.Direction = default;
                                    movementPack.Type = TopDownKCCMovementType.None;
                                }

                            }
                        }
                        movementPack.MaxPenetration = FPMath.Abs(localDiff);
                    }

                    // any real contact contributes to correction and average normal
                    var localCorrection = localNormal * -localDiff;
                    movementPack.Correction += localCorrection;
                }
            }

            return movementPack;
        }
    }
}
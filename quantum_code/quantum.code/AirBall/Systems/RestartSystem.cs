using Photon.Deterministic;
using Quantum.Core;

namespace Quantum.AirBall
{
    public unsafe class RestartSystem : SystemSignalsOnly, ISignalOnReadyToPlay
    {
        public void OnReadyToPlay(Frame f)
        {
            ResetPlayers(f);
            ResetBall(f);

            f.Events.Restarted();
        }

        private void ResetBall(FrameBase f)
        {
            f.Filter<BallComponent>().Next(out var entity, out _);
            f.Unsafe.TryGetPointer<Transform2D>(entity, out var ballTr);
            f.Unsafe.TryGetPointer<PhysicsBody2D>(entity, out var ballPb);

            var spawns = f.Filter<Spawn, Transform2D>();
            while (spawns.Next(out _, out var spawn, out var spawnTr))
            {
                if (spawn.index != 0) continue;

                ballTr->Position = spawnTr.Position;
                ballPb->Velocity = FPVector2.Zero;
                break;
            }
        }

        private static void ResetPlayers(Frame f)
        {
            var players = f.Filter<PlayerLink, Transform2D>();
            while (players.NextUnsafe(out _, out var playerLink, out var playerTr))
            {
                var spawns = f.Filter<Spawn, Transform2D>();
                while (spawns.Next(out _, out var spawn, out var spawnTr))
                {
                    if (spawn.index != f.PlayerToActorId(playerLink->Player)) continue;

                    playerTr->Position = spawnTr.Position;
                    break;
                }
            }
        }
    }
}
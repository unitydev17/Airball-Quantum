namespace Quantum.AirBall
{
    public class SystemManagementSystem : SystemSignalsOnly, ISignalOnGoalScored, ISignalOnReadyToPlay, ISignalDisableSystems
    {
        public void OnGoalScored(Frame f, int gateIndex)
        {
            DisableMainSystems(f);
        }

        public void OnReadyToPlay(Frame f)
        {
            EnableMainSystems(f);
        }

        private static void DisableMainSystems(Frame f)
        {
            f.SystemDisable<PlayerMovementSystem>();
            f.SystemDisable<Core.PhysicsSystem2D>();
        }

        private static void EnableMainSystems(Frame f)
        {
            f.SystemEnable<PlayerMovementSystem>();
            f.SystemEnable<Core.PhysicsSystem2D>();
        }

        public void DisableSystems(Frame f)
        {
            DisableMainSystems(f);
        }
    }
}
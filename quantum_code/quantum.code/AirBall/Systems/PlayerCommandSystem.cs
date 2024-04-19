using Quantum.AirBall.Commands;

namespace Quantum.AirBall
{
    public unsafe class PlayerCommandSystem : SystemMainThreadFilter<PlayerCommandSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef entity;
            public PlayersApproved* playersApproved;
        }


        public override void Update(Frame f, ref Filter filter)
        {
            var players = f.Filter<PlayerLink>();
            var counter = 0;
            while (players.Next(out _, out _)) counter++;


            for (var i = 0; i < counter; i++)
            {
                CheckContinueCmd(f, filter, i, counter);
                CheckDisconnectCmd(f, i);
            }
        }

        private static void CheckDisconnectCmd(Frame f, int i)
        {
            if (!(f.GetPlayerCommand(i) is DisconnectCommand command)) return;
            f.Events.Disconnect(command.actorId);
            f.Signals.DisableSystems();
        }

        private static void CheckContinueCmd(Frame f, Filter filter, int i, int counter)
        {
            if (!(f.GetPlayerCommand(i) is ContinueCommand)) return;

            if (counter == 1)
            {
                ContinueGame(f);
                return;
            }

            var count = ++filter.playersApproved->value;
            if (count != f.PlayerCount) return;

            filter.playersApproved->value = 0;
            ContinueGame(f);
        }

        private static void ContinueGame(Frame f)
        {
            f.Events.PlayersReady();
            f.Signals.OnReadyToPlay();
        }
    }
}
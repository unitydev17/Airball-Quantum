namespace Quantum.AirBall
{
    public unsafe class ScoreSystem : SystemSignalsOnly, ISignalOnGoalScored
    {
        public void OnGoalScored(Frame f, int gateIndex)
        {
            var players = f.Filter<PlayerLink, Score>();
            var goalStruct = new GoalStruct();

            while (players.NextUnsafe(out _, out var playerLink, out var score))
            {
                var actorId = f.PlayerToActorId(playerLink->Player);
                if (actorId != gateIndex) score->value++;

                if (playerLink->isMaster)
                {
                    goalStruct.value_1 = score->value;
                }
                else
                {
                    goalStruct.value_2 = score->value;
                }
            }

            f.Events.Goal(goalStruct); // communicate with Unity side
        }
    }
}
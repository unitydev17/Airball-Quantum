namespace Quantum.AirBall
{
    public class GoalSystem : SystemSignalsOnly, ISignalOnTriggerEnter2D
    {
        public void OnTriggerEnter2D(Frame f, TriggerInfo2D info)
        {
            var gateIndex = f.Get<Gate>(info.Other).index;
            f.Signals.OnGoalScored(gateIndex);
        }
    }
}
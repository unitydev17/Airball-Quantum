using Photon.Deterministic;

namespace Quantum.AirBall.Commands
{
    public class SpawnCommand : DeterministicCommand
    {
        public bool isMaster;
        public int actorNum;
        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref isMaster);
            stream.Serialize(ref actorNum);
        }
    }
}
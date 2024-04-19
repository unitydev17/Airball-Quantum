using Photon.Deterministic;

namespace Quantum.AirBall.Commands
{
    public class DisconnectCommand : DeterministicCommand
    {
        public int actorId;
        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref actorId);
        }
    }
}
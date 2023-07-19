using Unity.NetCode;

namespace Xedrial.NetCode.Components
{
    public struct PlayerCommand : ICommandData
    {
        public NetworkTick Tick { get; set; }
        
        public byte Right;
        public byte Left;
        public byte Thrust;
        public byte ReverseThrust;
        public byte SelfDestruct;
        public byte Shoot;
        public byte RotateRight;
        public byte RotateLeft;
    }
}

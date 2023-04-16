using Unity.NetCode;

namespace Xedrial.NetCode.Commands
{
    public struct SendClientGameRpc : IRpcCommand
    {
        public int LevelWidth;
        public int LevelHeight;
        public float PlayerForce;
        public float RotationSpeed;
        public float BulletVelocity;
        public float BulletsPerSecond;
    }
}

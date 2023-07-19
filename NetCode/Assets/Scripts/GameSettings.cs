using Unity.Entities;

namespace AsteroidsDamage
{
    public struct GameSettings : IComponentData
    {
        public int NumberOfAsteroids;
        public float AsteroidVelocity;
        public int LevelWidth;
        public int LevelHeight;
        public float PlayerForce;
        public float RotationSpeed;
        public float BulletForce;
        public float BulletsPerSecond;
    }
}

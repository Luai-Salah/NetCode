using Unity.Entities;
using UnityEngine;

namespace AsteroidsDamage
{
    public class GameSettingsAuthoring : MonoBehaviour
    {
        public int m_NumberOfAsteroids;
        public float m_AsteroidVelocity;
        public int m_LevelWidth;
        public int m_LevelHeight;
        public float m_PlayerForce;
        public float m_RotationSpeed;
        public float m_BulletForce;
        public float m_BulletsPerSecond;

        public class GameSettingsBaker : Baker<GameSettingsAuthoring>
        {
            public override void Bake(GameSettingsAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new GameSettings
                    {
                        NumberOfAsteroids = authoring.m_NumberOfAsteroids,
                        AsteroidVelocity = authoring.m_AsteroidVelocity,
                        LevelWidth = authoring.m_LevelWidth,
                        LevelHeight = authoring.m_LevelHeight,
                        PlayerForce = authoring.m_PlayerForce,
                        RotationSpeed = authoring.m_RotationSpeed,
                        BulletForce = authoring.m_BulletForce,
                        BulletsPerSecond = authoring.m_BulletsPerSecond
                    });
            }
        }
    }
}
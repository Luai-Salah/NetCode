using Unity.Entities;

namespace AsteroidsDamage
{
    public struct AsteroidAuthoringComponent : IComponentData
    {
        public Entity Prefab;
    }
}
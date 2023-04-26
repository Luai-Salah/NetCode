using Unity.Entities;

namespace AsteroidsDamage
{
    [GenerateAuthoringComponent]
    public struct AsteroidAuthoringComponent : IComponentData
    {
        public Entity Prefab;
    }
}
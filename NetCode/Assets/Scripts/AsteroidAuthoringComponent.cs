using Unity.Entities;
using UnityEngine;

namespace AsteroidsDamage
{
    [GenerateAuthoringComponent]
    public struct AsteroidAuthoringComponent : IComponentData
    {
        public Entity Prefab;
    }
}
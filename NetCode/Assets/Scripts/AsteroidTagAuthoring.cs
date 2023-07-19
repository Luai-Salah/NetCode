using Unity.Entities;
using UnityEngine;

namespace AsteroidsDamage
{
    public class AsteroidTagAuthoring : MonoBehaviour
    {
        public class AsteroidTagBaker : Baker<AsteroidTagAuthoring>
        {
            public override void Bake(AsteroidTagAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new AsteroidTag());
            }
        }
    }
}
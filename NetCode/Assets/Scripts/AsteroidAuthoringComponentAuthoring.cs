using Unity.Entities;
using UnityEngine;

namespace AsteroidsDamage
{
    public class AsteroidAuthoringComponentAuthoring : MonoBehaviour
    {
        public GameObject m_Prefab;

        public class AsteroidAuthoringComponentBaker : Baker<AsteroidAuthoringComponentAuthoring>
        {
            public override void Bake(AsteroidAuthoringComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new AsteroidAuthoringComponent
                    {
                        Prefab = GetEntity(authoring.m_Prefab, TransformUsageFlags.Dynamic)
                    });
            }
        }
    }
}
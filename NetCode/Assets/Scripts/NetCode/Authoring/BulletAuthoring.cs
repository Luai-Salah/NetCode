using Unity.Entities;
using UnityEngine;
using Xedrial.NetCode.Components;

namespace Xedrial.NetCode.Authoring
{
    public class BulletAuthoring : MonoBehaviour
    {
        public GameObject m_Prefab;

        public class BulletAuthoringComponentBaker : Baker<BulletAuthoring>
        {
            public override void Bake(BulletAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new BulletPrefab { Prefab = GetEntity(authoring.m_Prefab, TransformUsageFlags.Dynamic) });
            }
        }
    }
}
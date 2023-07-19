using Unity.Entities;
using UnityEngine;
using Xedrial.NetCode.Server.Components;

namespace Xedrial.NetCode.Server.Authoring
{
    public class BulletLifetimeAuthoring : MonoBehaviour
    {
        public float m_MaxValue;

        public class BulletLifetimeBaker : Baker<BulletLifetimeAuthoring>
        {
            public override void Bake(BulletLifetimeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BulletLifetime { MaxValue = authoring.m_MaxValue });
            }
        }
    }
}
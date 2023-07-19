using Unity.Entities;
using UnityEngine;
using Xedrial.NetCode.Components;

namespace Xedrial.NetCode.Client.Authoring
{
    public class BulletSpeedAuthoring : MonoBehaviour
    {
        public float m_Value;

        public class BulletSpeedBaker : Baker<BulletSpeedAuthoring>
        {
            public override void Bake(BulletSpeedAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BulletSpeed { Value = authoring.m_Value });
            }
        }
    }
}
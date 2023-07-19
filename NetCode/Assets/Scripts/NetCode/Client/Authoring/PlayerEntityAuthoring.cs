using Unity.Entities;
using UnityEngine;
using Xedrial.NetCode.Components;

namespace Xedrial.NetCode.Client.Authoring
{
    public class PlayerEntityAuthoring : MonoBehaviour
    {
        public GameObject m_Entity;

        public class PlayerEntityBaker : Baker<PlayerEntityAuthoring>
        {
            public override void Bake(PlayerEntityAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new PlayerEntity { Entity = GetEntity(authoring.m_Entity, TransformUsageFlags.Dynamic) });
            }
        }
    }
}
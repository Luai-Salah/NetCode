using Unity.Entities;
using UnityEngine;
using Xedrial.NetCode.Server.Components;

namespace Xedrial.NetCode.Server.Authoring
{
    public class PlayerPrefabAuthoring : MonoBehaviour
    {
        public GameObject m_Prefab;

        public class PlayerPrefabBaker : Baker<PlayerPrefabAuthoring>
        {
            public override void Bake(PlayerPrefabAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerPrefab
                {
                    Value = GetEntity(authoring.m_Prefab, TransformUsageFlags.None)
                });
            }
        }
    }
}
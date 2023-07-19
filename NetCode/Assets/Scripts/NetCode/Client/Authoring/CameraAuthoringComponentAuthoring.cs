using Unity.Entities;
using UnityEngine;
using Xedrial.NetCode.Client.Components;

namespace Xedrial.NetCode.Client.Authoring
{
    public class CameraAuthoringComponentAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject m_Prefab;

        public class CameraAuthoringComponentBaker : Baker<CameraAuthoringComponentAuthoring>
        {
            public override void Bake(CameraAuthoringComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new CameraAuthoringComponent { Prefab = GetEntity(authoring.m_Prefab, TransformUsageFlags.Dynamic) });
            }
        }
    }
}
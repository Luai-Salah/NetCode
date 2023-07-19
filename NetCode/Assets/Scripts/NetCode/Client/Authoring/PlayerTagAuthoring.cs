using Unity.Entities;
using UnityEngine;
using Xedrial.NetCode.Components;

namespace Xedrial.NetCode.Client.Authoring
{
    public class PlayerTagAuthoring : MonoBehaviour
    {
        public class PlayerTagBaker : Baker<PlayerTagAuthoring>
        {
            public override void Bake(PlayerTagAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerTag());
            }
        }
    }
}
using Unity.Entities;
using UnityEngine;
using Xedrial.NetCode.Components;

namespace Xedrial.NetCode.Client.Authoring
{
    public class PlayerCommandBufferAuthoring : MonoBehaviour
    {
        private class PlayerCommandBufferBaker : Baker<PlayerCommandBufferAuthoring>
        {
            public override void Bake(PlayerCommandBufferAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddBuffer<PlayerCommand>(entity);
            }
        }
    }
}
using Unity.Entities;
using Unity.Mathematics;

namespace Xedrial.NetCode.Components
{
    public struct BulletSpawnOffset : IComponentData
    {
        public float3 Value;
    }
}
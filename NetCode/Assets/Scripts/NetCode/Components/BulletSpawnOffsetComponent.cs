using Unity.Entities;
using Unity.Mathematics;

namespace Xedrial.NetCode.Components
{
    public struct BulletSpawnOffsetComponent : IComponentData
    {
        public float3 Value;
    }
}
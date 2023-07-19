using Unity.Entities;

namespace Xedrial.NetCode.Components
{
    public struct BulletPrefab : IComponentData
    {
        public Entity Prefab;
    }
}
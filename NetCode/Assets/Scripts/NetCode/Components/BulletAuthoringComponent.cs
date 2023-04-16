using Unity.Entities;

namespace Xedrial.NetCode.Components
{
    [GenerateAuthoringComponent]
    public struct BulletAuthoringComponent : IComponentData
    {
        public Entity Prefab;
    }
}
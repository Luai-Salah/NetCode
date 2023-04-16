using Unity.Entities;

namespace Xedrial.NetCode.Components
{
    [GenerateAuthoringComponent]
    public struct PlayerEntityComponent : IComponentData
    {
        public Entity PlayerEntity;
    }
}
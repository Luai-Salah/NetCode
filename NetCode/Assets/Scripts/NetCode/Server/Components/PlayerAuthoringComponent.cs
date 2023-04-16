using Unity.Entities;

namespace Xedrial.NetCode.Commands
{
    [GenerateAuthoringComponent]
    public struct PlayerAuthoringComponent : IComponentData
    {
        public Entity Prefab;
    }
}
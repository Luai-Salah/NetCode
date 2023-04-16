using Unity.Entities;

namespace Xedrial.NetCode.Components
{
    public struct PlayerSpawningStateComponent : IComponentData
    {
        public int IsSpawning;
    }
}
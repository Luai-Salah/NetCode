using Unity.Entities;
using Unity.NetCode;

namespace Xedrial.NetCode.Components
{
    public struct PlayerStateComponent : IComponentData
    {
        [GhostField] public int Value;
    }
}
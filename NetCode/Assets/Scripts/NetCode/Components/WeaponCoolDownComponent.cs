using Unity.Entities;

namespace Xedrial.NetCode.Components
{
    public struct WeaponCoolDownComponent : IComponentData
    {
        public uint Value;
    }
}
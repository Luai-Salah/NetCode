using Unity.Entities;

namespace Xedrial.NetCode.Components
{
    [GenerateAuthoringComponent]
    public struct BulletSpeedComponent : IComponentData
    {
        public float Value;
    }
}
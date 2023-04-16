using Unity.Entities;

namespace Xedrial.NetCode.Client.Components
{
    public struct CameraFollowComponent : IComponentData
    {
        public Entity Target;
    }
}
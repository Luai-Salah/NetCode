using Unity.Entities;

namespace Xedrial.NetCode.Client.Components
{
    public struct CameraAuthoringComponent : IComponentData
    {
        public Entity Prefab;
    }
}

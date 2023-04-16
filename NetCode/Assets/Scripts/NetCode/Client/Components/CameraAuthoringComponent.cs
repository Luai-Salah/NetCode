using Unity.Entities;

namespace Xedrial.NetCode.Client.Components
{
    [GenerateAuthoringComponent]
    public struct CameraAuthoringComponent : IComponentData
    {
        public Entity Prefab;
    }
}

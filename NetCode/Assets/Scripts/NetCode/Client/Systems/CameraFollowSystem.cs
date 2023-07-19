using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Xedrial.NetCode.Client.Components;

namespace Xedrial.NetCode.Client.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class CameraFollowSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            ComponentLookup<LocalTransform> translationData = SystemAPI.GetComponentLookup<LocalTransform>();

            Entities
                .ForEach((Entity entity, in CameraFollowComponent cameraFollow) =>
                {
                    if (!translationData.TryGetComponent(cameraFollow.Target, out LocalTransform targetTranslation))
                        return;
                    
                    targetTranslation.Position.z = translationData[entity].Position.z;
                    translationData[entity] = targetTranslation;
                }).Run();
        }
    }
}
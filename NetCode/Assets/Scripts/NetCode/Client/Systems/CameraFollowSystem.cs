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
            ComponentDataFromEntity<Translation> translationData = GetComponentDataFromEntity<Translation>();

            Entities
                .ForEach((Entity entity, in CameraFollowComponent cameraFollow) =>
                {
                    if (!translationData.TryGetComponent(cameraFollow.Target, out Translation targetTranslation))
                        return;
                    
                    float z = translationData[entity].Value.z;
                    translationData[entity] = new Translation {
                        Value = new float3(targetTranslation.Value.xy, z)
                    };
                }).Run();
        }
    }
}
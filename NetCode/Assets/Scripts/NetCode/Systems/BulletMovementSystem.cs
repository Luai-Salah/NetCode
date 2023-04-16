using Unity.NetCode;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

using Xedrial.NetCode.Components;

namespace Xedrial.NetCode.Systems
{
    [UpdateInWorld(TargetWorld.ClientAndServer)]
    [UpdateInGroup(typeof(GhostPredictionSystemGroup))]
    public partial class BulletMovementSystem : SystemBase
    {
        //This is a special NetCode group that provides a "prediction tick" and a fixed "DeltaTime"
        private GhostPredictionSystemGroup m_PredictionGroup;
        
        protected override void OnCreate()
        {
            // m_BeginSimEcb = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

            //We will grab this system so we can use its "prediction tick" and "DeltaTime"
            m_PredictionGroup = World.GetOrCreateSystem<GhostPredictionSystemGroup>();
        }
        
        protected override void OnUpdate()
        {
            uint currentTick = m_PredictionGroup.PredictingTick;
            float deltaTime = m_PredictionGroup.Time.DeltaTime;

            Entities
                .WithAll<BulletTag>()
                .ForEach((ref Translation translation, in PredictedGhostComponent predictedGhost,
                    in Rotation rotation, in BulletSpeedComponent speed) =>
                {
                    if (!GhostPredictionSystemGroup.ShouldPredict(currentTick, predictedGhost))
                        return;

                    translation.Value += speed.Value * deltaTime * math.mul(rotation.Value, math.up());
                }).ScheduleParallel();
        }
    }
}
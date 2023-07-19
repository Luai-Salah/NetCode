using Unity.NetCode;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

using Xedrial.NetCode.Components;

namespace Xedrial.NetCode.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    public partial class BulletMovementSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            Entities
                .WithAll<PredictedGhost, Simulate, BulletTag>()
                .ForEach((ref LocalTransform transform, in BulletSpeed speed) =>
                {
                    transform.Position += speed.Value * deltaTime * math.mul(transform.Rotation, math.up());
                }).ScheduleParallel();
        }
    }
}
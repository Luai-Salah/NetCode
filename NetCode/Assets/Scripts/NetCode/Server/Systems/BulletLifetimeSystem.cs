using Unity.Entities;
using Unity.NetCode;

using Xedrial.NetCode.Server.Components;

namespace Xedrial.NetCode.Server.Systems
{
    //Only our server will decide when bullets die of old age
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public partial class BulletLifetimeSystem : SystemBase
    {
        //We will be using the BeginSimulationEntityCommandBuffer to record our structural changes
        private BeginSimulationEntityCommandBufferSystem m_BeginSimEcb;
    
        //This is a special NetCode group that provides a "prediction tick" and a fixed "DeltaTime"
        private GhostPredictionSystemGroup m_PredictionGroup;

        protected override void OnCreate()
        {
            //Grab the CommandBuffer for structural changes
            m_BeginSimEcb = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

            //We will grab this system so we can use its "DeltaTime"
            m_PredictionGroup = World.GetOrCreateSystem<GhostPredictionSystemGroup>();
        }

        protected override void OnUpdate()
        {
            //We create our CommandBuffer and add .AsParallelWriter() because we will be scheduling parallel jobs
            var commandBuffer = m_BeginSimEcb.CreateCommandBuffer().AsParallelWriter();

            //We must declare local variables before using them in the job below
            float deltaTime = m_PredictionGroup.Time.DeltaTime;

            //Our query writes to the BulletAgeComponent
            //The reason we don't need to add .WithAll<BulletTag>() here is because referencing the BulletAgeComponent
            //requires the Entities to have a BulletAgeComponent and only Bullets have those
            Entities.ForEach((Entity entity, int nativeThreadIndex, ref BulletLifetimeComponent lifetime) =>
            {
                if (lifetime.Value >= lifetime.MaxValue)
                    commandBuffer.DestroyEntity(nativeThreadIndex, entity);
                
                lifetime.Value += deltaTime;
            }).ScheduleParallel();
            
            m_BeginSimEcb.AddJobHandleForProducer(Dependency);
        }
    }
}
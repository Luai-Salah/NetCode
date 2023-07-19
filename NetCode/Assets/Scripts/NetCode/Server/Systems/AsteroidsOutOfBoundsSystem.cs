using UnityEngine;

using Unity.Entities;
using Unity.Transforms;

using AsteroidsDamage;
using Xedrial.NetCode.Components;

namespace Xedrial.NetCode.Server.Systems
{
    //We cannot use [UpdateInGroup(typeof(ServerSimulationSystemGroup))] because we already have a group defined
    //So we specify instead what world the system must run, ServerWorld
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    //We are adding this system within the FixedStepSimulationGroup
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(EndFixedStepSimulationEntityCommandBufferSystem))]
    public partial class AsteroidsOutOfBoundsSystem : SystemBase
    {
        //We are going to use the EndFixedStepSimECB
        //This is because when we use Unity Physics our physics will run in the FixedStepSimulationSystem
        //We are dipping our toes into placing our systems in specific system groups
        //The FixedStepSimGroup has its own EntityCommandBufferSystem we will use to make the structural change
        //of adding the DestroyTag
        private EndFixedStepSimulationEntityCommandBufferSystem m_EndFixedStepSimEcb;

        protected override void OnCreate()
        {
            //We grab the EndFixedStepSimECB for our OnUpdate
            m_EndFixedStepSimEcb = World.GetOrCreateSystemManaged<EndFixedStepSimulationEntityCommandBufferSystem>();

            //We want to make sure we don't update until we have our GameSettingsComponent
            //because we need the data from this component to know where the perimeter of our cube is
            RequireForUpdate<GameSettings>();
        }

        protected override void OnUpdate()
        {
            //We want to run this as parallel jobs so we need to add "AsParallelWriter" when creating
            //our command buffer
            var commandBuffer = m_EndFixedStepSimEcb.CreateCommandBuffer().AsParallelWriter();

            //We must declare our local variables that we will use in our job
            var settings = SystemAPI.GetSingleton<GameSettings>();

            //This time we query entities with components by using "WithAll" tag
            //This makes sure that we only grab entities with an AsteroidTag component so we don't affect other entities
            //that might have passed the perimeter of the cube  
            Entities
                .WithAll<AsteroidTag>()
                .ForEach((Entity entity, int entityInQueryIndex, in LocalTransform transform) =>
                {
                    //We check if the current Translation value is out of bounds
                    if (Mathf.Abs(transform.Position.x) <= settings.LevelWidth / 2f &&
                        Mathf.Abs(transform.Position.y) <= settings.LevelHeight / 2f)
                        return;
                    
                    //If it is out of bounds wee add the DestroyTag component to the entity and return
                    commandBuffer.AddComponent(entityInQueryIndex, entity, new DestroyTag());
                }).ScheduleParallel();

            //We add the dependencies to the CommandBuffer that will be playing back these structural changes (adding a DestroyTag)
            m_EndFixedStepSimEcb.AddJobHandleForProducer(Dependency);

        }
    }
}
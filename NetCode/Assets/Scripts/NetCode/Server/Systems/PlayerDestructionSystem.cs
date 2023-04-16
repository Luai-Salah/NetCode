using Unity.Entities;
using Unity.NetCode;
using Xedrial.NetCode.Components;

namespace Xedrial.NetCode.Server.Systems
{
    //We are going to update LATE once all other systems are complete
    //because we don't want to destroy the Entity before other systems have
    //had a chance to interact with it if they need to
    [UpdateInWorld(TargetWorld.Server)]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class PlayerDestructionSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem m_EndSimEcb;

        protected override void OnCreate()
        {
            //We grab the EndSimulationEntityCommandBufferSystem to record our structural changes
            m_EndSimEcb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
    
        protected override void OnUpdate()
        {
            // We add "AsParallelWriter" when we create our command buffer because we want
            // to run our jobs in parallel
            var commandBuffer = m_EndSimEcb.CreateCommandBuffer().AsParallelWriter();

            //We are going to need to update the NCE CommandTargetComponent so we set the argument to false (not read-only)
            var commandTargetFromEntity = GetComponentDataFromEntity<CommandTargetComponent>();

            // We now any entities with a DestroyTag and an PlayerTag
            // We could just query for a DestroyTag, but we might want to run different processes
            // if different entities are destroyed, so we made this one specifically for Players
            // We query specifically for players because we need to clear the NCE when they are destroyed
            // In order to write over a variable that we pass through to a job we must include "WithNativeDisableParallelForRestriction"
            // It means "yes we know what we are doing, allow us to write over this variable"
            Entities
                .WithNativeDisableParallelForRestriction(commandTargetFromEntity)
                .WithAll<DestroyTag, PlayerTag>()
                .ForEach((Entity entity, int entityInQueryIndex, in PlayerEntityComponent playerEntity) =>
                {
                    // Reset the CommandTargetComponent on the Network Connection Entity to the player
                    // We are able to find the NCE the player belongs to through the PlayerEntity component
                    CommandTargetComponent state = commandTargetFromEntity[playerEntity.PlayerEntity]; 
                    state.targetEntity = Entity.Null;
                    commandTargetFromEntity[playerEntity.PlayerEntity] = state;

                    //Then destroy the entity
                    commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                }).ScheduleParallel();

            //We then add the dependencies of these jobs to the EndSimulationEntityCCommandBufferSystem
            //that will be playing back the structural changes recorded in this system
            m_EndSimEcb.AddJobHandleForProducer(Dependency);
        }
    }
}

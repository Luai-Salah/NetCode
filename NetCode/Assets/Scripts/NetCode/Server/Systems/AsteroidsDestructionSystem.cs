using Unity.NetCode;
using Unity.Entities;

using AsteroidsDamage;
using Xedrial.NetCode.Components;
using Xedrial.Physics;

namespace Xedrial.NetCode.Server.Systems
{
    //We cannot use [UpdateInGroup(typeof(ServerSimulationSystemGroup))] because we already have a group defined
    //So we specify instead what world the system must run, ServerWorld
    [UpdateInWorld(TargetWorld.Server)]
    //We are going to update LATE once all other systems are complete
    //because we don't want to destroy the Entity before other systems have
    //had a chance to interact with it if they need to
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class AsteroidsDestructionSystem : SystemBase
    {
        private PhysicsWorld m_PhysicsWorld;

        protected override void OnCreate()
        {
            m_PhysicsWorld = World.GetOrCreateSystem<PhysicsWorld>();
        }

        protected override void OnUpdate()
        {
            //We now any entities with a DestroyTag and an AsteroidTag
            //We could just query for a DestroyTag, but we might want to run different processes
            //if different entities are destroyed, so we made this one specifically for Asteroids
            Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .WithAll<DestroyTag, AsteroidTag>()
                .ForEach((Entity entity, in RigidBody2DComponent rb) =>
                {
                    EntityManager.DestroyEntity(entity);
                    m_PhysicsWorld.DestroyBody(rb.Body);
                }).Run();

        }
    }
}
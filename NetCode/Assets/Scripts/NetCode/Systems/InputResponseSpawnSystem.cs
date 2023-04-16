using Unity.NetCode;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Networking.Transport.Utilities;

using Xedrial.NetCode.Components;

using AsteroidsDamage;

namespace Xedrial.NetCode.Systems
{
    // InputResponseSpawnSystem runs on both the Client and Server
    // It is predicted on the client but "decided" on the server
    [UpdateInWorld(TargetWorld.ClientAndServer)]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class InputResponseSpawnSystem : SystemBase
    {
        //We will use the BeginSimulationEntityCommandBufferSystem for our structural changes
        private BeginSimulationEntityCommandBufferSystem m_BeginSimEcb;

        //This is a special NetCode group that provides a "prediction tick" and a fixed "DeltaTime"
        private GhostPredictionSystemGroup m_PredictionGroup;

        //This will save our Bullet prefab to be used to spawn bullets 
        private Entity m_BulletPrefab;

        //We are going to use this for "weapon cooldown"
        private const int COOL_DOWN_TICKS_COUNT = 30;


        protected override void OnCreate()
        {
            //This will grab the BeginSimulationEntityCommandBuffer system to be used in OnUpdate
            m_BeginSimEcb = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

            //This will grab the BeginSimulationEntityCommandBuffer system to be used in OnUpdate
            m_PredictionGroup = World.GetOrCreateSystem<GhostPredictionSystemGroup>();
        
            //We check to ensure GameSettingsComponent exists to know if the SubScene has been streamed in
            //We need the SubScene for actions in our OnUpdate()
            RequireSingletonForUpdate<GameSettingsComponent>(); 
            // Make sure we have the bullet prefab to be able to create the predicted spawning
            RequireSingletonForUpdate<BulletAuthoringComponent>();
        }

        protected override void OnUpdate()
        {

            //Here we set the prefab we will use
            if (m_BulletPrefab == Entity.Null)
            {
                //We grab the converted PrefabCollection Entity's BulletAuthoringComponent
                //and set m_BulletPrefab to its Prefab value
                Entity foundPrefab = GetSingleton<BulletAuthoringComponent>().Prefab;
                m_BulletPrefab = GhostCollectionSystem.CreatePredictedSpawnPrefab(EntityManager, foundPrefab);
                //we must "return" after setting this prefab because if we were to continue into the Job
                //we would run into errors because the variable was JUST set (ECS funny business)
                //comment out return and see the error
                return;
            }
        
            //We need a CommandBuffer because we will be making structural changes (creating bullet entities)
            var commandBuffer = m_BeginSimEcb.CreateCommandBuffer().AsParallelWriter();

            //Must declare our local variables before the jobs in the .ForEach()
            Entity bulletPrefab = m_BulletPrefab;
            //These are special NetCode values needed to work the prediction system
            uint currentTick = m_PredictionGroup.PredictingTick;

            //We will grab the buffer of player command from the player entity
            BufferFromEntity<PlayerCommand> inputFromEntity = GetBufferFromEntity<PlayerCommand>(true);

            //We are looking for player entities that have PlayerCommands in their buffer
            Entities
                .WithReadOnly(inputFromEntity)
                .WithAll<PlayerTag, PlayerCommand>()
                .ForEach((Entity entity, int entityInQueryIndex, ref BulletSpawnOffsetComponent bulletOffset,
                    ref WeaponCoolDownComponent weaponCoolDown, in Rotation rotation, in Translation translation,
                    in GhostOwnerComponent ghostOwner, in PredictedGhostComponent prediction) =>
                {
                    //Here we check if we SHOULD do the prediction based on the tick, if we shouldn't, we return
                    if (!GhostPredictionSystemGroup.ShouldPredict(currentTick, prediction))
                        return;

                    //We grab the buffer of commands from the player entity
                    DynamicBuffer<PlayerCommand> input = inputFromEntity[entity];

                    //We then grab the Command from the current tick (which is the PredictingTick)
                    //if we cannot get it at the current tick we make sure shoot is 0
                    //This is where we will store the current tick data
                    if (!input.GetDataAtTick(currentTick, out PlayerCommand inputData))
                        inputData.Shoot = 0;

                    //Here we add the destroy tag to the player if the self-destruct button was pressed
                    if (inputData.SelfDestruct == 1)
                    {  
                        commandBuffer.AddComponent<DestroyTag>(entityInQueryIndex, entity);
                    }

                    bool canShoot = weaponCoolDown.Value == 0 || SequenceHelpers.IsNewer(currentTick, weaponCoolDown.Value);
                    if (inputData.Shoot == 0 || !canShoot)
                        return;

                    // We create the bullet here
                    Entity bullet = commandBuffer.Instantiate(entityInQueryIndex, bulletPrefab);
                    //We declare it as a predicted spawning for player spawned objects by adding a special component
                    commandBuffer.AddComponent(entityInQueryIndex, bullet, new PredictedGhostSpawnRequestComponent());

                    //we set the bullets position as the player's position + the bullet spawn offset
                    //math.mul(rotation.Value,bulletOffset.Value) finds the position of the bullet offset in the given rotation
                    //think of it as finding the LocalToParent of the bullet offset (because the offset needs to be rotated in the players direction)
                    var newPosition = new Translation {Value = translation.Value + math.mul(rotation.Value, bulletOffset.Value)};

                    commandBuffer.SetComponent(entityInQueryIndex, bullet, newPosition);
                    commandBuffer.SetComponent(entityInQueryIndex, bullet, rotation);
                    commandBuffer.SetComponent(entityInQueryIndex, bullet,
                        new GhostOwnerComponent {NetworkId = ghostOwner.NetworkId});

                    weaponCoolDown.Value = currentTick + COOL_DOWN_TICKS_COUNT;
                }).ScheduleParallel();

            //We must add our dependency to the CommandBuffer because we made structural changes
            m_BeginSimEcb.AddJobHandleForProducer(Dependency);
        }
    }
}
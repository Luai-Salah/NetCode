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
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class InputResponseSpawnSystem : SystemBase
    {
        //We will use the BeginSimulationEntityCommandBufferSystem for our structural changes
        private BeginSimulationEntityCommandBufferSystem m_BeginSimEcb;

        //This will save our Bullet prefab to be used to spawn bullets 
        private Entity m_BulletPrefab;

        //We are going to use this for "weapon cooldown"
        private const int k_CoolDownTicksCount = 30;


        protected override void OnCreate()
        {
            //This will grab the BeginSimulationEntityCommandBuffer system to be used in OnUpdate
            m_BeginSimEcb = World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();

            //We check to ensure GameSettingsComponent exists to know if the SubScene has been streamed in
            //We need the SubScene for actions in our OnUpdate()
            RequireForUpdate<GameSettings>(); 
            // Make sure we have the bullet prefab to be able to create the predicted spawning
            RequireForUpdate<BulletPrefab>();
        }

        protected override void OnUpdate()
        {

            //Here we set the prefab we will use
            if (m_BulletPrefab == Entity.Null)
            {
                //We grab the converted PrefabCollection Entity's BulletAuthoringComponent
                //and set m_BulletPrefab to its Prefab value
                m_BulletPrefab = SystemAPI.GetSingleton<BulletPrefab>().Prefab;
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
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();

            //We will grab the buffer of player command from the player entity
            BufferLookup<PlayerCommand> inputFromEntity = SystemAPI.GetBufferLookup<PlayerCommand>(true);

            //We are looking for player entities that have PlayerCommands in their buffer
            Entities
                .WithReadOnly(inputFromEntity)
                .WithAll<PlayerCommand, PredictedGhost, Simulate>()
                .ForEach((Entity entity, int entityInQueryIndex, ref BulletSpawnOffset bulletOffset,
                    ref WeaponCoolDownComponent weaponCoolDown, in LocalTransform transform,
                    in GhostOwner ghostOwner) =>
                {
                    
                    
                    //We grab the buffer of commands from the player entity
                    DynamicBuffer<PlayerCommand> input = inputFromEntity[entity];

                    //We then grab the Command from the current tick (which is the PredictingTick)
                    //if we cannot get it at the current tick we make sure shoot is 0
                    //This is where we will store the current tick data
                    if (!input.GetDataAtTick(networkTime.ServerTick, out PlayerCommand inputData))
                        inputData.Shoot = 0;

                    //Here we add the destroy tag to the player if the self-destruct button was pressed
                    if (inputData.SelfDestruct == 1)
                    {  
                        commandBuffer.AddComponent<DestroyTag>(entityInQueryIndex, entity);
                    }

                    bool canShoot = weaponCoolDown.Value == 0 || SequenceHelpers.IsNewer(networkTime.ServerTick.SerializedData, weaponCoolDown.Value);
                    if (inputData.Shoot == 0 || !canShoot || !networkTime.IsFirstTimeFullyPredictingTick)
                        return;

                    // We create the bullet here
                    Entity bullet = commandBuffer.Instantiate(entityInQueryIndex, bulletPrefab);
                    //We declare it as a predicted spawning for player spawned objects by adding a special component
                    commandBuffer.AddComponent<PredictedGhostSpawnRequest>(entityInQueryIndex, bullet);

                    //we set the bullets position as the player's position + the bullet spawn offset
                    //math.mul(rotation.Value,bulletOffset.Value) finds the position of the bullet offset in the given rotation
                    //think of it as finding the LocalToParent of the bullet offset (because the offset needs to be rotated in the players direction)
                    var bulletTransform = transform;
                    bulletTransform.Position = transform.Position + math.mul(transform.Rotation, bulletOffset.Value);
                    commandBuffer.SetComponent(entityInQueryIndex, bullet, bulletTransform);
                    commandBuffer.SetComponent(entityInQueryIndex, bullet,
                        new GhostOwner {NetworkId = ghostOwner.NetworkId});

                    weaponCoolDown.Value = networkTime.ServerTick.SerializedData + k_CoolDownTicksCount;
                }).ScheduleParallel();

            //We must add our dependency to the CommandBuffer because we made structural changes
            m_BeginSimEcb.AddJobHandleForProducer(Dependency);
        }
    }
}
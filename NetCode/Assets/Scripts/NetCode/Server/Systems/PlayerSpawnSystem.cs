using System.Diagnostics;
using AsteroidsDamage;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Xedrial.NetCode.Commands;
using Xedrial.NetCode.Components;
using Xedrial.NetCode.Server.Components;

namespace Xedrial.NetCode.Server.Systems
{
    //This tag is only used by the systems in this file so we define it here
    public struct PlayerSpawnInProgressTag : IComponentData
    {
    }

    //Only the server will be running this system to spawn the player
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class PlayerSpawnSystem : SystemBase
    {
        private BeginSimulationEntityCommandBufferSystem m_BeginSimEcb;
        private Entity m_Prefab;

        protected override void OnCreate()
        {
            m_BeginSimEcb = World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
            
            //We check to ensure GameSettingsComponent exists to know if the SubScene has been streamed in
            //We need the SubScene for actions in our OnUpdate()
            RequireForUpdate<GameSettings>(); 
        }

        protected override void OnUpdate()
        {
            //Here we set the prefab we will use
            if (m_Prefab == Entity.Null)
            {
                //We grab the converted PrefabCollection Entity's PlayerAuthoringComponent
                //and set m_Prefab to its Prefab value
                m_Prefab = SystemAPI.GetSingleton<PlayerPrefab>().Value;
                //we must "return" after setting this prefab because if we were to continue into the Job
                //we would run into errors because the variable was JUST set (ECS funny business)
                //comment out return and see the error
                return;
            }

            //Because of how ECS works we must declare local variables that will be used within the job
            EntityCommandBuffer commandBuffer = m_BeginSimEcb.CreateCommandBuffer();
            Entity playerPrefab = m_Prefab;
            var rand = new Random((uint) Stopwatch.GetTimestamp());
            var gameSettings = SystemAPI.GetSingleton<GameSettings>();

            //GetComponentDataFromEntity allows us to grab data from an entity that we don't have access to
            //until we are within a job
            //We know we will need to get the PlayerSpawningStateComponent from an NCE but we don't know which one yet
            //So we create a variable that will get PlayerSpawningStateComponent from an entity
            var playerStateFromEntity = GetComponentLookup<PlayerSpawningStateComponent>();

            //Similar to playerStateFromEntity, these variables WILL get data from an entity (in the job below)
            //but do not have it currently
            var commandTargetFromEntity = GetComponentLookup<CommandTarget>();
            var networkIdFromEntity = GetComponentLookup<NetworkId>();

            //We are looking for NCEs with a PlayerSpawnRequestRpc
            //That means the client associated with that NCE wants a player to be spawned for them
            Entities
                .ForEach((Entity entity, in PlayerSpawnRequestRpc _,
                    in ReceiveRpcCommandRequest requestSource) =>
                {
                    //We immediately destroy the request so we act on it once
                    commandBuffer.DestroyEntity(entity);

                    //These are checks to see if the NCE has disconnected or if there are any other issues
                    //These checks are pulled from Unity samples and we have left them in even though they seem
                    //Is there a PlayerSpawningState on the NCE
                    //Is there a CommandTargetComponent on the NCE
                    //Is the CommandTargetComponent targetEntity != Entity.Null
                    //Is the PlayerSpawningState == 0
                    //If all those are true we continue with spawning, otherwise we don't

                    if (!playerStateFromEntity.HasComponent(requestSource.SourceConnection) ||
                        !commandTargetFromEntity.HasComponent(requestSource.SourceConnection) ||
                        commandTargetFromEntity[requestSource.SourceConnection].targetEntity != Entity.Null ||
                        playerStateFromEntity[requestSource.SourceConnection].IsSpawning != 0
                    )
                        return;

                    //We create our player prefab
                    Entity player = commandBuffer.Instantiate(playerPrefab);

                    //We will spawn our player in the center-ish of our game
                    float width = gameSettings.LevelWidth * .2f;
                    float height = gameSettings.LevelHeight * .2f;

                    var transform = LocalTransform.Identity;
                    transform.Position = new float3(rand.NextFloat(-width, width), rand.NextFloat(-height, height), 0);

                    //Here we set the components that already exist on the Player prefab
                    commandBuffer.SetComponent(player, transform);
                    //This sets the GhostOwnerComponent value to the NCE NetworkId
                    commandBuffer.SetComponent(player, new GhostOwner {NetworkId = networkIdFromEntity[requestSource.SourceConnection].Value});
                    //This sets the PlayerEntity value in PlayerEntityComponent to the NCE
                    commandBuffer.SetComponent(player, new PlayerEntity {Entity = requestSource.SourceConnection});

                    //Here we add a component that was not included in the Player prefab, PlayerSpawnInProgressTag
                    //This is a temporary tag used to make sure the entity was able to be created and will be removed
                    //in PlayerCompleteSpawnSystem below    
                    commandBuffer.AddComponent(player, new PlayerSpawnInProgressTag());

                    //We update the PlayerSpawningStateComponent tag on the NCE to "currently spawning" (1)
                    playerStateFromEntity[requestSource.SourceConnection] = new PlayerSpawningStateComponent {IsSpawning = 1};
                }).Schedule();


            m_BeginSimEcb.AddJobHandleForProducer(Dependency);
        }
    }

    //We want to complete the spawn before ghosts are sent on the server
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateBefore(typeof(GhostSendSystem))]
    public partial class PlayerCompleteSpawnSystem : SystemBase
    {
        private BeginSimulationEntityCommandBufferSystem m_BeginSimEcb;

        protected override void OnCreate()
        {
            m_BeginSimEcb = World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer commandBuffer = m_BeginSimEcb.CreateCommandBuffer();

            //GetComponentDataFromEntity allows us to grab data from an entity that we don't have access to
            //until we are within a job
            //We don't know exactly which NCE we currently want to grab data from, but we do know we will want to
            //so we use GetComponentDataFromEntity to prepare ECS that we will be grabbing this data from an entity
            var playerStateFromEntity = GetComponentLookup<PlayerSpawningStateComponent>();
            var commandTargetFromEntity = GetComponentLookup<CommandTarget>();
            var connectionFromEntity = GetComponentLookup<NetworkStreamConnection>();

            Entities.WithAll<PlayerSpawnInProgressTag>()
                .ForEach((Entity entity, in PlayerEntity player) =>
                {
                    // This is another check from Unity samples
                    // This ensures there was no disconnect
                    if (!playerStateFromEntity.HasComponent(player.Entity) ||
                        !connectionFromEntity[player.Entity].Value.IsCreated)
                    {
                        //Player was disconnected during spawn, or other error so delete
                        commandBuffer.DestroyEntity(entity);
                        return;
                    }

                    //If there was no error with spawning the player we can remove the PlayerSpawnInProgressTag
                    commandBuffer.RemoveComponent<PlayerSpawnInProgressTag>(entity);

                    //We now update the NCE to point at our player entity
                    commandTargetFromEntity[player.Entity] = new CommandTarget {targetEntity = entity};
                    //We can now say that our player is no longer spawning so we set IsSpawning = 0 on the NCE
                    playerStateFromEntity[player.Entity] = new PlayerSpawningStateComponent {IsSpawning = 0};
                }).Schedule();
            
            m_BeginSimEcb.AddJobHandleForProducer(Dependency);
        }
    }
}
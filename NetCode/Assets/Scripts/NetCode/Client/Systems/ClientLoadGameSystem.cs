using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

using Xedrial.NetCode.Commands;

using AsteroidsDamage;

//This will only run on the client because it updates in ClientSimulationSystemGroup (which the server does not have)
namespace Xedrial.NetCode.Client.Systems
{
    [UpdateInGroup(typeof(ClientSimulationSystemGroup))]
    [UpdateBefore(typeof(RpcSystem))]
    public partial class ClientLoadGameSystem : SystemBase
    {
        private BeginSimulationEntityCommandBufferSystem m_BeginSimEcb;

        protected override void OnCreate()
        {
            //We will be using the BeginSimECB
            m_BeginSimEcb = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

            //Requiring the ReceiveRpcCommandRequestComponent ensures that update is only run when an NCE exists
            RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<SendClientGameRpc>(), ComponentType.ReadOnly<ReceiveRpcCommandRequestComponent>()));   
            //This is just here to make sure the Sub Scene is streamed in before the client sets up the level data
            RequireSingletonForUpdate<GameSettingsComponent>();
        }

        protected override void OnUpdate()
        {
            //We must declare our local variables before using them within a job (.ForEach)
            EntityCommandBuffer commandBuffer = m_BeginSimEcb.CreateCommandBuffer();
            Entity gameSettingsEntity = GetSingletonEntity<GameSettingsComponent>();
            
            BufferFromEntity<OutgoingRpcDataStreamBufferComponent> rpcFromEntity
                = GetBufferFromEntity<OutgoingRpcDataStreamBufferComponent>();
            ComponentDataFromEntity<GameSettingsComponent> getGameSettingsComponentData
                = GetComponentDataFromEntity<GameSettingsComponent>();

            Entities
                .ForEach((Entity entity, in SendClientGameRpc request, in ReceiveRpcCommandRequestComponent requestSource) =>
                {
                    //This destroys the incoming RPC so the code is only run once
                    commandBuffer.DestroyEntity(entity);

                    //Check for disconnects before moving forward
                    if (!rpcFromEntity.HasComponent(requestSource.SourceConnection))
                        return;

                    //Set the game size (unnecessary right now but we are including it to show how it is done)
                    getGameSettingsComponentData[gameSettingsEntity] = new GameSettingsComponent
                    {
                        LevelWidth = request.LevelWidth,
                        LevelHeight = request.LevelHeight,
                        PlayerForce = request.PlayerForce,
                        BulletForce = request.BulletVelocity,
                        BulletsPerSecond = request.BulletsPerSecond,
                        RotationSpeed = request.RotationSpeed
                    };

                    //These update the NCE with NetworkStreamInGame (required to start receiving snapshots)
                    commandBuffer.AddComponent(requestSource.SourceConnection, default(NetworkStreamInGame));
            
                    //This tells the server "I loaded the level"
                    //First we create an entity called levelReq that will have 2 necessary components
                    //Next we add the RPC we want to send (SendServerGameLoadedRpc) and then we add
                    //SendRpcCommandRequestComponent with our TargetConnection being the NCE with the server (which will send it to the server)
                    Entity levelReq = commandBuffer.CreateEntity();
                    commandBuffer.AddComponent(levelReq, new SendServerGameLoadedRpc());
                    commandBuffer.AddComponent(levelReq, new SendRpcCommandRequestComponent {TargetConnection = requestSource.SourceConnection});

                    Debug.Log("Client loaded game");
                }).Schedule();

            m_BeginSimEcb.AddJobHandleForProducer(Dependency);
        }
    }
}

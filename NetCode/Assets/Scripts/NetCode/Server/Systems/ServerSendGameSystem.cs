using AsteroidsDamage;
using Unity.Entities;
using Unity.NetCode;
using Xedrial.NetCode.Commands;

namespace Xedrial.NetCode.Server.Systems
{
    //This component is only used by this system so we define it in this file
    public struct SentClientGameRpcTag : IComponentData
    {
    }

    //This system should only be run by the server (because the server sends the game settings)
    //By specifying to update in group ServerSimulationSystemGroup it also specifies that it must
    //be run by the server
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    [UpdateBefore(typeof(RpcSystem))]
    public partial class ServerSendGameSystem : SystemBase
    {
        private BeginSimulationEntityCommandBufferSystem m_Barrier;

        protected override void OnCreate()
        {
            m_Barrier = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
            RequireSingletonForUpdate<GameSettingsComponent>();
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer commandBuffer = m_Barrier.CreateCommandBuffer();

            var serverData = GetSingleton<GameSettingsComponent>();

            Entities
                .WithNone<SentClientGameRpcTag>()
                .ForEach((Entity entity, in NetworkIdComponent _) =>
                {
                    commandBuffer.AddComponent(entity, new SentClientGameRpcTag());
                    Entity req = commandBuffer.CreateEntity();
                    commandBuffer.AddComponent(req, new SendClientGameRpc
                    {
                        LevelWidth = serverData.LevelWidth,
                        LevelHeight = serverData.LevelHeight,
                        PlayerForce = serverData.PlayerForce,
                        RotationSpeed = serverData.RotationSpeed,
                        BulletVelocity = serverData.BulletForce,
                        BulletsPerSecond = serverData.BulletsPerSecond
                    });

                    commandBuffer.AddComponent(req, new SendRpcCommandRequestComponent {TargetConnection = entity});
                }).Schedule();

            m_Barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
using Unity.NetCode;
using Unity.Entities;
using Unity.Transforms;

using Xedrial.NetCode.Client.Components;
using Xedrial.NetCode.Components;

namespace Xedrial.NetCode.Client.Systems
{
    //We are updating only in the client world because only the client must specify exactly which player entity it "owns"
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    //We will be updating after NetCode's GhostSpawnClassificationSystem because we want
    //to ensure that the PredictedGhostComponent (which it adds) is available on the player entity to identify it
    [UpdateInGroup(typeof(GhostSimulationSystemGroup))]
    [UpdateAfter(typeof(GhostSpawnClassificationSystem))]
    public partial class PlayerGhostSpawnClassificationSystem : SystemBase
    {
        private BeginSimulationEntityCommandBufferSystem m_BeginSimEcb;

        //We will store the Camera prefab here which we will attach when we identify our player entity
        private Entity m_CameraPrefab;

        protected override void OnCreate()
        {
            m_BeginSimEcb = World.GetExistingSystemManaged<BeginSimulationEntityCommandBufferSystem>();

            //We need to make sure we have NCE before we start the update loop (otherwise it's unnecessary)
            RequireForUpdate<NetworkId>();
            RequireForUpdate<CameraAuthoringComponent>();
        }

        protected override void OnUpdate()
        {
            //Here we set the prefab we will use
            if (m_CameraPrefab == Entity.Null)
            {
                //We grab our camera and set our variable
                m_CameraPrefab = GetSingleton<CameraAuthoringComponent>().Prefab;
                return;
            }
        
            EntityCommandBuffer.ParallelWriter commandBuffer = m_BeginSimEcb.CreateCommandBuffer().AsParallelWriter();
        
            //The "playerEntity" is the NCE
            var networkIdComponent = SystemAPI.GetSingleton<NetworkId>();

            //We will look for Player prefabs that we have not added a "PlayerClassifiedTag" to (which means we have checked the player if it is "ours")
            Entities
                .WithAll<PlayerTag>()
                .WithNone<PlayerClassifiedTag>()
                .ForEach((Entity entity, int entityInQueryIndex, in GhostOwner ghostOwnerComponent) =>
                {
                    // If this is true this means this Player is mine (because the GhostOwnerComponent value is equal to the NetworkId)
                    // Remember the GhostOwnerComponent value is set by the server and is ghosted to the client
                    if (ghostOwnerComponent.NetworkId == networkIdComponent.Value)
                    {
                        // This creates our camera
                        //Entity cameraEntity = commandBuffer.Instantiate(entityInQueryIndex, camera);
                        // This is how you "attach" a prefab entity to another
                        //commandBuffer.AddComponent(entityInQueryIndex, cameraEntity, new CameraFollowComponent {Target = entity});
                    }
                    // This means we have classified this Player prefab
                    commandBuffer.AddComponent(entityInQueryIndex, entity, new PlayerClassifiedTag() );
                }).ScheduleParallel();

            m_BeginSimEcb.AddJobHandleForProducer(Dependency);
        }
    }
}

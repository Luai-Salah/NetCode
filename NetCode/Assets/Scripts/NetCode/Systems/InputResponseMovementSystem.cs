using Unity.NetCode;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

using Xedrial.NetCode.Components;

using AsteroidsDamage;

namespace Xedrial.NetCode.Systems
{
    //InputResponseMovementSystem runs on both the Client and Server
    //It is predicted on the client but "decided" on the server
    [UpdateInWorld(TargetWorld.ClientAndServer)]
    // [UpdateInGroup(typeof(PredictedPhysicsSystemGroup))]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class InputResponseMovementSystem : SystemBase
    {
        //This is a special NetCode group that provides a "prediction tick" and a fixed "DeltaTime"
        private GhostPredictionSystemGroup m_PredictionGroup;
        
        protected override void OnCreate()
        {
            // m_BeginSimEcb = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

            //We will grab this system so we can use its "prediction tick" and "DeltaTime"
            m_PredictionGroup = World.GetOrCreateSystem<GhostPredictionSystemGroup>();
        }

        protected override void OnUpdate()
        {
            //No need for a CommandBuffer because we are not making any structural changes to any entities
            //We are setting values on components that already exist
            // var commandBuffer = m_BeginSimEcb.CreateCommandBuffer().AsParallelWriter();

            //These are special NetCode values needed to work the prediction system
            uint currentTick = m_PredictionGroup.PredictingTick;
            float deltaTime = m_PredictionGroup.Time.DeltaTime;

            //We must declare our local variables before the .ForEach()
            var gameSettings = GetSingleton<GameSettingsComponent>();

            //We will grab the buffer of player commands from the player entity
            BufferFromEntity<PlayerCommand> inputFromEntity = GetBufferFromEntity<PlayerCommand>(true);
            //We are looking for player entities that have PlayerCommands in their buffer
            // ReSharper disable once UnusedParameter.Local
            Entities
                .WithReadOnly(inputFromEntity)
                .WithAll<PlayerTag, PlayerCommand>()
                .ForEach((Entity entity, int entityInQueryIndex, ref Rotation rotation, ref Translation translation, 
                    in PredictedGhostComponent prediction) =>
                {
                    // Here we check if we SHOULD do the prediction based on the tick, if we shouldn't, we return
                    if (!GhostPredictionSystemGroup.ShouldPredict(currentTick, prediction))
                        return;
            
                    //We grab the buffer of commands from the player entity
                    DynamicBuffer<PlayerCommand> input = inputFromEntity[entity];

                    //We then grab the Command from the current tick (which is the PredictingTick)
                    //if we cannot get it at the current tick we make sure shoot is 0
                    //This is where we will store the current tick data
                    if (!input.GetDataAtTick(currentTick, out PlayerCommand inputData))
                        inputData.Shoot = 0;

                    var move = new float3
                    {
                        x = inputData.Right - inputData.Left,
                        y = inputData.Thrust - inputData.ReverseThrust,
                        z = 0
                    };
                    
                    translation.Value += gameSettings.PlayerForce * deltaTime * move;

                    float angle = (inputData.RotateLeft - inputData.RotateRight) * gameSettings.RotationSpeed * deltaTime;
                    rotation.Value = math.mul(rotation.Value, quaternion.RotateZ(angle));
                }).ScheduleParallel();

            //No need to .AddJobHandleForProducer() because we did not need a CommandBuffer to make structural changes
        }
    
    }
}
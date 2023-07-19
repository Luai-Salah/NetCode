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
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    // [UpdateInGroup(typeof(PredictedPhysicsSystemGroup))]
    [UpdateInGroup(typeof(PredictedFixedStepSimulationSystemGroup))]
    public partial class InputResponseMovementSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<GameSettings>();
        }

        protected override void OnUpdate()
        {
            //No need for a CommandBuffer because we are not making any structural changes to any entities
            //We are setting values on components that already exist
            // var commandBuffer = m_BeginSimEcb.CreateCommandBuffer().AsParallelWriter();

            float deltaTime = SystemAPI.Time.DeltaTime;

            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            //We must declare our local variables before the .ForEach()
            var gameSettings = SystemAPI.GetSingleton<GameSettings>();

            //We will grab the buffer of player commands from the player entity
            BufferLookup<PlayerCommand> inputFromEntity = SystemAPI.GetBufferLookup<PlayerCommand>(true);
            //We are looking for player entities that have PlayerCommands in their buffer
            // ReSharper disable once UnusedParameter.Local
            Entities
                .WithReadOnly(inputFromEntity)
                .WithAll<PlayerCommand, PredictedGhost, Simulate>()
                .ForEach((Entity entity, int entityInQueryIndex, ref LocalTransform transform) =>
                {
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
                    
                    transform.Position += gameSettings.PlayerForce * deltaTime * move;

                    float angle = (inputData.RotateLeft - inputData.RotateRight) * gameSettings.RotationSpeed * deltaTime;
                    transform.Rotation = math.mul(transform.Rotation, quaternion.RotateZ(angle));
                }).Schedule();

            //No need to .AddJobHandleForProducer() because we did not need a CommandBuffer to make structural changes
        }
    }
}
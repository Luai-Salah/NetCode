using UnityEngine;

using Unity.NetCode;
using Unity.Entities;

using Xedrial.NetCode.Commands;
using Xedrial.NetCode.Components;

namespace Xedrial.NetCode.Client.Systems
{
    //This is a special SystemGroup introduced in NetCode 0.5
    //This group only exists on the client and is meant to be used when commands are being created
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    public partial class InputSystem : SystemBase
    {
        //We will use the BeginSimulationEntityCommandBufferSystem for our structural changes
        private BeginSimulationEntityCommandBufferSystem m_BeginSimEcb;

        //We need this system group so we can grab its "ServerTick" for prediction when we respond to Commands
        private ClientSimulationSystemGroup m_ClientSimulationSystemGroup;

        //We use this for thin client command generation
        private int m_FrameCount;

        protected override void OnCreate()
        {

            //This will grab the BeginSimulationEntityCommandBuffer system to be used in OnUpdate
            m_BeginSimEcb = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

            //We set our ClientSimulationSystemGroup who will provide its ServerTick needed for the Commands
            m_ClientSimulationSystemGroup = World.GetOrCreateSystem<ClientSimulationSystemGroup>();


            //The client must have loaded the game to spawn a player so we wait for the 
            //NetworkStreamInGame component added during the load game flow
            RequireSingletonForUpdate<NetworkStreamInGame>();
        }

        protected override void OnUpdate()
        {
            bool isThinClient = HasSingleton<ThinClientComponent>();
            if (HasSingleton<CommandTargetComponent>() && GetSingleton<CommandTargetComponent>().targetEntity == Entity.Null)
            {
                if (isThinClient)
                {
                    // No ghosts are spawned, so create a placeholder struct to store the commands in
                    Entity ent = EntityManager.CreateEntity();
                    EntityManager.AddBuffer<PlayerCommand>(ent);
                    SetSingleton(new CommandTargetComponent{targetEntity = ent});
                }
            }
            
            //We now have all our inputs
            byte left, thrust, reverseThrust, selfDestruct, shoot, rotateRight, rotateLeft;
            byte right = left = thrust = reverseThrust = selfDestruct = rotateRight = rotateLeft = shoot = 0;

            //We are adding this difference so we can use "Num Thin Client" in "Multiplayer Mode Tools"
            //These are the instructions if we are NOT a thin client
            if (!isThinClient)
            {
                if (Input.GetKey("d"))
                {
                    right = 1;
                }
                if (Input.GetKey("a"))
                {
                    left = 1;
                }
                if (Input.GetKey("w"))
                {
                    thrust = 1;
                }
                if (Input.GetKey("s"))
                {
                    reverseThrust = 1;
                }
                if (Input.GetKey("p"))
                {
                    selfDestruct = 1;
                }
                if (Input.GetKey("space"))
                {
                    shoot = 1;
                }
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    rotateRight = 1;
                }
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    rotateLeft = 1;
                }
            }
            else
            {
                // Spawn and generate some random inputs
                int state = (int) Time.ElapsedTime % 3;
                if (state == 0)
                {
                    left = 1;
                }
                else {
                    thrust = 1;
                }
                ++m_FrameCount;
                if (m_FrameCount % 100 == 0)
                {
                    shoot = 1;
                    m_FrameCount = 0;
                }
            }

            //We are sending the simulationSystemGroup tick so the server can playback our commands appropriately
            uint inputTargetTick = m_ClientSimulationSystemGroup.ServerTick;    
            //Must declare local variables before using them in the .ForEach()
            EntityCommandBuffer commandBuffer = m_BeginSimEcb.CreateCommandBuffer();
            // This is how we will grab the buffer of PlayerCommands from the player prefab
            BufferFromEntity<PlayerCommand> inputFromEntity = GetBufferFromEntity<PlayerCommand>();

            TryGetSingletonEntity<PlayerCommand>(out Entity targetEntity);
            Job.WithCode(() => {
                if (isThinClient && shoot != 0)
                {
                    // Special handling for thin clients since we can't tell if the ship is spawned or not
                    // This means every time we shoot we also send an RPC, but the Server protects against creating more Players
                    Entity req = commandBuffer.CreateEntity();
                    commandBuffer.AddComponent<PlayerSpawnRequestRpc>(req);
                    commandBuffer.AddComponent(req, new SendRpcCommandRequestComponent());
                }
                if (targetEntity == Entity.Null)
                {
                    if (shoot == 0)
                        return;
                    
                    Entity req = commandBuffer.CreateEntity();
                    commandBuffer.AddComponent<PlayerSpawnRequestRpc>(req);
                    commandBuffer.AddComponent(req, new SendRpcCommandRequestComponent());
                }
                else
                {
                    DynamicBuffer<PlayerCommand> input = inputFromEntity[targetEntity];
                    input.AddCommandData(
                        new PlayerCommand
                        {
                            Tick = inputTargetTick,
                            Left = left,
                            Right = right,
                            Thrust = thrust,
                            ReverseThrust = reverseThrust,
                            SelfDestruct = selfDestruct,
                            Shoot = shoot,
                            RotateLeft = rotateLeft,
                            RotateRight = rotateRight
                        }
                    );
                }
            }).Schedule();

            //We need to add the jobs dependency to the command buffer
            m_BeginSimEcb.AddJobHandleForProducer(Dependency);
        }
    }
}
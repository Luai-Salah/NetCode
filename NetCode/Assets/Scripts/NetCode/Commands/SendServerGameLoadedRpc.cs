using UnityEngine;

using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;

using AOT;

using Xedrial.NetCode.Components;

namespace Xedrial.NetCode.Commands
{
    [BurstCompile]
    public struct SendServerGameLoadedRpc : IComponentData, IRpcCommandSerializer<SendServerGameLoadedRpc>
    {
        //Necessary boilerplate
        public void Serialize(ref DataStreamWriter writer, in RpcSerializerState state, in SendServerGameLoadedRpc data)
        {
        }
        
        //Necessary boilerplate
        public void Deserialize(ref DataStreamReader reader, in RpcDeserializerState state, ref SendServerGameLoadedRpc data)
        {
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(RpcExecutor.ExecuteDelegate))]
        private static void InvokeExecute(ref RpcExecutor.Parameters parameters)
        {
            //Within here is where
            var rpcData = default(SendServerGameLoadedRpc);

            //Here we deserialize the received data
            rpcData.Deserialize(ref parameters.Reader, parameters.DeserializerState, ref rpcData);

            //Here we add 3 components to the NCE
            //The first, PlayerSpawningStateComponent will be used during our player spawn flow
            parameters.CommandBuffer.AddComponent(parameters.JobIndex, parameters.Connection, new PlayerSpawningStateComponent());
            //NetworkStreamInGame must be added to an NCE to start receiving Snapshots
            parameters.CommandBuffer.AddComponent(parameters.JobIndex, parameters.Connection, default(NetworkStreamInGame));
            //GhostConnectionPosition is added to be used in conjunction with GhostDistanceImportance (from the socket section)
            parameters.CommandBuffer.AddComponent(parameters.JobIndex, parameters.Connection, default(GhostConnectionPosition));

            //We add a log that we will remove later to show that this RPC has been executed
            //iOS will crash if Debug.Log is used within an RPC so we will remove this in the ARFoundation section
            Debug.Log("Server acted on confirmed game load");
        }

        //Necessary boilerplate
        private static readonly PortableFunctionPointer<RpcExecutor.ExecuteDelegate> s_InvokeExecuteFunctionPointer =
            new(InvokeExecute);
        public PortableFunctionPointer<RpcExecutor.ExecuteDelegate> CompileExecute()
        {
            return s_InvokeExecuteFunctionPointer;
        }
    }

    //Necessary boilerplate
    public class SendServerGameLoadedRpcCommandRequestSystem : RpcCommandRequestSystem<SendServerGameLoadedRpc, SendServerGameLoadedRpc>
    {
        [BurstCompile]
        private struct SendRpc : IJobEntityBatch
        {
            public SendRpcData Data;
            public void Execute(ArchetypeChunk chunk, int orderIndex)
            {
                Data.Execute(chunk, orderIndex);
            }
        }
        protected override void OnUpdate()
        {
            var sendJob = new SendRpc{Data = InitJobData()};
            ScheduleJobData(sendJob);
        }
    }
}
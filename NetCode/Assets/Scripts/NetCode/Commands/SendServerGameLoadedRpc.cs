using UnityEngine;

using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using AOT;
using Unity.Assertions;
using Unity.Burst.Intrinsics;
using Unity.Collections;
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
    [UpdateInGroup(typeof(RpcCommandRequestSystemGroup))]
    [CreateAfter(typeof(RpcSystem))]
    [BurstCompile]
    internal partial struct SendServerGameLoadedRpcSystem : ISystem
    {
        RpcCommandRequest<SendServerGameLoadedRpc, SendServerGameLoadedRpc> m_Request;
        [BurstCompile]
        private struct SendRpc : IJobChunk
        {
            public RpcCommandRequest<SendServerGameLoadedRpc, SendServerGameLoadedRpc>.SendRpcData Data;
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                Assert.IsFalse(useEnabledMask);
                Data.Execute(chunk, unfilteredChunkIndex);
            }
        }
        public void OnCreate(ref SystemState state)
        {
            m_Request.OnCreate(ref state);
        }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var sendJob = new SendRpc{Data = m_Request.InitJobData(ref state)};
            state.Dependency = sendJob.Schedule(m_Request.Query, state.Dependency);
        }
    }
}
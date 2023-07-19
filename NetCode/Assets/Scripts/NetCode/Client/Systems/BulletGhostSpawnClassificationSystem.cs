using Unity.NetCode;
using Unity.Entities;
using Unity.Networking.Transport.Utilities;

namespace Xedrial.NetCode.Client.Systems
{
    //This system will only run on the client and within GhostSimulationSystemGroup
    //and after GhostSpawnClassification system as is specified in the NetCode documentation
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(GhostSimulationSystemGroup))]
    [UpdateAfter(typeof(GhostSpawnClassificationSystem))]
    public partial class BulletGhostSpawnClassificationSystem : SystemBase
    {
        protected override void OnCreate()
        {
            //Both of these components are needed in the OnUpdate so we will wait until they exist to update
            RequireForUpdate<GhostSpawnQueue>();
            RequireForUpdate<PredictedGhostSpawnList>();
        }
        
        protected override void OnUpdate()
        {
            //This is the NetCode recommended method to identify predicted spawning for player spawned objects
            //More information can be found at: https://docs.unity3d.com/Packages/com.unity.netcode@0.5/manual/ghost-snapshots.html
            //under "Entity spawning"
            Entity spawnListEntity = SystemAPI.GetSingletonEntity<PredictedGhostSpawnList>();
            BufferLookup<PredictedGhostSpawn> spawnListLookup = SystemAPI.GetBufferLookup<PredictedGhostSpawn>();
            Dependency = Entities
                .WithAll<GhostSpawnQueue>()
                .WithoutBurst()
                .ForEach((DynamicBuffer<GhostSpawnBuffer> ghosts, DynamicBuffer<SnapshotDataBuffer> _) =>
                {
                    DynamicBuffer<PredictedGhostSpawn> spawnList = spawnListLookup[spawnListEntity];
                    for (int i = 0; i < ghosts.Length; i++)
                    {
                        GhostSpawnBuffer ghost = ghosts[i];
                        if (ghost.SpawnType != GhostSpawnBuffer.Type.Predicted)
                            continue;

                        for (int j = 0; j < spawnList.Length; ++j)
                        {
                            if (ghost.GhostType != spawnList[j].ghostType ||
                                SequenceHelpers.IsNewer(spawnList[j].spawnTick.SerializedData, ghost.ServerSpawnTick.SerializedData + 5) ||
                                !SequenceHelpers.IsNewer(spawnList[j].spawnTick.SerializedData + 5, ghost.ServerSpawnTick.SerializedData)
                            )
                                continue;
                                
                            ghost.PredictedSpawnEntity = spawnList[j].entity;
                            int lastIndex = spawnList.Length - 1;
                            spawnList[j] = spawnList[lastIndex];
                            spawnList.RemoveAt(lastIndex);
                            break;
                        }
                        
                        ghosts[i] = ghost;
                    }
                }).Schedule(Dependency);
        }
    }
}

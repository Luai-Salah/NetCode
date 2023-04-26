using System;
using System.Collections.Generic;

using Unity.Entities;
using Unity.NetCode;

namespace Xedrial.NetCode
{
    public class NetCodeBootstrap : ClientServerBootstrap
    {
        public override bool Initialize(string defaultWorldName)
        {
            var world = new World(defaultWorldName);
            World.DefaultGameObjectInjectionWorld = world;

            IReadOnlyList<Type> systems = DefaultWorldInitialization.GetAllSystems(
                WorldSystemFilterFlags.Default
            );
            
            GenerateSystemLists(systems);

            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, ExplicitDefaultWorldSystems);
#if !UNITY_DOTSRUNTIME
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);
#endif
            return true;
        }
    }
}
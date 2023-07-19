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
            CreateLocalWorld(defaultWorldName);
            return true;
        }
    }
}
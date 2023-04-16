using Unity.Collections;

using Unity.Entities;

namespace Xedrial.NetCode
{
    public struct ClientDataComponent : IComponentData
    {
        //Must used "FixedStringNBytes" instead of string in IComponentData
        //This is a DOTS requirement because IComponentData must be a struct
        public FixedString64Bytes ConnectToServerIp;
        public ushort GamePort;
    }

    public struct ServerDataComponent : IComponentData
    {
        public ushort GamePort;
    }

    public struct InitializeServerComponent : IComponentData
    {
    }

    public struct InitializeClientComponent : IComponentData
    {
    }
}
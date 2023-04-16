using Unity.Entities;
using UnityEngine;

namespace Xedrial.NetCode.Server.Components
{
    [GenerateAuthoringComponent]
    public struct BulletLifetimeComponent : IComponentData
    {
        public float MaxValue;
        [HideInInspector] public float Value;
    }
}
using Unity.Entities;
using UnityEngine;

namespace Xedrial.NetCode.Server.Components
{
    public struct BulletLifetime : IComponentData
    {
        public float MaxValue;
        [HideInInspector] public float Value;
    }
}
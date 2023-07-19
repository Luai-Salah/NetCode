using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using Xedrial.Physics.b2D.Components;

namespace Xedrial.NetCode.Server.Components
{
    [GhostComponentVariation(typeof(BodyDef), "Server Body Def")]
    [GhostComponent(PrefabType=GhostPrefabType.Server, SendTypeOptimization=GhostSendType.DontSend)]
    public struct ServerBodyDef : IComponentData
    {
    }
    
    [GhostComponentVariation(typeof(FixtureDef), "Server Fixture Def")]
    [GhostComponent(PrefabType=GhostPrefabType.Server, SendTypeOptimization=GhostSendType.DontSend)]
    public struct ServerFixtureDef : IComponentData
    {
    }
    
    [GhostComponentVariation(typeof(PhysicsBody2D), "Server Body")]
    [GhostComponent(PrefabType=GhostPrefabType.Server, SendTypeOptimization=GhostSendType.DontSend)]
    public struct ServerPhysicsBody2D : IComponentData
    {
    }
    
    [GhostComponentVariation(typeof(PhysicsCollider2D), "Server Collider")]
    [GhostComponent(PrefabType=GhostPrefabType.Server, SendTypeOptimization=GhostSendType.DontSend)]
    public struct ServerPhysicsCollider2D : IComponentData
    {
    }
}
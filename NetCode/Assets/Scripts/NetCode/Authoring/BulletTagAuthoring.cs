using Unity.Entities;
using UnityEngine;
using Xedrial.NetCode.Components;

namespace Xedrial.NetCode.Authoring
{
    public class BulletTagAuthoring : MonoBehaviour
    {
        public class BulletTagBaker : Baker<BulletTagAuthoring>
        {
            public override void Bake(BulletTagAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BulletTag());
            }
        }
    }
}
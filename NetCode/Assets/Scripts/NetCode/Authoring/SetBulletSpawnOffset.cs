using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Xedrial.NetCode.Components;

namespace Xedrial.NetCode.Authoring
{
    public class SetBulletSpawnOffset : MonoBehaviour
    {
        public GameObject m_BulletSpawn;
        
        private class SetBulletSpawnOffsetBaker : Baker<SetBulletSpawnOffset>
        {
            public override void Bake(SetBulletSpawnOffset authoring)
            {
                var bulletOffset = default(BulletSpawnOffset);

                Vector3 offsetVector = authoring.m_BulletSpawn.transform.position;
                bulletOffset.Value = new float3(offsetVector.x, offsetVector.y, offsetVector.z);

                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, bulletOffset);
                AddComponent<WeaponCoolDownComponent>(entity);
            }
        }
    }
}
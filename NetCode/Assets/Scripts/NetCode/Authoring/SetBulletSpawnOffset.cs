using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;

namespace Xedrial.NetCode.Components
{
    public class SetBulletSpawnOffset : MonoBehaviour, IConvertGameObjectToEntity
    {
        public GameObject m_BulletSpawn;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var bulletOffset = default(BulletSpawnOffsetComponent);

            Vector3 offsetVector = m_BulletSpawn.transform.position;
            bulletOffset.Value = new float3(offsetVector.x, offsetVector.y, offsetVector.z);        

            dstManager.AddComponentData(entity, bulletOffset);
            dstManager.AddComponent<WeaponCoolDownComponent>(entity);
        }
    }
}
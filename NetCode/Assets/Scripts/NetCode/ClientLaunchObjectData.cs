using UnityEngine;

namespace Xedrial.NetCode
{
    public class ClientLaunchObjectData : MonoBehaviour
    {
        public string IPAddress => m_IPAddress;
        
        [SerializeField] private string m_IPAddress = "127.0.0.1";
    }
}
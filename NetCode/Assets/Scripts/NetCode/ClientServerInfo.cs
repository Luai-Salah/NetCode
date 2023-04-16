using UnityEngine;

namespace Xedrial.NetCode
{
    public class ClientServerInfo : MonoBehaviour
    {
        public bool IsServer
        {
            get => m_IsServer;
            set => m_IsServer = value;
        }

        public bool IsClient
        {
            get => m_IsClient;
            set => m_IsClient = value;
        }

        public string ConnectToServerIp
        {
            get => m_ConnectToServerIp;
            set => m_ConnectToServerIp = value;
        }

        public ushort GamePort
        {
            get => m_GamePort;
            set => m_GamePort = value;
        }

        [SerializeField] private bool m_IsServer;
        [SerializeField] private bool m_IsClient;
        [SerializeField] private string m_ConnectToServerIp;
        [SerializeField] private ushort m_GamePort = 5001;
    }
}
using System.Net;
using UnityEngine;
using UnityEngine.UIElements;

namespace Xedrial.NetCode.UI
{
    public class ManualConnectScreen : VisualElement
    {
        private TextField m_GameIp;
        private TextField m_PlayerName;
        private IPAddress m_MyIP;

        public new class UxmlFactory : UxmlFactory<ManualConnectScreen, UxmlTraits> { }

        public ManualConnectScreen() => RegisterCallback<GeometryChangedEvent>(OnGeometryChange);


        private void OnGeometryChange(GeometryChangedEvent evt)
        {
            // 
            // PROVIDE ACCESS TO THE FORM ELEMENTS THROUGH VARIABLES
            // 
            m_GameIp = this.Q<TextField>("game-ip");
            m_PlayerName = this.Q<TextField>("player-name");

            //  CLICKING CALLBACKS
            this.Q("launch-connect-game")?.RegisterCallback<ClickEvent>(ClickedJoinGame);

            UnregisterCallback<GeometryChangedEvent>(OnGeometryChange);
        }

        private static void ClickedJoinGame(ClickEvent _)
        {
            Debug.Log("clicked manual connect");
        }
    }
}
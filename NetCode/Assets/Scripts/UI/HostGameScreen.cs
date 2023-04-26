using System.Net;
using UnityEngine;
using UnityEngine.UIElements;

namespace Xedrial.NetCode.UI
{
    public class HostGameScreen : VisualElement
    {
        private TextField m_GameName;
        private TextField m_GameIP;
        private TextField m_PlayerName;
        private IPAddress m_MyIP;

        public new class UxmlFactory : UxmlFactory<HostGameScreen, UxmlTraits>
        {
        }

        public HostGameScreen()
        {
            RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
        }

        private void OnGeometryChange(GeometryChangedEvent evt)
        {
            // 
            // PROVIDE ACCESS TO THE FORM ELEMENTS THROUGH VARIABLES
            // 
            m_GameName = this.Q<TextField>("game-name");
            m_GameIP = this.Q<TextField>("game-ip");
            m_PlayerName = this.Q<TextField>("player-name");

            //  CLICKING CALLBACKS
            this.Q("launch-host-game")?
                .RegisterCallback<ClickEvent>(ClickedHostGame);

            UnregisterCallback<GeometryChangedEvent>(OnGeometryChange);
        }

        private static void ClickedHostGame(ClickEvent _)
        {
            Debug.Log("clicked host game");
        }
    }
}
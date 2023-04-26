using System.Net;
using UnityEngine;
using UnityEngine.UIElements;

namespace Xedrial.NetCode.UI
{
    public class JoinGameScreen : VisualElement
    {
        private Label m_GameName;
        private Label m_GameIp;
        private TextField m_PlayerName;
        private IPAddress m_MyIP;

        public new class UxmlFactory : UxmlFactory<JoinGameScreen, UxmlTraits> { }

        public JoinGameScreen() => RegisterCallback<GeometryChangedEvent>(OnGeometryChange);

        private void OnGeometryChange(GeometryChangedEvent evt)
        {
            // 
            // PROVIDE ACCESS TO THE FORM ELEMENTS THROUGH VARIABLES
            // 
            m_GameName = this.Q<Label>("game-name");
            m_GameIp = this.Q<Label>("game-ip");
            m_PlayerName = this.Q<TextField>("player-name");

            //  CLICKING CALLBACKS
            this.Q("launch-join-game")?.RegisterCallback<ClickEvent>(ClickedJoinGame);
            
            UnregisterCallback<GeometryChangedEvent>(OnGeometryChange);
        }

        private static void ClickedJoinGame(ClickEvent _)
        {
            Debug.Log("clicked client game");
        }

        public void LoadJoinScreenForSelectedServer(GameObject localGame)
        {
            m_GameName = this.Q<Label>("game-name");
            m_GameIp = this.Q<Label>("game-ip");
            
            m_GameName.text = localGame.name;
            m_GameIp.text = localGame.name;
        }
    }
}
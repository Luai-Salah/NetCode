using UnityEngine.UIElements;

namespace Xedrial.NetCode.UI
{
    public class TitleScreenManager : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<TitleScreenManager, UxmlTraits>
        {
        }

        private VisualElement m_TitleScreen;
        private VisualElement m_HostScreen;
        private VisualElement m_JoinScreen;
        private VisualElement m_ManualConnectScreen;

        public TitleScreenManager()
        {
            RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
        }

        private void OnGeometryChange(GeometryChangedEvent evt)
        {
            m_TitleScreen = this.Q("TitleScreen");
            m_HostScreen = this.Q("HostGameScreen");
            m_JoinScreen = this.Q("JoinGameScreen");
            m_ManualConnectScreen = this.Q("ManualConnectScreen");

            m_TitleScreen?
                .Q("host-local-button")?
                .RegisterCallback<ClickEvent>(_ => EnableHostScreen());
            
            m_TitleScreen?
                .Q("join-local-button")?
                .RegisterCallback<ClickEvent>(_ => EnableJoinScreen());
            
            m_TitleScreen?
                .Q("manual-connect-button")?
                .RegisterCallback<ClickEvent>(_ => EnableManualScreen());

            m_HostScreen?
                .Q("main-menu-button")?
                .RegisterCallback<ClickEvent>(_ => EnableTitleScreen());
            
            m_JoinScreen?
                .Q("main-menu-button")?
                .RegisterCallback<ClickEvent>(_ => EnableTitleScreen());
            
            m_ManualConnectScreen?
                .Q("main-menu-button")?
                .RegisterCallback<ClickEvent>(_ => EnableTitleScreen());
            
            UnregisterCallback<GeometryChangedEvent>(OnGeometryChange);
        }

        private void EnableHostScreen()
        {
            DisableAllScreens();
            m_HostScreen.style.display = DisplayStyle.Flex;
        }

        public void EnableJoinScreen()
        {
            DisableAllScreens();
            m_JoinScreen.style.display = DisplayStyle.Flex;
        }

        private void EnableManualScreen()
        {
            DisableAllScreens();
            m_ManualConnectScreen.style.display = DisplayStyle.Flex;
        }

        private void EnableTitleScreen()
        {
            DisableAllScreens();
            m_TitleScreen.style.display = DisplayStyle.Flex;
        }

        private void DisableAllScreens()
        {
            m_HostScreen.style.display = DisplayStyle.None;
            m_JoinScreen.style.display = DisplayStyle.None;
            m_ManualConnectScreen.style.display = DisplayStyle.None;
            m_TitleScreen.style.display = DisplayStyle.None;
        }
    }
}
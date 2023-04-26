using UnityEngine;
using UnityEngine.UIElements;

namespace Xedrial.NetCode.UI
{
    public class GameUIManager : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<GameUIManager, UxmlTraits>
        {
        }

        private VisualElement m_LeaveArea;

        public GameUIManager() => RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

        private void OnGeometryChanged(GeometryChangedEvent _)
        {
            m_LeaveArea = this.Q("quit-game");
            
            m_LeaveArea?.RegisterCallback<ClickEvent>(_ =>
            {
                Debug.Log("Clicked quit game");
            });
            UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
    }
}
using UnityEngine.UIElements;

namespace Core.Utils
{
    [UxmlElement]
    public partial class UISwitch : Toggle
    {
        public UISwitch()
        {
            var inner = new VisualElement();
            this.Q<VisualElement>("unity-checkmark").Add(inner);
            inner.AddToClassList("inner");
        }
    }
    
}

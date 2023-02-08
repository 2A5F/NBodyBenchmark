using UnityEngine;
using UnityEngine.UIElements;

namespace Core
{

    public class FPS : MonoBehaviour
    {
        public UIDocument document;
        private VisualElement root;
        private Label label;
        
        [Tooltip("帧率刷新速率")]
        public float refreshRate = 0.25f;

        private void Start()
        {
            root = document.rootVisualElement;
            label = root.Q<Label>("FPSLabel");
        }

        private float unscaledDeltaTime;
        
        private float timeInc;
        
        private void Update()
        {
            unscaledDeltaTime = Time.unscaledDeltaTime;
            timeInc += unscaledDeltaTime;
            if (timeInc >= refreshRate)
            {
                timeInc = 0;
                label.text = $"{(1 / unscaledDeltaTime):0.0} ({(unscaledDeltaTime * 1000):0.0}ms)";
            }
        }
    }

}

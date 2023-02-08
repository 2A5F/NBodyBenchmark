using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Utils
{

    public class Blur : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<Blur, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits { }

        public static readonly RenderTexture BlurTexture = StaticAssets.Get<RenderTexture>("UiBlur.rendertexture");

        public Blur()
        {
            style.backgroundImage = new StyleBackground(Background.FromRenderTexture(BlurTexture));

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            container = new VisualElement
            {
                name = "blur-box-container"
            };
            container.AddToClassList("blur-box-container");
            container.style.flexGrow = new StyleFloat(1);
            hierarchy.Add(container);
        }

        public bool MouseOn { get; private set; }

        public void RegMouseOn()
        {
            RegisterCallback<MouseEnterEvent>(SetMouseOnTrue);
            RegisterCallback<MouseLeaveEvent>(SetMouseOnFalse);
        }

        private void SetMouseOnTrue<T>(T _)
        {
            MouseOn = true;
        }

        private void SetMouseOnFalse<T>(T _)
        {
            MouseOn = false;
        }

        private readonly VisualElement container;

        public override VisualElement contentContainer => container;

        private Vector2 screenSize;
        private Rect panelRect;

        void CalcPanelRect()
        {
            var screen_size = new Vector2(Screen.width, Screen.height);
            if (screen_size == screenSize) return;
            screenSize = screen_size;
            try
            {
                var screen_lt = RuntimePanelUtils.ScreenToPanel(panel, Vector2.zero);
                var screen_rb = RuntimePanelUtils.ScreenToPanel(panel, screen_size);
                panelRect = new Rect(screen_lt, screen_rb);
            }
            catch
            {
                // ignored
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent e)
        {
            ReRender();
        }

        public void ReRender()
        {
            CalcPanelRect();
            var offset_rect = this.WorldToLocal(panelRect);
            style.backgroundPositionX =
                new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Left, offset_rect.x));
            style.backgroundPositionY =
                new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Top, offset_rect.y));
            style.backgroundSize =
                new StyleBackgroundSize(new BackgroundSize(offset_rect.width, offset_rect.height));
        }
    }

}

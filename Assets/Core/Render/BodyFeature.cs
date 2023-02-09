using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Core.Render
{

    public class BodyFeature : ScriptableRendererFeature
    {
        public BodyRender render;

        private BodyPass pass;

        public override void Create()
        {
            pass = new() { render = render };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(pass);
        }
    }

}

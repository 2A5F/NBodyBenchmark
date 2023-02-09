using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Core.Render
{

    public class BodyPass : ScriptableRenderPass
    {
        public BodyRender render;

        public BodyPass()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            profilingSampler = new ProfilingSampler(nameof(BodyPass));
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var count = render.param.count;
            if (count == 0) return;

            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, profilingSampler))
            {
                cmd.SetGlobalConstantBuffer(render.param_buf, BodyRender.InitParamID, 0, BodyRender.ParamBufSize);
                cmd.SetGlobalBuffer(BodyRender.BodiesID, render.bodies_buf);
                cmd.SetGlobalBuffer(BodyRender.RendersID, render.renders_buf);
                cmd.DrawMeshInstancedProcedural(render.bodyMesh, 0, render.bodyMaterial, -1, (int)count);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }
    }

}

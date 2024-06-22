using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Core.Utils
{
    
    public class DualKawaseBlurPass : ScriptableRenderPass, IDisposable
    {
        public readonly DualKawaseBlurFeature feature;
        public readonly RTHandle renderTexture;
        public readonly Material mat;
        
        public readonly int DownPass;
        public readonly int UpPass;
        
        public static readonly int MainTex = Shader.PropertyToID("_MainTex");
        public static readonly int Offset = Shader.PropertyToID("_Offset");
        
        public DualKawaseBlurPass(DualKawaseBlurFeature feature)
        {
            this.feature = feature;
            renderTexture = RTHandles.Alloc(feature.renderTexture);
            mat = StaticAssets.Get<Material>("DualKawaseBlur.mat");
            DownPass = mat.FindPass("Down");
            UpPass = mat.FindPass("Up");
        }
        
        private class PassData
        {
            public TextureHandle input;
            public TextureHandle output;
            public Material mat;
            public int pass;
        }
        
        private static readonly Dictionary<int, string> DownPassNames = new();
        private static readonly Dictionary<int, string> UpPassNames = new();
        private static readonly Dictionary<int, string> DownTmpNames = new();
        private static readonly Dictionary<int, string> UpTmpNames = new();
        
        private static MaterialPropertyBlock property_block = new();
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (!feature.renderTexture) return;
            
            var resData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();
            var camDesc = cameraData.cameraTargetDescriptor;
            var colorFormat = camDesc.graphicsFormat;
            
            #region Scale RT
            
            if (feature.renderTexture.width != camDesc.width || feature.renderTexture.height != camDesc.height)
            {
                feature.renderTexture.Release();
                feature.renderTexture.width = camDesc.width;
                feature.renderTexture.height = camDesc.height;
                feature.renderTexture.Create();
            }
            
            #endregion
            
            var target = renderGraph.ImportTexture(renderTexture);
            
            var preTexture = resData.activeColorTexture;
            
            property_block.SetFloat(Offset, feature.radius);
            
            Span<float> scales = stackalloc float[feature.quality + 1];
            scales[0] = 1f;
            var scale = 1f;
            
            for (var i = 0; i < feature.quality; i++)
            {
                var pass_name = DownPassNames.AddOrGet(i, static i => $"Dual Kawase Blur Down {i}");
                var tmp_name = DownTmpNames.AddOrGet(i, static i => $"Dual Kawase Blur Down Tmp {i}");
                scale /= feature.scale;
                scales[i + 1] = scale;
                using var builder = renderGraph.AddRasterRenderPass(pass_name, out PassData data);
                builder.AllowPassCulling(false);
                data.mat = mat;
                data.pass = DownPass;
                data.input = preTexture;
                data.output = preTexture = renderGraph.CreateTexture(
                    new TextureDesc(new Vector2(scale, scale), true)
                    {
                        useDynamicScaleExplicit = true,
                        colorFormat = colorFormat,
                        name = tmp_name,
                    }
                );
                builder.SetInputAttachment(data.input, 0);
                builder.SetRenderAttachment(data.output, 0, AccessFlags.WriteAll);
                builder.SetRenderFunc<PassData>(static (data, ctx) =>
                {
                    property_block.SetTexture(MainTex, data.input);
                    ctx.cmd.DrawProcedural(Matrix4x4.identity, data.mat, data.pass, MeshTopology.Triangles, 3, 1,
                        property_block);
                });
            }
            
            for (var i = 0; i < feature.quality; i++)
            {
                var pass_name = UpPassNames.AddOrGet(i, static i => $"Dual Kawase Blur Up {i}");
                var tmp_name = UpTmpNames.AddOrGet(i, static i => $"Dual Kawase Blur Up Tmp {i}");
                scale = scales[feature.quality - i - 1];
                var is_last = i + 1 == feature.quality;
                using var builder = renderGraph.AddRasterRenderPass(pass_name, out PassData data);
                builder.AllowPassCulling(false);
                data.mat = mat;
                data.pass = UpPass;
                data.input = preTexture;
                if (is_last) data.output = target;
                else
                {
                    data.output = preTexture = renderGraph.CreateTexture(
                        new TextureDesc(new Vector2(scale, scale), true)
                        {
                            useDynamicScaleExplicit = true,
                            colorFormat = colorFormat,
                            name = tmp_name,
                        }
                    );
                }
                builder.SetInputAttachment(data.input, 0);
                builder.SetRenderAttachment(data.output, 0, AccessFlags.WriteAll);
                builder.SetRenderFunc<PassData>(static (data, ctx) =>
                {
                    property_block.SetTexture(MainTex, data.input);
                    ctx.cmd.DrawProcedural(Matrix4x4.identity, data.mat, data.pass, MeshTopology.Triangles, 3, 1,
                        property_block);
                });
            }
        }
        
        public void Dispose()
        {
            renderTexture.Release();
        }
    }
    
}

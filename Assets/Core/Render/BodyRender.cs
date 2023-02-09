using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Core.Render
{

    [CreateAssetMenu(fileName = "Body Render", menuName = "Body Render", order = 0)]
    public class BodyRender : ScriptableObject
    {
        public Mesh bodyMesh;
        public Material bodyMaterial;
        public ComputeShader initShader;

        private int initKernel;

        private void Awake()
        {
            initKernel = initShader.FindKernel("init");
        }

        // ReSharper disable once InconsistentNaming
        public struct init_param
        {
            // ReSharper disable once NotAccessedField.Local
            public uint count;
            // ReSharper disable once NotAccessedField.Local
            public uint seed;
            // ReSharper disable once NotAccessedField.Local
            public float speed_limit;
            // ReSharper disable once NotAccessedField.Local
            public float space_size;
            // ReSharper disable once NotAccessedField.Local
            public float2 weight_range;
            // ReSharper disable once NotAccessedField.Local
            public float2 velocity_range;
        }

        public init_param param;
        public ComputeBuffer param_buf;

        public ComputeBuffer bodies_buf;
        public ComputeBuffer renders_buf;
        public ComputeBuffer velocities_buf;

        public const int Float2Size = sizeof(float) * 2;
        public const int Float3Size = sizeof(float) * 3;
        public const int ParamBufSize = sizeof(uint) * 2 + sizeof(float) * 2 + Float2Size * 2;
        public const int BodySize = sizeof(float) * 2;
        public const int BodyRenderSize = Float3Size * 2;
        public const int BodyVelocity = Float3Size;

        public void Init(in Init init)
        {
            var seed = init.useSeed ? init.seed : (uint)(Time.unscaledTimeAsDouble * 1000);
            
            param = new init_param
            {
                count = (uint)init.count,
                seed = seed,
                speed_limit = init.speedLimit,
                space_size = init.spaceSize,
                weight_range = new float2(init.minWeight, init.maxWeight),
                velocity_range = new float2(init.minVelocity, init.maxVelocity),
            };
            param_buf = new ComputeBuffer(1, ParamBufSize, ComputeBufferType.Constant);
            param_buf.SetData(new List<init_param> { param });
            initShader.SetConstantBuffer(InitParamID, param_buf, 0, ParamBufSize);

            bodies_buf = new ComputeBuffer(init.count, BodySize, ComputeBufferType.Structured);
            initShader.SetBuffer(initKernel, BodiesID, bodies_buf);

            renders_buf = new ComputeBuffer(init.count, BodyRenderSize, ComputeBufferType.Structured);
            initShader.SetBuffer(initKernel, RendersID, renders_buf);

            velocities_buf = new ComputeBuffer(init.count, BodyVelocity, ComputeBufferType.Structured);
            initShader.SetBuffer(initKernel, VelocitiesID, velocities_buf);

            initShader.Dispatch(initKernel, IntCeilDiv(init.count, 1024), 1, 1);
        }
        
        public void Reset()
        {
            param = default;
            param_buf?.Release();
            param_buf = null;
            bodies_buf?.Release();
            bodies_buf = null;
            renders_buf?.Release();
            renders_buf = null;
            velocities_buf?.Release();
            velocities_buf = null;
        }

        public static readonly int InitParamID = Shader.PropertyToID("init_param");
        public static readonly int BodiesID = Shader.PropertyToID("bodies");
        public static readonly int RendersID = Shader.PropertyToID("renders");
        public static readonly int VelocitiesID = Shader.PropertyToID("velocities");

        private int IntCeilDiv(int x, int y)
        {
            var r = x / y;
            var m = x % y;
            return r + m != 0 ? 1 : 0;
        }
    }

}

using System.Runtime.InteropServices;
using Core.Gpu;
using Unity.Collections;
using Unity.Entities;

namespace Core
{

    public struct Init : IComponentData
    {
        public NativeArray<VFXBodyData> vfxBodyData;
        public float speedLimit;
        public float spaceSize;
        public float minWeight;
        public float maxWeight;
        public float minVelocity;
        public float maxVelocity;
        public uint seed;
        public int count;
        [MarshalAs(UnmanagedType.U1)]
        public bool useSeed;
        [MarshalAs(UnmanagedType.U1)]
        public bool useGpu;
    }

    public struct Inited : IComponentData { }
    public struct VfxInited : IComponentData { }

    public struct InitPrefab : IComponentData
    {
        public Entity bodyPrefab;
    }

    public struct ReInit : IComponentData
    {
        public Init init;
    }
}

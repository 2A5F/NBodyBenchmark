using System.Runtime.InteropServices;
using Unity.Entities;

namespace Core
{

    public struct Init : IComponentData
    {
        public uint seed;
        [MarshalAs(UnmanagedType.U1)]
        public bool useSeed;
        public int count;
        public float speedLimit;
        public float spaceSize;
        public float minWeight;
        public float maxWeight;
        public float minVelocity;
        public float maxVelocity;
    }

    public struct Inited : IComponentData { }

    public struct InitPrefab : IComponentData
    {
        public Entity bodyPrefab;
    }

    public struct ReInit : IComponentData
    {
        public Init init;
    }
}

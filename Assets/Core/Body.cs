using Unity.Entities;
using Unity.Mathematics;

namespace Core
{

    public struct Body : IComponentData
    {
        public float weight;
        public float size;
    }
    
    public struct BodyLastPos : IComponentData
    {
        public float3 lastPos;
    }

    public struct BodyVelocity : IComponentData
    {
        public float3 velocity;
    }
}

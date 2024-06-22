using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace Core.Gpu
{
    
    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct VFXBodyData
    {
        public Vector4 color;
        public Vector4 pos;
        public Vector4 lastPos;
        public Vector4 velocity;
        public float weight;
        public float size;
    }
    
}

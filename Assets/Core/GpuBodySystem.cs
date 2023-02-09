using System;
using System.Collections.Generic;
using Core.Render;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Core
{

    public class GpuBodySystem : MonoBehaviour
    {
        public BodyRender render;

        public void Start()
        {
            // render.Init(new Init
            // {
            //     count = 100,
            //     speedLimit = 299_792_458,
            //     spaceSize = 1000,
            //     minWeight = 10,
            //     maxWeight = 100,
            //     minVelocity = 0,
            //     maxVelocity = 10,
            // });
        }
        
        private void OnDestroy() => render.Reset();
    }

}

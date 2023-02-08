using System;
using UnityEngine;

namespace Core.Utils
{

    [Serializable]
    public class UiBlurConfig
    {
        public UiBlurMethod method;
        public UiBoxBlurConfig boxBlurConfig;
        public UIGaussianBlurConfig gaussianBlurConfig;
    }

    [Serializable]
    public class UiBoxBlurConfig
    {
        [Range(1, 16)]
        public int quality = 1;
        [Range(0, 100)]
        public float radius = 1;
        public bool scaling = true;
    }

    [Serializable]
    public class UIGaussianBlurConfig
    {
        [Range(0, 6)]
        [Tooltip("[降采样次数] 向下采样的次数。此值越大,则采样间隔越大,需要处理的像素点越少,运行速度越快。")]
        public int downSamplingTimes = 2;
        [Range(0.0f, 20.0f)]
        [Tooltip("[模糊扩散度] 进行高斯模糊时，相邻像素点的间隔。此值越大相邻像素间隔越远，图像越模糊。但过大的值会导致失真。")]
        public float spread = 3.0f;
        [Range(0, 8)]
        [Tooltip("[迭代次数] 此值越大,则模糊操作的迭代次数越多，模糊效果越好，但消耗越大。")]
        public int iterations = 3;
    }

}

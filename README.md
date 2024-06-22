# NBodyBenchmark

一个用 unity ecs 写的 n 体计算基准测试
- 纯暴力运算，每个天体都会和所有其他天界计算引力

---

以下是使用 7950x + 7900xtx cpu 模式运行一万个天体的帧率

![image](https://user-images.githubusercontent.com/13982338/217540428-c051f6e1-e95a-4507-99c8-eb7c855e6028.png)

---

以下是使用 7950x + 7900xtx gpu 模式运行六万个天体的帧率  
 > *为什么是 65535，因为到计算着色器 dispatch 上限了，有空改成多 pass*

![image](https://github.com/2A5F/NBodyBenchmark/assets/13982338/1325665f-5b12-422a-b895-300bfdaea4cc)


### 操作

- `esc`  
  菜单
- `space`  
  暂停
- `1`, `2`, `3`, `4`, `5`  
  调速度
- `w`, `s`, `a`, `d`  
  前后左右
- `e`, `q`  
  上下
- `鼠标按住右键拖动`  
  旋转视角
- `鼠标滚轮`  
  调整移动速度 
- `shift`  
  临时加速移动 
 

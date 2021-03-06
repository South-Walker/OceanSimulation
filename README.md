# OceanSimulation
## 简介
开坑海洋模拟，基于Unity利用多种模型（正弦波、Gerstner波、海洋统计学模型）在GPU与CPU上实现对海洋表面的模拟
## 正弦波叠加  
正弦波是非常简单的基于高度场的模型，只会影响水面竖直方向，单个正弦波可以描述成如下形式：<br><br>
![](/Formula/SinesW.gif)<br><br>
在时刻t下，顶点具体的高度等于所有正弦波的累加：<br><br>
![](/Formula/SinesH.gif)<br><br>
为了正确渲染图元，除了顶点位置外还需要计算出顶点的法线信息，显然在每一时刻，曲面H是由x与y确定的曲面，故其法线表达式如下：<br><br>
![](/Formula/SinesN.gif)<br><br>
其中P为对应顶点的位置，表示为：<br><br>
![](/Formula/SinesP.gif)<br><br>
至此便得到了所需的所有关系，采用4个正弦波实现的效果如下图：<br><br>
![](/Gif/Sines.gif)<br><br>
正弦波叠加适合模拟较为平静的水面，当我们想获得更陡峭的波峰与更宽广的波谷时，两个明显的思路分别是修改高度场函数与水平移动顶点，让其波峰陡峭处顶点更加密集,基于后者思路的一个实现就是Gerstner波模型

## Gerstner波
Gerstner波模型并不是只基于高度场的模型，在该模型中，时刻t下，顶点的位置函数为：<br><br>
![](/Formula/GerstnerP.gif)<br><br>
其中Q是表示波峰陡度的参数，取0时等同于正弦波，最大取频率与振幅乘积的倒数，此时波峰最陡。<br>
计算法线，可得：<br><br>
![](/Formula/GerstnerN.gif)<br><br>
采用4个Gerstner波实现的效果如下图:<br><br>
![](/Gif/Gerstner.gif)<br><br>
* 项目中是通过切线与半切线的叉积得到的法线<br>
##
可以观察到其较正弦波模型有更陡峭的波峰与更宽广的波谷。
## 海洋统计学模型
### 基础原理
有学者根据大量海洋浮标的实际运动，在高度场上建立了更加贴合现实且具有良好数学特性的海洋表面模型,在时刻t下某坐标对应的水面高度如下：<br><br>
![](/Formula/DFTH.gif)<br><br>
其中输入参数为顶点水平面坐标，方便起见写成了向量形式，向量k具体取值如下：<br><br>
![](/Formula/DFTK.gif)<br><br>
其中，n与m满足如下关系：<br><br>
![](/Formula/DFTn.gif)<br><br>
![](/Formula/DFTm.gif)<br><br>
显然该式在形式上类似于一个傅里叶反演，对应的频率域函数表示如下：<br><br>
![](/Formula/DFTht.gif)<br><br>
![](/Formula/DFTw.gif)<br><br>
![](/Formula/DFTht0.gif)<br><br>
![](/Formula/DFTPh.gif)<br><br>
* ![](/Formula/tildeh0x.gif) 是 ![](/Formula/tildeh0.gif) 的共轭复数
* ![](/Formula/A.gif) 表示波峰高度 ![](/Formula/VecW.gif) ,表示风向
* ![](/Formula/L.gif) 满足等式 ![](/Formula/DFTL.gif) 表示风速 ![](/Formula/V.gif) 对波峰高度的限制。
* ![](/Formula/varepsilon_r.gif) 与 ![](/Formula/varepsilon_i.gif) 是随机数，根据其服从的分布条件生成的波浪会有所区别，本项目中服从均值为0方差为1的正态分布
* 为了获得服从正态分布的随机数，先用内置库函数取得了服从均匀分布的随机数，随后使用Box-Muller算法将其转变为服从正态分布的随机数

### 在CPU上计算
原理上通过上述等式已经可以基于高度场描述海洋曲面了，但为了获得更陡峭的波峰与更宽广的波谷，可以借鉴Gerstner波模型的思路，加入顶点在水平面上的位移,在这里用向量 ![](/Formula/VecD.gif) 表示，具体计算方法如下：<br><br>
![](/Formula/DFTD.gif)<br><br>
出于方便渲染考虑，往往还需要计算出对应顶点的法线<br><br>
![](/Formula/DFTNor.gif)<br><br>
（此处法线没有考虑水平位移，应该使用切线与半切线的叉积计算）


基于上述方程在CPU上运算，对于16x16个顶点的平面，效果图如下：(黑线是用来验证法线正确性的，请忽略)<br><br>
![](/Gif/DFT.gif)<br><br>
* 由于计算量过大，这个级别基本上到了性能瓶颈

暴力求解上述方程计算量过大，观察到高度场函数与水平面上的位移函数都满足离散傅里叶反变换的基本形式，故可以考虑使用快速傅里叶变换算法来优化。
说到快速傅里叶变换（FFT）一个非常标准的实现是Cooley与Tuckey发明的蝶形算法，基于该思想在CPU上实现的二维FFT被封装在FFTHelper类中。<br><br>
![](/Gif/FFT.gif)<br><br>
上图对应的是一个基于二维FFT算法在CPU上计算的，包含32x32个顶点的平面。

### 在GPU上计算
在GPU上进行上述运算稍微复杂一点，首先注意到为了计算初始的 ![](/Formula/tildeh0.gif) 值，需要实现能够在GPU上求解的随机数算法，一个非常经典的算法如下：<br>
```c
//一维
float random (float s)
{
    return fract(sin(s*12.9898)*
        43758.5453123);
}
//二维
float random (vec2 st) 
{
    return fract(sin(dot(st.xy,
                         vec2(12.9898,78.233)))*
        43758.5453123);
}
```
原理大概是随着乘数变大，小数部分的粒度将正弦波的流动性破坏为伪随机的混沌<sup>[4]</sup>。具体的常量取值是魔值。<br>
在GPU上计算得到的结果是经由RenderTexture传递的，几张主要的RenderTexture如下：<br>
* 某参数下的初始频谱，前两个通道存储复数 ![](/Formula/tildeh0.gif) 后两个通道则存储 ![](/Formula/tildeh0x.gif)<br>
![](/Png/h0.png)<br><br>
* 某参数下的 ![](/Formula/omegaKT.gif) 值，直接受时间变量影响，只使用一个通道存储 <br>
![](/Png/omegakt.png)<br><br>
* 某参数下的高度值，在渲染时用来进行凹凸映射，使用两个通道存储，实际使用时不会使用虚部值。<br>
![](/Png/height.png)<br><br>
* 某参数下的水平扰动，使用四个通道存储两个方向上的移动。<br>
![](/Png/displace.png)<br><br>
* 某参数下的法线扰动，使用的并非是上述法线公式，而是使用微元法计算出曲面在x与z方向上的偏导方向后进行叉积得到。<br>
![](/Png/normal.png)<br><br>

除此之外，在现实中的海面往往会具有WhiteCap现象，这是由于扰动过于剧烈使得某些波浪破碎后发生的反射率骤变<sup>[6]</sup>。在渲染中考虑该效应可以增添真实感，对于满足如下特征的顶点变换<br><br>
![](/Formula/WhiteCapq.gif)<br><br>
计算是否生成WhiteCap的函数如下：<br><br>
![](/Formula/WhiteCapb.gif)<br><br>
![](/Formula/WhiteCapjaco.gif)<br><br>
* 其中 ![](/Formula/epsilon.gif) 为阈值， ![](/Formula/Upsilon.gif) 是开关函数

其中由于本模型中水平方向偏移倍率为常数，且根据 ![](/Formula/VecD.gif) 定义，可以把行列式写成如下形式：<br><br>
![](/Formula/WhiteCapmyjaco.gif)<br><br>
偏导数的解析解较为复杂，但是可以使用微元法，从纹理中采样获得数值解，渲染得到的WhiteCap纹理如下：<br>
![](/Png/whitecap.png)<br><br>

基于上述所有，套用Phong光照模型渲染的结果如下：<br><br>
![](/Gif/GPUFFT.gif)<br><br>

## TODO
>* 重构

## 引用
[1] [GPU Gems](https://developer.nvidia.com/gpugems/GPUGems/gpugems_ch01.html) <br>
[2] [Ocean simulation part one: using the discrete Fourier transform](https://www.keithlantz.net/2011/10/ocean-simulation-part-one-using-the-discrete-fourier-transform/) <br>
[3] Tessendorf, Jerry. Simulating Ocean Water. In SIGGRAPH 2002 Course Notes #9 (Simulating Nature: Realistic and Interactive Techniques), ACM Press. <br>
[4] [The Book of Shaders](https://thebookofshaders.com/)<br>
[5] [Mistral-Water](https://github.com/AlphaMistral/Mistral-Water)<br>
[6] Dupuy J, Bruneton E. Real-time animation and rendering of ocean whitecaps[C]//SIGGRAPH Asia 2012 Technical Briefs. ACM, 2012: 15.<br>
[7] [whitecaps](https://github.com/jdupuy/whitecaps)<br>

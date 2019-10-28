# OceanSimulation
## 简介
开坑海洋模拟，基于Unity利用多种模型（正弦波、Gerstner波、海洋统计学模型）在GPU与CPU上实现对海洋表面的模拟
## 正弦波叠加  
正弦波是非常简单的基于高度场的模型，只会影响水面竖直方向，单个正弦波可以描述成如下形式：<br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/SinesW.gif" alt="show" /><br><br>
在时刻t下，顶点具体的高度等于所有正弦波的累加：<br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/SinesH.gif" alt="show" /><br><br>
为了正确渲染图元，除了顶点位置外还需要计算出顶点的法线信息，显然在每一时刻，曲面H是由x与y确定的曲面，故其法线表达式如下：<br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/SinesN.gif" alt="show" /><br><br>
其中P为对应顶点的位置，表示为：<br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/SinesP.gif" alt="show" /><br><br>
至此便得到了所需的所有关系，采用4个正弦波实现的效果如下图：<br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Gif/Sines.gif" alt="show" /><br><br>
正弦波叠加适合模拟较为平静的水面，当我们想获得更陡峭的波峰与更宽广的波谷时，两个明显的思路分别是修改高度场函数与水平移动顶点，让其波峰陡峭处顶点更加密集,基于后者思路的一个实现就是Gerstner波模型

## Gerstner波
Gerstner波模型并不是只基于高度场的模型，在该模型中，时刻t下，顶点的位置函数为：<br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/GerstnerP.gif" alt="show" /><br><br>
其中Q是表示波峰陡度的参数，取0时等同于正弦波，最大取频率与振幅乘积的倒数，此时波峰最陡。<br>
计算法线，可得：<br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/GerstnerN.gif" alt="show" /><br><br>
采用4个Gerstner波实现的效果如下图:<br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Gif/Gerstner.gif" alt="show" /><br><br>
>* 项目中是通过切线与半切线的叉积得到的法线<br>
##
可以观察到其较正弦波模型有更陡峭的波峰与更宽广的波谷。
## 海洋统计学模型
有学者根据大量海洋浮标的实际运动，在高度场上建立了更加贴合现实且具有良好数学特性的海洋表面模型,在时刻t下某坐标对应的水面高度如下：<br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/DFTH.gif" alt="show" /><br><br>
其中输入参数为顶点水平面坐标，方便起见写成了向量形式，向量k具体取值如下：<br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/DFTK.gif" alt="show" /><br><br>
其中，n与m满足如下关系：<br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/DFTn.gif" alt="show" /><br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/DFTm.gif" alt="show" /><br><br>
显然该式在形式上类似于一个傅里叶反演，对应的频率域函数表示如下：<br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/DFTht.gif" alt="show" /><br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/DFTht0.gif" alt="show" /><br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/DFTPh.gif" alt="show" /><br><br>
>*  <img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/tildeh0x.gif" alt="show" /> 是 <img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/tildeh0.gif" alt="show" /> 的共轭复数
>*  <img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/A.gif" alt="show" /> 表示波峰高度 <img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/VecW.gif" alt="show" /> ,表示风向
>* <img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/L.gif" alt="show" /> 满足等式 <img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/DFTL.gif" alt="show" /> 表示风速 <img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/V.gif" alt="show" /> 对波峰高度的限制。


原理上通过上述等式已经可以基于高度场描述海洋曲面了，但为了获得更陡峭的波峰与更宽广的波谷，可以借鉴Gerstner波模型的思路，加入顶点在水平面方向的位移,在这里用向量 <img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/VecD.gif" alt="show" /> 表示，具体计算方法如下：<br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/DFTD.gif" alt="show" /><br><br>
出于方便渲染考虑，往往还需要计算出对应顶点的法线<br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/DFTNor.gif" alt="show" /><br><br>
基于上述公式直接暴力求解DFT得到的结果如下:<br><br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Gif/temp.gif" alt="show" /><br><br>

>* 项目中是利用频率域上平移的相关性质将坐标轴移动到中心，而非如引用<sup>[2]</sup>中进行了变量代换<br>
## todo
>* 傅里叶反演写成傅里叶变换了

## 引用
[1] [GPU Gems](https://developer.nvidia.com/gpugems/GPUGems/gpugems_ch01.html) <br>
[2] [Ocean simulation part one: using the discrete Fourier transform](https://www.keithlantz.net/2011/10/ocean-simulation-part-one-using-the-discrete-fourier-transform/) <br>
[3] Tessendorf, Jerry. Simulating Ocean Water. In SIGGRAPH 2002 Course Notes #9 (Simulating Nature: Realistic and Interactive Techniques), ACM Press. 

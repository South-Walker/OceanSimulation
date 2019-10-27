# OceanSimulation
## 简介
开坑海洋模拟，基于Unity利用多种模型（正弦波、Gerstner波、海洋统计学模型）在GPU与CPU上实现对海洋表面的模拟
## 正弦波叠加  
>* 正弦波是非常简单的基于高度场的模型，只会影响水面竖直方向，单个正弦波可以描述成如下形式：<br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/SinesW.gif" alt="show" />
在时刻t下，顶点具体的高度等于所有正弦波的累加：<br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/SinesH.gif" alt="show" />
为了正确渲染图元，除了顶点位置外还需要计算出顶点的法线信息，显然在每一时刻，曲面H是由x与y确定的曲面，故其法线表达式如下：<br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/SinesN.gif" alt="show" />
其中P为对应顶点的位置，表示为：<br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/SinesP.gif" alt="show" />
至此便得到了所需的所有关系，采用4个正弦波实现的效果如下图：<br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Gif/Sines.gif" alt="show" />
正弦波叠加适合模拟较为平静的水面，当我们想获得更陡峭的波峰与更宽广的波谷时，两个明显的思路分别是修改高度场函数与水平移动顶点，让其波峰陡峭处顶点更加密集

## Gerstner波
Gerstner波模型并不是只基于高度场的模型，在该模型中，时刻t下，顶点的位置函数为：<br>
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Formula/GerstnerP.gif" alt="show" />
其中Q是表示波峰陡度的参数，取0时等同于正弦波，最大取频率与振幅乘积的倒数，此时波峰最陡，对于法线，


###

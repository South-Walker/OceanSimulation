# OceanSimulation
## 简介
	开坑海洋模拟，基于Unity利用多种模型（正弦波、Gerstner波、海洋统计学模型）在GPU与CPU上实现对海洋表面的模拟
## 正弦波叠加  
>* 正弦波是非常简单的基于高度场的模型，只会影响水面竖直方向，单个正弦波可以描述成如下形式：<br>
<img src="http://chart.googleapis.com/chart?cht=tx&chl=W_i(x,y,t)=A_isin[\vec{D_i}\cdot(x,y)\omega_i%2Bt\phi_i]">
在时刻t下，顶点具体的高度等于所有正弦波的累加：<br>
<img src="http://chart.googleapis.com/chart?cht=tx&chl=H(x,y)=\sum_{}W_i(x,y,t)">
为了正确渲染图元，除了顶点位置外还需要计算出顶点的法线信息，显然在每一时刻，曲面H是由x与y确定的曲面，故其法线表达式如下：<br>
<img src="http://chart.googleapis.com/chart?cht=tx&chl=N(x,y)=\left(\frac{\partial{P}}{\partial{x}}\right)\times\left(\frac{\partial{P}}{\partial{y}}\right)">
其中P为对应顶点的位置，表示为：<br>
<img src="http://chart.googleapis.com/chart?cht=tx&chl=P(x,y)=(x,y,H(x,y)))">
至此便得到了所需的所有关系，采用4个正弦波实现的效果如下图：
<img src="https://github.com/South-Walker/OceanSimulation/blob/master/Gif/Sines.gif" alt="show" />
正弦波叠加适合模拟较为平静的水面，当我们想获得更陡峭的波峰与更宽广的波谷时，两个明显的思路分别是修改高度场函数与水平移动顶点，让其波峰陡峭处顶点更加密集

## Gerstner波

###

# OceanSimulation
## 简介
	开坑海洋模拟，基于Unity利用多种模型（正弦波、Gerstner波、海洋统计学模型）在GPU与CPU上实现对海洋表面的模拟
## 正弦波叠加  
>* 正弦波是非常简单的基于高度场的模型，只会影响水面竖直方向，单个正弦波可以描述成如下形式：：
<img src="http://chart.googleapis.com/chart?cht=tx&chl= W_i(x,y,t)={A_i}sin[\vec{D_i}\cdot(x,y)\omega_i+t\phi_i]" style="border:none;">

<img src="http://chart.googleapis.com/chart?cht=tx&chl= \x=\frac{-b\pm\sqrt{b^2-4ac}}{2a}" style="border:none;">
###

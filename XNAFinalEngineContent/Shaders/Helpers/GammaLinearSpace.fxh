/***********************************************************************************************************************************************
Copyright (c) 2008-2013, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
                         Departamento de Ciencias e Ingeniería de la Computación - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

•	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

•	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

•	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

Author: Schneider, José Ignacio (jis@cs.uns.edu.ar)
************************************************************************************************************************************************/

// The sRGB’s gamma curve is an approximation of an exponential function with an exponent of 2.2. 
// I said it is an approximation because the actual gamma curve is linear in the lower values (they try to reduce noise when something is scanned or filmed)
// and the other part try to compensate this to construct an approximation of a 2.2 gamma curve.
// If you want to be completely accurate you need to implement this function:
//						V/12.92 						if V is lower than 0.03928
//						(0.055 + V / 1.055)^2.4 		if V is greater than 0.3928
// But probably it is a waste of precious GPU cycles.
// Actually, to improve performance a simplification could be made. The gamma value could be assumed to be 2.0.
// That made the gamma and digamma functions a lot simpler because we can use a square root and a square function.
#if defined(Windows)
	#define ACCURATE_GAMMA // The accurate gamma option utilizes a 2.2 gamma value and the other utilizes a 2.0 gamma value.
#endif

// Converts from linear RGB space to gamma.
float3 LinearToGamma(float3 color)
{
	#if defined(ACCURATE_GAMMA)
		// pow(x, y) is traduced as exp(log(x) * y). If x is 0 then log(x) will be –inf. So I have to avoid the pow(o, y) situation somehow.
		color = max(color, 0.0001f);
		return pow(color, 1 / 2.2);
	#else
		// Faster but a little inaccurate.
		return sqrt(color);
	#endif
}

// Converts from gamma space to linear RGB.
float3 GammaToLinear(float3 color)
{	
	#if defined(ACCURATE_GAMMA)
		// pow(x, y) is traduced as exp(log(x) * y). If x is 0 then log(x) will be –inf. So I have to avoid the pow(o, y) situation somehow.
		color = max(color, 0.0001f);
		return pow(color, 2.2);
	#else
		// Faster but a little inaccurate.
		return color * color;
	#endif
}

// Converts from gamma space to linear RGB.
float4 GammaToLinear(float4 color)
{
	#if defined(ACCURATE_GAMMA)
		// pow(x, y) is traduced as exp(log(x) * y). If x is 0 then log(x) will be –inf. So I have to avoid the pow(o, y) situation somehow.
		color = max(color, 0.0001f);
		return pow(color, 2.2);
	#else
		// Faster but a little inaccurate.
		return color * color;
	#endif
}
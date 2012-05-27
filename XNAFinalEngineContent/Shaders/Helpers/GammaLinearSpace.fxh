/***********************************************************************************************************************************************
Copyright (c) 2008-2012, Laboratorio de Investigaci�n y Desarrollo en Visualizaci�n y Computaci�n Gr�fica - 
                         Departamento de Ciencias e Ingenier�a de la Computaci�n - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

�	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

�	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

�	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

Author: Schneider, Jos� Ignacio (jis@cs.uns.edu.ar)
************************************************************************************************************************************************/

// Converts from linear RGB space to gamma.
float3 LinearToGamma(float3 color)
{
	// pow(x, y) is traduced as exp(log(x) * y). If x is 0 then log(x) will be �inf. So I have to avoid the pow(o, y) situation somehow.
	color = max(color, 0.0001f);
	return pow(color, 1 / 2.2);
    // Faster but a little inaccurate.
	return sqrt(color);
}

// Converts from gamma space to linear RGB.
float3 GammaToLinear(float3 color)
{	
    // pow(x, y) is traduced as exp(log(x) * y). If x is 0 then log(x) will be �inf. So I have to avoid the pow(o, y) situation somehow.
	color = max(color, 0.0001f);
	return pow(color, 2.2);
	// Faster but a little inaccurate.
    return color * color;
}

// Converts from gamma space to linear RGB.
float4 GammaToLinear(float4 color)
{
	// pow(x, y) is traduced as exp(log(x) * y). If x is 0 then log(x) will be �inf. So I have to avoid the pow(o, y) situation somehow.
	color = max(color, 0.0001f);
	return pow(color, 2.2);
	// Faster but a little inaccurate.
    return color * color;
}
/***********************************************************************************************************************************************
Copyright (c) 2008-2012, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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

//#define NATURAL_ATTENUATION

// IMPORTANT: Do not normalize the L (light) vector. The attenuation function needs the distance.
// dot(L, L) = length(L)^2

// Same as Just Cause 2 and Crysis 2 (you can read GPU Pro 1 book for more information)
float BasicAttenuation(float3 L, float invLightRadius)
{
	float3 distance = L * invLightRadius;
	float attenuation = saturate(1 - dot(distance, distance)); // Equals float attenuation = saturate(1.0f - dot(L, L) / (lightRadius *  lightRadius)); 	
	return attenuation * attenuation;
} // BasicAttenuation

// Inspired on http://fools.slindev.com/viewtopic.php?f=11&t=21&view=unread#unread	
float NaturalAttenuation(float3 L, float invLightRadius)
{	
	float attenuationFactor = 30;
	
	float3 distance = L * invLightRadius;
	float attenuation = dot(distance, distance); // Equals float attenuation = dot(L, L) / (lightRadius *  lightRadius);
	attenuation = 1 / (attenuation * attenuationFactor + 1);
	// Second we move down the function therewith it reaches zero at abscissa 1:
	attenuationFactor = 1 / (attenuationFactor + 1); //attenuationFactor contains now the value we have to subtract
	attenuation = max(attenuation - attenuationFactor, 0); // The max fixes a bug.
	// Finally we expand the equation along the y-axis so that it starts with a function value of 1 again.
	attenuation /= 1 - attenuationFactor;
	return attenuation;
} // NaturalAttenuation

float Attenuation(float3 L, float invLightRadius)
{
	#if defined(NATURAL_ATTENUATION)		
		return NaturalAttenuation(L, invLightRadius);
	#else
		return BasicAttenuation(L, invLightRadius);
	#endif
} // Attenuation
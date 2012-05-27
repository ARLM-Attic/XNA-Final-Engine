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

float randomValue;

// Noise strength (0 = no effect, 1 = full effect)
float filmGrainStrength = 0.3;

// This accentuates the noise in the dark values. Use values greater than 1.
float accentuateDarkNoisePower = 4;

// The noise is both, random and static. With this we can accentuate or reduce the random noise.
// 1 is half random and half static, 0 is only static and more than 1 accentuate the random noise.
float randomNoiseStrength = 1.2;

float3 FilmGrain(float3 color, float2 uv)
{		
	//// The noise is a modification of the noise algorithm of Pat 'Hawthorne' Shearon.
	// Static noise
	float x = uv.x * uv.y * 50000;
	x = fmod(x, 13);
	x = x * x;
	float dx = fmod(x, 0.01);
	// Random noise
	float y = x * randomValue + randomValue;
	float dy = fmod(y, 0.01);
	// Noise
	float noise = saturate(0.1f + dx * 100) + saturate(0.1f + dy * 100) * randomNoiseStrength;
	// I want to maintain more or less the same luminance of the original color and right now the noise range is between 0 and 1.
	// If the range is changed to -1 to 1 some values will add luminance and some other will subtract.
	noise = noise * 2 - 1;
	// This accentuates the noise in the dark values. A dark color will give a number closer to 1 and a bright one will give a value closer to 0.
	float accentuateDarkNoise = pow(1 - (color. r + color.g + color.b) / 3, accentuateDarkNoisePower);
	// Color with noise	
	return color + color * noise * accentuateDarkNoise * filmGrainStrength;	
} // FilmGrain
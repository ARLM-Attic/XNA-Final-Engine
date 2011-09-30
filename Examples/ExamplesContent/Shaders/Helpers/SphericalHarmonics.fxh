/***********************************************************************************************************************************************

From Xen: Graphics API for XNA and Ravi Ramamoorthi's paper.
License: Microsoft_Permissive_License
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)

************************************************************************************************************************************************/

// Storage for environment spherical harmonic L2
float3	sphericalHarmonicBase[9] : GLOBAL;

// Sample the spherical harmonic L2, outputting linear light
float3 SampleSH(float3 normal)
{
	float3 light = 
		sphericalHarmonicBase[0].xyz + 
		sphericalHarmonicBase[1].xyz * normal.x +  
		sphericalHarmonicBase[2].xyz * normal.y + 
		sphericalHarmonicBase[3].xyz * normal.z + 
		sphericalHarmonicBase[4].xyz * (normal.x * normal.y) +
		sphericalHarmonicBase[5].xyz * (normal.y * normal.z) + 
		sphericalHarmonicBase[6].xyz * (normal.x * normal.z) + 
		sphericalHarmonicBase[7].xyz * ((normal.z * normal.z) - (1.0f / 3.0f)) + 
		sphericalHarmonicBase[8].xyz * ((normal.x * normal.x) - (normal.y * normal.y));
		
	// Clamp to zero	
	return max(0, light);
	
	/*
	// Ravi Ramamoorthi method
	float3 light = 0.429043 * sphericalHarmonicBase[8].xyz * ((normal.x * normal.x) - (normal.y * normal.y)) +
	               0.743125 * sphericalHarmonicBase[6].xyz * (normal.z * normal.z) +
				   0.886227 * sphericalHarmonicBase[0].xyz -
				   0.247708 * sphericalHarmonicBase[6].xyz +
				   2 * 0.429043 * (sphericalHarmonicBase[4] * normal.x * normal.y + sphericalHarmonicBase[7] * normal.x * normal.z +  sphericalHarmonicBase[5] * normal.y * normal.z) +
				   2 * 0.511664 * (sphericalHarmonicBase[3] * normal.x + sphericalHarmonicBase[1] * normal.y + sphericalHarmonicBase[2] * normal.z);
	// Clamp to zero	
	return max(0, light);
	*/
} // SampleSH
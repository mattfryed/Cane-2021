Shader "Hidden/PostEffects"
{

	Properties
	{
		//_MainTex("Main Texture", 2D) = "white" {}
	    effectsChoice("effectsChoice", Int) = 0
		//AKF
		_AKFRadius("_AKFRadius", Float) = 1
		_AKFMaskRadius("_AKFMaskRadius", Float) = 1
		_AKFSharpness("_AKFSharpness", Vector) = (1,1,1,1)
		_AKFSampleStep("_AKFSampleStep", Int) = 2
		_AKFOverlapX("_AKFOverlapX", Float) = 1
		_AKFOverlapY("_AKFOverlapY", Float) = 1

		//LIC
		_LICScale("_LICScale", Float) = 1
		_LICMaxLen("_LICMaxLen", Float) = 1
		_LICVariance("_LICVariance", Float) = 1

	}

	CGINCLUDE
#include "UnityCG.cginc"
#include "Common.cginc"
#include "Noise.cginc"
#include "Color.cginc"
#include "Canvas.cginc"
#include "Edge.cginc"
#include "Smooth.cginc"
#include "AKF.cginc"
#include "SBR.cginc"
#include "BF.cginc"
#include "WCR.cginc"
#include "FXDoG.cginc"

	ENDCG

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass //0
		{
			Name "Entry"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragEntry
			ENDCG
		}

		Pass //1
		{
			Stencil
			{
				Ref 1
				Comp Equal
			}

			Name "MaskFace"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragMask
			float4 fragMask(v2f_img i) : SV_Target{ return 1.0; }
			ENDCG
		}
		Pass //2
		{
			Stencil
			{
				Ref 2
				Comp Equal
			}

			Name "MaskBody"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragMask
			float4 fragMask(v2f_img i) : SV_Target{ return 0.5; }
			ENDCG
		}
		
		Pass //3
		{
			Name "SBR"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragSBR
			ENDCG
		}
		Pass//4
		{
			Name "WCR"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragWCR
			ENDCG
		}
		Pass//5
		{
			Name "HandTremor"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragHandTremor
			ENDCG
		}
		Pass//6
		{
			Name "BF"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragBF
			ENDCG
		}
		Pass//7
		{
			Name "FBF"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragFBF
			ENDCG
		}
		Pass//8
		{
			Name "AKF"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragAKF
			ENDCG
		}
		Pass//9
		{
			Name "SNN"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragSNN
			ENDCG
		}
		Pass//10
		{
			Name "Posterize"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragPosterize
			ENDCG
		}
		Pass//11
		{
			Name "Outline"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragOutline
			ENDCG
		}
		Pass//12
		{
			Name "FXDoGGradient"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragFXDoGGradient
			ENDCG
		}
		Pass//13
		{
			Name "FXDoGTangent"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragFXDoGTangent
			ENDCG
		}
		Pass//14
		{
			Name "TFM"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragTFM
			ENDCG
		}
		Pass//15
		{
			Name "LIC"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragLIC
			ENDCG
		}
		Pass//16
		{
			Name "Lerp"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragLerp
			ENDCG
		}
		Pass//17
		{
			Name "Sobel3"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragSobel3
			ENDCG
		}
		Pass//18
		{
			Name "GBlur"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragGBlur
			ENDCG
		}
		Pass//19
		{
			Name "GBlur2"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragGBlur2
			ENDCG
		}
		Pass//20
		{
			Name "Sharpen"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragSharpen
			ENDCG
		}
		Pass//21
		{
			Name "UnsharpMask"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragUnsharpMask
			ENDCG
		}
		Pass//22
		{
			Name "Complementary"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragComplementary
			ENDCG
		}
		Pass//23
		{
			Name "RGB2HSV"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragRGB2HSV
			ENDCG
		}
		Pass//24
		{
			Name "HSV2RGB"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragHSV2RGB
			ENDCG
		}
		Pass//25
		{
			Name "RGB2HSL"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragRGB2HSL
			ENDCG
		}
		Pass//26
		{
			Name "HSL2RGB"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragHSL2RGB
			ENDCG
		}
		Pass//27
		{
			Name "RGB2YUV"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragRGB2YUV
			ENDCG
		}
		Pass//28
		{
			Name "YUV2RGB"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragYUV2RGB
			ENDCG
		}
		Pass//29
		{
			Name "RGB2LAB"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragRGB2LAB
			ENDCG
		}
		Pass//30
		{
			Name "LAB2RGB"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragLAB2RGB
			ENDCG
		}
		Pass//31
		{
			Name "GNoise"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragGNoise
			ENDCG
		}
		Pass//32
		{
			Name "SNoise"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragSNoise
			ENDCG
		}
		Pass//33
		{
			Name "FNoise"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragFNoise
			ENDCG
		}
		Pass//34
		{
			Name "VNoise"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragVNoise
			ENDCG
		}

		Pass//35
		{
			Name "Test"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragTest
			ENDCG
		}
		Pass//36
		{
			Name "TestBF"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragTestBF
			ENDCG
		}
	}
}

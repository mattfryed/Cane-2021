
//Comment/Uncomment this depending on your indirect instancing setting
//#define INDIRECT_INSTANCING

#define MIN_BLADE_SEGMENTS 2
#define MAX_BLADE_SEGMENTS 5
#define BLADE_SEG_DIFF (MAX_BLADE_SEGMENTS - MIN_BLADE_SEGMENTS)

//SRP STUFF
#ifndef SRP
	#ifndef SHADOW_CASTER
		#define GRASS_SHADOW_COORDS(num) SHADOW_COORDS(num)
	#else
		#define GRASS_SHADOW_COORDS(num)
	#endif
#else

	#define GRASS_SHADOW_COORDS(num) float3 shadowCoord : TEXCOORD5;

	float3 _WorldSpaceLightPos0;
#endif


#if defined(SRP) && defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
	//idunno, i feel like its just not really feasible to support lightmap stuff rn bc too much work
	//theres no easy way to get it integrated into the bake system etc so getting it to receive lightmaps is tricky
	//#define REQUIRES_LIGHTMAP_UV
#endif


#if !defined(DEFERRED)
uniform half4 _LightColor0;
#endif

uniform float numTexturesPctUV;

#ifdef SRP
CBUFFER_START(UnityPerMaterial)
#endif
uniform float4 _noiseScale;
uniform float4 _noiseSpeed;
uniform float3 windDir;
uniform float3 windDir2;
uniform float4 windTint;

uniform sampler2D _MainTex;
uniform float4 _MainTex_ST;

uniform float numTextures;
uniform int textureAtlasScalingCutoff;

uniform sampler2D dhfParamMap;
uniform sampler2D colorMap;
uniform sampler2D typeMap;

uniform sampler3D _NoiseTex;

uniform half4 _Color;
uniform bool alphaLock;
uniform half alphaClip;
uniform half _AO;
uniform half bladeWidth;
uniform half bladeSharp;
uniform half bladeHeight;
uniform half ambientCO;
uniform float widthLODscale;
uniform half4 variance;
uniform half3 _LOD;
uniform half grassFade;
uniform half grassFadeSharpness;
uniform half seekSun;
uniform half topViewPush;
uniform half flatnessMult;

#if defined(MULTI_SEGMENT)
uniform float bladeLateralCurve;
uniform float bladeVerticalCurve;
uniform float bladeStiffness;
#endif

#if !defined(DEFERRED)

uniform half ambientCOShadow;
uniform half edgeLight;
uniform half edgeLightSharp;

#else
uniform float _Metallic;
uniform float _Gloss;
uniform float specularMult;

uniform sampler2D _SpecMap;

uniform sampler2D _OccMap;
uniform float occMult;

struct FragmentOutput {
	float4 albedo : SV_Target0;
	float4 specular : SV_Target1;
	float4 normal : SV_Target2;
	float4 light : SV_Target3;
};
#endif

#if defined(GF_USE_DITHER) || defined(SHADOW_CASTER) || defined(SEMI_TRANSPARENT)
uniform sampler3D _DitherMaskLOD;
#define DITHERMASK_REQUIRED
#endif

float _instancePct;
float _instanceLod;

#ifdef SRP
CBUFFER_END
#endif

#ifdef INDIRECT_INSTANCING
uniform float4x4 objToWorldMatrix;
uniform float4x4 worldToObjMatrix;
#endif

#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2) && !defined(SHADOWS_DEPTH)
	#define FOG_ON
#endif



struct v2g {
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;

	#if defined(REQUIRES_LIGHTMAP_UV)
	float2 lightmapUV : TEXCOORD1;
	#endif

	#if defined(RENDERMODE_MESH)
	float4 norm : NORMAL;
	#endif
};





//UNITY_INSTANCING_CBUFFER_START(Props)
//UNITY_DEFINE_INSTANCED_PROP(int, _totalInstances)
//UNITY_DEFINE_INSTANCED_PROP(float, _instanceLod)
//UNITY_INSTANCING_CBUFFER_END


//TERRAIN VARS
#if !defined(RENDERMODE_MESH)

uniform sampler2D terrainHeightMap;
uniform sampler2D terrainNormalMap;
uniform float3 terrainSize;
uniform float3 invTerrainSize;
uniform float2 terrainChunkSize;
uniform float terrainExpansion;
uniform float terrainMapOffset;

//UNITY_INSTANCING_CBUFFER_START(Props)
//UNITY_DEFINE_INSTANCED_PROP(float2, _chunkPos)
//UNITY_DEFINE_INSTANCED_PROP(int, _totalInstances)
//UNITY_DEFINE_INSTANCED_PROP(float, _instanceLod)
//UNITY_INSTANCING_CBUFFER_END
uniform float2 _chunkPos;
#endif


#if (defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHT_SHADOWS)) && !defined(_ADDITIONAL_LIGHTS_VERTEX) && defined(SRP_PER_PIXEL_SECONDARY_LIGHTS)
#define SRP_FRAGMENT_ADDITIONAL_LIGHTS
#endif



//#if defined(FORWARD_ADD) || defined(SRP_FRAGMENT_ADDITIONAL_LIGHTS)
#define FRAGMENT_REQUIRES_WORLDPOS
//#endif

#if defined(DEFERRED) || defined(SRP_FRAGMENT_ADDITIONAL_LIGHTS)
#define FRAGMENT_REQUIRES_NORMAL
#endif



#if !defined(SHADOW_CASTER)
struct g2f {
	float4 pos : SV_POSITION;
	float4 col : COLOR;

	float4 uv : TEXCOORD0;

	#if defined(REQUIRES_LIGHTMAP_UV)
	float2 lightmapUV : TEXCOORD1;
	#endif

	#if defined(FRAGMENT_REQUIRES_WORLDPOS)
	float3 worldPos : TEXCOORD2;
	#endif

	#if defined(FRAGMENT_REQUIRES_NORMAL)
	float4 normal : NORMAL;
	#endif

	#if defined(SRP_FRAGMENT_ADDITIONAL_LIGHTS)
	float3 ogCol : TEXCOORD6;
	#endif

	#if !defined(DEFERRED)
	GRASS_SHADOW_COORDS(5)
	#endif
};
#else
struct g2f {
	float4 pos : SV_POSITION;

	//#if defined(SEMI_TRANSPARENT)
	float4 uv : TEXCOORD0;
	//#endif

};
#endif

struct VertexData {
	float3 vertex;
	float3 normal;
	float3 color;
	float4 dhfParams; //xyz = density, height, flatten, wind str
	float typeParams; //controls grass texture atlas index
};

struct RippleData {
	float4 pos; // w = strength
	float4 drssParams;//xyzw = decay, radius, sharpness, speed
};

struct Counter {
	uint4 val;
};

uniform StructuredBuffer<RippleData> rippleBuffer;
uniform StructuredBuffer<Counter> rippleCount;

uniform StructuredBuffer<RippleData> forcesBuffer;
uniform int forcesCount;

struct DummyVert {
	float4 vertex;
};
Shader "Custom/Grass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color",Color) = (1,1,1,1)
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _WindDirection("Wind direction",Vector) = (1,0,0,10)
        _NoiseMap("NoiseMap", 2D) = "white" {}
        [PowerSlider(3.0)]_WindStrength("WindStrength",Range(0,20)) = 10
    }
    SubShader
    {
        Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"}
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"

        Pass
        {
            Tags{"LightMode"="ForwardBase"}
            ZWrite On
            ZTest On
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdbase
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "Tessellation.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 worldNormal    : TEXCOORD1;
                float4 worldPosition   : TEXCOORD2;
                SHADOW_COORDS(3) 
            };

            struct GrassInfo{
                float4x4 localToWorld;
                float4 texParams;
            };
            StructuredBuffer<GrassInfo> _GrassBuffer;


            float2 _QuadSize;
            float4x4 _TerrianLocalToWorld;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Cutoff;
            float3 _Color;

            float4 _WindDirection;
            float _WindStrength;
            sampler2D _NoiseMap;
            float4 _PlayerPositions[100];
            int _Length;
            float _PushStrength;
            float3 applyWind(float3 positionWS,float3 grassUpWS,float3 windDir,float windStrength,float vertexLocalHeight){
                
                windDir = windDir - dot(windDir,grassUpWS);
                float x,y; 
                sincos(radians(-windStrength),x,y);
                vertexLocalHeight+=0.5* _QuadSize.y;
                return positionWS + (x * windDir + y * grassUpWS - grassUpWS) * vertexLocalHeight;
            }

            v2f vert (appdata input, uint instanceID : SV_InstanceID)
            {
                v2f o;
                float2 uv = input.uv;
                float4 positionOS = input.vertex;
                positionOS.xy = positionOS.xy * _QuadSize;
                GrassInfo grassInfo = _GrassBuffer[instanceID];
                uv = uv * grassInfo.texParams.xy + grassInfo.texParams.zw;
                float4 positionWS = mul(grassInfo.localToWorld,positionOS);
                positionWS /= positionWS.w;
                float3 grassUpDir = float3(0,1,0);
                float3 windDir = normalize(_WindDirection.xyz);
                float windStrength = _WindDirection.w ;
                grassUpDir = mul(grassInfo.localToWorld,float4(grassUpDir,0)).xyz;
                float noiseValue = tex2Dlod(_NoiseMap,float4((positionWS.xz - _Time.y) / 30,0,0)).r;
                noiseValue = sin(noiseValue * windStrength);
                windStrength += noiseValue * _WindStrength;
                float isPushRange = 0;
                for(int i = 0 ; i < _Length; i++)
                {
                if(isPushRange > 0) break;;
                float dis = distance(positionWS.xyz,_PlayerPositions[i].xyz);
                isPushRange = smoothstep(dis,dis+0.8,_PlayerPositions[i].w);            
                windDir.xz = normalize(_PlayerPositions[i].xyz-positionWS.xyz).xz*isPushRange + windDir.xz * (1-isPushRange);        
                }
                windStrength += _PushStrength*isPushRange;
                positionWS.xyz = applyWind(positionWS.xyz,grassUpDir,windDir,windStrength,positionOS.y);
                o.uv = uv;
                o.worldPosition = positionWS;
                o.worldNormal = mul(grassInfo.localToWorld,float4(input.normal,0)).xyz;
                o.pos = mul(UNITY_MATRIX_VP,positionWS);
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);
                clip (color.a-_Cutoff);
                color.rgb *= _Color;
                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}


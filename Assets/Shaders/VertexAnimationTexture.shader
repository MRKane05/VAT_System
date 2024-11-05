Shader "VitaHOT/VertexAnimationTexture"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Base (RGB) RefStrength (A)", 2D) = "white" {}

        _AnimationTex("Animation Texture", 2D) = "white" {}
        _NormalTex("Normals Texture", 2D) = "white" {}
        _BoundsMin("Bounds Min", float) = -1.607506
        _BoundsMax("Bounds Max", float) = 1.306095
        _AnimTime("Animation Time", float) = 0.0
        _TexHeight("Texture Height", float) = 2048
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        Pass
        {
            Tags {"LightMode" = "ForwardBase"}

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc" // for UnityObjectToWorldNormal
            #include "UnityLightingCommon.cginc" // for _LightColor0

            // Include necessary Unity headers
            #include "UnityCG.cginc"

            // Properties from Unity
            sampler2D _AnimationTex;
            sampler2D _NormalTex;
            sampler2D _MainTex;
            fixed _BoundsMin;
            fixed _BoundsMax;
            float _AnimTime;

            fixed _TexHeight;
            fixed _TexWidth;

            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                UNITY_FOG_COORDS(1)
                
                fixed4 diff : COLOR0; // diffuse lighting color
                float2 uv : TEXCOORD0;
            };


            v2f vert(appdata v)
            {
                v2f o;
                    
                // Convert UV coordinates to vertex index
                //float vertexIndex = v.vertexID + 0.5;
                //vertexIndex /= _TexHeight;  //This will need better handling I'd say

                // Sample the animation texture at the current time and vertex index
                float2 textureUV = float2(_AnimTime, v.uv2.y);
                float4 animTexSample = tex2Dlod(_AnimationTex, float4(textureUV, 0, 0));

                // Decode the color into position offsets using inverse lerp
                float3 displacement = float3(
                    lerp(_BoundsMin, _BoundsMax, animTexSample.r),
                    lerp(_BoundsMin, _BoundsMax, animTexSample.g),
                    lerp(_BoundsMin, _BoundsMax, animTexSample.b)
                );

                // Apply displacement to the vertex position
                v.vertex.xyz += displacement;

                o.pos = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o, o.pos);
                o.uv = v.uv;

                //Handle our normals lookupg
                float4 normalTexSample = tex2Dlod(_NormalTex, float4(textureUV, 0, 0));

                float3 vertNormal = float3(
                    lerp(-1.0, 1.0, normalTexSample.r),
                    lerp(-1.0, 1.0, normalTexSample.g),
                    lerp(-1.0, 1.0, normalTexSample.b)
                    );

                //v.normal = vertNormal;
                //Vertex normal and lighting handling
                half3 worldNormal = UnityObjectToWorldNormal(vertNormal);
                // dot product between normal and light direction for
                // standard diffuse (Lambert) lighting
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                // factor in the light color
                o.diff = nl * _LightColor0;
                // Include the ambient color
                o.diff.rgb += ShadeSH9(half4(worldNormal, 1));

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= i.diff;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }

            ENDCG
        }
    }

    FallBack "Diffuse"
}

Shader "LaboVR/UnlitShaded"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _FogColor ("Fog color", Color) = (0.05, 0.06, 0.08, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _FogColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                half shade : TEXCOORD1;
                half fog : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                float3 n = UnityObjectToWorldNormal(v.normal);
                float l = saturate(dot(n, normalize(float3(0.35, 0.85, 0.40))));
                o.shade = 0.45 + 0.55 * l;
                float3 viewPos = UnityObjectToViewPos(v.vertex);
                o.fog = saturate((length(viewPos) - 10.0) / 60.0);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * _Color;
                c.rgb *= i.shade;
                c.rgb = lerp(c.rgb, _FogColor.rgb, i.fog);
                c.a = 1.0;
                return c;
            }
            ENDCG
        }
    }
    Fallback Off
}

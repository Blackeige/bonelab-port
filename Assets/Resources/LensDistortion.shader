Shader "LaboVR/LensDistortion"
{
    Properties
    {
        _MainTex ("Eye texture", 2D) = "black" {}
        _Center ("Lens center (uv)", Vector) = (0.5, 0.5, 0, 0)
        _K ("Distortion k1 k2", Vector) = (0.22, 0.12, 0, 0)
        _Aspect ("Eye aspect (w/h)", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Opaque" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Center;
            float4 _K;
            float _Aspect;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                // Les sommets sont deja en coordonnees ecran normalisees (-1..1).
                o.pos = float4(v.vertex.x, v.vertex.y, 0.0, 1.0);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 d = i.uv - _Center.xy;
                d.x *= _Aspect;
                float r2 = dot(d, d);
                float f = 1.0 + _K.x * r2 + _K.y * r2 * r2;
                d *= f;
                d.x /= _Aspect;
                float2 uv = _Center.xy + d;
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                {
                    return fixed4(0.0, 0.0, 0.0, 1.0);
                }
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
    Fallback Off
}

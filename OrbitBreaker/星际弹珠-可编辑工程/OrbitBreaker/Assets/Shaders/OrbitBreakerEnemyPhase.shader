Shader "Pinball/OrbitBreakerEnemyPhase"
{
    Properties { _Color("Tint", Color) = (1,.12,.32,1) _Opacity("Opacity", Range(0,1)) = .46 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color; float _Opacity;
            struct v2f { float4 pos:SV_POSITION; float3 local:TEXCOORD0; float3 normal:TEXCOORD1; float3 eye:TEXCOORD2; };
            v2f vert(appdata_base v)
            {
                v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.local=v.vertex.xyz;
                o.normal=UnityObjectToWorldNormal(v.normal); o.eye=WorldSpaceViewDir(v.vertex); return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float3 a=abs(i.local);
                float edge=step(.475, min(max(a.x,a.y),min(max(a.y,a.z),max(a.x,a.z))));
                float rim=pow(1-saturate(abs(dot(normalize(i.normal),normalize(i.eye)))),2);
                float scan=.94+.06*sin(i.local.y*75-_Time.y*3);
                return fixed4(_Color.rgb*(.8+rim*.5+edge*.7)*scan, lerp(_Opacity,.85,edge));
            }
            ENDCG
        }
    }
}

Shader "Pinball/OrbitBreakerCosmicField"
{
 Properties { _Color("Tint", Color)=(0.12,0.22,0.5,1) }
 SubShader { Tags { "RenderType"="Opaque" } Pass { Cull Off
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
 v2f vert(appdata_base v) { v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.texcoord; return o; }
 float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
 float noise(float2 p) { float2 i=floor(p), f=frac(p); f=f*f*(3-2*f); return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y); }
 fixed4 frag(v2f i):SV_Target {
 float2 uv=i.uv; float2 p=uv*float2(9,12);
 float n=noise(p)+0.5*noise(p*2.1)+0.25*noise(p*4.3);
 float cloud=pow(saturate(n-0.42),3);
 float3 col=float3(0.005,0.009,0.035)+cloud*lerp(float3(0.06,0.2,0.5),float3(0.36,0.05,0.4),uv.x);
 float2 grid=uv*float2(170,210); float2 cell=floor(grid); float rnd=hash(cell);
 float star=1-smoothstep(0.015,0.10,length(frac(grid)-0.5));
 col += star*step(0.955,rnd)*lerp(float3(.3,.55,1),float3(1,.8,.55),rnd)*1.5;
 return float4(col,1); }
 ENDCG } }
}

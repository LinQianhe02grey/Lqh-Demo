Shader "Pinball/OrbitBreakerPlanet"
{
 Properties { _Color("Feedback", Color)=(0.1,0.4,1,1) }
 SubShader { Tags { "RenderType"="Opaque" } LOD 200
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 struct Input { float2 uv_MainTex; float3 worldPos; };
 float4 _Color;
 void surf(Input IN, inout SurfaceOutputStandard o) {
 float3 p=IN.worldPos*8; float2 uv=IN.uv_MainTex;
 // Object-space sphere coordinates produce continents without external textures.
 float3 q=mul(unity_WorldToObject,float4(IN.worldPos,1)).xyz;
 float a=_Time.y*.12; q.xz=mul(float2x2(cos(a),-sin(a),sin(a),cos(a)),q.xz);
 float n=sin(q.x*17+sin(q.z*13))*sin(q.y*19+sin(q.x*11))+0.4*sin(q.z*41+q.y*25);
 float land=smoothstep(0.05,0.3,n);
 float3 base=lerp(float3(.015,.15,.65),float3(.14,.64,.32),land);
 float polar=smoothstep(.36,.48,abs(q.y)); base=lerp(base,float3(.8,.95,1),polar);
 float cloud=smoothstep(.86,1.05,sin(q.x*30+q.y*10+a)*sin(q.z*24-q.y*18));
 base=lerp(base,float3(.8,.9,1),cloud*.7);
 o.Albedo=base; o.Emission=base*.18+_Color.rgb*.055; o.Metallic=.2; o.Smoothness=.65;
 }
 ENDCG }
 Fallback "Standard"
}

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// #warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'

Shader "Oriental Sky/Glass" {
	Properties
	{		
        _TintColor ("Reflection Color", Color) = (0.5,0.5,0.5,0.25)
        _SpecularColor ("Specular Color", Color) = (1.0,1.0,1.0,1.0)
		_Shininess ("Shininess", Range (0.01, 100)) = 0.078125
		_Gloss("Gloss", Float) = 1.0
	}
	
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Pass
		{
			Tags{ "LightMode" = "ForwardBase"}
			Lighting On
			ZWrite Off
			ZTest LEqual
			Fog { Mode Off }
			AlphaTest Off
			Blend SrcAlpha OneMinusSrcAlpha
			
			
CGPROGRAM

/*
		#pragma surface surf GlassSpecular

		
		half4 LightingGlassSpecular (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
		
	
			half3 h = normalize (lightDir + viewDir);
			float nh = max (0, dot (s.Normal, h));
			float spec = pow (nh, s.Specular*128.0) * s.Gloss;
	
			fixed4 c;
			c.rgb = (_LightColor0.rgb * _SpecColor.rgb * spec) * (atten * 2);
			c.a = s.Alpha + _LightColor0.a * _SpecColor.a * spec * atten;


			
			half4 c = half4(s. + specular, i.color.w );
			c.w *= 1 + max(0, length(c.xyz) - 1.0);


		}


		struct Input {
			float3 worldRefl;
		};

		samplerCUBE _ReflectCube;
		void surf (Input IN, inout SurfaceOutput o) { 
			o.
			o.Emission = texCUBE (_ReflectCube, IN.worldRefl).rgb * _TintColor.xyz;
		}
*/


#pragma vertex vert
#pragma fragment frag
			
#include "UnityCG.cginc"


half4 _TintColor;
samplerCUBE _ReflectCube;
half4 _SpecularColor;
half _Shininess;
half _Gloss;

struct data {
    float4 vertex : POSITION;
    float3 normal : NORMAL0;
};

struct v2f {
    float4 hpos : POSITION;
    float3 worldRefl : TEXCOORD0;
    half4 color : TEXCOORD1;
    half3 specular : TEXCOORD2;
};

v2f vert(data i){
    v2f o;
    o.hpos = UnityObjectToClipPos(i.vertex);
	
	float3 viewDir = normalize(WorldSpaceViewDir(i.vertex));
	float3 worldN = mul((float3x3)unity_ObjectToWorld, i.normal * 1.0);
	o.worldRefl = reflect( -viewDir, worldN );

	o.color = half4( _TintColor.xyz * 2.0, _TintColor.w * 2.0);

	float3 lightDir = normalize(WorldSpaceLightDir(i.vertex));
	half3 h = normalize (lightDir + viewDir);
	half nh = max (0, dot (worldN, h));
	half spec = pow (nh, _Shininess*128.0) * _Gloss;
	o.specular = _SpecularColor.xyz * spec;

	// TEST
	//o.specular = half4(spec,spec,spec,1);

	return o;
}

half4 frag( v2f i ) : COLOR
{
	// TEST
	//return half4(i.specular,1.0);

	half4 cReflect = texCUBE (_ReflectCube,normalize(i.worldRefl));
	half4 c = half4( cReflect.xyz * i.color.xyz + i.specular, i.color.w );
	// return cReflect.w > 0.8 ? float4(1,0,0,1) : c;
	c.w *= 1 + max(0, length(c.xyz) - 1.0);
	return c;
}


ENDCG
		}
	}
}

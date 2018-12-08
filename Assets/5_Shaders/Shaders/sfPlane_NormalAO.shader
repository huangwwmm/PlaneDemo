// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Super Fairy/Plane_NormalAO"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}
		
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
		[Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
		_MetallicGlossMap("Metallic", 2D) = "white" {}

		_BumpScale("Scale", Float) = 1.0
		_BumpMap("Normal AO Map", 2D) = "bump" {}

		////UNDONE:暂时不需要ParallaxMap
		//_Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
		//_ParallaxMap ("Height Map", 2D) = "black" {}
		//
		////UNDONE:已经不起作用，AO已经存在NormalMap中
		//_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
		//_OcclusionMap("Occlusion", 2D) = "white" {}

		//暂时保留，夏春光要用
		_EmissionColor("Color", Color) = (0,0,0)
		_EmissionMap("Emission", 2D) = "white" {}
		
		////UNDONE:暂时不需要
		//_DetailMask("Detail Mask", 2D) = "white" {}
		//
		////UNDONE:暂时不需要
		//_DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
		//_DetailNormalMapScale("Scale", Float) = 1.0
		//_DetailNormalMap("Normal Map", 2D) = "bump" {}
		//
		////UNDONE:暂时不需要
		//[Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0
		//
		////UNDONE:计划去掉
		//_PointLightIntensity("PointLightIntensity", Float) = 1.0


		// Blending state
		[HideInInspector] _Mode ("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _ZWrite ("__zw", Float) = 1.0
	}

	CGINCLUDE
		#define UNITY_SETUP_BRDF_INPUT MetallicSetup
	ENDCG

	SubShader
	{
		Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
	

		// ------------------------------------------------------------------
		//  Base forward pass (directional light, emission, lightmaps, ...)
		Pass
		{
			Name "FORWARD" 
			Tags { "LightMode" = "ForwardBase" }

			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]

			CGPROGRAM
			#pragma target 3.0
			// TEMPORARY: GLES2.0 temporarily disabled to prevent errors spam on devices without textureCubeLodEXT
			#pragma exclude_renderers gles
			
			// -------------------------------------
					
			//#pragma shader_feature _NORMALMAP
			//#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			//#pragma shader_feature _EMISSION
			//#pragma shader_feature _METALLICGLOSSMAP 
			//#pragma shader_feature ___ _DETAIL_MULX2
			//#pragma shader_feature _PARALLAXMAP

			#define _EMISSION 1
			#define _METALLICGLOSSMAP 1
			#define _NORMALMAP 1
			//#define UNITY_REQUIRE_FRAG_WORLDPOS 1
			
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog

			#pragma vertex sfVertForwardBase
			#pragma fragment sfFragmentBase

			#include "UnityStandardCoreForward.cginc"

			uniform float _PointLightIntensity;
			
						
			#ifdef _NORMALMAP
			half3 sfNormalInTangentSpace(float4 texcoords)
			{
				//half3 normalTangent = UnpackScaleNormal(tex2D (_BumpMap, texcoords.xy), _BumpScale);	
						
				float3 normalTextureResult = tex2D (_BumpMap, texcoords.xy).rgb;
				float2 unpack = (normalTextureResult.xy * 2 - 1)*_BumpScale;
				float normalX = unpack.x;
				float nomralY = -unpack.y;
				float normalZ =  sqrt(1 - saturate(dot(unpack.xy, unpack.xy)));
				half3 normalTangent = normalize(float3(normalX, nomralY, normalZ));
				normalTangent = normalize(float3(normalX, nomralY, normalZ));
				//ao = normalTextureResult.z;

			//	// SM20: instruction count limitation
			//	// SM20: no detail normalmaps
			//#if _DETAIL && !defined(SHADER_API_MOBILE) && (SHADER_TARGET >= 30) 
			//	half mask = DetailMask(texcoords.xy);
			//	half3 detailNormalTangent = UnpackScaleNormal(tex2D (_DetailNormalMap, texcoords.zw), _DetailNormalMapScale);
			//	#if _DETAIL_LERP
			//		normalTangent = lerp(
			//			normalTangent,
			//			detailNormalTangent,
			//			mask);
			//	#else				
			//		normalTangent = lerp(
			//			normalTangent,
			//			BlendNormals(normalTangent, detailNormalTangent),
			//			mask);
			//	#endif
			//#endif
				return normalTangent;
			}
			#endif
			
		#if UNITY_VERSION >= 566
			half3 sfPerPixelWorldNormal(float4 i_tex, float4 tangentToWorld[3])
		#else
			half3 sfPerPixelWorldNormal(float4 i_tex, half4 tangentToWorld[3])
		#endif
			{
			#ifdef _NORMALMAP
				half3 tangent = tangentToWorld[0].xyz;
				half3 binormal = tangentToWorld[1].xyz;
				half3 normal = tangentToWorld[2].xyz;
			
				#if UNITY_TANGENT_ORTHONORMALIZE
					normal = NormalizePerPixelNormal(normal);
			
					// ortho-normalize Tangent
					tangent = normalize (tangent - normal * dot(tangent, normal));
			
					// recalculate Binormal
					half3 newB = cross(normal, tangent);
					binormal = newB * sign (dot (newB, binormal));
				#endif
			
				half3 normalTangent = sfNormalInTangentSpace(i_tex);
				half3 normalWorld = NormalizePerPixelNormal(tangent * normalTangent.x + binormal * normalTangent.y + normal * normalTangent.z); // @TODO: see if we can squeeze this normalize on SM2.0 as well
			#else
				half3 normalWorld = normalize(tangentToWorld[2].xyz);
			#endif
				return normalWorld;
			}
			
			inline FragmentCommonData sfFragmentSetup (	float4 i_tex, 
														half3 i_eyeVec, 
														half3 i_viewDirForParallax, 
													#if UNITY_VERSION >= 566
														float4 tangentToWorld[3], 
													#else
														half4 tangentToWorld[3],
													#endif
														half3 i_posWorld)
			{
				//// UNDONE: 申远：最终手机版本不要支持Parallax Map，这个太消耗性能了。。。别忘了去掉
				//#if defined(_PARALLAXMAP)
				//	i_tex = Parallax(i_tex, i_viewDirForParallax);
				//#endif
			
				half alpha = Alpha(i_tex.xy);
				#if defined(_ALPHATEST_ON)
					clip (alpha - _Cutoff);
				#endif
			
				FragmentCommonData o = UNITY_SETUP_BRDF_INPUT (i_tex);
				o.normalWorld = sfPerPixelWorldNormal(i_tex, tangentToWorld);
				o.eyeVec = NormalizePerPixelNormal(i_eyeVec);
				o.posWorld = i_posWorld;

				// NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
				o.diffColor = PreMultiplyAlpha (o.diffColor, alpha, o.oneMinusReflectivity, /*out*/ o.alpha);
				return o;
			}
			
			VertexOutputForwardBase sfVertForwardBase (VertexInput v)
			{
				VertexOutputForwardBase o;
				UNITY_INITIALIZE_OUTPUT(VertexOutputForwardBase, o);
			
				float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
				#if UNITY_SPECCUBE_BOX_PROJECTION
					o.posWorld = posWorld.xyz;
				#endif

				o.pos = UnityObjectToClipPos(v.vertex);
				o.tex = TexCoords(v);
				o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
				float3 normalWorld = UnityObjectToWorldNormal(v.normal);

				#ifdef _TANGENT_TO_WORLD
					float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
			
					float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
					o.tangentToWorldAndPackedData[0].xyz = tangentToWorld[0];
					o.tangentToWorldAndPackedData[1].xyz = tangentToWorld[1];
					o.tangentToWorldAndPackedData[2].xyz = tangentToWorld[2];
				#else
					o.tangentToWorldAndPackedData[0].xyz = 0;
					o.tangentToWorldAndPackedData[1].xyz = 0;
					o.tangentToWorldAndPackedData[2].xyz = normalWorld;
				#endif

				//We need this for shadow receving
				TRANSFER_SHADOW(o);
			
				//PointLighting暂不考虑法线
				float3 toPointLight = float3(unity_4LightPosX0.x - posWorld.x, unity_4LightPosY0.x - posWorld.y, unity_4LightPosZ0.x - posWorld.z);
				o.ambientOrLightmapUV = half4(saturate(1 / (unity_4LightAtten0.x * dot(toPointLight, toPointLight))) * unity_LightColor[0].rgb, 1);
				//o.ambientOrLightmapUV = VertexGIForward(v, posWorld, normalWorld);
				
				//UNDONE:暂时不需要ParallaxMap
				//#ifdef _PARALLAXMAP
				//	TANGENT_SPACE_ROTATION;
				//	half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
				//	o.tangentToWorldAndPackedData[0].w = viewDirForParallax.x;
				//	o.tangentToWorldAndPackedData[1].w = viewDirForParallax.y;
				//	o.tangentToWorldAndPackedData[2].w = viewDirForParallax.z;
				//#endif
			
				#if UNITY_OPTIMIZE_TEXCUBELOD
					o.reflUVW 		= reflect(o.eyeVec, normalWorld);
				#endif
			
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
			
			// Ref: http://jcgt.org/published/0003/02/03/paper.pdf
			//		builtin_shaders-5.3.6p2\CGIncludes\UnityStandardBRDF.cginc(161):inline half SmithJointGGXVisibilityTerm (half NdotL, half NdotV, half roughness)
			inline float sfSmithJointGGXVisibilityTerm (float NdotL, float NdotV, float roughness)
			{
			#if 0
				// Original formulation:
				//	lambda_v	= (-1 + sqrt(a2 * (1 - NdotL2) / NdotL2 + 1)) * 0.5f;
				//	lambda_l	= (-1 + sqrt(a2 * (1 - NdotV2) / NdotV2 + 1)) * 0.5f;
				//	G			= 1 / (1 + lambda_v + lambda_l);
			
				// Reorder code to be more optimal
				half a		= roughness * roughness; // from unity roughness to true roughness
				half a2		= a * a;
			
				half lambdaV = NdotL * sqrt((-NdotV * a2 + NdotV) * NdotV + a2);
				half lambdaL = NdotV * sqrt((-NdotL * a2 + NdotL) * NdotL + a2);
			
				// Unity BRDF code expect already simplified data by (NdotL * NdotV)
				// return (2.0f * NdotL * NdotV) / (lambda_v + lambda_l + 1e-5f);
				return 2.0f / (lambdaV + lambdaL + 1e-5f);
			#else
			    // Approximation of the above formulation (simplify the sqrt, not mathematically correct but close enough)
				float a = roughness * roughness;
				float lambdaV = NdotL * (NdotV * (1 - a) + a);
				float lambdaL = NdotV * (NdotL * (1 - a) + a);
				//return 2.0f / (lambdaV + lambdaL + 1e-2f);
				return 2.0f / (lambdaV + lambdaL + 1e-5f);	// This function is not intended to be running on Mobile,
															// therefore epsilon is smaller than can be represented by half
			#endif
			}
			
			//Ref: builtin_shaders-5.3.6p2\CGIncludes\UnityStandardBRDF.cginc(238):inline half GGXTerm (half NdotH, half roughness)
			inline float sfGGXTerm (float NdotH, float roughness)
			{
				float a = roughness * roughness;
				float a2 = a * a;
				float d = NdotH * NdotH * (a2 - 1.f) + 1.f;
				//return a2 / (UNITY_PI * d * d + 1e-2f);
				return a2 / (UNITY_PI * d * d + 1e-7f); // This function is not intended to be running on Mobile,
														// therefore epsilon is smaller than what can be represented by half
			}
			
			// Main Physically Based BRDF
			// Derived from Disney work and based on Torrance-Sparrow micro-facet model
			//
			//   BRDF = kD / pi + kS * (D * V * F) / 4
			//   I = BRDF * NdotL
			//
			// * NDF (depending on UNITY_BRDF_GGX):
			//  a) Normalized BlinnPhong
			//  b) GGX
			// * Smith for Visiblity term
			// * Schlick approximation for Fresnel
			float4 SF_BRDF1_Unity_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
				half3 normal, half3 viewDir,
				UnityLight light, UnityIndirect gi)
			{
				float roughness = 1-oneMinusRoughness;
				float3 halfDir = Unity_SafeNormalize (light.dir + viewDir);

				bool isGammaSpace = true;//IsGammaSpace();
			
				// NdotV should not be negative for visible pixels, but it can happen due to perspective projection and normal mapping
				// In this case normal should be modified to become valid (i.e facing camera) and not cause weird artifacts.
				// but this operation adds few ALU and users may not want it. Alternative is to simply take the abs of NdotV (less correct but works too).
				// Following define allow to control this. Set it to 0 if ALU is critical on your platform.
				// This correction is interesting for GGX with SmithJoint visibility function because artifacts are more visible in this case due to highlight edge of rough surface
				// Edit: Disable this code by default for now as it is not compatible with two sided lighting used in SpeedTree.
				//#define UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV 0 
			
			//#if UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV
			//	// The amount we shift the normal toward the view vector is defined by the dot product.
			//	// This correction is only applied with SmithJoint visibility function because artifacts are more visible in this case due to highlight edge of rough surface
			//	half shiftAmount = dot(normal, viewDir);
			//	normal = shiftAmount < 0.0f ? normal + viewDir * (-shiftAmount + 1e-5f) : normal;
			//	// A re-normalization should be apply here but as the shift is small we don't do it to save ALU.
			//	//normal = normalize(normal);
			//
			//	// As we have modify the normal we need to recalculate the dot product nl. 
			//	// Note that  light.ndotl is a clamped cosine and only the ForwardSimple mode use a specific ndotL with BRDF3
			//	half nl = DotClamped(normal, light.dir);
			//#else
				float nl = light.ndotl;
			//#endif
				float nh =  BlinnTerm (normal, halfDir);
				float nv =  DotClamped(normal, viewDir);

				float lv =  DotClamped (light.dir, viewDir);
				float lh =  DotClamped (light.dir, halfDir);
			
			//#if UNITY_BRDF_GGX
				float V = sfSmithJointGGXVisibilityTerm (nl, nv, roughness);
				float D = sfGGXTerm (nh, roughness);
			//#else
			//	half V = SmithBeckmannVisibilityTerm (nl, nv, roughness);
			//	half D = NDFBlinnPhongNormalizedTerm (nh, RoughnessToSpecPower (roughness));
			//#endif

				//Test
				//half4 resultColor = half4(0, 0, 0, 1);
				//if(nh <= 0.0)
				//	resultColor.xyz += half3(0, 1, 0);
				//if(nv <= 0.0)
				//	resultColor.xyz += half3(1, 0, 0);
				//if(lv <= 0.0)
				//	resultColor.xyz += half3(0, 0, 1);
				//if(lh <= 0.0)
				//	resultColor.xyz += half3(0, 0, 1);
				//
				//return resultColor;
				//
				//if(V > 1000 && D > 1000)
				//	return half4(0, 1, 0, 1);
				//if(D > 1000)
				//	return half4(1, 0, 0, 1);
				//if(V > 1000)
				//	return half4(0, 0, 1, 1);
			
				float nlPow5 = Pow5 (1-nl);
				float nvPow5 = Pow5 (1-nv);
				float Fd90 = 0.5 + 2 * lh * lh * roughness;
				float disneyDiffuse = (1 + (Fd90-1) * nlPow5) * (1 + (Fd90-1) * nvPow5);
				
				// HACK: theoretically we should divide by Pi diffuseTerm and not multiply specularTerm!
				// BUT 1) that will make shader look significantly darker than Legacy ones
				// and 2) on engine side "Non-important" lights have to be divided by Pi to in cases when they are injected into ambient SH
				// NOTE: multiplication by Pi is part of single constant together with 1/4 now
				float specularTerm = (V * D) * (UNITY_PI/4); // Torrance-Sparrow model, Fresnel is applied later (for optimization reasons)
				if (isGammaSpace)
					specularTerm = sqrt(max(1e-4h, specularTerm));
				specularTerm = min(10, max(0, specularTerm * nl));
				//specularTerm = max(0, specularTerm * nl);
			
				float diffuseTerm = disneyDiffuse * nl;
			
				// surfaceReduction = Int D(NdotH) * NdotH * Id(NdotL>0) dH = 1/(realRoughness^2+1)
				float realRoughness = roughness*roughness;		// need to square perceptual roughness
				float surfaceReduction;
				if (isGammaSpace) 
					surfaceReduction = 1.0 - 0.28*realRoughness*roughness;		// 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
				else 
					surfaceReduction = 1.0 / (realRoughness*realRoughness + 1.0);			// fade \in [0.5;1]
			
				float grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));
			    float3 color =	//diffColor * (gi.diffuse + light.color * diffuseTerm)
			                    //+ specularTerm * light.color * 0.1//FresnelTerm (specColor, lh)
								//+ surfaceReduction * gi.specular * FresnelLerp (specColor, grazingTerm, nv);

								specularTerm * light.color * FresnelTerm (specColor, lh)
								+ diffColor * (gi.diffuse + light.color * saturate(diffuseTerm))
								+ surfaceReduction * gi.specular * saturate(FresnelLerp (specColor, grazingTerm, nv));
								//gi.specular;
								//FresnelLerp (specColor, grazingTerm, nv);
								//surfaceReduction;
			
				//return half4(specularTerm.xxx , 1);
				return float4(color, 1);
			}
			
			UnityLight _MainLight (half3 normalWorld)
			{
				UnityLight l;

				l.color = _LightColor0.rgb;
				l.dir = _WorldSpaceLightPos0.xyz;
				l.ndotl = LambertTerm (normalWorld, l.dir);
				return l;
			}

			half4 sfFragmentBase (VertexOutputForwardBase i) : SV_Target
			{
				//return i.ambientOrLightmapUV;
				//i.ambientOrLightmapUV *= _PointLightIntensity;
				FragmentCommonData s = sfFragmentSetup(	i.tex, 
														i.eyeVec, 
														IN_VIEWDIR4PARALLAX(i), 
													#if UNITY_VERSION >= 566
														i.tangentToWorldAndPackedData, 
													#else
														i.tangentToWorldAndPackedData,
													#endif
														IN_WORLDPOS(i));
			
			#if UNITY_OPTIMIZE_TEXCUBELOD
				s.reflUVW		= i.reflUVW;
			#endif
			
			// #if UNITY_VERSION >= 566
			// 	UnityLight mainLight = _MainLight (/*s.normalWorld*/);
			// #else
				UnityLight mainLight = _MainLight (s.normalWorld);
			// #endif
				half atten = SHADOW_ATTENUATION(i);			
			
				//half occlusion = Occlusion(i.tex.xy);
				half occlusion = tex2D (_BumpMap, i.tex.xy).b;

				////Debug For AO
				//return float4(occlusion.xxx, 1);

				UnityGI gi = FragmentGI (s, occlusion, i.ambientOrLightmapUV, atten, mainLight);
			
				#if UNITY_VERSION >= 566
				half4 c = SF_BRDF1_Unity_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect);
				#else
				half4 c = SF_BRDF1_Unity_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect);
				#endif

				// TEST
				// return c;

				//// UNDONE: 申远：是否有必要？我看输出的颜色只有黑色，应该是需要Light Probe才能起效？
				//#if UNITY_VERSION >= 566
				//c.rgb += UNITY_BRDF_GI (s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, occlusion, gi);
				//#else
				//c.rgb += UNITY_BRDF_GI (s.diffColor, s.specColor, s.oneMinusReflectivity, s.oneMinusRoughness, s.normalWorld, -s.eyeVec, occlusion, gi);
				//#endif
				
				#if defined(_EMISSION)	
					c.rgb += Emission(i.tex.xy);
				#endif
			
				UNITY_APPLY_FOG(i.fogCoord, c.rgb);

				//return OutputForward (c, s.alpha);
				half4 finalColor = OutputForward (c, s.alpha);

				return finalColor;
			}

			ENDCG
		}
		
		// ------------------------------------------------------------------
		//  Shadow rendering pass
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			
			ZWrite On ZTest LEqual
			Cull Off
			//Offset 0, -1

			CGPROGRAM
			#pragma target 2.0

			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma skip_variants SHADOWS_SOFT
			#pragma multi_compile_shadowcaster

			#pragma vertex vertShadowCaster
			#pragma fragment fragShadowCaster

			#include "UnityStandardShadow.cginc"

			ENDCG
		}
	}

	FallBack "VertexLit"
	CustomEditor "sfPlaneNormalAOShaderGUI"
}

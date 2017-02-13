Shader "Outlined/NewOcclusionOutline" {
 Properties {
     _Color ("Main Color", Color) = (.5,.5,.5,1)
     _RimCol ("Rim Colour" , Color) = (1,0,0,1)
     _RimPow ("Rim Power", Float) = 1.0
     _MainTex ("Base (RGB)", 2D) = "white" {}
 }
 SubShader {
     Pass {
             Name "Behind"
             Tags { "RenderType"="transparent" "Queue" = "Transparent" }
             Blend SrcAlpha OneMinusSrcAlpha
             ZTest Greater               // here the check is for the pixel being greater or closer to the camera, in which
             Cull Back                   // case the model is behind something, so this pass runs
             ZWrite Off
             LOD 200                    
            
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             #include "UnityCG.cginc"
            
             struct v2f {
                 float4 pos : SV_POSITION;
                 float2 uv : TEXCOORD0;
                 float3 normal : TEXCOORD1;      // Normal needed for rim lighting
                 float3 viewDir : TEXCOORD2;     // as is view direction.
             };
            
             sampler2D _MainTex;
             float4 _RimCol;
             float _RimPow;
            
             float4 _MainTex_ST;
            
             v2f vert (appdata_tan v)
             {
                 v2f o;
                 o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                 o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                 o.normal = normalize(v.normal);
                 o.viewDir = normalize(ObjSpaceViewDir(v.vertex));       //this could also be WorldSpaceViewDir, which would
                 return o;                                               //return the World space view direction.
             }
            
             half4 frag (v2f i) : COLOR
             {
                 half Rim = 1 - saturate(dot(normalize(i.viewDir), i.normal));       //Calculates where the model view falloff is       
                                                                                                                                    //for rim lighting.
                
                 half4 RimOut = _RimCol * pow(Rim, _RimPow);
                 return RimOut;
             }
             ENDCG
         }
 
         Pass {
             Name "BASE"
             ZWrite On
             ZTest LEqual
             Blend SrcAlpha OneMinusSrcAlpha
             Material {
                 Diffuse [_Color]
                 Ambient [_Color]
             }
             Lighting On
             SetTexture [_MainTex] {
                 ConstantColor [_Color]
                 Combine texture * constant
             }
             SetTexture [_MainTex] {
                 Combine previous * primary DOUBLE
             }
         }
                
     }
     //FallBack "VertexLit"
     FallBack "Diffuse"
 }
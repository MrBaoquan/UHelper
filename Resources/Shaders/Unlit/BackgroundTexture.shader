Shader "UHelper/BackgroundTexture"
{
    Properties
    {
        _MainTex ("Texture2", 2D) = "white" {}
        [Toggle(FLIP_VERTICAL)] _FLIP_VERTICAL("Flip Vertical", Float) = 1
        [Toggle(FLIP_HORIZONTAL)] _FLIP_HORIZONTAL("Flip Horizontal", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
 
        Pass
        {
            ZWrite Off
            ZTest Always
 
            CGPROGRAM
            #pragma shader_feature FLIP_VERTICAL
            #pragma shader_feature FLIP_HORIZONTAL
 
            #pragma vertex vert
            #pragma fragment frag
         
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
 
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
 
            v2f vert( appdata v )
            {
                v2f o;
                o.vertex = UnityObjectToClipPos( v.vertex );
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                return o;
            }
 
            fixed4 frag( v2f i ) : SV_Target
            {
                #ifdef FLIP_VERTICAL
                i.uv.y = 1 - i.uv.y;
                #endif

                #ifdef FLIP_HORIZONTAL
                i.uv.x = 1 - i.uv.x;
                #endif

                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
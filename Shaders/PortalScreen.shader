Shader "ImmersiveEntrance/PortalScreen"
{
    Properties
    {
        _MainTex ("Render Texture", 2D) = "white" {}
        _CropLeft   ("Crop Left",   Range(0, 0.5)) = 0
        _CropRight  ("Crop Right",  Range(0, 0.5)) = 0
        _CropTop    ("Crop Top",    Range(0, 0.5)) = 0
        _CropBottom ("Crop Bottom", Range(0, 0.5)) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry+1"
        }
        Pass
        {
            ZWrite On
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos  : TEXCOORD0;
                float2 uv         : TEXCOORD1;
            };

            sampler2D _MainTex;
            float _CropLeft;
            float _CropRight;
            float _CropTop;
            float _CropBottom;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = UnityObjectToClipPos(IN.positionOS);
                OUT.screenPos  = ComputeScreenPos(OUT.positionCS);
                OUT.uv         = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                clip(uv.x - _CropLeft);
                clip(uv.y - _CropBottom);
                clip((1.0 - uv.x) - _CropRight);
                clip((1.0 - uv.y) - _CropTop);

                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                return tex2D(_MainTex, screenUV);
            }
            ENDHLSL
        }
    }
}
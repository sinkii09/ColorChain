Shader "Sprites/Outline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 10)) = 1
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

                // Sample outline
                fixed outline = 0;
                float2 pixelSize = _MainTex_TexelSize.xy * _OutlineWidth;

                outline += tex2D(_MainTex, IN.texcoord + float2(pixelSize.x, 0)).a;
                outline += tex2D(_MainTex, IN.texcoord + float2(-pixelSize.x, 0)).a;
                outline += tex2D(_MainTex, IN.texcoord + float2(0, pixelSize.y)).a;
                outline += tex2D(_MainTex, IN.texcoord + float2(0, -pixelSize.y)).a;
                outline += tex2D(_MainTex, IN.texcoord + float2(pixelSize.x, pixelSize.y)).a;
                outline += tex2D(_MainTex, IN.texcoord + float2(-pixelSize.x, pixelSize.y)).a;
                outline += tex2D(_MainTex, IN.texcoord + float2(pixelSize.x, -pixelSize.y)).a;
                outline += tex2D(_MainTex, IN.texcoord + float2(-pixelSize.x, -pixelSize.y)).a;

                outline = min(outline, 1.0);

                // Mix outline with sprite
                fixed4 outlinePixel = _OutlineColor * outline;
                c = lerp(outlinePixel, c, c.a);
                c.rgb *= c.a;

                return c;
            }
            ENDCG
        }
    }
}

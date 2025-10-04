Shader "Sprites/TileShine"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Shine Settings)]
        _ShineColor ("Shine Color", Color) = (1,1,1,0.5)
        _ShineWidth ("Shine Width", Range(0.01, 0.5)) = 0.15
        _ShineSpeed ("Shine Speed", Range(0, 5)) = 1
        _ShineAngle ("Shine Angle", Range(-180, 180)) = 45
        _ShineInterval ("Shine Interval", Range(0, 10)) = 3
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
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
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _ShineColor;
            float _ShineWidth;
            float _ShineSpeed;
            float _ShineAngle;
            float _ShineInterval;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex).xyz;
                return OUT;
            }

            // Simple pseudo-random function
            float random(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Sample base sprite
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

                // Generate random offset per tile
                float randomOffset = random(IN.worldPos.xy) * 10.0;

                // Calculate shine timing
                float totalCycle = _ShineSpeed + _ShineInterval;
                float cycleTime = fmod(_Time.y + randomOffset, totalCycle);

                // Only shine during active period
                if (cycleTime > _ShineSpeed)
                {
                    c.rgb *= c.a;
                    return c;
                }

                // Simple diagonal: top-left (0,1) to bottom-right (1,0)
                // This creates a value from 0 to 2 going diagonally
                float diagonal = IN.texcoord.x - IN.texcoord.y + 1.0; // Range: 0 to 2

                // Shine position moves from 0 to 2
                float shinePos = (cycleTime / _ShineSpeed) * 2.0;

                // How far from shine center
                float dist = abs(diagonal - shinePos);

                // Create bright line
                float shine = max(0, 1.0 - dist / _ShineWidth);
                shine = shine * shine * shine; // Sharpen

                // Add white glow
                c.rgb += _ShineColor.rgb * shine * _ShineColor.a * c.a;

                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}

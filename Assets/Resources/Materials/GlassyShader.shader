Shader "Unlit/GlassyShader"
{
    Properties
    {
        _Color ("Main Color", Color) = (0.5, 0.5, 0.5, 1)
        _SpecColor ("Specular Color", Color) = (1, 1, 1, 1)
        _SpecIntensity ("Specular Intensity", Range(0, 1)) = 0.5
        _Glossiness ("Glossiness", Range(0, 1)) = 0.5
        _MainTex ("Base Texture", 2D) = "white" { }
        _ReflectTex ("Reflection Texture", Cube) = "_Skybox" { }
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            float _SpecIntensity;
            float _Glossiness;
            float4 _SpecColor;
            float4 _Color;
            samplerCUBE _ReflectTex; // Изменено на Cube texture

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = normalize(v.normal);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Преломление и отражение для создания стеклянного эффекта
                float3 viewDir = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, i.pos).xyz);
                float3 reflectDir = reflect(viewDir, i.normal);
                float3 refractDir = refract(viewDir, i.normal, 1.0 / 1.5); // Преломление с коэффициентом 1.5 для стекла

                // Получение отражений
                half4 reflection = texCUBE(_ReflectTex, reflectDir); // Используем texCUBE для кубической текстуры

                // Вычисление бликованной области
                float specular = pow(max(dot(i.normal, viewDir), 0.0), 10.0) * _SpecIntensity;
                half4 specularColor = _SpecColor * specular;

                // Цвет материала + отражения + блики
                half4 finalColor = lerp(reflection, _Color, 0.5) + specularColor;
                finalColor.rgb = finalColor.rgb * _Color.rgb;

                return finalColor;
            }
            ENDCG
        }
    }
}

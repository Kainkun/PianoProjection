Shader "Unlit/PerspectiveShift"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uvq : TEXCOORD0;
                uint id : SV_VertexID;
            };

            struct v2f
            {
                float3 uvq : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Q[4];


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); //object to camera space

                o.uvq.x = v.uvq.x * _Q[v.id];
                o.uvq.y = v.uvq.y * _Q[v.id];
                o.uvq.z = _Q[v.id];
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uvq.xy/i.uvq.z);
                return col;
            }
            ENDCG
        }
    }
}

// =====================================================
// BrushStrokeFade.shader - 筆ストロークで左から右へ黒くなるフェードシェーダー
// 使い方: SceneTransitionManager が Resources.Load で読み込み、RawImage に適用する。
//         _Progress を -0.125 → 1.125 でアニメートすること（ノイズによるはみ出し分を含む）。
// =====================================================
Shader "Custom/UI/BrushStrokeFade"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}

        // アニメーション用（C#から SetFloat で駆動する）
        _Progress      ("Progress",       Float)        =  0.0
        _EdgeSoftness  ("Edge Softness",  Float)        =  0.025
        _NoiseScale    ("Noise Scale",    Float)        =  0.10
        _NoiseFreq     ("Noise Frequency",Float)        =  7.0

        // Unity UI システムが内部で使うステンシル系プロパティ
        _StencilComp      ("Stencil Comparison", Float) = 8
        _Stencil          ("Stencil ID",          Float) = 0
        _StencilOp        ("Stencil Operation",   Float) = 0
        _StencilWriteMask ("Stencil Write Mask",  Float) = 255
        _StencilReadMask  ("Stencil Read Mask",   Float) = 255
        _ColorMask        ("Color Mask",          Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType"      = "Transparent"
            "PreviewType"     = "Plane"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull     Off
        Lighting Off
        ZWrite   Off
        ZTest    [unity_GUIZTestMode]
        Blend    SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            float _Progress;
            float _EdgeSoftness;
            float _NoiseScale;
            float _NoiseFreq;

            // ─── スムーズな 1D バリューノイズ ───
            float valueNoise(float t)
            {
                float i = floor(t);
                float f = frac(t);
                f = f * f * (3.0 - 2.0 * f); // smoothstep で C1 連続
                float a = frac(sin(i        * 127.1 + 311.7) * 43758.5453);
                float b = frac(sin((i + 1.0)* 127.1 + 311.7) * 43758.5453);
                return lerp(a, b, f);
            }

            // ─── 4 オクターブ重ねて筆先の凹凸を表現 ───
            // 戻り値は [-NoiseScale, +NoiseScale] の範囲
            float brushEdgeOffset(float y)
            {
                float n = 0.0;
                n += valueNoise(y * _NoiseFreq          ) * 0.50; // 大きなうねり
                n += valueNoise(y * _NoiseFreq * 2.37   ) * 0.30; // 中程度の変化
                n += valueNoise(y * _NoiseFreq * 5.83   ) * 0.15; // 毛先の細かさ
                n += valueNoise(y * _NoiseFreq * 12.41  ) * 0.05; // 微細テクスチャ
                return (n * 2.0 - 1.0) * _NoiseScale;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // UV.x にノイズを乗せた「見かけ上の横位置」を求め、
                // それが _Progress を超えているかで黒/透明を決める
                float edge  = i.uv.x + brushEdgeOffset(i.uv.y);
                float alpha = 1.0 - smoothstep(
                    _Progress - _EdgeSoftness,
                    _Progress + _EdgeSoftness,
                    edge
                );
                return fixed4(0.0, 0.0, 0.0, alpha);
            }
            ENDCG
        }
    }
}

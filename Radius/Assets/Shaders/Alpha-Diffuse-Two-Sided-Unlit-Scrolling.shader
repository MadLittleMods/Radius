Shader "Transparent/Diffuse (Two-Sided), Unlit, and Scrolling" {

Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_AlphaMultiplier ("Alpha Multiplier", Float) = 1
	_xScrollSpeed ("X Scroll Speed", Float) = 2.0
	_yScrollSpeed ("Y Scroll Speed", Float) = 0
}

SubShader {
	ZWrite Off
	Cull Off
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 200

	CGPROGRAM
	#pragma surface surf NoLighting alpha vertex:vert

	sampler2D _MainTex;
	fixed4 _Color;

	struct Input {
		float2 uv_MainTex;
	};
	
	
	fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
    {
        fixed4 c;
        c.rgb = s.Albedo; 
        c.a = s.Alpha;
        return c;
    }
	
	// Scroll the texture
	float _xScrollSpeed;
	float _yScrollSpeed;
	void vert (inout appdata_full v) {
		v.texcoord.x = v.texcoord.x + _xScrollSpeed * _Time;
		v.texcoord.y = v.texcoord.y + _yScrollSpeed * _Time;
	}

	float _AlphaMultiplier;
	void surf (Input IN, inout SurfaceOutput o) {
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
		o.Albedo = c.rgb;
		o.Alpha = c.a * _AlphaMultiplier;
	}
	ENDCG

}

Fallback "Transparent/Diffuse"
	
}

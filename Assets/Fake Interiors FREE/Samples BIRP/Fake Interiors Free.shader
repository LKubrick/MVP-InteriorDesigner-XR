// Made with Amplify Shader Editor v1.9.3.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AmplifyShaderFakeInteriors/Fake Interiors FREE"
{
	Properties
	{
		_RoomColorTint("Room Color Tint", Color) = (1,1,1,0)
		_RoomIntensity("Room Intensity", Range( 0 , 100)) = 1
		[Toggle(_FACADETEXTURE_ON)] _FacadeTexture("Facade Texture", Float) = 0
		[NoScaleOffset]_FacadeAlbedo("Facade Albedo", 2D) = "black" {}
		_FacadeTiling("Facade Tiling", Range( 0.5 , 20)) = 0.5
		[NoScaleOffset]_FacadeSmoothness("Facade Smoothness", 2D) = "white" {}
		_SmoothnessValue("Smoothness Value", Range( 0 , 1)) = 1
		[NoScaleOffset]_Floor("Floor", 2D) = "white" {}
		_FloorTexTiling("Floor Tex Tiling", Range( 0 , 10)) = 0
		[NoScaleOffset]_Wall("Wall", 2D) = "white" {}
		_WalltexTiling("Wall tex Tiling", Range( 0 , 100)) = 0
		[Toggle(_TOGGLEPROPLAYER_ON)] _TogglePropLayer("Toggle Prop Layer", Float) = 0
		[NoScaleOffset]_Props("Props", 2D) = "white" {}
		_PropsTexTiling("Props Tex Tiling", Range( 0 , 100)) = 0
		[NoScaleOffset]_Back("Back", 2D) = "white" {}
		_BackWallTexTiling("Back Wall Tex Tiling", Range( 0 , 100)) = 0
		[NoScaleOffset]_Ceiling("Ceiling", 2D) = "white" {}
		_CeilingTexTiling("Ceiling Tex Tiling", Range( 0 , 100)) = 0
		[Toggle(_SWITCHPLANE_ON)] _SwitchPlane("Switch Plane", Float) = 0
		_RoomTile("Room Tile", Range( 0.1 , 10)) = 0
		_RoomsXYZPropsW("Rooms X Y Z , Props W", Vector) = (1,1,1,1)
		_PositionOffsetXYZroomsWprops("Position Offset, XYZ = rooms, W = props", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" "DisableBatching" = "True" }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _FACADETEXTURE_ON
		#pragma shader_feature_local _SWITCHPLANE_ON
		#pragma shader_feature_local _TOGGLEPROPLAYER_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 vertexToFrag143_g2;
			float2 uv_texcoord;
		};

		uniform float4 _RoomColorTint;
		uniform float4 _RoomsXYZPropsW;
		uniform float _RoomTile;
		uniform float4 _PositionOffsetXYZroomsWprops;
		uniform sampler2D _Props;
		uniform float _PropsTexTiling;
		uniform sampler2D _Wall;
		uniform float _WalltexTiling;
		uniform sampler2D _Back;
		uniform float _BackWallTexTiling;
		uniform sampler2D _Floor;
		uniform float _FloorTexTiling;
		uniform sampler2D _Ceiling;
		uniform float _CeilingTexTiling;
		uniform float _RoomIntensity;
		uniform sampler2D _FacadeAlbedo;
		uniform float4 _FacadeAlbedo_ST;
		uniform float _FacadeTiling;
		uniform sampler2D _FacadeSmoothness;
		uniform float _SmoothnessValue;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			o.vertexToFrag143_g2 = ase_vertex3Pos;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 temp_output_13_0_g2 = ( ( _RoomsXYZPropsW + float4( -1E-05,-1E-05,-1E-05,-1E-05 ) ) * _RoomTile );
			#ifdef _SWITCHPLANE_ON
				float staticSwitch147_g2 = (i.vertexToFrag143_g2).z;
			#else
				float staticSwitch147_g2 = (i.vertexToFrag143_g2).x;
			#endif
			float4 appendResult146_g2 = (float4(i.vertexToFrag143_g2 , staticSwitch147_g2));
			float4 InterpVertexPos148_g2 = appendResult146_g2;
			float4 temp_output_10_0_g2 = ( InterpVertexPos148_g2 - _PositionOffsetXYZroomsWprops );
			float4 appendResult151_g2 = (float4(_WorldSpaceCameraPos , 1.0));
			float4 temp_output_150_0_g2 = mul( unity_WorldToObject, appendResult151_g2 );
			#ifdef _SWITCHPLANE_ON
				float staticSwitch157_g2 = (temp_output_150_0_g2).z;
			#else
				float staticSwitch157_g2 = (temp_output_150_0_g2).x;
			#endif
			float4 appendResult158_g2 = (float4((temp_output_150_0_g2).xyz , staticSwitch157_g2));
			float4 TransCameraPos159_g2 = appendResult158_g2;
			float4 V211_g2 = ( TransCameraPos159_g2 - _PositionOffsetXYZroomsWprops );
			float4 V116_g2 = ( temp_output_10_0_g2 - V211_g2 );
			float4 temp_output_24_0_g2 = ( ( ( ( floor( ( temp_output_13_0_g2 * temp_output_10_0_g2 ) ) + step( float4( 0,0,0,0 ) , V116_g2 ) ) / temp_output_13_0_g2 ) - V211_g2 ) / V116_g2 );
			float Y36_g2 = (temp_output_24_0_g2).y;
			float newPlane27_g2 = (temp_output_24_0_g2).w;
			float Z30_g2 = (temp_output_24_0_g2).z;
			float4 temp_output_102_0_g2 = ( ( newPlane27_g2 * V116_g2 ) + V211_g2 );
			#ifdef _SWITCHPLANE_ON
				float2 staticSwitch103_g2 = (temp_output_102_0_g2).xy;
			#else
				float2 staticSwitch103_g2 = (temp_output_102_0_g2).yz;
			#endif
			float2 break98_g2 = ( staticSwitch103_g2 * _PropsTexTiling );
			float2 appendResult96_g2 = (float2(break98_g2.y , break98_g2.x));
			float2 appendResult97_g2 = (float2(break98_g2.x , break98_g2.y));
			#ifdef _SWITCHPLANE_ON
				float2 staticSwitch95_g2 = appendResult97_g2;
			#else
				float2 staticSwitch95_g2 = appendResult96_g2;
			#endif
			float4 tex2DNode89_g2 = tex2Dbias( _Props, float4( staticSwitch95_g2, 0, -1.0) );
			float4 break90_g2 = tex2DNode89_g2;
			float4 appendResult92_g2 = (float4(break90_g2.r , break90_g2.g , break90_g2.b , 0.0));
			#ifdef _TOGGLEPROPLAYER_ON
				float4 staticSwitch93_g2 = tex2DNode89_g2;
			#else
				float4 staticSwitch93_g2 = appendResult92_g2;
			#endif
			float4 PropsVar94_g2 = staticSwitch93_g2;
			float temp_output_123_0_g2 = step( newPlane27_g2 , ( Z30_g2 * (PropsVar94_g2).w ) );
			float ifLocalVar114_g2 = 0;
			if( temp_output_123_0_g2 > 0.0 )
				ifLocalVar114_g2 = newPlane27_g2;
			else if( temp_output_123_0_g2 == 0.0 )
				ifLocalVar114_g2 = Z30_g2;
			else if( temp_output_123_0_g2 < 0.0 )
				ifLocalVar114_g2 = Z30_g2;
			float X35_g2 = (temp_output_24_0_g2).x;
			float temp_output_113_0_g2 = step( ifLocalVar114_g2 , X35_g2 );
			float ifLocalVar116_g2 = 0;
			if( temp_output_113_0_g2 <= 0.0 )
				ifLocalVar116_g2 = X35_g2;
			else
				ifLocalVar116_g2 = ifLocalVar114_g2;
			float2 break78_g2 = ( (( ( Z30_g2 * V116_g2 ) + V211_g2 )).xy * _WalltexTiling );
			float2 appendResult77_g2 = (float2(break78_g2.x , break78_g2.y));
			float4 WallVar87_g2 = tex2D( _Wall, appendResult77_g2 );
			float4 ifLocalVar120_g2 = 0;
			if( temp_output_123_0_g2 <= 0.0 )
				ifLocalVar120_g2 = WallVar87_g2;
			else
				ifLocalVar120_g2 = PropsVar94_g2;
			float4 BackVar68_g2 = tex2D( _Back, ( (( ( X35_g2 * V116_g2 ) + V211_g2 )).zy * _BackWallTexTiling ) );
			float4 ifLocalVar119_g2 = 0;
			if( temp_output_113_0_g2 <= 0.0 )
				ifLocalVar119_g2 = BackVar68_g2;
			else
				ifLocalVar119_g2 = ifLocalVar120_g2;
			float2 temp_output_56_0_g2 = (( ( Y36_g2 * V116_g2 ) + V211_g2 )).xz;
			float Y_inverted39_g2 = (V116_g2).y;
			float4 lerpResult63_g2 = lerp( tex2D( _Floor, ( temp_output_56_0_g2 * _FloorTexTiling ) ) , tex2D( _Ceiling, ( temp_output_56_0_g2 * _CeilingTexTiling ) ) , step( 0.0 , Y_inverted39_g2 ));
			float4 CeilVar66_g2 = lerpResult63_g2;
			float4 ifLocalVar110_g2 = 0;
			if( Y36_g2 <= ifLocalVar116_g2 )
				ifLocalVar110_g2 = CeilVar66_g2;
			else
				ifLocalVar110_g2 = ifLocalVar119_g2;
			float4 temp_output_43_0_g2 = ( _RoomColorTint * ( ifLocalVar110_g2 * _RoomIntensity ) );
			float2 uv_FacadeAlbedo = i.uv_texcoord * _FacadeAlbedo_ST.xy + _FacadeAlbedo_ST.zw;
			float2 uv1138_g2 = ( uv_FacadeAlbedo * _FacadeTiling );
			float4 tex2DNode48_g2 = tex2D( _FacadeAlbedo, uv1138_g2 );
			float4 lerpResult42_g2 = lerp( temp_output_43_0_g2 , tex2DNode48_g2 , tex2DNode48_g2.a);
			#ifdef _FACADETEXTURE_ON
				float4 staticSwitch41_g2 = lerpResult42_g2;
			#else
				float4 staticSwitch41_g2 = temp_output_43_0_g2;
			#endif
			o.Albedo = staticSwitch41_g2.rgb;
			#ifdef _FACADETEXTURE_ON
				float staticSwitch136_g2 = ( tex2D( _FacadeSmoothness, uv1138_g2 ).r * _SmoothnessValue );
			#else
				float staticSwitch136_g2 = 0.0;
			#endif
			o.Smoothness = staticSwitch136_g2;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEFakeInspector"
}
/*ASEBEGIN
Version=19303
Node;AmplifyShaderEditor.FunctionNode;561;4928,-768;Inherit;False;Fake Interiors Free;0;;2;4e116884b139baf4cab6f1a9e439902d;0;0;2;COLOR;160;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;5248,-768;Float;False;True;-1;2;ASEFakeInspector;0;0;Standard;AmplifyShaderFakeInteriors/Fake Interiors FREE;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;0;0;561;160
WireConnection;0;4;561;0
ASEEND*/
//CHKSM=EB6C8CAAF436DEE9A11BFDB28BA07028A93E9D53
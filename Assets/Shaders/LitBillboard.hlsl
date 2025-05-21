
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Billboard Shader
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

float3 Calculate(float3 Position, float Pitch, float orthographic) {
    float cosPitch = cos(Pitch);
    float sinPitch = sin(Pitch);
    float3x3 pitchRotationMatrix = float3x3(
        1,        0,         0,
        0, cosPitch, -sinPitch,
        0, sinPitch,  cosPitch
    );
    float3 pitchedPosition = mul(pitchRotationMatrix, Position);

    float3 cameraForward;
	if (0.5f < orthographic) {
        cameraForward = -UNITY_MATRIX_V[2].xyz;
    } else {
        float3 cameraPosition = _WorldSpaceCameraPos;
        float3 objectPosition = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
        cameraForward = normalize(objectPosition - cameraPosition);
    }
    cameraForward.y = 0.0f;
    cameraForward = normalize(cameraForward);

    float3 right   = normalize(cross(float3(0, 1, 0), cameraForward));
    float3 up      = cross(cameraForward, right);
    float3 forward = cameraForward;
    float3x3 billboardYawMatrix = float3x3(
        right.x, up.x, forward.x,
        right.y, up.y, forward.y,
        right.z, up.z, forward.z
    );
    return mul(billboardYawMatrix, pitchedPosition);
}

void Billboard_float(in float3 In, in float Pitch, in float Orthographic, out float3 Out) {
	Out = Calculate(In, radians(Pitch), Orthographic);
}


/*
#ifndef SHADOW_SHADER_H
#define SHADOW_SHADER_H

	// Methods

	inline float4x4 TRS(float3 position, float2 scale, float2 pivot, float4x4 camera) {
		float3 cameraForward = normalize(float3(camera[0][2], 0.0f, camera[2][2]));
		float  yaw  = atan2(cameraForward.x, cameraForward.z);
		float  cosY = cos(yaw);
		float  sinY = sin(yaw);
		float3 right   = float3(cosY,  0.0f, -sinY);
		float3 up      = float3(0.0f,  1.0f,  0.0f);
		float3 forward = float3(sinY,  0.0f,  cosY);

		float4x4 m = 0.0f;
		m[0][0] = right.x * scale.x;
		m[1][0] = right.y * scale.x;
		m[2][0] = right.z * scale.x;
		m[3][0] = 0.0f;
		m[0][1] = up.x * scale.y;
		m[1][1] = up.y * scale.y;
		m[2][1] = up.z * scale.y;
		m[3][1] = 0.0f;
		m[0][2] = forward.x;
		m[1][2] = forward.y;
		m[2][2] = forward.z;
		m[3][2] = 0.0f;
		m[0][3] = position.x + pivot.x * right.x + pivot.y * up.x;
		m[1][3] = position.y + pivot.x * right.y + pivot.y * up.y;
		m[2][3] = position.z + pivot.x * right.z + pivot.y * up.z;
		m[3][3] = 1.0f;
		return m;
	}

	inline float4x4 InverseAffineTransform(float4x4 m) {
		float3x3 rotation = float3x3(
			m[1].yzx * m[2].zxy - m[1].zxy * m[2].yzx,
			m[0].zxy * m[2].yzx - m[0].yzx * m[2].zxy,
			m[0].yzx * m[1].zxy - m[0].zxy * m[1].yzx);
		float det = dot(m[0].xyz, rotation[0]);
		rotation = transpose(rotation);
		rotation *= rcp(det);
		float3 position = mul(rotation, -m._14_24_34);

		m._11_21_31_41 = float4(rotation._11_21_31, 0.0f);
		m._12_22_32_42 = float4(rotation._12_22_32, 0.0f);
		m._13_23_33_43 = float4(rotation._13_23_33, 0.0f);
		m._14_24_34_44 = float4(position,           1.0f);

		return m;
	}



	// Lifecycle

	void Setup() {
		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			ShadowDrawData data = _ShadowDrawData[unity_InstanceID];
			unity_ObjectToWorld = TRS(data.position, data.scale, data.pivot, unity_CameraToWorld);
			unity_WorldToObject = InverseAffineTransform(unity_ObjectToWorld);
		#endif
	}

	void Billboard_float(
		in  float3 In,
		in  float2 Pitch,
		out float3 Out) {

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			ShadowDrawData data = _ShadowDrawData[unity_InstanceID];
			Out        = In;
			Out_Tiling = data.tiling;
			Out_Offset = data.offset;
		#else
			Out        = In;
			Out_Tiling = In_Tiling;
			Out_Offset = In_Offset;
		#endif
	}

#endif
*/
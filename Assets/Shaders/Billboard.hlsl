#ifndef BILLBOARD_SHADER_H
#define BILLBOARD_SHADER_H



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Billboard Shader
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

float3 Calculate(float3 position, float pitch, float orthographic) {
	float cosPitch = cos(pitch);
	float sinPitch = sin(pitch);
	float3x3 pitchRotationMatrix = float3x3(
		1,        0,         0,
		0, cosPitch, -sinPitch,
		0, sinPitch,  cosPitch
	);
	float3 pitchedPosition = mul(pitchRotationMatrix, position);

	float3 cameraForward;
	if (0.5f < orthographic) {
		cameraForward = -UNITY_MATRIX_V[2].xyz;
	} else {
		float3 cameraPosition = _WorldSpaceCameraPos;
		float3 objectPosition = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
		cameraForward = normalize(objectPosition - cameraPosition);
	}
	cameraForward.y = 0;
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



#endif

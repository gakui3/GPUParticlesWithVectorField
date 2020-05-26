#ifndef __QUATERNION__
#define __QUATERNION__

#define HALF_DEG2RAD 8.72664625e-3

float4 quaternion(float3 normalizedAxis, float degree) {
	float rad = degree * HALF_DEG2RAD;
	return float4(normalizedAxis * sin(rad), cos(rad));
}
float4 qmul(float4 a, float4 b) {
	return float4(a.w * b.xyz + b.w * a.xyz + cross(a.xyz, b.xyz), a.w * b.w - dot(a.xyz, b.xyz));
}
float3 qrotate(float4 q, float3 v) {
	return qmul(qmul(q, float4(v, 0)), float4(-q.xyz, q.w)).xyz;
}
float3 qrotateinv(float4 q, float3 v) {
	return qmul(qmul(float4(-q.xyz, q.w), float4(v, 0)), q).xyz;
}

// Rotate a vector with a rotation quaternion.
// http://mathworld.wolfram.com/Quaternion.html
float3 rotateWithQuaternion(float3 v, float4 r)
{
    float4 r_c = r * float4(-1, -1, -1, 1);
    return qmul(r, qmul(float4(v, 0), r_c)).xyz;
}

float4 getAngleAxisRotation(float3 axis, float angle){
	axis = normalize(axis);
	float s,c;
	sincos(angle,s,c);
	return float4(axis.x*s,axis.y*s,axis.z*s,c);
}

float3 rotateAngleAxis(float3 v, float3 axis, float angle){
	float4 q = getAngleAxisRotation(axis,angle);
	return rotateWithQuaternion(v,q);
}

float4 fromToRotation(float3 from, float3 to){
	float3
		v1 = normalize(from),
		v2 = normalize(to),
		cr = cross(v1,v2);
	float4 q = float4( cr,1+dot(v1,v2) );
	return normalize(q);
}
#endif

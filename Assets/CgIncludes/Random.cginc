#ifndef RANDOM_INCLUDED
#define RANDOM_INCLUDED

float hash( float n ) {
    return frac(sin(n) * 43758.5453);
}

float rand( float2 co ){
  return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
}

float3 rand3( float2 seed ){
	float t = sin(seed.x + seed.y * 1e3);
	return float3(frac(t*1e4), frac(t*1e6), frac(t*1e5));
}

float srand(float2 v, float s){
	v *= s;
	float2 p = frac(v);
	v = floor(v);
	float
		r00 = rand(v),
		r01 = rand(v + float2(0,1)),
		r10 = rand(v + float2(1,0)),
		r11 = rand(v + float2(1,1));
	return lerp(lerp(r00, r10, p.x), lerp(r01, r11, p.x), p.y);
}

float3 srand3(float2 v, float s){
	v *= s;
	float2 p = frac(v);
	v = floor(v);
	float3
		r00 = rand3(v),
		r01 = rand3(v + float2(0,1)),
		r10 = rand3(v + float2(1,0)),
		r11 = rand3(v + float2(1,1));
	return lerp(lerp(r00, r10, p.x), lerp(r01, r11, p.x), p.y);
}

#endif // RANDOM_INCLUDED

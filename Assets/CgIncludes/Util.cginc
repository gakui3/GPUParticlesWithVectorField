#ifndef UTIL_INCLUDED
#define UTIL_INCLUDED
			
float3 hsv_to_rgb(float3 HSV)
{
	float3 RGB = HSV.z;

	float var_h = HSV.x * 6;
	float var_i = floor(var_h);   // Or ... var_i = floor( var_h )
	float var_1 = HSV.z * (1.0 - HSV.y);
	float var_2 = HSV.z * (1.0 - HSV.y * (var_h-var_i));
	float var_3 = HSV.z * (1.0 - HSV.y * (1-(var_h-var_i)));
	if      (var_i == 0) { RGB = float3(HSV.z, var_3, var_1); }
	else if (var_i == 1) { RGB = float3(var_2, HSV.z, var_1); }
	else if (var_i == 2) { RGB = float3(var_1, HSV.z, var_3); }
	else if (var_i == 3) { RGB = float3(var_1, var_2, HSV.z); }
	else if (var_i == 4) { RGB = float3(var_3, var_1, HSV.z); }
	else                 { RGB = float3(HSV.z, var_1, var_2); }

	return (RGB);

}



float3 rgb_to_hsv_no_clip(float3 RGB)
{

	float3 HSV;
	float minChannel, maxChannel;
	if (RGB.x > RGB.y) {
		maxChannel = RGB.x;
		minChannel = RGB.y;
	}

	else {
		maxChannel = RGB.y;
		minChannel = RGB.x;
	}

	if (RGB.z > maxChannel) maxChannel = RGB.z;
	if (RGB.z < minChannel) minChannel = RGB.z;

	HSV.xy = 0;
	HSV.z = maxChannel;
	float delta = maxChannel - minChannel;             //Delta RGB value
	if (delta != 0) {                    // If gray, leave H & S at zero
		HSV.y = delta / HSV.z;
		float3 delRGB;
		delRGB = (HSV.zzz - RGB + 3*delta) / (6.0*delta);
		if      ( RGB.x == HSV.z ) HSV.x = delRGB.z - delRGB.y;
		else if ( RGB.y == HSV.z ) HSV.x = ( 1.0/3.0) + delRGB.x - delRGB.z;
		else if ( RGB.z == HSV.z ) HSV.x = ( 2.0/3.0) + delRGB.y - delRGB.x;
	}

	return (HSV);
}

fixed value_to_rate(fixed v, fixed min, fixed max)
{
	return (v - min) / (max - min);
}
fixed value_to_rate_clamp(fixed v, fixed min, fixed max)
{
	return clamp(value_to_rate(v, min, max), 0, 1);
}


//processingのmap関数
float _norm(float value, float low, float hight){
	return(value-low)/(hight-low);
}

float _Lerp(float sourceValue1, float sourceValue2, float amount){
	return sourceValue1 + (sourceValue2 - sourceValue1) * amount;
}
	
float Map(float value, float low1, float hight1, float low2, float hight2){
	return _Lerp(low2, hight2, _norm(value, low1, hight1));
}


float3 mod(float3 a, float3 b)
{
   	return frac(abs(a / b)) * abs(b);
}

float2 mod(float2 a, float b)
{
	return frac(abs(a/b)) * abs(b);
}

float mod(float a, float b)
{
	return frac(abs(a/b)) * abs(b);
}

float3 repeat(float3 pos, float3 span)
{
    return mod(pos, span) - span * 0.5;
}


//http://t-pot.com/program/90_BitonicSort/index.html
//screenpositionの値を1次元に置き換える
//pos is 0~1
float convert2dto1d(float2 pos)
{
	float2 elem2d = _ScreenParams.xy * pos;
	elem2d = floor(elem2d);
	float elem1d = elem2d.y * _ScreenParams.x + elem2d.x;

	return elem1d;
}

//その逆
//配列をtextureにとか使えそう
//もっと最適化できるっぽい
//http://qiita.com/YVT/items/c695ab4b3cf7faa93885
float2 convert1dto2d(float index)
{
	float2 dst;
	dst.x = fmod (index,  _ScreenParams.x) / _ScreenParams.x;
	dst.y = floor(index / _ScreenParams.x) / _ScreenParams.y;

	return dst;
}
#endif

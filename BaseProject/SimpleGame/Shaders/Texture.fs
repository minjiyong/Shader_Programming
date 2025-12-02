#version 330

layout(location=0) out vec4 FragColor;

in vec2 v_Tex;

uniform sampler2D u_TexID;
uniform sampler2D u_TexID1;
uniform int u_Method;      // 0:normal, 1:BlurH, 2:BlurV, 3:Merge
uniform float u_Time;   // 게임에서 넘겨주는 시간(초 단위 정도)

const float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);

// 2D → 1D 해시 노이즈
float hash21(vec2 p)
{
    p = fract(p * vec2(123.34, 345.45));
    p += dot(p, p + 34.345);
    return fract(p.x * p.y);
}

vec4 Lens()
{
    // 원래 쓰던 텍스처 좌표 (Y 뒤집기)
    vec2 uv = vec2(v_Tex.x, 1.0 - v_Tex.y);

    // 렌즈 중심과 반지름
    vec2  center = vec2(0.5, 0.5); // 중앙
    float radius = 0.4;            // 렌즈 영향 반경 (0~0.5 사이 값이 무난)

    // 렌즈 테두리용 얇은 라인 두께
    float border = 0.005;

    // 중심에서의 벡터와 거리
    vec2  offset = uv - center;
    float dist   = length(offset);

    // 기본 색: 렌즈 밖 또는 초기값
    vec4 color = texture(u_TexID, uv);

    // 렌즈 영역 안에서만 왜곡 적용
    if (dist < radius)
    {
        // 거리 비율 [0,1]
        float t = dist / radius;

        // 왜곡 강도 (p > 1 이면 중심 확대 효과)
        float power = 2.0;
        float newDist = radius * pow(t, power);

        // dist가 0일 때 나누기 방지
        vec2 dir = (dist > 1e-4) ? (offset / dist) : vec2(0.0);

        // 샘플링할 새 좌표 = 중심 + 방향 * 줄어든 거리
        vec2 newUV = center + dir * newDist;

        // 혹시 벗어날 수 있으니 보정
        newUV = clamp(newUV, vec2(0.0), vec2(1.0));

        // 렌즈 안쪽에서는 왜곡된 좌표로 샘플링
        color = texture(u_TexID, newUV);

        // 중앙 쪽 밝기 살짝 올려서 렌즈 느낌 추가
        float brightness = 1.0 + (1.0 - t) * 0.3;
        color.rgb *= brightness;

        // 테두리에 살짝 라인(하이라이트) 넣고 싶으면
        if (dist > radius - border)
        {
            color.rgb = mix(color.rgb, vec3(1.0), 0.4);
        }
    }

    return color;
}

vec4 RainDrop() 
{
    // 지금까지 쓰던 텍스처 좌표 (Y 뒤집기 포함)
    vec2 uv = vec2(v_Tex.x, 1.0 - v_Tex.y);

    // 기본 배경 색 (비효과 없을 때)
    vec4 baseColor = texture(u_TexID, uv);

    // -----------------------------
    // 1) 화면을 격자로 나눠서 각 칸에 "물방울 후보" 배치
    // -----------------------------
    // tiling.x : x 방향 칸 수, tiling.y : y 방향 칸 수
    vec2 tiling = vec2(18.0, 10.0);      // 값 키우면 물방울 개수↑, 작으면 물방울 개수↓
    vec2 gUV    = uv * tiling;           // [0,1] → [0,tiling]
    vec2 cell   = floor(gUV);            // 현재 픽셀이 속한 셀 (정수 좌표)
    vec2 cellUV = fract(gUV);            // 셀 안에서의 국소 좌표 [0,1]

    // 이 셀에 물방울이 있는지 랜덤하게 결정
    float rnd    = hash21(cell);
    float hasDrop = step(0.75, rnd);     // 0.75 이상이면 물방울 존재 (약 25% 확률)

    // 이 셀에는 물방울 자체가 없으면 그냥 원본 색 리턴
    if (hasDrop < 0.5)
        return baseColor;

    // -----------------------------
    // 2) 셀 내부에서 물방울의 x 위치 / 속도 등을 랜덤하게 결정
    // -----------------------------
    float rndX = hash21(cell + 5.13);    // x 위치용 랜덤
    float rndS = hash21(cell + 8.72);    // 속도용 랜덤

    float dropX = mix(0.2, 0.8, rndX);   // 셀 안에서 [0.2, 0.8] 사이에 물방울 중심 x
    float speed = mix(0.20, 0.60, rndS); // 떨어지는 속도 범위

    // 시간에 따라 아래로 흘러내리도록 y 위치 계산
    float t  = fract(u_Time * speed + rnd * 10.0);  // 0~1 반복
    // y=1.2에서 시작해서 y=-0.4까지 내려감 (셀 위에서 아래까지)
    float dropY = 1.2 - t * 1.6;

    vec2 dropPos = vec2(dropX, dropY);  // 셀 기준 물방울 중심

    // -----------------------------
    // 3) 물방울 모양(타원형) 마스크 계산
    // -----------------------------
    vec2 d  = cellUV - dropPos;          // 현재 픽셀 - 물방울 중심
    vec2 ed = vec2(d.x, d.y * 1.3);      // y를 늘려서 길쭉한 타원 느낌
    float r    = 0.23;                   // 물방울 반지름(셀 기준)
    float dist = length(ed);             // 타원 거리

    // 안쪽 = 1, 바깥 = 0 으로 부드럽게 떨어지는 마스크
    float insideDrop = smoothstep(r, r - 0.04, dist);

    // 드롭도 없고, 꼬리 영역도 아닌 픽셀은 바로 리턴 (최적화)
    if (insideDrop <= 0.0 && (cellUV.y > dropPos.y || abs(cellUV.x - dropPos.x) > 0.04))
        return baseColor;

    // -----------------------------
    // 4) 물방울 내부: 굴절(렌즈) 효과로 배경 왜곡
    // -----------------------------
    // ed 방향으로부터 "법선" 비슷한 방향 벡터
    vec2 dir = (dist > 1e-4) ? (ed / dist) : vec2(0.0);

    // 중심에서 멀어질수록 굴절량 줄어들게
    float strength = max(r - dist, 0.0);    // 중심에서 r까지 거리 남은 정도
    float refractScale = 0.05;              // 굴절 강도 계수 (크면 왜곡↑)

    // 물방울 안에서 uv를 "안쪽"으로 밀어넣는 느낌 (볼록렌즈)
    vec2 offset = -dir * strength * refractScale;

    // cellUV 기준 offset → 실제 uv 기준으로 환산 (tiling 나눠주기)
    vec2 refractUV = uv + offset / tiling;
    refractUV = clamp(refractUV, vec2(0.0), vec2(1.0));

    vec4 dropColor = texture(u_TexID, refractUV);

    // 물방울 중심 쪽을 살짝 더 밝게 해서 볼록 물방울 느낌
    float centerGlow = insideDrop * insideDrop;
    dropColor.rgb *= 1.0 + centerGlow * 0.35;

    // -----------------------------
    // 5) 물방울 아래쪽 "흐르는 꼬리" (streak) 효과
    // -----------------------------
    float streak = 0.0;
    if (cellUV.y < dropPos.y)   // 물방울 아래쪽만 꼬리 후보
    {
        float dx = abs(cellUV.x - dropPos.x);
        float dy = dropPos.y - cellUV.y;

        float streakWidth = 0.04;  // 꼬리 가로 폭
        float streakLen   = 0.50;  // 꼬리 최대 길이

        if (dx < streakWidth && dy < streakLen)
        {
            // x로 갈수록, 아래로 갈수록 점점 약해지는 꼬리
            float sx = 1.0 - dx / streakWidth;
            float sy = 1.0 - dy / streakLen;
            streak = sx * sy;
        }
    }

    if (streak > 0.0)
    {
        // 꼬리 부분은 아래로 당겨진 배경을 다시 샘플링해서 "흐릿하게 늘어진" 느낌
        vec2 streakOffset = vec2(0.0, -0.07 * streak);
        vec2 streakUV = uv + streakOffset / tiling;
        streakUV = clamp(streakUV, vec2(0.0), vec2(1.0));

        vec4 streakColor = texture(u_TexID, streakUV);
        streakColor.rgb *= 0.9;  // 약간 어둡게

        // 꼬리 쪽에서는 dropColor랑 섞어서 자연스럽게 연결
        dropColor = mix(dropColor, streakColor, streak * 0.7);
    }

    // -----------------------------
    // 6) 최종 블렌딩: 기본 배경 + 물방울/꼬리 색
    // -----------------------------
    float dropAlpha = max(insideDrop, streak) * hasDrop;
    vec4 finalColor = mix(baseColor, dropColor, dropAlpha);

    return finalColor;
}

vec4 Chromatic()
{
    // 기존에 쓰던 텍스처 좌표 (Y 뒤집기 유지)
    vec2 uv = vec2(v_Tex.x, 1.0 - v_Tex.y);

    // 화면 중심 기준으로 방향/거리 계산
    vec2 center = vec2(0.5, 0.5);
    vec2 dir    = uv - center;
    float dist  = length(dir) + 1e-6;   // 0 나누기 방지
    vec2 ndir   = dir / dist;           // 정규화 방향

    // 중심에서는 거의 색수차 없음, 가장자리로 갈수록 강해지게
    float baseShift = 0.002;            // 가운데 근처 색수차
    float edgeShift = 0.012;            // 가장자리 색수차
    float k = smoothstep(0.0, 0.8, dist);
    float shift = mix(baseShift, edgeShift, k);

    // 시간으로 살짝 요동치게 하고 싶으면 u_Time 있으면 사용
    // uniform float u_Time; 가 이미 있다면 주석 해제해도 됨
    // shift *= (1.0 + 0.25 * sin(u_Time * 2.0));

    vec2 offset = ndir * shift;

    // 각 채널별로 약간 다른 위치에서 샘플링
    vec2 uvR = clamp(uv + offset,       0.0, 1.0);
    vec2 uvG = uv;                                   // 기준
    vec2 uvB = clamp(uv - offset * 0.8, 0.0, 1.0);  // B는 조금 덜 이동

    float r = texture(u_TexID, uvR).r;
    float g = texture(u_TexID, uvG).g;
    float b = texture(u_TexID, uvB).b;

    vec3 col = vec3(r, g, b);

    // 가장자리 살짝 어둡게(비네팅)해서 색수차 강조
    float vignette = 1.0 - 0.45 * smoothstep(0.5, 0.9, dist);
    col *= vignette;

    return vec4(col, 1.0);
}

vec4 Pixelization()
{
    float resol = (sin(u_Time) + 1) * 100;
    float tx = floor(v_Tex.x * resol) / resol;
    float ty = floor(v_Tex.y * resol) / resol;
    return texture(u_TexID, vec2(tx, ty));
}

vec4 BlurH()
{             
    vec2 tex_offset = 1.0 / textureSize(u_TexID, 0); // gets size of single texel
    vec3 result = texture(u_TexID, v_Tex).rgb * weight[0]; // current fragment's contribution

    for(int i = 1; i < 5; ++i)
    {
        result += texture(u_TexID, v_Tex + vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
        result += texture(u_TexID, v_Tex - vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
    }

    return vec4(result, 1.0);
}

vec4 BlurV()
{             
    vec2 tex_offset = 1.0 / textureSize(u_TexID, 0); // gets size of single texel
    vec3 result = texture(u_TexID, v_Tex).rgb * weight[0]; // current fragment's contribution

    for(int i = 1; i < 5; ++i)
    {
        result += texture(u_TexID, v_Tex + vec2(0.0, tex_offset.y * i)).rgb * weight[i];
        result += texture(u_TexID, v_Tex - vec2(0.0, tex_offset.y * i)).rgb * weight[i];
    }

    return vec4(result, 1.0);
}

vec4 Merge()
{             
    const float gamma = 2.2;
    vec3 hdrColor = texture(u_TexID, vec2(v_Tex.x, 1-v_Tex.y)).rgb;      
    vec3 bloomColor = texture(u_TexID1, v_Tex).rgb;
    hdrColor += bloomColor; 

    // tone mapping
    vec3 result = vec3(1.0) - exp(-hdrColor * 5);

    // also gamma correct while we're at it       
    result = pow(result, vec3(1.0 / gamma));

    return vec4(result, 1.0);
} 


void main()
{
    FragColor = vec4(0);
    if(u_Method == 0){
        FragColor = texture(u_TexID, vec2(v_Tex.x, 1 - v_Tex.y));
    }
    else if(u_Method == 1){
        FragColor = BlurH();
    }
    else if(u_Method == 2){
        FragColor = BlurV();
    }
    else if(u_Method == 3){
        FragColor = Merge();
    }

    //FragColor = Lens();
    //FragColor = RainDrop();
    //FragColor = Chromatic();
    //FragColor = Pixelization();
}
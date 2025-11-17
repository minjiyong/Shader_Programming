#version 330 core
layout(location = 0) out vec4 FragColor;

in vec2 v_UV;            // [0,1]^2
uniform float u_Time;    // seconds
uniform float u_Seed;    // (선택) 랜덤 시드, 없으면 0.0으로 둬도 됨

// -------------------- 의사난수 해시 --------------------
float hash11(float x){
    x = fract(x * 0.1031);
    x *= x + 33.33;
    x *= x + x;
    return fract(x);
}
vec2 hash21(float x){
    // 1D -> 2D
    float n = sin(x * 1.1447 + 17.23) * 43758.5453;
    return fract(vec2(n, n * 1.2154));
}

// -------------------- 드롭 파라미터 생성 --------------------
// idx: 드롭의 전역 인덱스(시간이 흐르며 증가), seed: 사용자 시드
struct Drop {
    vec2  pos;       // [0,1]^2 낙하 지점
    float c;         // 전파 속도 (UV/sec)
    float k;         // 공간 주파수 (링 밀도) → period in xi = 1/k
    float t0;        // 낙하 시각
    float L;         // 전면 통과 후 유효 xi 구간 길이 (≈ 허용 링 길이)
};
Drop make_drop(int idx, float seed, float rate){
    float fid = float(idx) + seed*97.31;

    // 위치 랜덤
    vec2  p = hash21(fid*12.9898) ;                 // [0,1)
    // 속도 랜덤
    float c = mix(0.25, 0.55, hash11(fid*78.233));  // 0.25~0.55 UV/sec
    // 링 간격(주파수) 랜덤
    float k = mix(5.0, 12.0, hash11(fid*39.425));   // 5~12

    // 1 or 2 사이클만 허용되도록 xi-길이 L 설정
    float cycles = (hash11(fid*5.421) < 0.5) ? 1.0 : 2.0;
    // 살짝 랜덤 여유를 줘서 0.9~1.2배
    float L = (cycles / k) * mix(0.9, 1.2, hash11(fid*3.117));

    // 드롭 발생 시각: 균일 레이트 기반
    float t0 = float(idx) / rate;

    Drop d; d.pos=p; d.c=c; d.k=k; d.t0=t0; d.L=L; return d;
}

// 매끈한 front 게이트: front에서 켜지고, L 이후 꺼진다 → 1~2회만 보이게
float gate_segment(float xi, float L){
    const float eps = 0.002;  // front 두께
    float on  = smoothstep(0.0, eps, xi);           // front 뒤에서만 켜짐
    float off = smoothstep(L, L+eps, xi);           // L 이후 꺼짐
    return clamp(on * (1.0 - off), 0.0, 1.0);
}

// 단일 드롭 파형: front 뒤에서만 sin 진행, 거리 감쇠 포함
float drop_wave(vec2 uv, Drop d, float t){
    vec2  q  = uv - d.pos;
    float r  = length(q);
    float xi = d.c * (t - d.t0) - r;          // retarded coordinate
    float g  = gate_segment(xi, d.L);         // 1~2 사이클만 허용

    // (선택) 아주 약한 도메인 왜곡으로 자연스러운 물결
    // 너무 과하면 물리감이 떨어지니 소량만
    float a = atan(q.y, q.x);
    float wobble = 0.006 * sin(3.0*a + 0.7*t + 4.1*xi);

    // 진행파: sin(2π k (xi + wobble))
    float w  = sin(6.28318530718 * d.k * (xi + wobble));

    // 감쇠: 거리 감쇠(중앙 에너지 보존 상징적), front 뒤 꼬리 부드럽게
    float amp_r = exp(-1.8 * r);                      // r-감쇠
    float amp_x = smoothstep(0.0, 0.15, xi) *         // front 직후 살짝 키움
                  smoothstep(d.L, d.L*0.7, xi);       // 끝으로 갈수록 약하게

    return g * w * amp_r * amp_x;
}

void main(){
    vec2 uv = v_UV;               // 해상도 보정 없음 (요청 사항)
    float t  = u_Time;

    // -------- 드롭 스트림 구성 --------
    // 초당 rate 개 발생. 현재 시각 근처의 최근 EMIT개만 합산 (성능/가독)
    const float RATE = 0.65;      // drops per second
    const int   EMIT = 9;         // 최근 n개만 그리면 충분
    int   base = int(floor(t * RATE));

    float v = 0.0;
    for (int i = 0; i < EMIT; ++i) {
        int idx = base - i;                 // 최근 드롭부터 거슬러 올라감
        Drop d  = make_drop(idx, u_Seed, RATE);
        v += drop_wave(uv, d, t);
    }

    // 여러 드롭이 겹칠 수 있으니 살짝 톤 매핑
    // v 대략 [-N, N] → 0~1로 눌러준다
    float s = 0.5 + 0.38 * tanh(v);   // 부드러운 압축

    // 색상(물색 그라데이션)
    vec3 deep  = vec3(0.05, 0.12, 0.20);
    vec3 mid   = vec3(0.12, 0.35, 0.60);
    vec3 light = vec3(0.75, 0.90, 1.00);
    vec3 col   = mix(mid, light, s);
    col        = mix(deep, col, 0.85);

    // 얇은 하이라이트(등고선)
    float hl = smoothstep(0.92, 1.0, s);
    col += 0.10 * hl;

    // 감마 보정
    col = pow(col, vec3(1.0/2.2));
    FragColor = vec4(col, 1.0);
}
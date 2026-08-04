// js/effects/scanline.js
window.Effects = window.Effects || {};

window.Effects.scanline = {
  id: 'scanline',
  label: '사이버 스캔',
  // 우주선 내부 같은 어두운 파란색 배경
  clearColor: [0.01, 0.02, 0.05, 1.0], 
  uniformNames: ['uLineDensity', 'uScanSpeed', 'uZoneSize', 'uColorPulse'],
  
  // UI 슬라이더 파라미터 (직관적인 제어)
  params: {
    lineDensity: { value: 50.0, min: 10.0, max: 100.0, step: 1.0, label: 'Line Density' }, // 주사선 촘촘함
    scanSpeed: { value: 1.5, min: 0.1, max: 5.0, step: 0.1, label: 'Scan Speed' },     // 스캔 바 속도
    zoneSize: { value: 0.3, min: 0.05, max: 0.8, step: 0.01, label: 'Active Zone' },   // 스캔 영역 크기
    colorPulse: { value: 2.0, min: 0.5, max: 10.0, step: 0.1, label: 'Color Pulse' },  // 색상 깜빡임 속도
  },

  fragmentShader: [
    'precision mediump float;',
    'varying vec2 vUv;',
    'uniform float uTime;',
    'uniform float uLineDensity;',
    'uniform float uScanSpeed;',
    'uniform float uZoneSize;',
    'uniform float uColorPulse;',

    'void main() {',
    '  vec2 uv = vUv;',
    '  ',
    '  // 1. 기본 배경 그리드 패턴 (매우 미세하게)',
    '  vec2 gridUv = fract(uv * 50.0);',
    '  float grid = step(0.95, gridUv.x) + step(0.95, gridUv.y);',
    '  vec3 backColor = vec3(0.05, 0.1, 0.2) * grid;',
    '  ',
    '  // 2. 가로 주사선 (Scanlines) 생성',
    '  float lines = sin(uv.y * uLineDensity * 3.14159);',
    '  lines = pow(mix(0.0, 1.0, lines * 0.5 + 0.5), 2.0); // 선을 더 가늘고 뚜렷하게',
    '  ',
    '  // 3. 위아래로 움직이는 스캔 영역 (Active Zone) 계산',
    '  // sin 함수로 0~1 사이를 왕복하는 스캔 바 좌표',
    '  float scanPos = sin(uTime * uScanSpeed) * 0.5 + 0.5;',
    '  ',
    '  // 현재 픽셀이 스캔 바 근처(ZoneSize)에 있는지 확인',
    '  float distToScan = abs(uv.y - scanPos);',
    '  float zoneMask = step(distToScan, uZoneSize);',
    '  ',
    '  // 영역 가장자리로 갈수록 부드럽게 감쇠',
    '  zoneMask *= (1.0 - distToScan / uZoneSize);',
    '  ',
    '  // 4. 색상 연출 (네온 톤)',
    '  // 시간에 따라 사이언 -> 블루 -> 그린으로 미세하게 변화',
    '  vec3 baseColor = vec3(0.0, 0.8, 1.0); // Cyan',
    '  baseColor.g += sin(uTime * uColorPulse) * 0.2;',
    '  baseColor.b -= sin(uTime * uColorPulse) * 0.2;',
    '  ',
    '  // 주사선 색상',
    '  vec3 lineColor = baseColor * lines;',
    '  ',
    '  // 스캔 영역 내부의 발광(Glow) 효과',
    '  vec3 glowColor = baseColor * zoneMask * 1.2;',
    '  ',
    '  // 스캔 바 경계선에 강한 하이라이트',
    '  float beam = step(distToScan, 0.005) * 2.0;',
    '  ',
    '  // 최종 합성',
    '  vec3 finalColor = backColor + lineColor + glowColor + (baseColor * beam);',
    '  ',
    '  // 알파 값: 스캔 영역과 주사선이 있는 곳만 밝게',
    '  float alpha = (zoneMask * 0.7) + (lines * 0.2) + beam + grid * 0.2;',
    '  ',
    '  gl_FragColor = vec4(finalColor, alpha);',
    '}'
  ].join('\n'),

  applyUniforms: function (gl, loc, params) {
    if (loc.uLineDensity) gl.uniform1f(loc.uLineDensity, params.lineDensity.value);
    if (loc.uScanSpeed) gl.uniform1f(loc.uScanSpeed, params.scanSpeed.value);
    if (loc.uZoneSize) gl.uniform1f(loc.uZoneSize, params.zoneSize.value);
    if (loc.uColorPulse) gl.uniform1f(loc.uColorPulse, params.colorPulse.value);
  }
};
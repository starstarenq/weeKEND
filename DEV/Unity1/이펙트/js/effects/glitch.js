// js/effects/glitch.js
window.Effects = window.Effects || {};

window.Effects.glitch = {
  id: 'glitch',
  label: '홀로그램 글리치',
  clearColor: [0.05, 0.05, 0.1, 1.0], // 홀로그램 느낌의 어두운 청록색 배경
  uniformNames: ['uScale', 'uSpeed', 'uGlitchAmount', 'uScanlineCount', 'uRGBShift'],
  
  // UI 슬라이더 바인딩 파라미터
  params: {
    scale: { value: 3.0, min: 0.5, max: 10.0, step: 0.1, label: 'Noise Scale' },
    speed: { value: 2.0, min: 0.1, max: 10.0, step: 0.1, label: 'Speed' },
    glitchAmount: { value: 0.4, min: 0.0, max: 1.0, step: 0.01, label: 'Glitch Intensity' },
    scanlineCount: { value: 120.0, min: 20.0, max: 500.0, step: 10.0, label: 'Scanline Density' },
    rgbShift: { value: 0.03, min: 0.0, max: 0.1, step: 0.001, label: 'RGB Shift' }
  },

  // WebGL 프래그먼트 셰이더
  fragmentShader: [
    'precision mediump float;',
    'varying vec2 vUv;',
    
    'uniform sampler2D uNoiseTexture;',
    'uniform float uTime;',
    'uniform float uScale;',
    'uniform float uGlitchAmount;',
    'uniform float uScanlineCount;',
    'uniform float uRGBShift;',

    'void main() {',
    '  vec2 uv = vUv;',
    '  float time = uTime;',
    '  ',
    '  // 1. Y축을 불연속적인 단층(Block) 형태로 분할',
    '  float blockY = floor(uv.y * 20.0) / 20.0;',
    '  ',
    '  // 2. 노이즈 텍스처를 참조하여 단층별 난수(Random Offset) 계산',
    '  vec2 noiseUv = vec2(blockY * uScale, fract(time * 0.5));',
    '  float noiseVal = texture2D(uNoiseTexture, noiseUv).r;',
    '  ',
    '  // 특정 임계값을 넘을 때만 찢어지는(Glitch) X축 오프셋 발생',
    '  float glitchThreshold = 1.0 - uGlitchAmount;',
    '  float offsetX = 0.0;',
    '  if (noiseVal > glitchThreshold) {',
    '    offsetX = (noiseVal - glitchThreshold) * 0.5 * (step(0.5, fract(noiseVal * 13.0)) * 2.0 - 1.0);',
    '  }',
    '  ',
    '  // Glitch 오프셋 적용된 UV 좌표',
    '  vec2 distortedUv = vec2(uv.x + offsetX, uv.y);',
    '  ',
    '  // 3. RGB 색수차(Chromatic Aberration / RGB Shift) 효과',
    '  vec2 shift = vec2(uRGBShift * noiseVal, 0.0);',
    '  float r = texture2D(uNoiseTexture, (distortedUv + shift) * uScale).r;',
    '  float g = texture2D(uNoiseTexture, distortedUv * uScale).g;',
    '  float b = texture2D(uNoiseTexture, (distortedUv - shift) * uScale).b;',
    '  ',
    '  // Base Hologram Color (청록/사이언 톤 베이스)',
    '  vec3 holoColor = vec3(r * 0.2, g * 0.9, b * 1.0);',
    '  ',
    '  // 4. 주사선(Scanline) 효과 연출',
    '  float scanline = sin(uv.y * uScanlineCount * 3.14159) * 0.5 + 0.5;',
    '  scanline = pow(scanline, 1.5); // 주사선 대비 강조',
    '  ',
    '  // 5. 플리커(Flicker / 화면 깜빡임) 효과',
    '  float flicker = 0.85 + 0.15 * sin(time * 50.0);',
    '  ',
    '  // 최종 컬러 합성 (홀로그램 알투파 알파 처리)',
    '  vec3 finalColor = holoColor * scanline * flicker;',
    '  float alpha = (r + g + b) / 3.0 * scanline;',
    '  ',
    '  gl_FragColor = vec4(finalColor, alpha);',
    '}'
  ].join('\n'),

  // 유니폼 변수 매핑 함수
  applyUniforms: function (gl, loc, params) {
    gl.uniform1f(loc.uScale, params.scale.value);
    gl.uniform1f(loc.uGlitchAmount, params.glitchAmount.value);
    gl.uniform1f(loc.uScanlineCount, params.scanlineCount.value);
    gl.uniform1f(loc.uRGBShift, params.rgbShift.value);
  }
};
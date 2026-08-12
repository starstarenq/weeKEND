// 모드: 안개 — fbm 노이즈를 하늘색과 안개색 사이의 블렌딩 비율로 사용한다.
// 디졸브와 달리 discard 없이 smoothstep으로 부드럽게 섞기만 해서 뿌옇게 낀 느낌을 낸다.
// density는 안개가 덮이는 정도(임계값), softness는 경계가 얼마나 부드럽게 퍼지는지를 조절한다.
window.Effects = window.Effects || {};
window.Effects.fog = {
  id: 'fog',
  label: '안개',
  clearColor: [0, 0, 0, 1],
  uniformNames: ['uScale', 'uOctaves', 'uDensity', 'uSoftness', 'uSkyColor', 'uFogColor'],
  params: {
    scale: { value: 3.0, min: 0.5, max: 10, step: 0.1, label: 'Scale' },
    octaves: { value: 4, min: 1, max: 8, step: 1, label: 'Octaves' },
    speed: { value: 0.15, min: 0, max: 3, step: 0.1, label: 'Speed' },
    density: { value: 0.35, min: 0, max: 1, step: 0.01, label: 'Density' },
    softness: { value: 0.3, min: 0.02, max: 0.6, step: 0.01, label: 'Softness' },
  },
  fragmentShader: [
    'precision mediump float;',
    'varying vec2 vUv;',
    'uniform sampler2D uNoiseTexture;',
    'uniform float uTime;',
    'uniform float uScale;',
    'uniform int uOctaves;',
    'uniform float uDensity;',
    'uniform float uSoftness;',
    'uniform vec3 uSkyColor;',
    'uniform vec3 uFogColor;',
    '',
    'const int MAX_OCTAVES = 8;',
    '',
    'float sampleNoise(vec2 uv) {',
    '  return texture2D(uNoiseTexture, uv).r;',
    '}',
    '',
    'float fbm(vec2 uv) {',
    '  float value = 0.0;',
    '  float amplitude = 0.5;',
    '  float frequency = 1.0;',
    '  float maxValue = 0.0;',
    '  for (int i = 0; i < MAX_OCTAVES; i++) {',
    '    if (i >= uOctaves) break;',
    '    vec2 offset = vec2(float(i) * 13.7, float(i) * 7.3);',
    '    value += amplitude * sampleNoise(uv * frequency + offset);',
    '    maxValue += amplitude;',
    '    amplitude *= 0.5;',
    '    frequency *= 2.0;',
    '  }',
    '  return value / maxValue;',
    '}',
    '',
    'void main() {',
    '  vec2 uv = vUv * uScale + vec2(uTime * 0.04, uTime * 0.015);',
    '  float n = fbm(uv);',
    '  float t = smoothstep(uDensity - uSoftness, uDensity + uSoftness, n);',
    '  vec3 color = mix(uSkyColor, uFogColor, t);',
    '  gl_FragColor = vec4(color, 1.0);',
    '}',
  ].join('\n'),
  applyUniforms: function (gl, loc, params) {
    gl.uniform1f(loc.uScale, params.scale.value);
    gl.uniform1i(loc.uOctaves, Math.round(params.octaves.value));
    gl.uniform1f(loc.uDensity, params.density.value);
    gl.uniform1f(loc.uSoftness, params.softness.value);
    gl.uniform3fv(loc.uSkyColor, [0.53, 0.66, 0.78]);
    gl.uniform3fv(loc.uFogColor, [0.88, 0.90, 0.93]);
  },
};

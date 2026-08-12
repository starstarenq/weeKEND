// 모드: 디졸브 — fbm 노이즈 값을 임계치(dissolve)로 잘라
//   - 노이즈 < 임계치: discard (오브젝트가 사라진 부분)
//   - 임계치 근방(edgeWidth 폭): 단색 발광 엣지 컬러
//   - 그 이상: 오브젝트의 단색 베이스 컬러
// dissolve를 0→1로 올리면 노이즈가 낮은 영역부터 순서대로 타들어가듯 사라진다.
window.Effects = window.Effects || {};
window.Effects.dissolve = {
  id: 'dissolve',
  label: '디졸브',
  // discard로 그려지지 않는 픽셀이 생기므로 매 프레임 클리어가 필요하다.
  clearColor: [0, 0, 0, 1],
  uniformNames: ['uScale', 'uOctaves', 'uDissolve', 'uEdgeWidth', 'uBaseColor', 'uEdgeColor'],
  params: {
    scale: { value: 3.0, min: 0.5, max: 10, step: 0.1, label: 'Scale' },
    octaves: { value: 4, min: 1, max: 8, step: 1, label: 'Octaves' },
    speed: { value: 0.3, min: 0, max: 3, step: 0.1, label: 'Speed' },
    dissolve: { value: 0.5, min: 0, max: 1, step: 0.01, label: 'Dissolve' },
    edgeWidth: { value: 0.06, min: 0.01, max: 0.3, step: 0.01, label: 'Edge Width' },
  },
  fragmentShader: [
    'precision mediump float;',
    'varying vec2 vUv;',
    'uniform sampler2D uNoiseTexture;',
    'uniform float uTime;',
    'uniform float uScale;',
    'uniform int uOctaves;',
    'uniform float uDissolve;',
    'uniform float uEdgeWidth;',
    'uniform vec3 uBaseColor;',
    'uniform vec3 uEdgeColor;',
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
    '  vec2 uv = vUv * uScale + vec2(uTime * 0.05, uTime * 0.03);',
    '  float n = fbm(uv);',
    '',
    '  if (n < uDissolve) {',
    '    discard;',
    '  }',
    '',
    '  float edge = smoothstep(uDissolve, uDissolve + uEdgeWidth, n);',
    '  vec3 color = mix(uEdgeColor, uBaseColor, edge);',
    '  gl_FragColor = vec4(color, 1.0);',
    '}',
  ].join('\n'),
  applyUniforms: function (gl, loc, params) {
    gl.uniform1f(loc.uScale, params.scale.value);
    gl.uniform1i(loc.uOctaves, Math.round(params.octaves.value));
    gl.uniform1f(loc.uDissolve, params.dissolve.value);
    gl.uniform1f(loc.uEdgeWidth, params.edgeWidth.value);
    gl.uniform3fv(loc.uBaseColor, [0.05, 0.35, 0.55]);
    gl.uniform3fv(loc.uEdgeColor, [1.0, 0.45, 0.1]);
  },
};

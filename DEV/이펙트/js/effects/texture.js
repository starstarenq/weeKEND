// 모드: 단일 텍스처 — 노이즈 텍스처를 가공 없이 타일링해서 그대로 보여준다.
window.Effects = window.Effects || {};
window.Effects.texture = {
  id: 'texture',
  label: '단일 텍스처',
  clearColor: [0, 0, 0, 1],
  uniformNames: ['uScale'],
  params: {
    scale: { value: 1.0, min: 0.5, max: 10, step: 0.1, label: 'Scale' },
  },
  fragmentShader: [
    'precision mediump float;',
    'varying vec2 vUv;',
    'uniform sampler2D uNoiseTexture;',
    'uniform float uScale;',
    '',
    'void main() {',
    '  vec3 color = texture2D(uNoiseTexture, vUv * uScale).rgb;',
    '  gl_FragColor = vec4(color, 1.0);',
    '}',
  ].join('\n'),
  applyUniforms: function (gl, loc, params) {
    gl.uniform1f(loc.uScale, params.scale.value);
  },
};

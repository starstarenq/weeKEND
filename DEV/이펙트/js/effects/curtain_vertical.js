// js/effects/curtain_vertical.js
window.Effects = window.Effects || {};

window.Effects.curtain_vertical = {
  id: 'curtain_vertical',
  label: '상하 커튼 닫힘/열림',
  clearColor: [0.0, 0.0, 0.0, 1.0],
  // app.js가 location을 생성할 custom uniform 목록
  uniformNames: ['uScale', 'uProgress', 'uFoldDensity', 'uCurvedEdge'],

  params: {
    progress: { value: 0.5, min: 0.0, max: 1.0, step: 0.01, label: 'Curtain Open/Close' },
    foldDensity: { value: 15.0, min: 2.0, max: 40.0, step: 1.0, label: 'Vertical Folds' },
    curvedEdge: { value: 0.08, min: 0.0, max: 0.3, step: 0.01, label: 'Bottom Wave' },
    scale: { value: 2.0, min: 0.5, max: 5.0, step: 0.1, label: 'Texture Scale' }
  },

  fragmentShader: [
    'precision mediump float;',
    'varying vec2 vUv;',

    'uniform sampler2D uNoiseTexture;',
    'uniform float uTime;',
    'uniform float uScale;',
    'uniform float uProgress;',
    'uniform float uFoldDensity;',
    'uniform float uCurvedEdge;',

    'void main() {',
    '  vec2 uv = vUv;',
    '  ',
    '  // 1. 밑단 물결(Wave) 파동 계산',
    '  float noise = texture2D(uNoiseTexture, vec2(uv.x * 3.0, uTime * 0.2)).r;',
    '  float wave = sin(uv.x * uFoldDensity) * uCurvedEdge * (1.0 - uProgress) + (noise - 0.5) * 0.03;',
    '  ',
    '  // 2. 상하 경계선 계산 (Progress 0: 완전히 열림, 1: 완전히 닫힘)',
    '  float topCurtainBottom = (1.0 - uProgress) + wave;',
    '  ',
    '  // 커튼 영역 밖(열린 공간) 투명/어둡게 처리',
    '  if (uv.y < topCurtainBottom) {',
    '    gl_FragColor = vec4(0.0, 0.0, 0.0, 0.0);',
    '    return;',
    '  }',
    '  ',
    '  // 3. 커튼 말림 UV 재계산',
    '  vec2 curtainUv = uv;',
    '  float curtainHeight = max(1.0 - topCurtainBottom, 0.001);',
    '  curtainUv.y = (uv.y - topCurtainBottom) / curtainHeight;',
    '  ',
    '  // 4. 음영 및 질감 계산',
    '  float foldShadow = sin(curtainUv.x * uFoldDensity * 3.14159);',
    '  foldShadow = 0.5 + 0.5 * foldShadow;',
    '  ',
    '  float rollShadow = sin(curtainUv.y * 20.0);',
    '  rollShadow = 0.8 + 0.2 * rollShadow;',
    '  ',
    '  vec3 texColor = texture2D(uNoiseTexture, curtainUv * uScale).rgb;',
    '  vec3 baseColor = vec3(0.75, 0.12, 0.18);', // 붉은 커튼 색상
    '  ',
    '  vec3 finalColor = baseColor * (0.35 + 0.65 * foldShadow) * rollShadow * (0.8 + 0.2 * texColor.r);',
    '  ',
    '  // 밑단 하이라이트',
    '  float edgeDist = abs(uv.y - topCurtainBottom);',
    '  if (edgeDist < 0.012) {',
    '    finalColor += vec3(0.4, 0.3, 0.1) * (1.0 - edgeDist / 0.012);',
    '  }',
    '  ',
    '  gl_FragColor = vec4(finalColor, 1.0);',
    '}'
  ].join('\n'),

  // app.js의 render() 루프에서 바인딩해 주는 부분
  applyUniforms: function (gl, loc, params) {
    if (loc.uScale) gl.uniform1f(loc.uScale, params.scale.value);
    if (loc.uProgress) gl.uniform1f(loc.uProgress, params.progress.value);
    if (loc.uFoldDensity) gl.uniform1f(loc.uFoldDensity, params.foldDensity.value);
    if (loc.uCurvedEdge) gl.uniform1f(loc.uCurvedEdge, params.curvedEdge.value);
  }
};
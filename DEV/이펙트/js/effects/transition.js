// js/effects/transition.js
window.Effects = window.Effects || {};

window.Effects.transition = {
  id: 'transition',
  label: '디졸브 화면 전환',
  clearColor: [0.0, 0.0, 0.0, 1.0],
  uniformNames: ['uScale', 'uSpeed', 'uProgress', 'uEdgeWidth', 'uEdgeColor', 'uMode'],

  // UI 조작 파라미터
  params: {
    progress: { value: 0.0, min: 0.0, max: 1.0, step: 0.01, label: 'Transition Progress' }, // 0: Scene A, 1: Scene B
    edgeWidth: { value: 0.08, min: 0.0, max: 0.2, step: 0.005, label: 'Edge Thickness' },
    scale: { value: 3.0, min: 0.5, max: 10.0, step: 0.1, label: 'Noise Scale' },
    speed: { value: 0.5, min: 0.0, max: 2.0, step: 0.1, label: 'Auto Play Speed' },
    mode: { value: 0.0, min: 0.0, max: 1.0, step: 1.0, label: 'Auto Mode (0:Manual, 1:Auto)' }
  },

  // 프래그먼트 셰이더
  fragmentShader: [
    'precision mediump float;',
    'varying vec2 vUv;',

    'uniform sampler2D uNoiseTexture;',
    'uniform float uTime;',
    'uniform float uScale;',
    'uniform float uProgress;',
    'uniform float uSpeed;',
    'uniform float uEdgeWidth;',
    'uniform float uMode;',

    'void main() {',
    '  vec2 uv = vUv;',
    '  ',
    '  // 1. 자동 재생 모드 또는 수동 슬라이더 진행도 계산',
    '  float progress = uProgress;',
    '  if (uMode > 0.5) {',
    '    // 시간에 따라 0.0 ~ 1.0 사이를 핑퐁(왕복) 재생',
    '    progress = sin(uTime * uSpeed) * 0.5 + 0.5;',
    '  }',
    '  ',
    '  // 2. 노이즈 텍스처 참조 (화면 전환 패턴)',
    '  float noise = texture2D(uNoiseTexture, uv * uScale).r;',
    '  ',
    '  // 3. Scene A (이전 화면) 컬러 연출 - 푸른색 계열 쉐이딩/패턴',
    '  vec3 sceneA = vec3(0.1, 0.3, 0.6) * (0.6 + 0.4 * sin(uv.x * 10.0 + uTime));',
    '  ',
    '  // 4. Scene B (다음 화면) 컬러 연출 - 붉은/황금색 계열 쉐이딩/패턴',
    '  vec3 sceneB = vec3(0.8, 0.5, 0.2) * (0.6 + 0.4 * cos(uv.y * 10.0 - uTime));',
    '  ',
    '  // 5. 발광 엣지(Edge) 컬러 설정 (불꽃/에너지 느낌의 주황빛)',
    '  vec3 edgeColor = vec3(1.0, 0.5, 0.1);',
    '  ',
    '  // 6. 디졸브 경계면 및 엣지 임계값 계산',
    '  float threshold = progress;',
    '  float edgeThreshold = threshold + uEdgeWidth;',
    '  ',
    '  vec3 finalColor;',
    '  ',
    '  if (noise < threshold) {',
    '    // 노이즈 값이 진행도보다 작으면 Scene B(새로운 장면)로 전환 완료',
    '    finalColor = sceneB;',
    '  } else if (noise < edgeThreshold) {',
    '    // 경계선 영역: 타들어가는 발광 엣지 표시',
    '    float edgeFactor = (noise - threshold) / max(uEdgeWidth, 0.0001);',
    '    // 엣지와 Scene B 부드럽게 혼합',
    '    finalColor = mix(edgeColor * 2.0, sceneB, edgeFactor);',
    '  } else {',
    '    // 아직 전환되지 않은 Scene A(기존 장면)',
    '    finalColor = sceneA;',
    '  }',
    '  ',
    '  gl_FragColor = vec4(finalColor, 1.0);',
    '}'
  ].join('\n'),

  // 유니폼 변수 바인딩 함수
  applyUniforms: function (gl, loc, params) {
    if (loc.uScale) gl.uniform1f(loc.uScale, params.scale.value);
    if (loc.uSpeed) gl.uniform1f(loc.uSpeed, params.speed.value);
    if (loc.uProgress) gl.uniform1f(loc.uProgress, params.progress.value);
    if (loc.uEdgeWidth) gl.uniform1f(loc.uEdgeWidth, params.edgeWidth.value);
    if (loc.uMode) gl.uniform1f(loc.uMode, params.mode.value);
  }
};
// js/effects/curtain.js
window.Effects = window.Effects || {};

window.Effects.curtain = {
  id: 'curtain',
  label: '커튼 닫힘/열림',
  clearColor: [0.0, 0.0, 0.0, 1.0], // 검은색 배경
  uniformNames: ['uScale', 'uProgress', 'uFoldDensity', 'uCurvedEdge'],
  
  // UI 슬라이더 바인딩 파라미터
  params: {
    progress: { value: 0.5, min: 0.0, max: 1.0, step: 0.01, label: 'Curtain Open/Close' }, // 0: 완전히 열림, 1: 완전히 닫힘
    foldDensity: { value: 12.0, min: 2.0, max: 30.0, step: 1.0, label: 'Curtain Folds' }, // 커튼 주름 개수
    curvedEdge: { value: 0.15, min: 0.0, max: 0.5, step: 0.01, label: 'Wave Elasticity' }, // 밑단/경계 왜곡 정도
    scale: { value: 2.0, min: 0.5, max: 5.0, step: 0.1, label: 'Texture Scale' }
  },

  // WebGL 프래그먼트 셰이더
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
    '  // 1. 노이즈를 이용한 밑단 및 경계선 왜곡 (자연스러운 커튼 주름 파동)',
    '  float noise = texture2D(uNoiseTexture, vec2(uv.y * 2.0, uTime * 0.2)).r;',
    '  float wave = sin(uv.y * uFoldDensity) * uCurvedEdge * (1.0 - uProgress);',
    '  ',
    '  // 2. 화면 좌/우 양쪽에서 중앙으로 닫히는 커튼 경계 좌표 계산',
    '  // uProgress가 1일 때 중앙(0.5)에서 합쳐짐',
    '  float leftThreshold = uProgress * 0.5 + wave + (noise - 0.5) * 0.05;',
    '  float rightThreshold = 1.0 - (uProgress * 0.5 + wave + (noise - 0.5) * 0.05);',
    '  ',
    '  // 3. 커튼 영역 마스크 (좌측 커튼 OR 우측 커튼)',
    '  bool isLeftCurtain = uv.x < leftThreshold;',
    '  bool isRightCurtain = uv.x > rightThreshold;',
    '  ',
    '  if (!isLeftCurtain && !isRightCurtain) {',
    '    // 커튼이 열려있는 빈 공간 (투명 처리)',
    '    discard;',
    '  }',
    '  ',
    '  // 4. 커튼이 구겨지면서 압축되는 UV 좌표 재계산 (구겨짐 원근감 연출)',
    '  vec2 curtainUv = uv;',
    '  if (isLeftCurtain) {',
    '    curtainUv.x = uv.x / max(leftThreshold, 0.001);',
    '  } else {',
    '    curtainUv.x = (1.0 - uv.x) / max(1.0 - rightThreshold, 0.001);',
    '  }',
    '  ',
    '  // 5. 커튼 주름의 음영(Lighting/Shading) 및 질감 계산',
    '  float foldShadow = sin(curtainUv.x * uFoldDensity * 3.14159);',
    '  foldShadow = 0.5 + 0.5 * foldShadow; // 0.0 ~ 1.0 범위 보정',
    '  ',
    '  // 노이즈 텍스처로 실크/천 질감 입히기',
    '  vec3 texColor = texture2D(uNoiseTexture, curtainUv * uScale).rgb;',
    '  ',
    '  // 붉은 벨벳/실크 커튼 기본 색상',
    '  vec3 baseColor = vec3(0.8, 0.1, 0.15);',
    '  ',
    '  // 최종 색상 합성 (주름 음영 + 텍스처 명암 적용)',
    '  vec3 finalColor = baseColor * (0.4 + 0.6 * foldShadow) * (0.7 + 0.3 * texColor.r);',
    '  ',
    '  // 테두리 발광/하이라이트 (닫히는 순간 경계선 강조)',
    '  float edgeDist = min(abs(uv.x - leftThreshold), abs(uv.x - rightThreshold));',
    '  if (edgeDist < 0.01) {',
    '    finalColor += vec3(0.3, 0.2, 0.1) * (1.0 - edgeDist / 0.01);',
    '  }',
    '  ',
    '  gl_FragColor = vec4(finalColor, 1.0);',
    '}'
  ].join('\n'),

  // 유니폼 변수 매핑 함수
  applyUniforms: function (gl, loc, params) {
    gl.uniform1f(loc.uScale, params.scale.value);
    gl.uniform1f(loc.uProgress, params.progress.value);
    gl.uniform1f(loc.uFoldDensity, params.foldDensity.value);
    gl.uniform1f(loc.uCurvedEdge, params.curvedEdge.value);
  }
};
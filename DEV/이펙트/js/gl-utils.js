// 모든 이펙트가 공유하는 WebGL 보일러플레이트: 셰이더 컴파일, 프로그램 링크,
// 풀스크린 쿼드(화면 전체를 덮는 사각형) 생성, 공용 버텍스 셰이더.
window.GLUtils = (function () {
  // clip space -1~1 좌표를 그대로 통과시키고, 0~1 범위의 uv를 프래그먼트 셰이더로 넘긴다.
  const QUAD_VERTEX_SHADER = [
    'attribute vec2 aPosition;',
    'varying vec2 vUv;',
    '',
    'void main() {',
    '  vUv = aPosition * 0.5 + 0.5;',
    '  gl_Position = vec4(aPosition, 0.0, 1.0);',
    '}',
  ].join('\n');

  function compileShader(gl, type, source) {
    const shader = gl.createShader(type);
    gl.shaderSource(shader, source);
    gl.compileShader(shader);
    if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
      const info = gl.getShaderInfoLog(shader);
      gl.deleteShader(shader);
      throw new Error('셰이더 컴파일 실패: ' + info);
    }
    return shader;
  }

  function createProgram(gl, vsSource, fsSource) {
    const vs = compileShader(gl, gl.VERTEX_SHADER, vsSource);
    const fs = compileShader(gl, gl.FRAGMENT_SHADER, fsSource);
    const program = gl.createProgram();
    gl.attachShader(program, vs);
    gl.attachShader(program, fs);
    gl.linkProgram(program);
    if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
      const info = gl.getProgramInfoLog(program);
      throw new Error('프로그램 링크 실패: ' + info);
    }
    return program;
  }

  function createFullscreenQuad(gl) {
    const vertices = new Float32Array([
      -1, -1,
       1, -1,
      -1,  1,
       1,  1,
    ]);
    const buffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
    gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);
    return { buffer: buffer };
  }

  return {
    QUAD_VERTEX_SHADER: QUAD_VERTEX_SHADER,
    compileShader: compileShader,
    createProgram: createProgram,
    createFullscreenQuad: createFullscreenQuad,
  };
})();

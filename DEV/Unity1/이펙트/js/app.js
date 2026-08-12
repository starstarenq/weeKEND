// 진입점 컨트롤러: WebGL 컨텍스트/텍스처를 한 번만 준비하고,
// js/effects/*.js에 정의된 이펙트들 사이를 버튼으로 전환하며 렌더링한다.
(function () {
  const canvas = document.getElementById('glcanvas');
  // alpha: false — 이 노이즈 텍스처는 알파 채널이 항상 0이라, 알파를 페이지
  // 배경과 블렌딩하면 RGB 값과 무관하게 검게 보이는 문제가 있어 불투명하게 만든다.
  const contextOptions = { alpha: false };
  const gl = canvas.getContext('webgl', contextOptions) || canvas.getContext('experimental-webgl', contextOptions);

  if (!gl) {
    alert('WebGL을 사용할 수 없습니다.');
    return;
  }

  function resize() {
    const dpr = window.devicePixelRatio || 1;
    const w = Math.floor(canvas.clientWidth * dpr);
    const h = Math.floor(canvas.clientHeight * dpr);
    if (canvas.width !== w || canvas.height !== h) {
      canvas.width = w;
      canvas.height = h;
      gl.viewport(0, 0, w, h);
    }
  }
  window.addEventListener('resize', resize);
  resize();

  const quad = GLUtils.createFullscreenQuad(gl);

  let noiseTexture = null;
  try {
    const result = TGALoader.loadTGATextureFromBase64(gl, NOISE_TGA_BASE64);
    noiseTexture = result.texture;
    console.log('노이즈 텍스처 로드 완료:', result.width + 'x' + result.height);
  } catch (err) {
    console.error(err);
  }

  // 이펙트별로 프로그램을 한 번만 컴파일해서 재사용한다.
  const compiledCache = {};
  function getCompiled(effect) {
    if (compiledCache[effect.id]) return compiledCache[effect.id];
    const program = GLUtils.createProgram(gl, GLUtils.QUAD_VERTEX_SHADER, effect.fragmentShader);
    const aPosition = gl.getAttribLocation(program, 'aPosition');
    const uniformNames = ['uNoiseTexture', 'uTime'].concat(effect.uniformNames || []);
    const loc = {};
    uniformNames.forEach(function (name) {
      loc[name] = gl.getUniformLocation(program, name);
    });
    const entry = { program: program, aPosition: aPosition, loc: loc };
    compiledCache[effect.id] = entry;
    return entry;
  }

  // 현재 이펙트의 params 스키마로부터 슬라이더 UI를 자동 생성한다.
  const controlsEl = document.getElementById('controls');
  function buildControls(effect) {
    controlsEl.innerHTML = '';
    Object.keys(effect.params).forEach(function (key) {
      const p = effect.params[key];
      const label = document.createElement('label');

      const nameText = document.createTextNode(p.label + ' ');
      const valueSpan = document.createElement('span');
      const isInt = p.step >= 1;
      valueSpan.textContent = isInt ? String(p.value) : p.value.toFixed(2);

      const input = document.createElement('input');
      input.type = 'range';
      input.min = String(p.min);
      input.max = String(p.max);
      input.step = String(p.step);
      input.value = String(p.value);
      input.addEventListener('input', function () {
        const v = parseFloat(input.value);
        p.value = v;
        valueSpan.textContent = isInt ? String(v) : v.toFixed(2);
      });

      label.appendChild(nameText);
      label.appendChild(valueSpan);
      label.appendChild(input);
      controlsEl.appendChild(label);
    });
  }

  // 모드 버튼 바인딩
  let activeEffect = Effects.texture;
  const buttons = Array.prototype.slice.call(document.querySelectorAll('#modeButtons button'));
  function selectEffect(id) {
    const isSlash = id === 'slash';
    canvas.style.display = isSlash ? 'none' : 'block';
    buttons.forEach(function (b) {
      b.classList.toggle('active', b.getAttribute('data-effect') === id);
    });

    if (isSlash) {
      activeEffect = null;
      if (window.SlashScene) {
        window.SlashScene.activate();
        buildControls({ params: window.SlashScene.params });
      }
      return;
    }
    if (window.SlashScene) window.SlashScene.deactivate();
    activeEffect = Effects[id];
    buildControls(activeEffect);
  }
  buttons.forEach(function (btn) {
    btn.addEventListener('click', function () {
      selectEffect(btn.getAttribute('data-effect'));
    });
  });
  selectEffect(activeEffect.id);

  function render(effect, elapsedTime) {
    const c = getCompiled(effect);
    gl.useProgram(c.program);

    const clear = effect.clearColor || [0, 0, 0, 1];
    gl.clearColor(clear[0], clear[1], clear[2], clear[3]);
    gl.clear(gl.COLOR_BUFFER_BIT);

    gl.bindBuffer(gl.ARRAY_BUFFER, quad.buffer);
    gl.enableVertexAttribArray(c.aPosition);
    gl.vertexAttribPointer(c.aPosition, 2, gl.FLOAT, false, 0, 0);

    if (noiseTexture) {
      gl.activeTexture(gl.TEXTURE0);
      gl.bindTexture(gl.TEXTURE_2D, noiseTexture);
      gl.uniform1i(c.loc.uNoiseTexture, 0);
    }
    gl.uniform1f(c.loc.uTime, elapsedTime);
    effect.applyUniforms(gl, c.loc, effect.params);

    gl.drawArrays(gl.TRIANGLE_STRIP, 0, 4);
  }

  let time = 0;
  let lastNow = null;
  function frame(now) {
    if (lastNow === null) lastNow = now;
    const dt = (now - lastNow) / 1000;
    lastNow = now;
    if (activeEffect) {
      const speedParam = activeEffect.params.speed;
      time += dt * (speedParam ? speedParam.value : 0);
      resize();
      render(activeEffect, time);
    }
    requestAnimationFrame(frame);
  }
  requestAnimationFrame(frame);
})();

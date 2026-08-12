// 모드: Slash — WebGL 셰이더가 아니라 SVG로 그린 사이드뷰 캐릭터가 검을 휘두르는 장면이다.
// 검(#slashSword)과 이펙트 그룹(#slashFx)이 같은 pivot에서 매 프레임 동일한 회전각을
// 공유하기 때문에, Classic/ 검흔 스프라이트가 검의 궤적을 그대로 따라간다.
window.SlashScene = (function () {
  // 슬라이더로 조절하는 파라미터. duration은 검흔 이펙트를 포함한 베기 한 사이클의
  // 전체 재생 시간(ms)이며, 트리거 시점의 값을 그 회차 재생에 고정해 재생 도중
  // 슬라이더를 움직여도 각도가 튀지 않게 한다.
  var params = {
    duration: { value: 650, min: 200, max: 2000, step: 50, label: '지속 시간(ms)' },
  };
  var runDuration = params.duration.value; // 진행 중인 재생에 고정된 duration

  var arcFrames = [
    'Classic/1/Classic_01.png', 'Classic/1/Classic_02.png', 'Classic/1/Classic_03.png',
    'Classic/1/Classic_04.png', 'Classic/1/Classic_05.png', 'Classic/1/Classic_06.png',
  ];
  var sparkFrames = [
    'Classic/4/Classic_19.png', 'Classic/4/Classic_20.png', 'Classic/4/Classic_21.png',
    'Classic/4/Classic_22.png', 'Classic/4/Classic_23.png', 'Classic/4/Classic_24.png',
  ];

  // [progress%, value] 키프레임. swordKF/bodyKF의 구간 비율은 실제 베기처럼
  // 준비(anticipation)-스냅백-주 동작(main swing)-마무리(follow-through) 리듬을 따른다.
  var swordKF = [[0, -38], [18, -38], [24, -72], [31, -18], [40, 112], [48, 82], [72, 82], [100, -38]];
  var bodyKF = [[0, -2], [18, -2], [24, -14], [31, 18], [45, 2], [72, 2], [100, -2]];
  var arcOpKF = [[0, 0], [28, 0], [33, 1], [48, 1], [70, 0], [100, 0]];
  var sparkOpKF = [[0, 0], [35, 0], [39, 1], [45, 1], [52, 0], [100, 0]];
  var ARC_FRAME_WINDOW = [28, 55];
  var SPARK_FRAME_WINDOW = [35, 52];

  var stageEl, svgEl, bodyEl, swordEl, fxEl, arcImg, sparkImg;
  var ready = false;
  var rafId = null;
  var startTime = null;

  function lerpKF(kf, p) {
    for (var i = 0; i < kf.length - 1; i++) {
      var a = kf[i], b = kf[i + 1];
      if (p <= b[0]) {
        var span = b[0] - a[0];
        var t = span <= 0 ? 1 : (p - a[0]) / span;
        if (t < 0) t = 0;
        if (t > 1) t = 1;
        return a[1] + (b[1] - a[1]) * t;
      }
    }
    return kf[kf.length - 1][1];
  }

  function frameFor(images, windowRange, p) {
    var start = windowRange[0], end = windowRange[1];
    if (p <= start) return images[0];
    if (p >= end) return images[images.length - 1];
    var t = (p - start) / (end - start);
    var idx = Math.min(images.length - 1, Math.floor(t * images.length));
    return images[idx];
  }

  function setHref(imgEl, href) {
    imgEl.setAttribute('href', href);
    imgEl.setAttributeNS('http://www.w3.org/1999/xlink', 'href', href);
  }

  function setPose(p) {
    var swordAngle = lerpKF(swordKF, p);
    var bodyAngle = lerpKF(bodyKF, p);
    // 검과 이펙트가 같은 각도를 공유해야 "회전에 맞춰 이펙트가 따라온다"가 성립한다.
    var rotate = 'rotate(' + swordAngle.toFixed(2) + ')';
    swordEl.setAttribute('transform', rotate);
    fxEl.setAttribute('transform', rotate);
    bodyEl.setAttribute('transform', 'rotate(' + bodyAngle.toFixed(2) + ')');

    var arcOp = lerpKF(arcOpKF, p);
    var sparkOp = lerpKF(sparkOpKF, p);
    arcImg.setAttribute('opacity', arcOp.toFixed(2));
    sparkImg.setAttribute('opacity', sparkOp.toFixed(2));
    if (arcOp > 0.01) setHref(arcImg, frameFor(arcFrames, ARC_FRAME_WINDOW, p));
    if (sparkOp > 0.01) setHref(sparkImg, frameFor(sparkFrames, SPARK_FRAME_WINDOW, p));
  }

  function step(now) {
    var elapsed = now - startTime;
    var p = Math.min(100, (elapsed / runDuration) * 100);
    setPose(p);
    rafId = p < 100 ? requestAnimationFrame(step) : null;
  }

  function init() {
    if (ready) return;
    stageEl = document.getElementById('slashStage');
    svgEl = document.getElementById('slashSvg');
    bodyEl = document.getElementById('slashBody');
    swordEl = document.getElementById('slashSword');
    fxEl = document.getElementById('slashFx');
    arcImg = document.getElementById('slashFxArc');
    sparkImg = document.getElementById('slashFxSpark');
    svgEl.addEventListener('click', trigger);
    ready = true;
    setPose(0);
  }

  function trigger() {
    init();
    if (rafId) cancelAnimationFrame(rafId);
    runDuration = params.duration.value;
    startTime = performance.now();
    rafId = requestAnimationFrame(step);
  }

  function activate() {
    init();
    stageEl.classList.add('active');
    trigger();
  }

  function deactivate() {
    if (!ready) return;
    stageEl.classList.remove('active');
    if (rafId) {
      cancelAnimationFrame(rafId);
      rafId = null;
    }
  }

  return { activate: activate, deactivate: deactivate, trigger: trigger, params: params };
})();

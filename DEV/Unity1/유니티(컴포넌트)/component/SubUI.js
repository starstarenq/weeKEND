(() => {
  "use strict";
  const { UI } = window.GameComponents;
  const { UIBase } = UI;

  // 부모 UI(SceneUI 혹은 다른 UI) 안에 자리잡는 하위 패널/위젯.
  // 자체 <svg> 루트를 만들지 않고 부모의 좌표계 안에서 x/y로 배치된다.
  // (예: Telemetry 패널, Legend 패널처럼 화면 일부를 담당하는 재사용 가능한 UI 블록)
  class SubUI extends UIBase {
    constructor({ x, y, width, height, name = "" } = {}) {
      super({ x, y, width, height, name });
    }

    attachTo(parent) {
      parent.add(this);
      this.open();
      return this;
    }
  }

  UI.SubUI = SubUI;
})();

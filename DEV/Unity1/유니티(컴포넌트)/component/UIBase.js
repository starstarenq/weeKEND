(() => {
  "use strict";
  const { UI } = window.GameComponents;
  const { UIElement } = UI;

  // SceneUI / SubUI / PopupUI 가 공통으로 상속하는 "화면 단위" UI의 부모 클래스.
  // 실제 내용 구성은 하위 클래스가 init()을 오버라이드해서 채운다. init()은 최초 open() 시 한 번만 호출된다.
  class UIBase extends UIElement {
    constructor({ x = 0, y = 0, width = 0, height = 0, name = "" } = {}) {
      super({ x, y, name });
      this.width = width;
      this.height = height;
      this.opened = false;
      this._inited = false;
    }

    // 하위 클래스가 오버라이드해서 자식 컴포넌트(UIImage/UIText/UIButton 등)를 구성하는 훅.
    init() {}

    open() {
      if (!this._inited) {
        this._inited = true;
        this.init();
      }
      this.opened = true;
      this.show();
      this.onOpen();
      return this;
    }

    close() {
      this.opened = false;
      this.hide();
      this.onClose();
      return this;
    }

    onOpen() {}
    onClose() {}
  }

  UI.UIBase = UIBase;
})();

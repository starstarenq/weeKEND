(() => {
  "use strict";
  const { UI } = window.GameComponents;
  const { SVG_NS, UIBase } = UI;

  // 화면(패널) 하나를 통째로 소유하는 최상위 UI.
  // 자체 <svg> 루트를 DOM 컨테이너에 마운트하고 크기 변화를 추적하며,
  // 자식으로 SubUI들을 담고, PopupUI는 언제나 popupLayer(최상단)에 스택으로 쌓아 올린다.
  class SceneUI extends UIBase {
    constructor({ container, name = "" } = {}) {
      super({ name });
      this.container = typeof container === "string" ? document.querySelector(container) : container;
      if (!this.container) throw new Error(`SceneUI: container를 찾을 수 없습니다. (${container})`);

      this.svg = document.createElementNS(SVG_NS, "svg");
      this.svg.setAttribute("class", "scene-ui");
      this.svg.style.cssText = "display:block; width:100%; height:100%; overflow:visible;";
      this.svg.appendChild(this.node);

      this.popupLayer = document.createElementNS(SVG_NS, "g");
      this.popupLayer.setAttribute("data-name", "popup-layer");
      this.svg.appendChild(this.popupLayer);
      this.popupStack = [];

      this.viewWidth = 0;
      this.viewHeight = 0;

      this._resizeObserver = new ResizeObserver(() => this._syncViewBox());
    }

    mount() {
      this.container.appendChild(this.svg);
      this._resizeObserver.observe(this.svg);
      this._syncViewBox();
      this.open();
      return this;
    }

    _syncViewBox() {
      const width = this.svg.clientWidth;
      const height = this.svg.clientHeight;
      if (!width || !height) return;
      this.viewWidth = width;
      this.viewHeight = height;
      this.svg.setAttribute("viewBox", `0 0 ${width} ${height}`);
      this.onResize(width, height);
      this.popupStack.forEach((popup) => popup.reposition && popup.reposition());
    }

    // 하위 클래스가 오버라이드해서 크기 변화에 맞춰 레이아웃을 다시 잡는다.
    onResize() {}

    // PopupUI는 항상 popupLayer(최상단)에 붙여 SubUI들보다 위에 그려지도록 한다.
    openPopup(popup) {
      popup.parent = this;
      this.popupLayer.appendChild(popup.node);
      this.popupStack.push(popup);
      popup.open();
      return popup;
    }

    closePopup(popup) {
      popup.close();
      const index = this.popupStack.indexOf(popup);
      if (index !== -1) this.popupStack.splice(index, 1);
      if (popup.node.parentNode) popup.node.parentNode.removeChild(popup.node);
    }

    destroy() {
      this._resizeObserver.disconnect();
      super.destroy();
      if (this.svg.parentNode) this.svg.parentNode.removeChild(this.svg);
    }
  }

  UI.SceneUI = SceneUI;
})();

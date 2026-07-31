(() => {
  "use strict";
  const components = window.GameComponents = window.GameComponents || {};
  const UI = components.UI = components.UI || {};
  const SVG_NS = "http://www.w3.org/2000/svg";

  // UIImage / UIText / UIButton 과 UIBase(SceneUI, SubUI, PopupUI) 모두가 상속하는 최상위 부모.
  // svg <g> 하나를 소유하며 위치(transform)와 자식 목록만 관리하는 가벼운 노드다.
  class UIElement {
    constructor({ x = 0, y = 0, name = "" } = {}) {
      this.x = x;
      this.y = y;
      this.name = name;
      this.parent = null;
      this.children = [];
      this.active = true;
      this.node = document.createElementNS(SVG_NS, "g");
      if (name) this.node.setAttribute("data-name", name);
      this._applyTransform();
    }

    _applyTransform() {
      this.node.setAttribute("transform", `translate(${this.x}, ${this.y})`);
    }

    setPosition(x, y) {
      this.x = x;
      this.y = y;
      this._applyTransform();
      return this;
    }

    add(child) {
      this.children.push(child);
      child.parent = this;
      this.node.appendChild(child.node);
      return child;
    }

    remove(child) {
      const index = this.children.indexOf(child);
      if (index === -1) return;
      this.children.splice(index, 1);
      if (child.node.parentNode === this.node) this.node.removeChild(child.node);
      child.parent = null;
    }

    show() {
      this.active = true;
      this.node.removeAttribute("display");
      return this;
    }

    hide() {
      this.active = false;
      this.node.setAttribute("display", "none");
      return this;
    }

    update(dt) {
      this.children.forEach((child) => child.update(dt));
    }

    destroy() {
      this.children.slice().forEach((child) => child.destroy());
      this.children = [];
      if (this.node.parentNode) this.node.parentNode.removeChild(this.node);
      this.parent = null;
    }
  }

  UI.SVG_NS = SVG_NS;
  UI.UIElement = UIElement;
})();

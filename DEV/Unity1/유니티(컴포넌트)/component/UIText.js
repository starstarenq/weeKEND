(() => {
  "use strict";
  const { UI } = window.GameComponents;
  const { SVG_NS, UIElement } = UI;

  // svg <text> 기반의 텍스트 컴포넌트.
  class UIText extends UIElement {
    constructor({ x, y, text = "", fontSize = 12, fill = "#eef4ff", weight = 400, align = "start", letterSpacing = 0, uppercase = false, name } = {}) {
      super({ x, y, name });
      this.uppercase = uppercase;
      this.textNode = document.createElementNS(SVG_NS, "text");
      this.textNode.setAttribute("font-size", fontSize);
      this.textNode.setAttribute("fill", fill);
      this.textNode.setAttribute("font-weight", weight);
      this.textNode.setAttribute("text-anchor", align);
      if (letterSpacing) this.textNode.setAttribute("letter-spacing", letterSpacing);
      this.node.appendChild(this.textNode);
      this.setText(text);
    }

    setText(text) {
      this.textNode.textContent = this.uppercase ? String(text).toUpperCase() : text;
      return this;
    }

    setFill(color) {
      this.textNode.setAttribute("fill", color);
      return this;
    }
  }

  UI.UIText = UIText;
})();

(() => {
  "use strict";
  const { UI } = window.GameComponents;
  const { SVG_NS, UIElement } = UI;

  // svg 형태의 이미지 컴포넌트.
  // href가 주어지면 <image>를, 아니면 <rect>/<circle> 벡터 스와치를 그린다(아이콘/색상 점 용도).
  class UIImage extends UIElement {
    constructor({ x, y, width = 16, height = 16, shape = "rect", href = "", fill = "#69a8ff", radius = 0, name } = {}) {
      super({ x, y, name });
      this.width = width;
      this.height = height;
      this.shape = href ? "image" : shape;

      if (href) {
        this.graphic = document.createElementNS(SVG_NS, "image");
        this.graphic.setAttribute("href", href);
        this.graphic.setAttribute("width", width);
        this.graphic.setAttribute("height", height);
      } else if (this.shape === "circle") {
        this.graphic = document.createElementNS(SVG_NS, "circle");
        this.graphic.setAttribute("cx", width / 2);
        this.graphic.setAttribute("cy", height / 2);
        this.graphic.setAttribute("r", Math.min(width, height) / 2);
        this.graphic.setAttribute("fill", fill);
      } else {
        this.graphic = document.createElementNS(SVG_NS, "rect");
        this.graphic.setAttribute("width", width);
        this.graphic.setAttribute("height", height);
        this.graphic.setAttribute("rx", radius);
        this.graphic.setAttribute("ry", radius);
        this.graphic.setAttribute("fill", fill);
      }
      this.node.appendChild(this.graphic);
    }

    setFill(color) {
      if (this.shape !== "image") this.graphic.setAttribute("fill", color);
      return this;
    }

    setSize(width, height) {
      this.width = width;
      this.height = height;
      if (this.shape === "circle") {
        this.graphic.setAttribute("cx", width / 2);
        this.graphic.setAttribute("cy", height / 2);
        this.graphic.setAttribute("r", Math.min(width, height) / 2);
      } else {
        this.graphic.setAttribute("width", width);
        this.graphic.setAttribute("height", height);
      }
      return this;
    }
  }

  UI.UIImage = UIImage;
})();

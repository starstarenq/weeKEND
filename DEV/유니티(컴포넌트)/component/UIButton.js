(() => {
  "use strict";
  const { UI } = window.GameComponents;
  const { UIElement, UIImage, UIText } = UI;

  // 배경(UIImage) + 라벨(UIText) 조합으로 구성되는 버튼 컴포넌트.
  class UIButton extends UIElement {
    constructor({
      x, y, width = 160, height = 38, label = "",
      fill = "#17243a", hoverFill = "#203250", disabledFill = "#101a29",
      textFill = "#dce8f8", disabledTextFill = "#4c5c74",
      fontSize = 13, radius = 10, onClick = () => {}, name
    } = {}) {
      super({ x, y, name });
      this.width = width;
      this.height = height;
      this.fill = fill;
      this.hoverFill = hoverFill;
      this.disabledFill = disabledFill;
      this.textFill = textFill;
      this.disabledTextFill = disabledTextFill;
      this.onClickHandler = onClick;
      this.disabled = false;

      this.background = this.add(new UIImage({ width, height, fill, radius, shape: "rect" }));
      this.label = this.add(new UIText({
        x: width / 2,
        y: height / 2 + fontSize * .35,
        text: label,
        fontSize,
        fill: textFill,
        align: "middle"
      }));

      this.node.style.cursor = "pointer";
      this.node.addEventListener("pointerenter", () => this._applyState(this.disabled ? "disabled" : "hover"));
      this.node.addEventListener("pointerleave", () => this._applyState(this.disabled ? "disabled" : "idle"));
      this.node.addEventListener("click", (event) => {
        if (this.disabled) return;
        this.onClickHandler(event);
      });
      this._applyState("idle");
    }

    _applyState(state) {
      if (state === "disabled") {
        this.background.setFill(this.disabledFill);
        this.label.setFill(this.disabledTextFill);
      } else if (state === "hover") {
        this.background.setFill(this.hoverFill);
        this.label.setFill(this.textFill);
      } else {
        this.background.setFill(this.fill);
        this.label.setFill(this.textFill);
      }
    }

    setLabel(text) {
      this.label.setText(text);
      return this;
    }

    setDisabled(disabled) {
      this.disabled = disabled;
      this.node.style.cursor = disabled ? "default" : "pointer";
      this._applyState(disabled ? "disabled" : "idle");
      return this;
    }
  }

  UI.UIButton = UIButton;
})();

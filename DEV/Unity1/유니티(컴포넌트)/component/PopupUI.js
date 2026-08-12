(() => {
  "use strict";
  const { UI } = window.GameComponents;
  const { UIElement, UIImage, UIBase } = UI;

  // SceneUI 위에 모달 형태로 떠오르는 팝업.
  // 반투명 배경(dim)으로 뒤 UI를 가리고, 실제 내용은 화면 중앙에 배치되는 content 그룹에 채운다.
  // scene.openPopup()으로 열리며 Open()/Close() 생명주기를 SceneUI의 popupStack이 관리한다.
  class PopupUI extends UIBase {
    constructor({ scene, width, height, dim = true, closeOnBackdrop = true, name = "" } = {}) {
      super({ width, height, name });
      this.scene = scene;
      this.backdrop = null;

      if (dim) {
        this.backdrop = new UIImage({ width: scene.viewWidth, height: scene.viewHeight, shape: "rect", fill: "rgba(4, 9, 17, .6)" });
        this.add(this.backdrop);
        if (closeOnBackdrop) {
          this.backdrop.node.addEventListener("click", () => this.scene.closePopup(this));
        }
      }

      // 실제 다이얼로그 내용은 씬 정중앙에 배치되는 content 그룹 안에 채워 넣는다.
      this.content = new UIElement({ name: `${name || "popup"}-content` });
      this.content.setPosition((scene.viewWidth - width) / 2, (scene.viewHeight - height) / 2);
      this.add(this.content);
    }

    // 부모 SceneUI 크기가 바뀌었을 때 배경/위치를 다시 맞춘다.
    reposition() {
      if (this.backdrop) this.backdrop.setSize(this.scene.viewWidth, this.scene.viewHeight);
      this.content.setPosition((this.scene.viewWidth - this.width) / 2, (this.scene.viewHeight - this.height) / 2);
    }
  }

  UI.PopupUI = PopupUI;
})();

// component/ConfirmPopupUI.js (또는 index.html Script 내부)
(() => {
  "use strict";
  const { UI } = window.GameComponents;

  class ConfirmPopupUI extends UI.PopupUI {
    /**
     * @param {Object} options
     * @param {SceneUI} options.scene - 팝업이 띄워질 최상위 SceneUI
     * @param {Function} options.onConfirm - '예' 선택 시 실행할 콜백
     * @param {Function} [options.onCancel] - '아니요' 선택 시 실행할 콜백
     */
    constructor({ scene, onConfirm, onCancel }) {
      super({ 
        scene, 
        width: 320, 
        height: 180, 
        dim: true, 
        closeOnBackdrop: false, // 배경 클릭으로 실수로 닫히는 것 방지
        name: "confirm-reset-popup" 
      });

      const width = 320;
      const height = 180;

      // 1. 팝업 배경 카드
      this.content.add(new UI.UIImage({
        width: width,
        height: height,
        fill: "#0f172a",
        radius: 16
      }));

      // 2. 질문 문구
      this.content.add(new UI.UIText({
        x: width / 2,
        y: 65,
        text: "초기화하시겠습니까?",
        fontSize: 18,
        fill: "#f8fafc",
        weight: 700,
        align: "middle"
      }));

      // 3. 버튼 영역: '아니요' (취소 / 팝업 닫기)
      this.content.add(new UI.UIButton({
        x: 24,
        y: 110,
        width: 128,
        height: 42,
        label: "아니요",
        fill: "#17243a",
        hoverFill: "#203250",
        onClick: () => {
          this.scene.closePopup(this);
          if (typeof onCancel === "function") onCancel();
        }
      }));

      // 4. 버튼 영역: '예' (초기화 실행 & 팝업 닫기)
      this.content.add(new UI.UIButton({
        x: 168,
        y: 110,
        width: 128,
        height: 42,
        label: "예",
        fill: "#315fd7",
        hoverFill: "#4272f5",
        onClick: () => {
          this.scene.closePopup(this);
          if (typeof onConfirm === "function") onConfirm();
        }
      }));
    }
  }

  UI.ConfirmPopupUI = ConfirmPopupUI;
})();
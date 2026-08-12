// component/SkillUI.js
(() => {
  "use strict";

  const { UI } = window.GameComponents;

  /**
   * 헤더 영역에 배치되는 Q 스킬 정보 및 쿨타임 컴포넌트
   */
  class SkillUI extends UI.SubUI {
    /**
     * @param {Object} options
     * @param {number} [options.x=0] - UI의 X 위치
     * @param {number} [options.y=0] - UI의 Y 위치
     * @param {number} [options.width=200] - 패널 너비
     * @param {number} [options.height=54] - 패널 높이
     * @param {string} [options.name="SkillWidget"] - 컴포넌트 이름
     */
    constructor({ x = 0, y = 0, width = 200, height = 54, name = "SkillWidget" } = {}) {
      super({ x, y, width, height, name });
    }

    init() {
      // 1. 메인 사각형 배경 카드
      this.bg = this.add(
        new UI.UIImage({
          width: this.width,
          height: this.height,
          fill: "rgba(12, 20, 34, 0.95)",
          radius: 12,
          name: "skill-bg"
        })
      );

      // 테두리 강조용 스트로크 라인 (SVG Rect)
      const border = document.createElementNS("http://www.w3.org/2000/svg", "rect");
      border.setAttribute("width", this.width);
      border.setAttribute("height", this.height);
      border.setAttribute("rx", "12");
      border.setAttribute("ry", "12");
      border.setAttribute("fill", "none");
      border.setAttribute("stroke", "#2d4265");
      border.setAttribute("stroke-width", "1");
      this.node.appendChild(border);

      // 2. 스킬 배지 (Q 아이콘 느낌의 사각형)
      this.iconBg = this.add(
        new UI.UIImage({
          x: 10,
          y: 10,
          width: 34,
          height: 34,
          fill: "#1e1b4b",
          radius: 8,
          name: "skill-icon-bg"
        })
      );

      this.iconText = this.add(
        new UI.UIText({
          x: 27,
          y: 31,
          text: "Q",
          fontSize: 16,
          fill: "#a855f7",
          weight: 700,
          align: "middle"
        })
      );

      // 3. 스킬 설명 타이틀 & 서브 설명
      this.title = this.add(
        new UI.UIText({
          x: 52,
          y: 22,
          text: "정지 탄환",
          fontSize: 12,
          fill: "#eef4ff",
          weight: 600
        })
      );

      this.desc = this.add(
        new UI.UIText({
          x: 52,
          y: 36,
          text: "READY",
          fontSize: 11,
          fill: "#a855f7",
          weight: 500
        })
      );

      // 4. 하단 쿨타임 진행 게이지 바
      this.gauge = this.add(
        new UI.UIImage({
          x: 10,
          y: this.height - 6,
          width: this.width - 20,
          height: 3,
          fill: "#a855f7",
          radius: 2,
          name: "skill-gauge"
        })
      );
    }

    /**
     * 스킬 쿨타임 상태 업데이트
     * @param {number} current - 현재 남은 쿨타임 (초)
     * @param {number} max - 전체 쿨타임 (초)
     */
    updateCooldown(current, max) {
      const fullWidth = this.width - 20;

      if (current <= 0) {
        // 스킬 사용 가능 상태
        this.desc.setText("READY").setFill("#a855f7");
        this.iconBg.setFill("#1e1b4b");
        this.iconText.setFill("#a855f7");
        this.gauge.setSize(fullWidth, 3).setFill("#a855f7");
      } else {
        // 쿨타임 진행 중
        const ratio = Math.max(0, 1 - current / max);
        this.desc.setText(`쿨타임: ${current.toFixed(1)}s`).setFill("#8fa1bd");
        this.iconBg.setFill("#17243a");
        this.iconText.setFill("#6b21a8");
        this.gauge.setSize(fullWidth * ratio, 3).setFill("#6b21a8");
      }
    }
  }

  UI.SkillUI = SkillUI;
})();
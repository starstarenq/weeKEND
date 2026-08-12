// monster.js
(() => {
  "use strict";

  class Monster extends window.GameComponents.PhysicsBody {
    constructor({ x, y }) {
      super({ x, y, radius: 18, mass: 1.5, drag: 2.0 });
      this.speed = 120;
      this.stasisTimer = 0;
    }

    applyStasis(duration = 3.0) {
      this.stasisTimer = duration;
      this.vx = 0;
      this.vy = 0;
    }

    // 충돌 시 호출되는 콜백 추가 (에러 방지 및 반사 작용)
    onCollision(normal) {
      if (normal) {
        this.vx += normal.x * 50;
        this.vy += normal.y * 50;
      }
    }

    update(dt, player, bounds) {
      if (this.stasisTimer > 0) {
        this.stasisTimer = Math.max(0, this.stasisTimer - dt);
        this.vx = 0;
        this.vy = 0;
        this.integrate(dt, bounds);
        return;
      }

      if (player) {
        const dx = player.x - this.x;
        const dy = player.y - this.y;
        const dist = Math.hypot(dx, dy) || 1;
        this.vx = (dx / dist) * this.speed;
        this.vy = (dy / dist) * this.speed;
      }

      this.integrate(dt, bounds);
    }

    render(ctx) {
      const isStunned = this.stasisTimer > 0;
      const color = isStunned ? "#c084fc" : "#ff6d78";
      const darkColor = isStunned ? "#581c87" : "#9f1239";

      window.GameComponents.rendering.drawEntity(ctx, this, {
        color,
        darkColor,
        label: isStunned ? `STASIS ${this.stasisTimer.toFixed(1)}s` : "Monster",
        eyeColor: isStunned ? "#a855f7" : "#ffffff"
      });
    }
  }

  window.Monster = Monster;
})();
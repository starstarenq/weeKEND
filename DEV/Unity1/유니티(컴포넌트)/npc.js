// npc.js
(() => {
  "use strict";

  class Npc extends window.GameComponents.PhysicsBody {
    constructor({ x, y, waypoints = [] }) {
      super({ x, y, radius: 16, mass: 1.0, drag: 3.0 });
      this.waypoints = waypoints;
      this.currentWaypointIndex = 0;
      this.speed = 80;
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
        this.vx += normal.x * 30;
        this.vy += normal.y * 30;
      }
    }

    update(dt, bounds) {
      if (this.stasisTimer > 0) {
        this.stasisTimer = Math.max(0, this.stasisTimer - dt);
        this.vx = 0;
        this.vy = 0;
        this.integrate(dt, bounds);
        return;
      }

      if (this.waypoints.length > 0) {
        const target = this.waypoints[this.currentWaypointIndex];
        const dx = target.x - this.x;
        const dy = target.y - this.y;
        const dist = Math.hypot(dx, dy);

        if (dist < 10) {
          this.currentWaypointIndex = (this.currentWaypointIndex + 1) % this.waypoints.length;
        } else {
          this.vx = (dx / dist) * this.speed;
          this.vy = (dy / dist) * this.speed;
        }
      }

      this.integrate(dt, bounds);
    }

    render(ctx) {
      const isStunned = this.stasisTimer > 0;
      const color = isStunned ? "#c084fc" : "#4ade80";
      const darkColor = isStunned ? "#581c87" : "#15803d";

      window.GameComponents.rendering.drawEntity(ctx, this, {
        color,
        darkColor,
        label: isStunned ? `STASIS ${this.stasisTimer.toFixed(1)}s` : "NPC",
        eyeColor: isStunned ? "#a855f7" : "#ffffff"
      });
    }
  }

  window.Npc = Npc;
})();
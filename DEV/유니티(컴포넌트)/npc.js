(() => {
  "use strict";

  const { PhysicsBody, Animator, rendering } = window.GameComponents;

  class Npc extends PhysicsBody {
    constructor({ x, y, waypoints = [] } = {}) {
      super({
        x: x ?? 100,
        y: y ?? 100,
        radius: 18,
        mass: 1,
        drag: 3,
        restitution: 0.75
      });

      this.animator = new Animator({ bobSpeed: 6, bobAmount: 4 });

      // 지정된 패트롤 경로(없으면 기본 사각형 경로)
      this.waypoints = waypoints.length > 0 ? waypoints : [
        { x: 100, y: 100 },
        { x: 500, y: 100 },
        { x: 500, y: 400 },
        { x: 100, y: 400 }
      ];

      this.currentWaypointIndex = 0;
      this.moveSpeed = 700;     // 이동 힘 (Force)
      this.maxSpeed = 110;      // 최대 속도
      this.reachDistance = 18;  // 목표 지점 도착 인정 거리
    }

    update(dt, bounds) {
      if (this.waypoints.length > 0) {
        const target = this.waypoints[this.currentWaypointIndex];

        const dx = target.x - this.x;
        const dy = target.y - this.y;
        const distance = Math.hypot(dx, dy);

        // 도착 시 다음 웨이포인트로 변경
        if (distance < this.reachDistance) {
          this.currentWaypointIndex = (this.currentWaypointIndex + 1) % this.waypoints.length;
        } else {
          // 목표 지점을 향해 이동 힘 적용
          const dirX = dx / distance;
          const dirY = dy / distance;
          this.applyForce(dirX * this.moveSpeed, dirY * this.moveSpeed, dt);
        }
      }

      // 물리 업데이트 및 화면 경계 제한
      this.limitSpeed(this.maxSpeed);
      this.integrate(dt, bounds);

      // 애니메이션 업데이트
      this.animator.update(this, dt);
    }

    onCollision(normal) {
      this.flash = 1;
    }

    render(ctx) {
      rendering.drawEntity(ctx, this, {
        color: "#4ade80",      // 녹색 계열
        darkColor: "#15803d",  // 어두운 녹색
        label: "NPC (Patrol)",
        eyeColor: "#ffffff"
      });
    }
  }

  window.Npc = Npc;
})();
(() => {
  "use strict";
  const { PhysicsBody, Animator, rendering } = window.GameComponents;

  class Monster extends PhysicsBody {
    constructor({ x, y }) {
      super({ x, y, radius: 27, mass: 1.8, drag: 4.2, restitution: .68 });
      this.acceleration = 610;
      this.maxSpeed = 165;
      this.animator = new Animator({ bobSpeed: 4.2, bobAmount: 4 });
      this.phase = Math.random() * Math.PI * 2;
    }

    update(dt, target, bounds) {
      this.phase += dt;
      const dx = target.x - this.x;
      const dy = target.y - this.y;
      const distance = Math.hypot(dx, dy) || 1;
      const orbit = Math.sin(this.phase * 1.7) * .42;
      const directionX = dx / distance - dy / distance * orbit;
      const directionY = dy / distance + dx / distance * orbit;
      const pursuit = distance > 115 ? 1 : -.48;
      this.applyForce(directionX * this.acceleration * pursuit, directionY * this.acceleration * pursuit, dt);
      this.limitSpeed(this.maxSpeed);
      this.integrate(dt, bounds);
      this.animator.update(this, dt);
    }

    onCollision() {
      this.flash = 1;
      this.phase += .8;
    }

    render(ctx) {
      rendering.drawEntity(ctx, this, {
        color: "#ff7883",
        darkColor: "#b92f53",
        label: "MONSTER",
        eyeColor: "#ffe36d"
      });
    }
  }

  window.Monster = Monster;
})();

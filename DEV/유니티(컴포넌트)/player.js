(() => {
  "use strict";
  const { PhysicsBody, Animator, rendering } = window.GameComponents;

  class Player extends PhysicsBody {
    constructor({ x, y }) {
      super({ x, y, radius: 22, mass: 1, drag: 7, restitution: .72 });
      this.acceleration = 1150;
      this.maxSpeed = 255;
      this.animator = new Animator({ bobSpeed: 7, bobAmount: 3 });
      this.dashCooldown = 0;
    }

    update(dt, keys, bounds) {
      let x = (keys.has("KeyD") || keys.has("ArrowRight") ? 1 : 0)
        - (keys.has("KeyA") || keys.has("ArrowLeft") ? 1 : 0);
      let y = (keys.has("KeyS") || keys.has("ArrowDown") ? 1 : 0)
        - (keys.has("KeyW") || keys.has("ArrowUp") ? 1 : 0);
      const length = Math.hypot(x, y) || 1;
      x /= length;
      y /= length;
      this.applyForce(x * this.acceleration, y * this.acceleration, dt);

      this.dashCooldown = Math.max(0, this.dashCooldown - dt);
      if (keys.has("Space") && this.dashCooldown === 0 && (x || y)) {
        this.vx += x * 240;
        this.vy += y * 240;
        this.flash = 1;
        this.dashCooldown = .65;
      }
      this.limitSpeed(this.maxSpeed + (this.dashCooldown > .48 ? 180 : 0));
      this.integrate(dt, bounds);
      this.animator.update(this, dt);
    }

    onCollision() {
      this.flash = 1;
    }

    render(ctx) {
      rendering.drawEntity(ctx, this, {
        color: "#72b4ff",
        darkColor: "#315fd7",
        label: "PLAYER"
      });
    }
  }

  window.Player = Player;
})();

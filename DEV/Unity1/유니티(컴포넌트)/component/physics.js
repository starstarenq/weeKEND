(() => {
  "use strict";
  const components = window.GameComponents = window.GameComponents || {};

  class PhysicsBody {
    constructor({ x, y, radius, mass = 1, drag = 5, restitution = .75 }) {
      Object.assign(this, { x, y, radius, mass, drag, restitution });
      this.vx = 0;
      this.vy = 0;
      this.rotation = 0;
      this.targetRotation = 0;
      this.bobOffset = 0;
      this.flash = 0;
      this.trail = [];
      this.trailTimer = 0;
    }

    applyForce(x, y, dt) {
      this.vx += x / this.mass * dt;
      this.vy += y / this.mass * dt;
    }

    limitSpeed(maxSpeed) {
      const speed = Math.hypot(this.vx, this.vy);
      if (speed > maxSpeed) {
        this.vx = this.vx / speed * maxSpeed;
        this.vy = this.vy / speed * maxSpeed;
      }
    }

    integrate(dt, bounds) {
      const damping = Math.exp(-this.drag * dt);
      this.vx *= damping;
      this.vy *= damping;
      this.x += this.vx * dt;
      this.y += this.vy * dt;
      if (Math.hypot(this.vx, this.vy) > 2) {
        this.targetRotation = Math.atan2(this.vy, this.vx);
      }
      components.collision.clampToBounds(this, bounds);
    }
  }

  components.PhysicsBody = PhysicsBody;
})();

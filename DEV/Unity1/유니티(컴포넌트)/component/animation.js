(() => {
  "use strict";
  const components = window.GameComponents = window.GameComponents || {};

  class Animator {
    constructor({ bobSpeed = 5, bobAmount = 3 } = {}) {
      this.time = Math.random() * 10;
      this.bobSpeed = bobSpeed;
      this.bobAmount = bobAmount;
    }

    update(entity, dt) {
      this.time += dt;
      const movement = Math.min(1, Math.hypot(entity.vx, entity.vy) / 120);
      entity.bobOffset = Math.sin(this.time * this.bobSpeed) * this.bobAmount * (.35 + movement);
      entity.rotation += (entity.targetRotation - entity.rotation) * Math.min(1, dt * 10);
      entity.flash = Math.max(0, entity.flash - dt * 4);
      entity.trailTimer -= dt;
      if (entity.trailTimer <= 0 && movement > .15) {
        entity.trail.push({ x: entity.x, y: entity.y, radius: entity.radius * .42 });
        if (entity.trail.length > 8) entity.trail.shift();
        entity.trailTimer = .035;
      }
      if (movement <= .15 && entity.trail.length) entity.trail.shift();
    }
  }

  components.Animator = Animator;
})();

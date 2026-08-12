// skill.js
(() => {
  "use strict";

  class StasisBullet extends window.GameComponents.PhysicsBody {
    constructor({ x, y }) {
      super({
        x,
        y,
        radius: 12,
        mass: 9999,
        drag: 10,
        restitution: 0.2
      });
      this.duration = 1.0; // 탄환 자체의 존속 시간
      this.isExpired = false;
      this.hitEntities = new Set(); // 이미 맞은 대상 중복 적용 방지
    }

    update(dt, bounds) {
      this.duration -= dt;
      if (this.duration <= 0) {
        this.isExpired = true;
      }
      this.vx = 0;
      this.vy = 0;
      this.integrate(dt, bounds);
    }

    render(ctx) {
      window.GameComponents.rendering.drawEntity(ctx, this, {
        color: "#a855f7",
        darkColor: "#6b21a8",
        label: `${Math.max(0, this.duration).toFixed(1)}s`,
        eyeColor: "#f3e8ff"
      });
    }
  }

  class SkillManager {
    constructor({ cooldown = 3.0 } = {}) {
      this.cooldown = cooldown;
      this.currentCooldown = 0;
      this.bullets = [];
    }

    useSkill(player) {
      if (this.currentCooldown > 0) return false;

      const bullet = new StasisBullet({ x: player.x, y: player.y });
      this.bullets.push(bullet);

      this.currentCooldown = this.cooldown;
      return true;
    }

    update(dt, bounds, entities = []) {
      this.currentCooldown = Math.max(0, this.currentCooldown - dt);

      for (let i = this.bullets.length - 1; i >= 0; i--) {
        const bullet = this.bullets[i];
        bullet.update(dt, bounds);

        entities.forEach(entity => {
          // 플레이어(Player)가 아니고 탄환 자신도 아닌 경우(Monster, Npc)만 스탯시스 적용
          const isPlayer = entity && entity.constructor && entity.constructor.name === "Player";

          if (entity && entity !== bullet && !isPlayer) {
            const collisionModule = window.GameComponents.collision;
            if (collisionModule && collisionModule.resolveCircleCollision) {
              const contact = collisionModule.resolveCircleCollision(bullet, entity, 0.5);

              if (contact.collided) {
                // 대상에게 3초간 멈춤 효과(stasis) 부여
                if (typeof entity.applyStasis === "function" && !bullet.hitEntities.has(entity)) {
                  entity.applyStasis(3.0);
                  bullet.hitEntities.add(entity); // 한 탄환에 다중 중복 적중 방지
                }

                if (entity.onCollision) {
                  entity.onCollision(contact.normal);
                }
              }
            }
          }
        });

        if (bullet.isExpired) {
          this.bullets.splice(i, 1);
        }
      }
    }

    render(ctx) {
      this.bullets.forEach(bullet => bullet.render(ctx));
    }
  }

  window.SkillManager = SkillManager;
})();
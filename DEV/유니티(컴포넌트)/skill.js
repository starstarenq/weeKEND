(() => {
  "use strict";

  const { PhysicsBody, rendering } = window.GameComponents;

  // 1초 동안 유지되는 고정 탄환 클래스
  class StasisBullet extends PhysicsBody {
    constructor({ x, y }) {
      super({
        x,
        y,
        radius: 12,
        mass: 9999, // 매우 무겁게 설정하여 다른 밀침 효과 최소화
        drag: 10,
        restitution: 0.2
      });
      this.duration = 1.0; // 1초 지속
      this.isExpired = false;
      this.flash = 1;
    }

    update(dt, bounds) {
      this.duration -= dt;
      if (this.duration <= 0) {
        this.isExpired = true;
      }
      // 멈춰있도록 속도 고정
      this.vx = 0;
      this.vy = 0;
      this.integrate(dt, bounds);
    }

    render(ctx) {
      rendering.drawEntity(ctx, this, {
        color: "#a855f7",       // 보라색 계열
        darkColor: "#6b21a8",
        label: `${Math.max(0, this.duration).toFixed(1)}s`,
        eyeColor: "#f3e8ff"
      });
    }
  }

  // 스킬 상태 및 발사 관리자 클래스
  class SkillManager {
    constructor({ cooldown = 3.0 } = {}) {
      this.cooldown = cooldown;      // 쿨타임 (3초)
      this.currentCooldown = 0;      // 남은 쿨타임
      this.bullets = [];            // 현재 필드에 존재하는 탄환들
    }

    // Q 키 입력을 받아 탄환 발사
    useSkill(player) {
      if (this.currentCooldown > 0) return false; // 쿨타임 중이면 사용 불가

      // 플레이어 위치에 1초 동안 멈추는 탄환 생성
      const bullet = new StasisBullet({ x: player.x, y: player.y });
      this.bullets.push(bullet);

      // 쿨타임 재설정
      this.currentCooldown = this.cooldown;
      return true;
    }

    update(dt, bounds, entities = []) {
      // 쿨타임 감소
      this.currentCooldown = Math.max(0, this.currentCooldown - dt);

      // 탄환 업데이트 및 만료된 탄환 제거
      for (let i = this.bullets.length - 1; i >= 0; i--) {
        const bullet = this.bullets[i];
        bullet.update(dt, bounds);

        // 몬스터 / NPC / 플레이어 등 다른 객체와 충돌 체크
        entities.forEach(entity => {
          if (entity && entity !== bullet) {
            const contact = window.GameComponents.collision.resolveCircleCollision(bullet, entity, 0.5);
            if (contact.collided && entity.onCollision) {
              entity.onCollision(contact.normal);
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
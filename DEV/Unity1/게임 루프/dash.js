// dash.js
class DashManager {
    constructor() {
        this.dashSpeedMultiplier = 2.5; // 대시 중 이동 속도 배율
        this.dashDuration = 200;       // 대시 지속 시간 (ms, 0.2초)
        this.cooldownTime = 3000;      // 대시 쿨타임 (ms, 3초)
        
        this.isDashing = false;
        this.isOnCooldown = false;
        this.lastDashTime = 0;
    }

    // 대시 시도
    triggerDash(currentTime) {
        if (this.isDashing || this.isOnCooldown) return false;

        this.isDashing = true;
        this.isOnCooldown = true;
        this.lastDashTime = currentTime;

        // 대시 지속 시간 종료 처리
        setTimeout(() => {
            this.isDashing = false;
        }, this.dashDuration);

        // 쿨타임 종료 처리
        setTimeout(() => {
            this.isOnCooldown = false;
        }, this.cooldownTime);

        return true;
    }

    // 대시 중인지 (무적 판정에 활용)
    isInvincible() {
        return this.isDashing;
    }

    // 대시 이동 속도 적용
    getSpeedModifier() {
        return this.isDashing ? this.dashSpeedMultiplier : 1.0;
    }

    // 남은 쿨다운 시간(초) 계산
    getRemainingCooldown(currentTime) {
        if (!this.isOnCooldown) return 0;
        const elapsed = currentTime - this.lastDashTime;
        return Math.max(0, ((this.cooldownTime - elapsed) / 1000).toFixed(1));
    }

    reset() {
        this.isDashing = false;
        this.isOnCooldown = false;
        this.lastDashTime = 0;
    }
}

window.dashManager = new DashManager();
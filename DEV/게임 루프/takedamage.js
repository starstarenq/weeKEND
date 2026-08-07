// takedamage.js
class TakeDamageManager {
    constructor() {
        this.invincibleDuration = 1000; // 피격 무적 시간 (1초)
        this.isInvincible = false;
        this.lastHitTime = 0;
        this.blinkInterval = 100;        // 깜빡임 주기 (0.1초 간격)
    }

    // 피격 시 무적 처리 시작
    triggerHit(currentTime) {
        this.isInvincible = true;
        this.lastHitTime = currentTime;

        // 1초 후 무적 해제
        setTimeout(() => {
            this.isInvincible = false;
        }, this.invincibleDuration);
    }

    // 현재 피격 무적 상태인지 확인
    checkInvincible() {
        return this.isInvincible;
    }

    // 깜빡임 효과를 위한 가시성 체크
    isVisible(currentTime) {
        if (!this.isInvincible) return true;
        const elapsed = currentTime - this.lastHitTime;
        // 지정된 주기마다 반전(true/false)시켜 깜빡이게 만듦
        return Math.floor(elapsed / this.blinkInterval) % 2 === 0;
    }

    reset() {
        this.isInvincible = false;
        this.lastHitTime = 0;
    }
}

window.takeDamageManager = new TakeDamageManager();
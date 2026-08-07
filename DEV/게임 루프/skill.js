// skill.js
class SkillManager {
    constructor(maxUltimateCount = 3) {
        this.maxUltimateCount = maxUltimateCount;
        this.remainingUltimateCount = maxUltimateCount;
    }

    // 스테이지 시작 시 사용 횟수 리셋
    reset() {
        this.remainingUltimateCount = this.maxUltimateCount;
    }

    // 궁극기 사용 가능 여부 확인 및 사용
    useUltimate() {
        if (this.remainingUltimateCount > 0) {
            this.remainingUltimateCount--;
            return true;
        }
        return false;
    }

    getRemainingCount() {
        return this.remainingUltimateCount;
    }
}

window.skillManager = new SkillManager(3);
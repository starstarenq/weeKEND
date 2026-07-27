/**
 * EmotionSystem 클래스: 감정 게이지 및 신족 간섭 매커니즘 연산
 */
export class EmotionSystem {
    constructor(initialValue = 50) {
        this.value = initialValue; // 0(불행) ~ 100(행복)
        this.ocyusisPrank = false;
        this.cosmoBless = false;
        this.cosmoEl = document.getElementById('cosmo-status');
        this.ocyusisEl = document.getElementById('ocyusis-status');
    }

    adjust(amount) {
        this.value = Math.max(10, Math.min(90, this.value + amount));
    }

    getDifficultyModifier() {
        if (this.value < 30) return 1.2;  // 불행 상태: 난이도 상승(피격 대미지 증가)
        if (this.value > 70) return 0.8;  // 행복 상태: 난이도 하락(피격 대미지 감소)
        return 1.0;
    }

    triggerDivineIntervention(gameManager) {
        // 불행할수록 악마 오큐시스의 장난(몬스터 공격력 30% 버프) 발동 확률 상승
        const prankChance = (100 - this.value) / 100;
        if (Math.random() < prankChance) {
            this.ocyusisPrank = true;
            this.ocyusisEl.innerText = "😈 오큐시스의 장난 발동!: 몬스터 공격력 +30% 증가";
            this.ocyusisEl.style.background = "rgba(239,68,68,0.3)";
            gameManager.log(`[신족의 간섭] 악마 오큐시스가 장난을 쳐서 던전 환경이 악화되었습니다.`);
            this.adjust(-15);
        }

        // 천사 코스모의 가호 발동 확률 (40%)
        if (Math.random() < 0.4) {
            // 기획서 예외 규정 반영: 두 효과가 동시에 겹치면 서로 무효화(상쇄)된다
            if (this.ocyusisPrank) {
                this.ocyusisPrank = false;
                this.cosmoBless = false;
                this.ocyusisEl.innerText = "😈 오큐시스의 장난: 코스모의 가호로 인해 상쇄됨";
                this.cosmoEl.innerText = "😇 코스모의 가호: 오큐시스의 장난을 무효화했습니다.";
                gameManager.log(`[시스템 융합] 코스모의 가호와 오큐시스의 장난이 충돌하여 상쇄되었습니다.`);
            } else {
                this.cosmoBless = true;
                this.cosmoEl.innerText = "😇 코스모의 가호 발동!: 주인공 공격 파워 및 속도 향상";
                this.cosmoEl.style.background = "rgba(59,130,246,0.3)";
                gameManager.log(`[신족의 간섭] 천사 코스모가 가호를 내려 전투를 지원합니다.`);
                this.adjust(15);
            }
        }
    }

    reset() {
        this.ocyusisPrank = false;
        this.cosmoBless = false;
        this.cosmoEl.innerText = "😇 코스모의 가호: 상시 대기 중 (확률적 발생)";
        this.ocyusisEl.innerText = "😈 오큐시스의 장난: 미발동 (전투 시 확률 발동)";
        this.cosmoEl.style.background = "rgba(59, 130, 246, 0.15)";
        this.ocyusisEl.style.background = "rgba(239, 68, 68, 0.15)";
    }
}

/**
 * [엔티티 1] Player: 플레이어의 생명력, 궁극기 잔여 횟수 및 피격 연출을 제어
 */
class Player {
    constructor(maxHp = 1250) {
        this.maxHp = maxHp;
        this.hp = maxHp;
        this.ultMax = 3;
        this.ultLeft = 3;
        this.element = document.getElementById('player');
    }

    takeDamage(amount) {
        this.hp = Math.max(0, this.hp - amount);
        
        // 피격 시 캐릭터 흔들림 시각 효과 (드래곤 엔진 타격감 구현)
        if (this.element) {
            this.element.style.transform = 'scale(1.2)';
            setTimeout(() => this.element.style.transform = 'scale(1)', 100);
        }
    }

    heal(amount) {
        this.hp = Math.min(this.maxHp, this.hp + amount);
    }

    useUltimate() {
        if (this.ultLeft > 0) {
            this.ultLeft--;
            return true;
        }
        return false;
    }

    isDead() {
        return this.hp <= 0;
    }
}

/**
 * [엔티티 2] Monster: 필드 인물들의 체력 상태 및 비선공 -> 심리스 적대화 상태를 관리
 */
class Monster {
    constructor(id, name, hp, x, y) {
        this.id = id;
        this.name = name;
        this.maxHp = hp;
        this.hp = hp;
        this.hostile = false; // 기본 비선공 (자유 전투 시스템)
        this.alive = true;
        this.element = document.getElementById(id);
        
        if (this.element) {
            this.element.style.left = `${x}px`;
            this.element.style.top = `${y}px`;
        }
    }

    takeDamage(amount) {
        this.hp = Math.max(0, this.hp - amount);
        
        // 랙돌 물리 엔진 느낌의 튕겨 나가는 회전/이동 애니메이션 연출
        if (this.element) {
            this.element.style.transform = 'translate(20px, -10px) rotate(15deg)';
            setTimeout(() => this.element.style.transform = 'none', 150);
        }

        if (this.hp <= 0) {
            this.alive = false;
            if (this.element) {
                this.element.style.opacity = '0.3';
                this.element.style.background = '#475569';
            }
        }
    }

    becomeHostile() {
        this.hostile = true;
        if (this.element) {
            this.element.classList.add('hostile');
        }
    }
}

/**
 * [시스템] EmotionSystem: 기획서 기반 감정 게이지 연산 및 신족(오큐시스/코스모)의 간섭 상쇄 규칙 제어
 */
class EmotionSystem {
    constructor(initialValue = 50) {
        this.value = initialValue; 
        this.ocyusisPrank = false;
        this.cosmoBless = false;
        this.cosmoEl = document.getElementById('cosmo-status');
        this.ocyusisEl = document.getElementById('ocyusis-status');
    }

    adjust(amount) {
        this.value = Math.max(10, Math.min(90, this.value + amount));
    }

    getDifficultyModifier() {
        if (this.value < 30) return 1.2;  // 불행도가 높을수록 적의 반격 공격력 상승
        if (this.value > 70) return 0.8;  // 행복도가 높을수록 난이도 하락
        return 1.0;
    }

    triggerDivineIntervention(gameManager) {
        // 불행도에 비례하여 악마 오큐시스의 장난 발생 확률 상승
        const prankChance = (100 - this.value) / 100;
        if (Math.random() < prankChance) {
            this.ocyusisPrank = true;
            this.ocyusisEl.innerText = "😈 오큐시스의 장난 발동!: 몬스터 공격력 +30% 증가";
            this.ocyusisEl.style.background = "rgba(239,68,68,0.3)";
            gameManager.log(`[신족의 간섭] 악마 오큐시스가 장난을 쳐서 던전 난이도가 상승했습니다.`);
            this.adjust(-15);
        }

        // 천사 코스모의 가호 발동 확률 (40%)
        if (Math.random() < 0.4) {
            // 기획서 시스템 반영: 두 효과가 동시에 겹치면 무효화(상쇄)된다.
            if (this.ocyusisPrank) {
                this.ocyusisPrank = false;
                this.cosmoBless = false;
                this.ocyusisEl.innerText = "😈 오큐시스의 장난: 코스모의 가호로 인해 상쇄됨";
                this.cosmoEl.innerText = "😇 코스모의 가호: 오큐시스의 장난을 무효화했습니다.";
                gameManager.log(`[시스템 충돌] 코스모의 가호와 오큐시스의 장난이 충돌하여 서로 소멸했습니다.`);
            } else {
                this.cosmoBless = true;
                this.cosmoEl.innerText = "😇 코스모의 가호 발동!: 주인공 공격 효율 및 파워 향상";
                this.cosmoEl.style.background = "rgba(59,130,246,0.3)";
                gameManager.log(`[신족의 간섭] 천사 코스모가 가호를 내려 전투를 안전하게 돕습니다.`);
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

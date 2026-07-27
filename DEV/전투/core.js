/**
 * [컨트롤러] GameManager: 통합 코어 게임 루프 엔진 및 인터페이스 통제
 */
class GameManager {
    constructor() {
        this.crystals = 0;
        this.isCombat = false;
        this.targetMobId = null;
        this.combatInterval = null;

        // entities.js의 클래스들을 인스턴스화하여 게임 구성
        this.player = new Player();
        this.emotionSystem = new EmotionSystem();
        
        this.monsters = {
            'mob-1': new Monster('mob-1', '인물 A (과거의 동료)', 300, 450, 80),
            'mob-2': new Monster('mob-2', '인물 B (직장 상사)', 450, 550, 200),
            'mob-3': new Monster('mob-3', '인물 C (유년기 기억)', 600, 700, 120)
        };

        this.updateUI();
    }

    log(message) {
        const logBox = document.getElementById('log-box');
        if (logBox) {
            logBox.innerHTML += `<br>${message}`;
            logBox.scrollTop = logBox.scrollHeight;
        }
    }

    updateUI() {
        const hpPercent = (this.player.hp / this.player.maxHp) * 100;
        document.getElementById('hp-fill').style.width = `${Math.max(0, hpPercent)}%`;
        document.getElementById('hp-text').innerText = `${this.player.hp} / ${this.player.maxHp}`;
        
        document.getElementById('emotion-gauge').style.width = `${this.emotionSystem.value}%`;
        
        let emotionDesc = "평온";
        if (this.emotionSystem.value > 70) emotionDesc = "행복 (몬스터 약화)";
        else if (this.emotionSystem.value < 30) emotionDesc = "불행 (몬스터 강화/위험)";
        document.getElementById('gauge-text').innerText = `현재 감정: ${this.emotionSystem.value}% (${emotionDesc})`;

        document.getElementById('crystal-count').innerText = this.crystals;
        document.getElementById('ult-count').innerText = this.player.ultLeft;
        document.getElementById('ult-btn').disabled = this.player.ultLeft <= 0 || !this.isCombat;

        const conditionEl = document.getElementById('player-condition');
        conditionEl.innerText = this.isCombat ? "⚔️ 실시간 전투 중!" : "비전투 (평화 상태)";
        conditionEl.style.color = this.isCombat ? "#ef4444" : "#10b981";
    }

    startCombatWith(mobId) {
        const mob = this.monsters[mobId];
        if (!mob || !mob.alive) return;

        if (!this.isCombat) {
            this.isCombat = true;
            this.targetMobId = mobId;
            mob.becomeHostile();
            
            this.log(`[심리스 진입] 인카운터 연출 없이 즉시 공간 내에서 배틀 모드가 시작되었습니다.`);
            this.emotionSystem.triggerDivineIntervention(this);
            this.startMonsterTickLoop();
        } else {
            this.targetMobId = mobId;
            this.log(`[타겟 전환] 공격 시선을 [${mob.name}]에게 맞춥니다.`);
        }
        this.updateUI();
    }

    startMonsterTickLoop() {
        if (this.combatInterval) clearInterval(this.combatInterval);
        
        this.combatInterval = setInterval(() => {
            if (!this.isCombat) {
                clearInterval(this.combatInterval);
                return;
            }

            const currentMob = this.monsters[this.targetMobId];
            if (currentMob && currentMob.alive) {
                let damage = Math.floor(Math.random() * 40) + 30;
                
                if (this.emotionSystem.ocyusisPrank) damage = Math.floor(damage * 1.3);
                damage = Math.floor(damage * this.emotionSystem.getDifficultyModifier());

                this.player.takeDamage(damage);
                this.log(`[피격] ${currentMob.name}의 실시간 역공으로 ${damage} 피해를 입었습니다.`);

                if (this.player.isDead()) {
                    this.log(`[사망] 기억 제어에 실패했습니다. 프롤로그 법원으로 이송 처리됩니다.`);
                    this.endCombat(false);
                }
            }
            this.updateUI();
        }, 1500);
    }

    executeBasicAttack() {
        if (!this.isCombat || !this.targetMobId) {
            this.log(`[알림] 타격할 대상이 없습니다. 필드의 인물 구체를 선택하십시오.`);
            return;
        }

        const mob = this.monsters[this.targetMobId];
        let dmg = Math.floor(Math.random() * 50) + 40;
        
        if (this.emotionSystem.cosmoBless) dmg = Math.floor(dmg * 1.2);

        mob.takeDamage(dmg);
        this.log(`[타격] ${mob.name}에게 ${dmg}의 환경 충돌 파괴 피해를 입혔습니다.`);

        if (!mob.alive) {
            this.log(`[소멸] ${mob.name}의 적대 형태가 정화되었습니다.`);
            const reward = Math.floor(Math.random() * 100) + 100;
            this.crystals += reward;
            this.log(`[재화 수급] 데스 크리스탈 +${reward} DC를 누적했습니다.`);

            this.findNextHostileTarget();
        }
        this.updateUI();
    }

    executeEvasion() {
        if (!this.isCombat) return;
        this.log(`[회피] Shift 퀵 모션 기동으로 타격 범위 바깥으로 이탈하며 심신이 안전화됩니다.`);
        this.player.heal(20);
        this.updateUI();
    }

    executeUltimate() {
        if (!this.isCombat) return;

        if (this.player.useUltimate()) {
            this.log(`[궁극기] 스킬 Q 적용! 기억 공간 전체에 파괴 물리 왜곡을 강제 출력합니다.`);
            
            for (let id in this.monsters) {
                let mob = this.monsters[id];
                if (mob.alive && mob.hostile) {
                    mob.takeDamage(250);
                    this.log(`[광역 파괴] 충돌 파편 효과가 ${mob.name}에게 250 대미지를 입힙니다.`);
                    if (!mob.alive) {
                        this.log(`[처치] ${mob.name}이 연쇄 소멸되었습니다.`);
                        this.crystals += 150;
                    }
                }
            }

            const bf = document.getElementById('battle-field');
            if (bf) {
                bf.style.background = '#ffffff';
                setTimeout(() => bf.style.background = '#0f172a', 100);
            }

            this.findNextHostileTarget();
            this.updateUI();
        }
    }

    findNextHostileTarget() {
        let anyAlive = false;
        for (let id in this.monsters) {
            if (this.monsters[id].alive && this.monsters[id].hostile) {
                this.targetMobId = id;
                anyAlive = true;
                this.log(`[오토 록온] 근접한 다른 적대적 인물 [${this.monsters[id].name}]에게 타겟이 전환됩니다.`);
                break;
            }
        }

        if (!anyAlive) {
            this.log(`[전투 승리] 구역 내 모든 위협 오브젝트 소멸. 평화 상태로 자동 리셋됩니다.`);
            this.endCombat(true);
        }
    }

    endCombat(isVictory) {
        this.isCombat = false;
        this.targetMobId = null;
        clearInterval(this.combatInterval);
        
        this.emotionSystem.reset();

        if (isVictory) {
            this.emotionSystem.adjust(10); 
        }
        this.updateUI();
    }
}

// 스크립트 결합 시 윈도우 인스턴스에 코어 코디네이터 등록
window.game = new GameManager();

import { Player } from './Player.js';
import { Monster } from './Monster.js';
import { EmotionSystem } from './EmotionSystem.js';

class GameManager {
    constructor() {
        this.crystals = 0;
        this.isCombat = false;
        this.targetMobId = null;
        this.combatInterval = null;

        // 개별 서브 모듈 인스턴스 조립 및 데이터 구성
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
            
            this.log(`[심리스 배틀] 로딩 화면 없이 필드 인카운터 그 자리에서 즉시 전투 페이즈가 열립니다.`);
            this.emotionSystem.triggerDivineIntervention(this);
            this.startMonsterTickLoop();
        } else {
            this.targetMobId = mobId;
            this.log(`[타겟 록온] 대상을 [${mob.name}]으로 실시간 전환합니다.`);
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
                this.log(`[피격] ${currentMob.name}의 실시간 타격으로 ${damage} 피해를 입었습니다.`);

                if (this.player.isDead()) {
                    this.log(`[사망] 주인공이 무기력하게 쓰러졌습니다... 사후세계 법원으로 영혼이 송환됩니다.`);
                    this.endCombat(false);
                }
            }
            this.updateUI();
        }, 1500);
    }

    executeBasicAttack() {
        if (!this.isCombat || !this.targetMobId) {
            this.log(`[경고] 타격 대상이 지정되지 않았습니다. 필드의 인물을 클릭해 먼저 선제공격하세요.`);
            return;
        }

        const mob = this.monsters[this.targetMobId];
        let dmg = Math.floor(Math.random() * 50) + 40;
        
        if (this.emotionSystem.cosmoBless) dmg = Math.floor(dmg * 1.2);

        mob.takeDamage(dmg);
        this.log(`[공격] ${mob.name}에게 타격 대미지 ${dmg}을 가했습니다.`);

        if (!mob.alive) {
            this.log(`[처치] ${mob.name}을 쓰러뜨렸습니다.`);
            const reward = Math.floor(Math.random() * 100) + 100;
            this.crystals += reward;
            this.log(`[획득] 데스 크리스탈 +${reward} DC 가 누적 보관됩니다.`);

            this.findNextHostileTarget();
        }
        this.updateUI();
    }

    executeEvasion() {
        if (!this.isCombat) return;
        this.log(`[회피] Shift 대시 모션으로 적의 충돌 가이드라인을 회피하고 생명력을 소량 추스릅니다.`);
        this.player.heal(20);
        this.updateUI();
    }

    executeUltimate() {
        if (!this.isCombat) return;

        if (this.player.useUltimate()) {
            this.log(`[궁극기 발동] 스킬 Q 적용: 왜곡된 기억 파동이 공간 전체를 타격합니다.`);
            
            for (let id in this.monsters) {
                let mob = this.monsters[id];
                if (mob.alive && mob.hostile) {
                    mob.takeDamage(250);
                    this.log(`[광역 피해] 파괴 파편이 ${mob.name}에게 250의 심각한 광역 충돌 대미지를 줍니다.`);
                    if (!mob.alive) {
                        this.log(`[처치] ${mob.name}이 연쇄 파괴되었습니다.`);
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
                this.log(`[자동 록온] 근접한 다른 적대 인물 [${this.monsters[id].name}]에게 심리스 카메라 앵글이 이동합니다.`);
                break;
            }
        }

        if (!anyAlive) {
            this.log(`[배틀 종료] 구역 내 모든 적대 위협 개체가 정화되었습니다. 비전투 모드로 전환됩니다.`);
            this.endCombat(true);
        }
    }

    endCombat(isVictory) {
        this.isCombat = false;
        this.targetMobId = null;
        clearInterval(this.combatInterval);
        
        this.emotionSystem.reset();

        if (isVictory) {
            this.emotionSystem.adjust(10); // 전투 승리로 긍정적 후회 반성을 거쳐 감정 수치 버프 획득
        }
        this.updateUI();
    }
}

// 브라우저 윈도우 스코프에 인스턴스 전역 바인딩 처리
window.game = new GameManager();

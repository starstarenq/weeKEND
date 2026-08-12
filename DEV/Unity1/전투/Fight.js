// Fight.js
export class Fight {
    /**
     * @param {Status} player - 고정 스탯의 플레이어 객체
     * @param {Status} monster - 랜덤 스탯의 몬스터 객체
     * @param {object} uiCallbacks - 화면과 로그를 갱신하기 위한 콜백 함수 모음
     */
    constructor(player, monster, uiCallbacks) {
        this.player = player;
        this.monster = monster;
        this.uiCallbacks = uiCallbacks; // { updateUI, addLog, onBattleEnd }
        this.battleInterval = null;
        this.isCombatStarted = false;
    }

    // 선제공격 및 실시간 전투 개시
    startBattle() {
        if (this.isCombatStarted) return;
        this.isCombatStarted = true;

        this.uiCallbacks.addLog(`⚔️ 선제공격! 주인공이 먼저 타격하며 실시간 전투가 시작됩니다.`, 'system');

        // 0.5초(500ms)마다 실시간 교전 진행
        this.battleInterval = setInterval(() => {
            this.executeTurn();
        }, 500);
    }

    // 1회 교전 처리 (체력 연산 및 판정)
    executeTurn() {
        // 1. 플레이어 선제공격
        this.monster.takeDamage(this.player.atk);
        this.uiCallbacks.addLog(`▶ 주인공이 몬스터에게 ${this.player.atk}의 피해를 입혔습니다.`, 'player');

        if (this.monster.isDead()) {
            this.endBattle(true);
            return;
        }

        // 2. 몬스터 반격
        this.player.takeDamage(this.monster.atk);
        this.uiCallbacks.addLog(`◀ 몬스터가 반격하여 주인공에게 ${this.monster.atk}의 피해를 입혔습니다.`, 'monster');

        if (this.player.isDead()) {
            this.endBattle(false);
            return;
        }

        // UI 상태 바 갱신
        this.uiCallbacks.updateUI();
    }

    // 전투 종료 처리
    endBattle(isPlayerWin) {
        clearInterval(this.battleInterval);
        this.uiCallbacks.updateUI();

        if (isPlayerWin) {
            this.uiCallbacks.addLog(`💀 몬스터를 처치했습니다! 전투에서 승리했습니다.`, 'win');
        } else {
            this.uiCallbacks.addLog(`❌ 주인공이 쓰러졌습니다. 전투에서 패배했습니다.`, 'lose');
        }

        // 시뮬레이션 흐름 제어를 위해 종료 알림 콜백 호출
        this.uiCallbacks.onBattleEnd(isPlayerWin);
    }
}

// 게임 전체 플레이 상태 열거형
const GameState = {
    MENU: 'MENU',          // 메뉴 화면
    PLAYING: 'PLAYING',    // 플레이 중
    GAMEOVER: 'GAMEOVER'   // 게임 오버
};

// 플레이어 상태 열거형
const PlayerState = {
    IDLE: 'IDLE',          // 일반 플레이 상태
    TIRED: 'TIRED',        // 피로 상태
    DEAD: 'DEAD'           // 게임 오버 상태
};

// 몬스터 상태 열거형
const MonsterState = {
    ALIVE: 'ALIVE',        // 생존 및 전투 가능
    STUNNED: 'STUNNED',    // 기절 상태
    DEFEATED: 'DEFEATED'   // 처치됨
};

// 게임 세부 데이터 초기값
const gameData = {
    version: "1.0.0",
    currentGameState: GameState.MENU,
    
    player: {
        name: "회고전사",
        level: 1,
        hp: 100,
        maxHp: 100,
        stamina: 100,
        state: PlayerState.IDLE
    },
    
    monster: {
        name: "야근 몬스터",
        hp: 50,
        maxHp: 50,
        damage: 15,
        state: MonsterState.ALIVE
    }
};

// 브라우저 환경 및 모듈 환경 호환성 확보
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { GameState, PlayerState, MonsterState, gameData };
}

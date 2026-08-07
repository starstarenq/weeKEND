// level.js
const DIFFICULTY_CONFIG = {
    EASY: {
        name: "쉬움",
        hpMultiplier: 0.8,        // 몬스터 체력/데미지 또는 스폰에 영향
        spawnRateModifier: 1.3,   // 몬스터가 더 천천히 생성됨 (ms)
        speedModifier: 0.85,      // 몬스터 이동 속도 감소
        playerMaxHp: 150
    },
    NORMAL: {
        name: "보통",
        hpMultiplier: 1.0,
        spawnRateModifier: 1.0,
        speedModifier: 1.0,
        playerMaxHp: 100
    },
    HARD: {
        name: "어려움",
        hpMultiplier: 1.3,
        spawnRateModifier: 0.75,  // 몬스터가 더 빠르게 생성됨
        speedModifier: 1.25,      // 몬스터 이동 속도 증가
        playerMaxHp: 80
    }
};

class LevelManager {
    constructor() {
        this.currentDifficulty = 'NORMAL';
        this.config = DIFFICULTY_CONFIG.NORMAL;
    }

    setDifficulty(difficultyKey) {
        if (DIFFICULTY_CONFIG[difficultyKey]) {
            this.currentDifficulty = difficultyKey;
            this.config = DIFFICULTY_CONFIG[difficultyKey];
            return true;
        }
        return false;
    }

    getDifficultyConfig() {
        return this.config;
    }
}

window.levelManager = new LevelManager();
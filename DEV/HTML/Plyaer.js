class Player {
    constructor(maxHealth, moveSpeed) {
        this.maxHealth = maxHealth;
        this.health = maxHealth;
        this.moveSpeed = moveSpeed;
        this.direction = { x: 0, y: 0 };
        this.isGameOver = false;

        this.currentCombo = 0;
        this.maxCombo = 3;
        this.lastAttackTime = 0;
        this.comboWindow = 1500; // 1.5초
    }

    move(x, y) {
        if (this.isGameOver) return `[게임오버] 행동 불가`;
        this.direction = { x: x, y: y };
        return `[이동] 방향: (${x}, ${y}), 속도: ${this.moveSpeed}`;
    }

    attack() {
        if (this.isGameOver) return `[게임오버] 행동 불가`;

        const now = Date.now();
        if (now - this.lastAttackTime > this.comboWindow) {
            this.currentCombo = 0;
        }

        this.currentCombo++;
        this.lastAttackTime = now;

        let result = `[공격] 콤보 시전! (${this.currentCombo}/${this.maxCombo})`;

        if (this.currentCombo >= this.maxCombo) {
            result += ` ★ 막타 콤보 작렬!`;
            this.currentCombo = 0;
        }

        return result;
    }

    takeDamage(baseDamage, monsterType, attackType) {
        if (this.isGameOver) return `[게임오버] 이미 사망 상태입니다.`;

        let monsterMultiplier = 1.0;
        switch (monsterType) {
            case 'Normal': monsterMultiplier = 1.0; break;
            case 'Medium': monsterMultiplier = 1.3; break;
            case 'MidBoss': monsterMultiplier = 1.7; break;
            case 'FinalBoss': monsterMultiplier = 2.5; break;
        }

        let attackMultiplier = 1.0;
        switch (attackType) {
            case 'Normal': attackMultiplier = 1.0; break;
            case 'Skill': attackMultiplier = 1.5; break;
        }

        const finalDamage = baseDamage * monsterMultiplier * attackMultiplier;
        this.health -= finalDamage;

        let result = `[피격] [${monsterType}]의 [${attackType}]! 데미지 ${finalDamage}을(를) 입었습니다.`;

        if (this.health <= 0) {
            this.health = 0;
            this.isGameOver = true;
            result += `\n[게임오버] 플레이어의 체력이 0이 되었습니다.`;
        }

        return result;
    }

    obtainEsperanza() {
        if (this.isGameOver) return `[게임오버] 사망한 플레이어는 회복할 수 없습니다.`;
        this.health = this.maxHealth;
        return `[아이템] 에스페란사 획득! 체력이 완전히 회복되었습니다.`;
    }
}

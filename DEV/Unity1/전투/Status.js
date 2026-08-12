// Status.js
export class Status {
    /**
     * @param {string} name - 캐릭터 이름 (주인공 / 몬스터)
     * @param {number} maxHp - 최대 체력
     * @param {number} atk - 공격력
     */
    constructor(name, maxHp, atk) {
        this.name = name;
        this.maxHp = maxHp;
        this.hp = maxHp; // 현재 체력은 최대 체력으로 초기화
        this.atk = atk;
    }

    // 몬스터 전용: 최소~최대 범위 내에서 무작위 스탯을 가진 Status 객체 생성
    static createRandomMonster(level = 1) {
        // 레벨이나 시뮬레이션 회차에 따라 무작위 스탯 부여 (예시 범위)
        const minHp = 80 + (level * 10);
        const maxHp = 160 + (level * 20);
        const minAtk = 8 + (level * 2);
        const maxAtk = 18 + (level * 3);

        const randomHp = Math.floor(Math.random() * (maxHp - minHp + 1)) + minHp;
        const randomAtk = Math.floor(Math.random() * (maxAtk - minAtk + 1)) + minAtk;

        return new Status(`레벨 ${level} 몬스터`, randomHp, randomAtk);
    }

    // 피해를 입는 메서드
    takeDamage(damage) {
        this.hp = Math.max(0, this.hp - damage);
    }

    // 사망 여부 확인
    isDead() {
        return this.hp <= 0;
    }
}

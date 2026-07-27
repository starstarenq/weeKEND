/**
 * Player 클래스: 주인공의 스탯 자원 상태 및 피격 연출 관리
 */
export class Player {
    constructor(maxHp = 1250) {
        this.maxHp = maxHp;
        this.hp = maxHp;
        this.ultMax = 3;
        this.ultLeft = 3;
        this.element = document.getElementById('player');
    }

    takeDamage(amount) {
        this.hp = Math.max(0, this.hp - amount);
        
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

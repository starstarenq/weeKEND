/**
 * Monster 클래스: 필드 인물들의 데이터 및 적대화, 충돌 연출 제어
 */
export class Monster {
    constructor(id, name, hp, x, y) {
        this.id = id;
        this.name = name;
        this.maxHp = hp;
        this.hp = hp;
        this.hostile = false; // 자유 전투 시스템 반영: 기본 비선공
        this.alive = true;
        this.element = document.getElementById(id);
        
        if (this.element) {
            this.element.style.left = `${x}px`;
            this.element.style.top = `${y}px`;
        }
    }

    takeDamage(amount) {
        this.hp = Math.max(0, this.hp - amount);
        
        // 랙돌 물리 엔진 느낌의 튕겨 나가는 회전 및 이동 물리 연출
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

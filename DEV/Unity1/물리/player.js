// player.js - 주인공 물리 엔티티 제어 클래스
class Player {
    constructor(x, y) {
        this.x = x;
        this.y = y;
        this.r = 15;
        this.spd = 4;
        this.vx = 0;
        this.vy = 0;
        this.fric = 0.85;
        this.hp = 100;
        this.maxHp = 100;
        this.isDash = false;
    }

    // 키보드 WASD 연동 가속도 오일러 적분 연산 및 감쇠 마찰 물리 적용
    update(keys, canvasWidth, canvasHeight) {
        if (keys['w'] || keys['ㅈ']) this.vy -= this.spd * 0.2;
        if (keys['s'] || keys['ㄴ']) this.vy += this.spd * 0.2;
        if (keys['a'] || keys['ㅁ']) this.vx -= this.spd * 0.2;
        if (keys['d'] || keys['ㅇ']) this.vx += this.spd * 0.2;

        // 마찰력 저항 처리
        this.vx *= this.fric;
        this.vy *= this.fric;

        // 외부 bounce.js 물리 엔진 속도 클램프 상호 검사
        if (typeof BounceEngine !== 'undefined') {
            BounceEngine.constrainImpactVelocity(this);
        }

        // 위치 변위 갱신
        this.x += this.vx;
        this.y += this.vy;

        // 화면 맵 경계 벽면 물리 강체 충돌 제어
        if (this.x < this.r) this.x = this.r;
        if (this.x > canvasWidth - this.r) this.x = canvasWidth - this.r;
        if (this.y < this.r) this.y = this.r;
        if (this.y > canvasHeight - this.r) this.y = canvasHeight - this.r;
    }

    // 회피 대시 가속 인가 시전
    triggerDash() {
        if (this.isDash) return;
        this.isDash = true;
        this.vx *= 3.5;
        this.vy *= 3.5;
        setTimeout(() => { this.isDash = false; }, 180);
    }

    // 그래픽 화면 드로잉 파이프라인
    draw(ctx) {
        ctx.beginPath();
        ctx.arc(this.x, this.y, this.r, 0, Math.PI * 2);
        ctx.fillStyle = this.isDash ? '#ffffff' : '#66fcf1';
        ctx.fill();
        ctx.closePath();

        // 머리 위 실시간 구조 체력 바 출력
        ctx.fillStyle = '#ff4d4d';
        ctx.fillRect(this.x - 15, this.y + this.r + 5, 30, 4);
        ctx.fillStyle = '#66fcf1';
        ctx.fillRect(this.x - 15, this.y + this.r + 5, (this.hp / this.maxHp) * 30, 4);
    }
}

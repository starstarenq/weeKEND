// boss.js - 보스 엔티티 인공지능 및 돌진 패턴 클래스
class Boss {
    constructor() {
        this.x = 600;
        this.y = 200;
        this.r = 30;
        this.hp = 200;
        this.maxHp = 200;
        this.name = "메카 로키";
        this.spd = 1.4;
        this.color = '#ff4d4d';
        this.isAggro = false;
        
        // FSM 인공지능 상태 변수 구조화
        this.state = 'normal'; // 'normal'(추적), 'charging'(선딜 충전), 'dashing'(초고속 돌진)
        this.chargeTimer = 0;
        this.dashDuration = 0;
        this.dashTargetAngle = 0;
        this.nextDashTime = 0;
    }

    // 챕터 전환에 따른 고유 엔티티 스펙 데이터 실시간 갱신 로더
    loadChapterData(ch) {
        this.isAggro = false;
        this.state = 'normal';
        this.nextDashTime = 0;
        if (ch === 1) {
            this.name = "메카 로키"; this.hp = 200; this.maxHp = 200; this.spd = 1.4; this.r = 30; this.color = '#ff4d4d';
        } else if (ch === 2) {
            this.name = "큐피트"; this.hp = 350; this.maxHp = 350; this.spd = 2.3; this.r = 24; this.color = '#ff69b4';
        } else {
            this.name = "무에르떼"; this.hp = 600; this.maxHp = 600; this.spd = 2.8; this.r = 38; this.color = '#8a2be2';
        }
    }

    // 보스 인공지능 및 궤적 이동 업데이트
    update(player, gameEmotion) {
        if (!this.isAggro || this.hp <= 0) return;

        const dist = Math.hypot(player.x - this.x, player.y - this.y);

        if (this.state === 'normal') {
            // 사정거리 범위(250px) 및 돌진 내부 쿨타임 충족 여부 체크 검사 후 차징 변환
            if (dist < 250 && Date.now() > this.nextDashTime) {
                this.state = 'charging';
                this.chargeTimer = 30; // 30프레임 선딜레이 고정 충전 대기
                this.dashTargetAngle = Math.atan2(player.y - this.y, player.x - this.x);
            } else {
                // 평시 일반 유도 추적 (기획서 반영 불행도 30% 미만 시 보스 속도 1.6배 갱신 법칙 결합)
                const angle = Math.atan2(player.y - this.y, player.x - this.x);
                const currentSpd = gameEmotion < 30 ? this.spd * 1.6 : this.spd;
                this.x += Math.cos(angle) * currentSpd;
                this.y += Math.sin(angle) * currentSpd;
            }
        } 
        else if (this.state === 'charging') {
            this.chargeTimer--;
            if (this.chargeTimer <= 0) {
                this.state = 'dashing';
                this.dashDuration = 15; // 15프레임 동안 고속 직선 돌폭 추진
            }
        } 
        else if (this.state === 'dashing') {
            const dashSpeed = this.spd * 4.5; // 기존 속도의 4.5배 물리 엔진 가속 법칙
            this.x += Math.cos(this.dashTargetAngle) * dashSpeed;
            this.y += Math.sin(this.dashTargetAngle) * dashSpeed;
            this.dashDuration--;

            if (this.dashDuration <= 0) {
                this.state = 'normal';
                this.nextDashTime = Date.now() + 2500; // 돌진 기동 종료 후 내부 재사용 대기 시간 2.5초 인가
            }
        }
    }

    // 보스 그래픽 드로잉 기믹 처리
    draw(ctx) {
        if (this.hp <= 0) return;

        ctx.beginPath();
        ctx.arc(this.x, this.y, this.r, 0, Math.PI * 2);
        
        // 패턴 상태 분기별 실시간 가시적 컬러 렌더링 맵핑
        if (!this.isAggro) ctx.fillStyle = '#555555';
        else if (this.state === 'charging') ctx.fillStyle = '#ffa500'; // 선딜레이 충전 경고: 주황색
        else if (this.state === 'dashing') ctx.fillStyle = '#ff0000';  // 초고속 직선 돌폭: 밝은 적색
        else ctx.fillStyle = '#b22222';                               // 평시 유도 추적: 어두운 적색
        
        ctx.fill();
        ctx.closePath();

        // 기획서 투영: 사정거리 250px 인디케이터 범주 원형 가이드선 시각 연출
        if (this.isAggro && this.state === 'normal') {
            ctx.beginPath();
            ctx.arc(this.x, this.y, 250, 0, Math.PI * 2);
            ctx.strokeStyle = 'rgba(255, 77, 77, 0.08)';
            ctx.lineWidth = 1;
            ctx.stroke();
            ctx.closePath();
        }

        // 이름 및 실시간 체력 정보 출력
        ctx.fillStyle = '#ffffff';
        ctx.font = '12px sans-serif';
        ctx.fillText(`${this.name} (HP: ${Math.round(this.hp)})`, this.x - 40, this.y - this.r - 10);
    }
}

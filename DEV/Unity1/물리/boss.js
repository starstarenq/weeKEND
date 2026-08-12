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

        this.chapter = 1;

        // 무에르떼 이미지
        this.image = new Image();
        this.image.src = "b.png";

        // FSM
        this.state = 'normal';
        this.chargeTimer = 0;
        this.dashDuration = 0;
        this.dashTargetAngle = 0;
        this.nextDashTime = 0;
    }

    // 챕터 데이터
    loadChapterData(ch) {
        this.chapter = ch;

        this.isAggro = false;
        this.state = 'normal';
        this.nextDashTime = 0;

        if (ch === 1) {
            this.name = "메카 로키";
            this.hp = 200;
            this.maxHp = 200;
            this.spd = 1.4;
            this.r = 30;
            this.color = "#ff4d4d";
        }
        else if (ch === 2) {
            this.name = "큐피트";
            this.hp = 350;
            this.maxHp = 350;
            this.spd = 2.3;
            this.r = 24;
            this.color = "#ff69b4";
        }
        else {
            this.name = "무에르떼";
            this.hp = 600;
            this.maxHp = 600;
            this.spd = 2.8;
            this.r = 38;
            this.color = "#8a2be2";
        }
    }

    update(player, gameEmotion) {

        if (!this.isAggro || this.hp <= 0) return;

        const dist = Math.hypot(
            player.x - this.x,
            player.y - this.y
        );

        if (this.state === "normal") {

            if (
                dist < 250 &&
                Date.now() > this.nextDashTime
            ) {

                this.state = "charging";
                this.chargeTimer = 30;

                this.dashTargetAngle =
                    Math.atan2(
                        player.y - this.y,
                        player.x - this.x
                    );

            } else {

                const angle =
                    Math.atan2(
                        player.y - this.y,
                        player.x - this.x
                    );

                const speed =
                    gameEmotion < 30
                        ? this.spd * 1.6
                        : this.spd;

                this.x += Math.cos(angle) * speed;
                this.y += Math.sin(angle) * speed;
            }
        }

        else if (this.state === "charging") {

            this.chargeTimer--;

            if (this.chargeTimer <= 0) {

                this.state = "dashing";
                this.dashDuration = 15;

            }
        }

        else if (this.state === "dashing") {

            const dashSpeed = this.spd * 4.5;

            this.x +=
                Math.cos(this.dashTargetAngle) *
                dashSpeed;

            this.y +=
                Math.sin(this.dashTargetAngle) *
                dashSpeed;

            this.dashDuration--;

            if (this.dashDuration <= 0) {

                this.state = "normal";

                this.nextDashTime =
                    Date.now() + 2500;
            }
        }
    }

    draw(ctx) {

        if (this.hp <= 0) return;

        // ============================
        // 무에르떼만 이미지 출력
        // ============================
        if (
            this.chapter === 3 &&
            this.image.complete
        ) {

            const size = 170;

            ctx.drawImage(
                this.image,
                this.x - size / 2,
                this.y - size / 2,
                size,
                size
            );

        } else {

            // 메카 로키 / 큐피트 기존 방식

            ctx.beginPath();

            ctx.arc(
                this.x,
                this.y,
                this.r,
                0,
                Math.PI * 2
            );

            if (!this.isAggro)
                ctx.fillStyle = "#555555";

            else if (this.state === "charging")
                ctx.fillStyle = "#ffa500";

            else if (this.state === "dashing")
                ctx.fillStyle = "#ff0000";

            else
                ctx.fillStyle = "#b22222";

            ctx.fill();

            ctx.closePath();
        }

        // 사정거리 원
        if (
            this.isAggro &&
            this.state === "normal"
        ) {

            ctx.beginPath();

            ctx.arc(
                this.x,
                this.y,
                250,
                0,
                Math.PI * 2
            );

            ctx.strokeStyle =
                "rgba(255,77,77,0.08)";

            ctx.lineWidth = 1;

            ctx.stroke();

            ctx.closePath();
        }

        // 이름
        ctx.fillStyle = "#ffffff";
        ctx.font = "12px sans-serif";

        ctx.fillText(
            `${this.name} (HP: ${Math.round(this.hp)})`,
            this.x - 45,
            this.y - this.r - 10
        );
    }
}
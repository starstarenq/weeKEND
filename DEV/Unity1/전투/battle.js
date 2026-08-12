// ==========================================
// 1. 글로벌 환경 설정 및 오브젝트 정의
// ==========================================
const canvas = document.getElementById("gameCanvas");
const ctx = canvas.getContext("2d");
const logContainer = document.getElementById("battleLog");

// 키 입력 상태 추적 데이터 구조
const keys = { w: false, a: false, s: false, d: false };

// 플레이어 객체 모델 (방향 정규화 변수 추가)
const player = {
    x: 100,
    y: 225,
    radius: 15,
    speed: 4,
    color: "#569cd6",
    attackRange: 130,      // 부채꼴 공격 사거리
    attackAngle: 80,       // 부채꼴 시야각 (좌우 각각 40도)
    dirX: 1,               // 현재 바라보는 방향 벡터 X (기본값 우측)
    dirY: 0,               // 현재 바라보는 방향 벡터 Y
    isAttacking: false,
    isUltActive: false,
    attackTimer: 0,
    ultTimer: 0,
    killCount: 0,
    ultRemaining: 3        // 기획서 기준 챕터당 궁극기 3회 제한 반영
};

// 적(몬스터) 객체 마스터 테이블
const enemy = {
    id: 1,
    x: 450,
    y: 225,
    radius: 18,
    speed: 1.5,
    color: "#aaaaaa",
    hp: 100,
    maxHp: 100,
    isAggro: false,
    moveTimer: 0,
    targetX: 450,
    targetY: 225
};

// ==========================================
// 2. 입력 감지 핸들러 및 리스너 (인터럽트 제어)
// ==========================================
window.addEventListener("keydown", (e) => {
    const key = e.key.toLowerCase();
    if (key === 'w' || key === 'a' || key === 's' || key === 'd') {
        keys[key] = true;
        updatePlayerDirection(); // 이동 키가 눌릴 때 방향 벡터 업데이트
    }
    // Q 키: 궁극기 발동 분기
    if (key === 'q' && !player.isUltActive) {
        processUltimateAttack();
    }
});

window.addEventListener("keyup", (e) => {
    const key = e.key.toLowerCase();
    if (key === 'w' || key === 'a' || key === 's' || key === 'd') {
        keys[key] = false;
        updatePlayerDirection(); // 키를 뗄 때도 남은 조합 키 기준으로 방향 유지
    }
});

// 마우스 좌클릭 이벤트: 전방 부채꼴 평타 공격 트리거
canvas.addEventListener("mousedown", (e) => {
    if (e.button === 0 && !player.isAttacking) {
        processNormalAttack();
    }
});

// 컴포넌트 유틸리티: 움직이는 입력 상태 조합으로 단일 전방 방향 벡터 도출
function updatePlayerDirection() {
    let moveX = 0;
    let moveY = 0;

    if (keys.w) moveY -= 1;
    if (keys.s) moveY += 1;
    if (keys.a) moveX -= 1;
    if (keys.d) moveX += 1;

    // 대각선 이동 시 크기가 1이 되도록 정규화 연산 수행 (Normalize)
    if (moveX !== 0 || moveY !== 0) {
        const length = Math.sqrt(moveX * moveX + moveY * moveY);
        player.dirX = moveX / length;
        player.dirY = moveY / length;
    }
}

// UI 출력을 위한 로그 빌더
function addLog(message, className = "log-system") {
    const logEntry = document.createElement("div");
    logEntry.className = className;
    logEntry.innerText = message;
    logContainer.appendChild(logEntry);
    logContainer.scrollTop = logContainer.scrollHeight;
}

// ==========================================
// 3. 기하학 기반 공격 메커니즘 엔진
// ==========================================

// 마우스 좌클릭: 전방 부채꼴 검증 알고리즘
function processNormalAttack() {
    player.isAttacking = true;
    player.attackTimer = 8; // 8프레임 렌더링 유지

    if (enemy.hp <= 0) return;

    // 1단계: 플레이어와 몬스터 간의 거리 측정 (원점 거리)
    const dx = enemy.x - player.x;
    const dy = enemy.y - player.y;
    const distance = Math.sqrt(dx * dx + dy * dy);

    // 2단계: 거리가 유효 범위 내부인지 1차 필터링
    if (distance <= player.attackRange) {
        
        // 3단계: 두 벡터(바라보는 전방 벡터 vs 타겟 벡터)의 사잇각 측정 (삼각함수 역함수 변환)
        const targetAngle = Math.atan2(dy, dx);                     // 타겟 사이 각도 라디안
        const playerAngle = Math.atan2(player.dirY, player.dirX);   // 내가 움직이는 정면 각도 라디안
        
        // 각도 차이 절대값 보정 연산
        let angleDiff = Math.abs(targetAngle - playerAngle);
        if (angleDiff > Math.PI) {
            angleDiff = (Math.PI * 2) - angleDiff; // 360도 반전 영역 예외 수정
        }

        const angleDeg = angleDiff * (180 / Math.PI); // 호도법 -> 육십분법 도(°) 변환

        // [기획 규칙] 각도가 부채꼴 허용 한도(총 80도의 절반인 40도) 내에 있는지 확인
        if (angleDeg <= player.attackAngle / 2) {
            const damage = 25;
            enemy.hp = Math.max(0, enemy.hp - damage);
            addLog(`⚔ [일반 공격] 전방 부채꼴 내부 타격 성공! 대미지 ${damage} 적용 (HP: ${enemy.hp}/100)`, "log-hit");

            if (!enemy.isAggro) {
                enemy.isAggro = true;
                enemy.color = "#ff4d4d";
                addLog(`❗ 몬스터 #${enemy.id} 번이 선제공격을 받고 적대 상태로 전환되었습니다!`, "log-aggro");
            }

            if (enemy.hp <= 0) processEnemyKilled();
        } else {
            addLog(`💨 [공격 실패] 전방에 적이 없거나 시야각(부채꼴) 바깥에 있습니다.`, "log-miss");
        }
    } else {
        addLog(`💨 [공격 실패] 사거리를 완전히 벗어난 위치입니다. (거리 부족)`, "log-miss");
    }
}

// Q 키: 기획서 고유 규칙을 담은 화면 전체 광역 궁극기 엔진
function processUltimateAttack() {
    if (player.ultRemaining <= 0) {
        addLog("❌ 이번 회차/챕터에서 사용할 수 있는 궁극기 횟수(3회)를 모두 소모했습니다.", "log-miss");
        return;
    }

    player.isUltActive = true;
    player.ultTimer = 20;
    player.ultRemaining--;

    if (enemy.hp > 0) {
        const ultDamage = 100; // 확정 원킬 계수 부여
        enemy.hp = Math.max(0, enemy.hp - ultDamage);
        addLog(`⚡ [궁극기 활성화] 전체 화면 광역 판정 돌입! 대미지 ${ultDamage} 부여 (남은 사용 가능 횟수: ${player.ultRemaining}/3)`, "log-ult");
        
        if (enemy.hp <= 0) processEnemyKilled();
    }
}

// [기획 수정 요구사항] 처치 완료 시 무한 리스폰 인프라 구조
function processEnemyKilled() {
    player.killCount++;
    addLog(`🎉 [처치 성공] 몬스터 #${enemy.id} 처치 완료! 현재 누적 킬 수: ${player.killCount}`, "log-spawn");
    
    // 즉각적인 런타임 재생성 컨포넌트 호출
    respawnEnemy();
}

function respawnEnemy() {
    enemy.id++;
    // 가시 반경 내부 겹침 예외 처리를 위해 랜덤 경계 범위 제한 스폰
    enemy.x = Math.random() * (canvas.width - 150) + 75;
    enemy.y = Math.random() * (canvas.height - 150) + 75;
    enemy.hp = 100;
    enemy.isAggro = false;
    enemy.color = "#aaaaaa";
    enemy.moveTimer = 0;
    
    addLog(`✨ 새로운 몬스터 #${enemy.id} 번이 다른 시공간 좌표에 생성되었습니다.`, "log-spawn");
}

// ==========================================
// 4. 상태 업데이트 및 렌더링 프레임 루프
// ==========================================
function update() {
    // 캐릭터 이동 가속성 제어
    if (keys.w) player.y -= player.speed;
    if (keys.s) player.y += player.speed;
    if (keys.a) player.x -= player.speed;
    if (keys.d) player.x += player.speed;

    player.x = Math.max(player.radius, Math.min(canvas.width - player.radius, player.x));
    player.y = Math.max(player.radius, Math.min(canvas.height - player.radius, player.y));

    // 이펙트 유지 타이머 제어
    if (player.isAttacking) {
        player.attackTimer--;
        if (player.attackTimer <= 0) player.isAttacking = false;
    }
    if (player.isUltActive) {
        player.ultTimer--;
        if (player.ultTimer <= 0) player.isUltActive = false;
    }

    // 적의 상태머신 AI 업데이트 루프
    if (enemy.hp > 0) {
        if (enemy.isAggro) {
            const dx = player.x - enemy.x;
            const dy = player.y - enemy.y;
            const distance = Math.sqrt(dx * dx + dy * dy);
            if (distance > 5) {
                enemy.x += (dx / distance) * enemy.speed;
                enemy.y += (dy / distance) * enemy.speed;
            }
        } else {
            enemy.moveTimer--;
            if (enemy.moveTimer <= 0) {
                enemy.targetX = Math.random() * (canvas.width - 60) + 30;
                enemy.targetY = Math.random() * (canvas.height - 60) + 30;
                enemy.moveTimer = Math.random() * 120 + 60;
            }
            const tDx = enemy.targetX - enemy.x;
            const tDy = enemy.targetY - enemy.y;
            const tDist = Math.sqrt(tDx * tDx + tDy * tDy);
            if (tDist > 2) {
                enemy.x += (tDx / tDist) * (enemy.speed * 0.6);
                enemy.y += (tDy / tDist) * (enemy.speed * 0.6);
            }
        }
    }
}

function render() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    // 1. 움직이는 전방 기준의 실시간 부채꼴(Sector) 가이드 영역 렌더링
    const baseAngle = Math.atan2(player.dirY, player.dirX);
    const startAngle = baseAngle - (player.attackAngle / 2) * (Math.PI / 180);
    const endAngle = baseAngle + (player.attackAngle / 2) * (Math.PI / 180);

    ctx.beginPath();
    ctx.moveTo(player.x, player.y);
    ctx.arc(player.x, player.y, player.attackRange, startAngle, endAngle);
    ctx.closePath();
    ctx.fillStyle = "rgba(79, 193, 255, 0.08)";
    ctx.fill();
    ctx.strokeStyle = "rgba(79, 193, 255, 0.3)";
    ctx.lineWidth = 1.5;
    ctx.stroke();

    // 일반 공격 충격 이펙트 오버레이
    if (player.isAttacking) {
        ctx.beginPath();
        ctx.moveTo(player.x, player.y);
        ctx.arc(player.x, player.y, player.attackRange, startAngle, endAngle);
        ctx.closePath();
        ctx.fillStyle = "rgba(79, 193, 255, 0.25)";
        ctx.fill();
    }

    // 궁극기(Q) 사용 화면 진동/섬광 이펙트 연출
    if (player.isUltActive) {
        ctx.fillStyle = "rgba(255, 204, 0, 0.2)";
        ctx.fillRect(0, 0, canvas.width, canvas.height);
    }

    // 2. 캐릭터 렌더링
    ctx.beginPath();
    ctx.arc(player.x, player.y, player.radius, 0, Math.PI * 2);
    ctx.fillStyle = player.color;
    ctx.fill();
    ctx.closePath();

    // 3. 적 활성화 시 UI 셋 드로잉
    if (enemy.hp > 0) {
        ctx.beginPath();
        ctx.arc(enemy.x, enemy.y, enemy.radius, 0, Math.PI * 2);
        ctx.fillStyle = enemy.color;
        ctx.fill();
        ctx.closePath();

        // 몬스터 이름 및 식별자 라벨링 출력
        ctx.fillStyle = "#ffffff";
        ctx.font = "11px Arial";
        ctx.textAlign = "center";
        ctx.fillText(`Mob #${enemy.id}`, enemy.x, enemy.y + 4);

        // 체력바 드로잉
        const barWidth = 40;
        const barHeight = 5;
        const barX = enemy.x - barWidth / 2;
        const barY = enemy.y - enemy.radius - 10;

        ctx.fillStyle = "#555555";
        ctx.fillRect(barX, barY, barWidth, barHeight);

        const hpRatio = enemy.hp / 100;
        ctx.fillStyle = "#ff4d4d";
        ctx.fillRect(barX, barY, barWidth * hpRatio, barHeight);
    }
}

// 실시간 무한 프레임 제어 엔진 구동
function gameLoop() {
    update();
    render();
    requestAnimationFrame(gameLoop);
}

gameLoop();

/**
 * 10년 차 C# 프로그래머 스타일의 구조화된 게임 매니저 스크립트
 */

// 1. 보상 아이템 데이터베이스 (기본 정보 및 수치 포맷 관리)
const RewardDatabase = [
    { id: 1, name: "낡은 철검", type: "무기", val: 10, desc: "공격력이 {0} 증가합니다." },
    { id: 2, name: "마법의 지팡이", type: "무기", val: 15, desc: "주문력이 {0} 증가합니다." },
    { id: 3, name: "사냥꾼의 활", type: "무기", val: 10, desc: "공격 속도가 {0}% 증가합니다." },
    { id: 6, name: "가죽 갑옷", type: "장비", val: 5, desc: "방어력이 {0} 증가합니다." },
    { id: 7, name: "강철 투구", type: "장비", val: 50, desc: "최대 체력이 {0} 증가합니다." },
    { id: 11, name: "근력 강화", type: "능력치 증가", val: 3, desc: "기본 힘 능력치가 {0} 증가합니다." },
    { id: 12, name: "민첩성 훈련", type: "능력치 증가", val: 3, desc: "기본 민첩 능력치가 {0} 증가합니다." }
];

// 2. 핵심 게임 상태 및 제어 로직을 캡슐화한 객체
const GameManager = {
    currentStage: 0,
    maxStages: 5,
    currentMultiplier: 1.0,
    currentSelectedRewards: [],

    // UI 엘리먼트 캐싱 캐시
    getElements: function() {
        return {
            logPanel: document.getElementById('log'),
            btnNext: document.getElementById('btn-next'),
            rewardUi: document.getElementById('reward-ui'),
            rewardList: document.getElementById('reward-list'),
            txtStage: document.getElementById('txt-stage'),
            txtMultiplier: document.getElementById('txt-multiplier')
        };
    },

    // 로그 창 출력 유틸리티 함수
    appendLog: function(message, color = '#ffffff') {
        const el = this.getElements();
        el.logPanel.innerHTML += `<div style="color: ${color}">${message}</div>`;
        el.logPanel.scrollTop = el.logPanel.scrollHeight;
    },

    // [핵심 1] 스테이지 진행 및 보스 조우 로직 기믹 처리
    progressStage: function() {
        const el = this.getElements();

        if (this.currentStage === 0) {
            el.logPanel.innerHTML = '';
            this.appendLog("========================================", "#00ffcc");
            this.appendLog("🎮 유년기 기억 던전 탐험을 시작합니다!", "#00ffcc");
            this.appendLog("========================================", "#00ffcc");
        }

        this.currentStage++;
        // 스테이지가 오를수록 0.2배씩 보상 가치 복리 형태 증가 공식
        this.currentMultiplier = 1.0 + (this.currentStage - 1) * 0.2;
        
        // UI 텍스트 데이터 갱신
        el.txtStage.innerText = `${this.currentStage} / ${this.maxStages} 스테이지`;
        el.txtMultiplier.innerText = `x${this.currentMultiplier.toFixed(1)}`;
        el.btnNext.disabled = true;

        this.appendLog(`<br>▶ [제 ${this.currentStage} 스테이지] 구역에 진입했습니다. (보상 배율: x${this.currentMultiplier.toFixed(1)})`, "#eab308");

        // 비동기 타이머 연출을 사용해 보스 조우 시점 제어
        setTimeout(() => {
            if (this.currentStage === 2) {
                this.appendLog("⚠ [WARNING] 전방에서 불길하고 흑화한 형상의 에너지가 요동칩니다!", "#ef4444");
                setTimeout(() => {
                    this.appendLog("⚔️ ── 중간 보스 [메카 로키]가 출현했습니다! ── ⚔️", "#f87171");
                    setTimeout(() => {
                        this.appendLog("..치열한 격전 끝에 [메카 로키]를 제압했습니다!", "#4ade80");
                        this.showRewardSelection();
                    }, 1200);
                }, 800);
            } else if (this.currentStage === 5) {
                this.appendLog("🚨 [BOSS EMERGENCY] 기억의 한계점인 학교 정문에 도달했습니다!", "#ef4444");
                setTimeout(() => {
                    this.appendLog("☠️ ── 최종 보스 [학교 건물]이 살아 움직이며 덤벼듭니다! ── ☠️", "#f87171");
                    setTimeout(() => {
                        this.appendLog("💥 건물의 핵심 기믹을 파괴하여 완전히 무너뜨렸습니다! 승리!", "#4ade80");
                        this.showRewardSelection();
                    }, 1500);
                }, 800);
            } else {
                this.appendLog("..일반 구역의 무기력한 자아의 파편들을 소탕했습니다.", "#aaa");
                this.showRewardSelection();
            }
        }, 600);
    },

    // [핵심 2] 중복 없는 랜덤 보상 3선 및 동적 텍스트 생성
    showRewardSelection: function() {
        const el = this.getElements();
        el.rewardUi.style.display = 'block';
        el.rewardList.innerHTML = '';

        // Fisher-Yates 기반 셔플 알고리즘 모방하여 3개 무작위 추출
        const shuffled = [...RewardDatabase].sort(() => 0.5 - Math.random());
        this.currentSelectedRewards = shuffled.slice(0, 3);

        this.currentSelectedRewards.forEach((reward, index) => {
            // 배율 계산 적용
            const finalVal = Math.round(reward.val * this.currentMultiplier);
            const formattedDesc = reward.desc.replace("{0}", finalVal);

            // 동적 보상 선택 버튼 인스턴스 생성 및 렌더링
            const btn = document.createElement('button');
            btn.className = 'btn-reward';
            btn.innerHTML = `<strong>[${reward.type}] ${reward.name}</strong><br><small>${formattedDesc}</small>`;
            btn.onclick = () => this.selectReward(index, reward.name, formattedDesc);
            el.rewardList.appendChild(btn);
        });
    },

    // [핵심 3] 유저 선택 처리 및 데이터 완결 가공
    selectReward: function(index, name, desc) {
        const el = this.getElements();
        el.rewardUi.style.display = 'none';
        
        this.appendLog(`<br>★ 보상 선택 완료: [${name}]을(를) 영구 획득했습니다.`, "#4ade80");
        this.appendLog(`효과 반영: ${desc}`, "#4ade80");

        if (this.currentStage < this.maxStages) {
            el.btnNext.disabled = false;
            el.btnNext.innerText = `${this.currentStage + 1} 스테이지 탐험 시작`;
        } else {
            this.appendLog("<br>========================================", "#00ffcc");
            this.appendLog("🎉 축하합니다! 유년기 던전의 모든 위협을 이겨내고 클리어했습니다!", "#00ffcc");
            this.appendLog("========================================", "#00ffcc");
            el.btnNext.disabled = false;
            el.btnNext.innerText = "다시 플레이하기";
            this.currentStage = 0; // 게임 리셋 루프 초기화
        }
    }
};

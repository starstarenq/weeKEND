// 전역 StageManager 객체 생성
window.StageManager = {
    // 상태 정의
    currentStatus: 'START', // START, SELECT, PLAY, RESULT
    selectedStage: null,
    totalStages: 3,

    // 초기화 함수
    init: function() {
        this.renderSelectScreen();
        this.updateDOM();
    },

    // 화면 전환 함수
    changeStage: function(nextStatus, stageNum = null) {
        this.currentStatus = nextStatus;
        if (stageNum !== null) {
            this.selectedStage = stageNum;
        }
        this.updateDOM();
    },

    // 스테이지 선택 화면 버튼 동적 생성
    renderSelectScreen: function() {
        const container = document.getElementById('stage-buttons');
        container.innerHTML = '';
        
        for (let i = 1; i <= this.totalStages; i++) {
            const btn = document.createElement('button');
            btn.className = 'stage-btn';
            btn.innerText = `스테이지 ${i}`;
            btn.onclick = () => this.changeStage('PLAY', i);
            container.appendChild(btn);
        }
    },

    // 플레이 결과 처리 함수
    completeStage: function(isSuccess) {
        const message = isSuccess 
            ? `🎉 스테이지 ${this.selectedStage} 클리어 성공!` 
            : `💀 스테이지 ${this.selectedStage} 플레이 실패...`;
        
        document.getElementById('result-message').innerText = message;
        this.changeStage('RESULT');
    },

    // DOM 상태 업데이트 (화면 표시/숨김)
    updateDOM: function() {
        // 모든 화면 숨기기
        document.getElementById('start-screen').classList.remove('active');
        document.getElementById('select-screen').classList.remove('active');
        document.getElementById('play-screen').classList.remove('active');
        document.getElementById('result-screen').classList.remove('active');

        // 현재 상태의 화면만 표시
        if (this.currentStatus === 'START') {
            document.getElementById('start-screen').classList.add('active');
        } else if (this.currentStatus === 'SELECT') {
            document.getElementById('select-screen').classList.add('active');
        } else if (this.currentStatus === 'PLAY') {
            document.getElementById('current-stage-title').innerText = `스테이지 ${this.selectedStage} 플레이 중`;
            document.getElementById('play-screen').classList.add('active');
        } else if (this.currentStatus === 'RESULT') {
            document.getElementById('result-screen').classList.add('active');
        }
    }
};

// 상단 우측 버튼 시스템 인터페이스와 load.js / save.js 코어를 매핑해주는 제어 허브
const StorageCallController = {
    initBindings() {
        const saveBtn = document.getElementById('btn-game-save');
        const loadBtn = document.getElementById('btn-game-load');

        // 저장 액션 가동
        if (saveBtn) {
            saveBtn.onclick = () => {
                if (window.GameSaver) {
                    window.GameSaver.saveToPartyFile(GameState.party, GameState.inventory);
                }
            };
        }

        // 불러오기 액션 가동
        if (loadBtn) {
            loadBtn.onclick = () => {
                if (window.GameLoadTracker) {
                    const masterSource = typeof embeddedItems !== 'undefined' ? embeddedItems : [];
                    
                    // 'party' 파일로부터 데이터 가공 및 화면 갱신 복원 실행
                    const success = window.GameLoadTracker.applyRestoration(GameState.party, GameState.inventory, masterSource);
                    if (success) {
                        GameState.render(); // 코어 UI 리렌더링 호출
                        alert("📂 'party' 파일로부터 성공적으로 파티원 세팅을 불러왔습니다.");
                    } else {
                        alert("📂 'party' 파일에 기록된 데이터가 없거나 올바르지 않습니다.");
                    }
                }
            };
        }
    }
};

// 안정적인 의존성 로드를 위해 타이밍 제어 매핑 결합
window.addEventListener('DOMContentLoaded', () => {
    setTimeout(() => {
        StorageCallController.initBindings();
    }, 50);
});

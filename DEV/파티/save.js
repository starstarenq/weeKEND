// load.js가 가공한 장착 데이터와 인벤토리 배열을 취합하여 'party' 파일 저장소에 쓰기를 처리하는 모듈
class GameSaver {
    static saveToPartyFile(partyInstance, inventoryInstance) {
        try {
            // [요구사항 반영] load.js의 정적 메서드를 호출하여 "현재 장착 상태"를 가공 추출받음
            if (!window.GameLoadTracker) {
                throw new Error("GameLoadTracker(load.js) 모듈이 로드되지 않았습니다.");
            }
            const currentEquipment = window.GameLoadTracker.captureEquippedState(partyInstance);

            // 가변 배열 보관함의 아이템 리스트 추출
            const currentInventory = inventoryInstance.slots.map(item => item.id);

            // 통합 영웅 파일 데이터 세트 생성
            const partyFilePackage = {
                timestamp: new Date().toLocaleString(),
                equipment: currentEquipment,
                inventory: currentInventory,
                currentSelected: partyInstance.getSelectedMember() ? partyInstance.getSelectedMember().name : null
            };

            // [요구사항 반영] 'party' 라는 명확한 식별 키 파일에 최종 데이터 직렬화 보관
            localStorage.setItem('party', JSON.stringify(partyFilePackage));

            alert(`💾 'party' 데이터 저장 완료!\n동기화 시각: ${partyFilePackage.timestamp}`);
            return true;
        } catch (error) {
            console.error("party 파일 쓰기 실패:", error);
            alert("❌ 'party' 데이터 파일 기록 중 치명적인 오류가 발생했습니다.");
            return false;
        }
    }
}

// 전역 윈도우 스코프 등록
window.GameSaver = GameSaver;

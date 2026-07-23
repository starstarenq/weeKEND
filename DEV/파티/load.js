// 현재 파티원들의 실시간 장착 물리 상태를 역동적으로 추적하고 캡처 스냅샷을 생성하는 모듈
class GameLoadTracker {
    /**
     * 현재 파티 인스턴스로부터 장착 상태(Equipped State)만을 정밀 캡처합니다.
     * @param {Party} partyInstance - 실시간 파티 객체
     * @returns {Object} 각 캐릭터별 장착 아이템 ID 정보 스냅샷
     */
    static captureEquippedState(partyInstance) {
        const equipmentSnapshot = {};

        // 파티원 전체를 순회하며 현재 장착 슬롯 상태 추출
        partyInstance.members.forEach(member => {
            equipmentSnapshot[member.name] = {
                Weapon: member.equipped.Weapon ? member.equipped.Weapon.id : null,
                Shield: member.equipped.Shield ? member.equipped.Shield.id : null,
                Helmet: member.equipped.Helmet ? member.equipped.Helmet.id : null,
                Cloak: member.equipped.Cloak ? member.equipped.Cloak.id : null
            };
        });

        return equipmentSnapshot;
    }

    /**
     * 'party' 저장소 데이터를 파싱하여 실시간 메모리 객체 구조로 리바인딩 복원합니다.
     */
    static applyRestoration(partyInstance, inventoryInstance, masterItemsArray) {
        const rawData = localStorage.getItem('party');
        if (!rawData) return false;

        const savePackage = JSON.parse(rawData);
        const itemMap = new Map(masterItemsArray.map(item => [item.id, item]));

        // 1. 장착 대상 아이템 ID 풀 수집
        const equippedIds = new Set();
        Object.values(savePackage.equipment).forEach(slots => {
            Object.values(slots).forEach(id => { if (id) equippedIds.add(id); });
        });

        // 2. 가변 배열 인벤토리 복원 (장착된 것 제외)
        inventoryInstance.slots = [];
        savePackage.inventory.forEach(itemId => {
            const item = itemMap.get(itemId);
            if (item && !equippedIds.has(itemId)) {
                inventoryInstance.addItem(item);
            }
        });

        // 3. 각 파티원 슬롯에 물리 객체 주소 강제 결합
        partyInstance.members.forEach(member => {
            const savedSlots = savePackage.equipment[member.name];
            if (savedSlots) {
                member.equipped = { Weapon: null, Shield: null, Helmet: null, Cloak: null };
                Object.keys(savedSlots).forEach(slotType => {
                    const itemId = savedSlots[slotType];
                    if (itemId) {
                        const item = itemMap.get(itemId);
                        if (item) member.equipped[slotType] = item;
                    }
                });
            }
        });

        // 4. 선택 탭 복원
        if (savePackage.currentSelected) {
            partyInstance.selectMember(savePackage.currentSelected);
        }

        return true;
    }
}

// 전역 윈도우 스코프 등록
window.GameLoadTracker = GameLoadTracker;

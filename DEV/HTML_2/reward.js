// reward.js
export class Reward {
    /**
     * @param {number} id - 고유 아이디
     * @param {string} name - 보상 이름
     * @param {string} type - 보상 종류 (Weapon, Equipment, StatUp)
     * @param {string} description - 효과 설명
     * @param {object} effect - 실제 스탯 반영 값
     */
    constructor(id, name, type, description, effect) {
        this.id = id;
        this.name = name;
        this.type = type;
        this.description = description;
        this.effect = effect;
    }

    // 타입별 한글 명칭 반환
    getTypeName() {
        switch (this.type) {
            case 'Weapon': return '무기';
            case 'Equipment': return '장비';
            case 'StatUp': return '능력치';
            default: return '일반';
        }
    }

    // 타입별 배지 색상 (이미지 시안 기준)
    getBadgeColor() {
        switch (this.type) {
            case 'Weapon': return '#ef4444';    // 빨간색 무기
            case 'Equipment': return '#3b82f6'; // 파란색 장비
            case 'StatUp': return '#10b981';    // 초록색 능력치
            default: return '#475569';
        }
    }

    // 카드 상단에 그려질 타입별 SVG 아이콘
    getIconSvg() {
        switch (this.type) {
            case 'Weapon': // 교차된 칼 (무기)
                return `
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://w3.org">
                    <path d="M21 3L14 10M21 3V7M21 3H17M10 14L3 21M3 21V17M3 21H7" stroke="#ef4444" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                    <path d="M19 5L5 19" stroke="#ef4444" stroke-width="2" stroke-linecap="round"/>
                </svg>`;
            case 'Equipment': // 갑옷/방패 형상 (장비)
                return `
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://w3.org">
                    <path d="M12 22C12 22 20 18 20 12V5L12 2L4 5V12C4 18 12 22 12 22Z" stroke="#3b82f6" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                </svg>`;
            case 'StatUp': // 위를 향하는 화살표 (능력치 증가)
                return `
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://w3.org">
                    <path d="M12 19V5M12 5L5 12M12 5L19 12" stroke="#10b981" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                </svg>`;
            default:
                return '';
        }
    }
}

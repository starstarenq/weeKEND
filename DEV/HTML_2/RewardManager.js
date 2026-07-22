// RewardManager.js
import { Reward } from './reward.js';

export class RewardManager {
    constructor() {
        this.rewardDatabase = [];
        this.initDatabase();
    }

    initDatabase() {
        // 무기 5개 (공격력 증가 위주)
        this.rewardDatabase.push(new Reward(1, "낡은 철검", "Weapon", "공격력 +10", { atk: 10 }));
        this.rewardDatabase.push(new Reward(2, "마법 지팡이", "Weapon", "공격력 +15", { atk: 15 }));
        this.rewardDatabase.push(new Reward(3, "사냥꾼 활", "Weapon", "공격력 +12", { atk: 12 }));
        this.rewardDatabase.push(new Reward(4, "암살자 단검", "Weapon", "공격력 +18", { atk: 18 }));
        this.rewardDatabase.push(new Reward(5, "성스러운 둔기", "Weapon", "공격력 +8", { atk: 8 }));

        // 장비 5개 (최대 체력 증가 위주)
        this.rewardDatabase.push(new Reward(6, "가죽 갑옷", "Equipment", "최대 체력 +30", { hp: 30 }));
        this.rewardDatabase.push(new Reward(7, "강철 투구", "Equipment", "최대 체력 +50", { hp: 50 }));
        this.rewardDatabase.push(new Reward(8, "신속의 장화", "Equipment", "최대 체력 +20", { hp: 20 }));
        this.rewardDatabase.push(new Reward(9, "치유의 반지", "Equipment", "최대 체력 +40", { hp: 40 }));
        this.rewardDatabase.push(new Reward(10, "마나 목걸이", "Equipment", "최대 체력 +25", { hp: 25 }));

        // 능력치 증가 5개 (하이브리드 효과)
        this.rewardDatabase.push(new Reward(11, "근력 강화", "StatUp", "공격력 +8", { atk: 8 }));
        this.rewardDatabase.push(new Reward(12, "민첩 훈련", "StatUp", "공격력 +5, 체력 +20", { atk: 5, hp: 20 }));
        this.rewardDatabase.push(new Reward(13, "지혜 깨달음", "StatUp", "체력 +85", { hp: 85 }));
        this.rewardDatabase.push(new Reward(14, "강인한 정신", "StatUp", "공격력 +10, 체력 +30", { atk: 10, hp: 30 }));
        this.rewardDatabase.push(new Reward(15, "황금의 행운", "StatUp", "공격력 +27", { atk: 27 }));
    }

    // 중복 없이 3개 뽑기
    getRandomRewards(count = 3) {
        const tempPool = [...this.rewardDatabase];
        const selected = [];

        for (let i = 0; i < count; i++) {
            if (tempPool.length === 0) break;
            const randomIndex = Math.floor(Math.random() * tempPool.length);
            // splice 처리 후 배열 안의 인스턴스를 올바르게 파싱해 추출
            selected.push(tempPool.splice(randomIndex, 1)[0]);
        }
        return selected;
    }

    // 화면 그리기 (디자인 시안 슬롯 완벽 재현)
    renderRewardOptions(containerId, onSelectCallback) {
        const container = document.getElementById(containerId);
        container.innerHTML = ''; 

        const currentOptions = this.getRandomRewards(3);

        currentOptions.forEach(reward => {
            const card = document.createElement('div');
            card.className = 'reward-card';
            
            // 시안과 일치하는 둥근 배지, 간격, 폰트 비율을 SVG 슬롯 형태로 그리는 마크업
            card.innerHTML = `
                <div class="reward-icon-wrapper">
                    ${reward.getIconSvg()}
                </div>
                <span class="reward-badge" style="background-color: ${reward.getBadgeColor()}">
                    ${reward.getTypeName()}
                </span>
                <div class="reward-name">${reward.name}</div>
                <div class="reward-desc">${reward.description}</div>
            `;
            
            card.addEventListener('click', () => {
                onSelectCallback(reward);
            });

            container.appendChild(card);
        });
    }
}

// [CORS 보안 에러 완화] item.json의 내용이 없을 경우를 대비해 스크립트 내부 백업 데이터로 동시 매핑합니다.
const itemDataBackup = [
  { "id": "w1", "name": "지옥 철광 대검", "type": "Weapon", "stats": { "Attack": 15, "Damage": "6-17" }, "description": "대검", "icon": "⚔️" },
  { "id": "w2", "name": "성스러운 비탄", "type": "Weapon", "stats": { "Attack": 12, "Damage": "7-13" }, "description": "철퇴", "icon": "🔱" },
  { "id": "s1", "name": "요새의 대형 방패", "type": "Shield", "stats": { "AC": 4 }, "description": "대형방패", "icon": "🛡️" },
  { "id": "s2", "name": "자헤이라의 하프 방패", "type": "Shield", "stats": { "AC": 3 }, "description": "하프방패", "icon": "🛡️" },
  { "id": "h1", "name": "정의의 투구", "type": "Helmet", "stats": { "AC": 1 }, "description": "투구", "icon": "🪖" },
  { "id": "c1", "name": "어둠의 망토", "type": "Cloak", "stats": { "AC": 1 }, "description": "망토", "icon": "🧥" }
];

class Player {
    constructor(name, baseAC, baseAttack, baseDamage, avatar) {
        this.name = name;
        this.baseAC = baseAC;
        this.baseAttack = baseAttack;
        this.baseDamage = baseDamage;
        this.avatar = avatar;
        this.equipped = { Weapon: null, Shield: null, Helmet: null, Cloak: null };
    }

    equipItem(item) {
        let unequippedItem = this.equipped[item.type] || null;
        this.equipped[item.type] = item;
        return unequippedItem;
    }

    unequipItem(slotType) {
        if (this.equipped[slotType]) {
            const item = this.equipped[slotType];
            this.equipped[slotType] = null;
            return item;
        }
        return null;
    }

    getEffectiveStats() {
        let ac = this.baseAC;
        let attack = this.baseAttack;
        let damage = this.baseDamage;

        Object.values(this.equipped).forEach(item => {
            if (item && item.stats) {
                if (item.stats.AC) ac += item.stats.AC;
                if (item.stats.Attack) attack += item.stats.Attack;
                if (item.stats.Damage) damage = item.stats.Damage;
            }
        });
        return { ac, attack, damage };
    }
}

class Inventory {
    constructor() {
        this.slots = []; // 가변 배열 형식으로 관리되는 인벤토리 슬롯
    }
    addItem(item) { this.slots.push(item); }
    removeItem(itemId) {
        const index = this.slots.findIndex(item => item.id === itemId);
        if (index !== -1) return this.slots.splice(index, 1)[0];
        return null;
    }
    getItems() { return this.slots; }
}

class Party {
    constructor() {
        this.members = [];
        this.selectedMember = null;
    }
    addMember(player) {
        this.members.push(player);
        if (!this.selectedMember) this.selectedMember = player;
    }
    selectMember(name) {
        const member = this.members.find(m => m.name === name);
        if (member) this.selectedMember = member;
        return this.selectedMember;
    }
    getSelectedMember() { return this.selectedMember; }
}

const GameState = {
    party: new Party(),
    inventory: new Inventory(),

    async init() {
        let items = [];
        try {
            // 로컬 서버 환경일 때 item.json 비동기 패치
            const response = await fetch('item.json');
            items = await response.json();
        } catch (error) {
            // 파일 직접 열기(file://) 환경일 때는 백업 상수로 전환하여 안전 구동 보장
            items = [...itemDataBackup];
        }
        
        items.forEach(item => this.inventory.addItem(item));

        // 파티원 3명 동적 빌드 (Karlach, Shadowheart, Jaheira)
        this.party.addMember(new Player("Karlach", 25, 11, "11-21", "🔥"));
        this.party.addMember(new Player("Shadowheart", 23, 12, "7-13", "✨"));
        this.party.addMember(new Player("Jaheira", 25, 12, "9-17", "🌿"));

        this.render();
    },

    render() {
        this.renderPartyTabs();
        this.renderCharacterSheet();
        this.renderInventory();
    },

    renderPartyTabs() {
        const tabsContainer = document.getElementById('party-tabs');
        tabsContainer.innerHTML = '';
        this.party.members.forEach(member => {
            const button = document.createElement('button');
            button.className = `party-tab ${this.party.getSelectedMember() === member ? 'active' : ''}`;
            button.innerHTML = `<span>${member.avatar}</span> ${member.name}`;
            button.onclick = () => {
                this.party.selectMember(member.name);
                this.render();
            };
            tabsContainer.appendChild(button);
        });
    },

    renderCharacterSheet() {
        const character = this.party.getSelectedMember();
        if (!character) return;

        document.getElementById('char-name').innerText = character.name;
        const stats = character.getEffectiveStats();
        document.getElementById('stat-ac').innerText = stats.ac;
        document.getElementById('stat-attack').innerText = (stats.attack >= 0 ? '+' : '') + stats.attack;
        document.getElementById('stat-damage').innerText = stats.damage;

        const slots = ['Weapon', 'Shield', 'Helmet', 'Cloak'];
        slots.forEach(slotType => {
            const slotEl = document.getElementById(`slot-${slotType.toLowerCase()}`);
            const item = character.equipped[slotType];

            if (item) {
                slotEl.innerHTML = `<div class="item-icon" title="${item.description}">${item.icon}</div><div class="item-name">${item.name}</div>`;
                slotEl.classList.add('has-item');
                slotEl.onclick = () => {
                    const unequipped = character.unequipItem(slotType);
                    if (unequipped) {
                        this.inventory.addItem(unequipped);
                        this.render();
                    }
                };
            } else {
                slotEl.innerHTML = `<div class="slot-placeholder">${slotType} 슬롯</div>`;
                slotEl.classList.remove('has-item');
                slotEl.onclick = null;
            }
        });
    },

    renderInventory() {
        const invContainer = document.getElementById('inventory-grid');
        invContainer.innerHTML = '';
        const items = this.inventory.getItems();

        items.forEach(item => {
            const itemCard = document.createElement('div');
            itemCard.className = 'inventory-item';
            itemCard.innerHTML = `
                <div class="item-main">
                    <span class="inv-icon">${item.icon}</span>
                    <div class="item-info">
                        <div class="inv-name">${item.name}</div>
                        <div class="inv-type">${this.translateType(item.type)}</div>
                    </div>
                </div>
            `;
            itemCard.onclick = () => {
                const character = this.party.getSelectedMember();
                if (character) {
                    const removedItem = this.inventory.removeItem(item.id);
                    if (removedItem) {
                        const oldItem = character.equipItem(removedItem);
                        if (oldItem) this.inventory.addItem(oldItem);
                    }
                    this.render();
                }
            };
            invContainer.appendChild(itemCard);
        });

        const minSlots = 12;
        if (items.length < minSlots) {
            for (let i = 0; i < (minSlots - items.length); i++) {
                const emptySlot = document.createElement('div');
                emptySlot.className = 'inventory-item empty';
                invContainer.appendChild(emptySlot);
            }
        }
    },

    translateType(type) {
        switch(type) {
            case 'Weapon': return '주무기';
            case 'Shield': return '보조방패';
            case 'Helmet': return '머리방어구';
            case 'Cloak': return '망토';
            default: return type;
        }
    }
};

window.addEventListener('DOMContentLoaded', () => { GameState.init(); });

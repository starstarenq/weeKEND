// --- 1. 글로벌 게임 데이터 상태 (State) ---
let player = {
    hp: 70,               // 현재 체력 (테스트를 위해 대미지 입은 상태로 시작)
    maxHp: 100,
    ultCount: 1,          // 궁극기 사용 가능 횟수
    crystal: 100,         // 보유한 인게임 재화 (데스 크리스탈)
    emotion: 52,          // 감정 게이지 (기획서 예시 스크린샷의 52% 반영)
    inventory: [],        // 획득한 보상 아이템 목록
    buffs: []             // 적용된 버프 목록
};

// --- 2. 초기화 및 UI 업데이트 함수 ---
function updateUI() {
    document.getElementById('hp').innerText = player.hp;
    document.getElementById('ult').innerText = player.ultCount;
    document.getElementById('crystal').innerText = player.crystal;
    
    // 감정 게이지 바 및 텍스트 업데이트
    const gaugeBar = document.getElementById('gaugeBar');
    gaugeBar.style.width = player.emotion + '%';
    
    let emotionStatus = "무난함";
    if (player.emotion <= 30) emotionStatus = "불행 (난이도 상승☠)";
    else if (player.emotion >= 70) emotionStatus = "행복 (난이도 하락👼)";
    
    document.getElementById('gaugeText').innerText = `감정 상태: ${emotionStatus} (${player.emotion}%)`;
}

function addLog(message) {
    const logBox = document.getElementById('systemLog');
    logBox.innerHTML += `<br>▶ ${message}`;
    logBox.scrollTop = logBox.scrollHeight; // 스크롤 최하단 이동
}

// --- 3. 삶의 갈림길 시스템 구현 ---
// 기획서: 아이템, 버프, 재화 세 가지 보상 중 하나를 선택
const forkRewards = [
    { type: 'item', name: '낡은 철검', desc: '공격 스킬 트리 개방용 무기', style: 'item-type' },
    { type: 'buff', name: '공격력 +15%', desc: '다음 보스전까지 상시 적용', style: 'buff-type' },
    { type: 'money', name: '데스 크리스탈 250개', desc: '휴식처 전용 보너스 재화', value: 250, style: 'money-type' }
];

function generateForkCards() {
    const container = document.getElementById('forkCards');
    container.innerHTML = '';
    
    forkRewards.forEach((reward, index) => {
        const card = document.createElement('div');
        card.className = `card ${reward.style}`;
        card.innerHTML = `
            <h4>${reward.type === 'item' ? '🎁 아이템' : reward.type === 'buff' ? '⚡ 버프' : '💎 재화'}</h4>
            <strong>${reward.name}</strong>
            <p style="font-size:11px; color:#aaa; margin:5px 0 0 0;">${reward.desc}</p>
        `;
        // 클릭 시 보상 획득 연동
        card.onclick = () => selectForkReward(index);
        container.appendChild(card);
    });
}

function selectForkReward(index) {
    const reward = forkRewards[index];
    
    if (reward.type === 'item') {
        player.inventory.push(reward.name);
        addLog(`[갈림길] 아이템 '${reward.name}'을(를) 획득했습니다.`);
    } else if (reward.type === 'buff') {
        player.buffs.push(reward.name);
        addLog(`[갈림길] 버프 '${reward.name}'이(가) 활성화되었습니다.`);
    } else if (reward.type === 'money') {
        player.crystal += reward.value;
        addLog(`[갈림길] 재화 수령으로 데스 크리스탈이 ${reward.value}개 증가했습니다.`);
    }
    
    updateUI();
    
    // 기획서 규칙: 갈림길 보상 획득 후 바로 기억의 휴식처(상점)로 진입
    document.getElementById('forkScreen').classList.remove('active');
    document.getElementById('shopScreen').classList.add('active');
    addLog("기억의 휴식처에 진입했습니다. 상점 정비가 가능합니다.");
}

// --- 4. 기억의 휴식처(상점) 시스템 구현 ---
function buyShopItem(itemType, cost) {
    if (player.crystal < cost) {
        addLog("⚠ 데스 크리스탈이 부족하여 구매할 수 없습니다.");
        return;
    }
    
    player.crystal -= cost;
    
    switch(itemType) {
        case 'heal': // 체력 회복 편의시설
            player.hp = player.maxHp;
            addLog(`[상점] 체력을 최대치로 회복했습니다. (-${cost} 크리스탈)`);
            break;
            
        case 'esperanza': // 기획서: 에스페란사 소비 시 체력 전회복 + 궁극기 횟수 +1
            player.hp = player.maxHp;
            player.ultCount += 1;
            addLog(`[상점] 에스페란사 사용! 체력 완치 및 궁극기 횟수가 중첩 증가했습니다. (-${cost} 크리스탈)`);
            break;
            
        case 'cheer': // 기획서: 응원의 구슬 구매 시 랜덤 효과를 랜덤 수치로 제공 (중첩 가능)
            const stats = ['치명타 확률', '방어력', '이동속도'];
            const randomStat = stats[Math.floor(Math.random() * stats.length)];
            const randomValue = Math.floor(Math.random() * 11) + 5; // 5% ~ 15% 무작위
            
            player.buffs.push(`${randomStat} +${randomValue}%`);
            addLog(`[상점] 응원의 구슬 개봉! [${randomStat} +${randomValue}%] 버프 획득. (-${cost} 크리스탈)`);
            break;
    }
    
    updateUI();
}

// --- 5. 다음 루프 이동 프로토타입 ---
function nextStage() {
    addLog("정비를 마치고 다음 기억 스테이지로 진입합니다...");
    
    // 테스트용 루프 재설정 (다시 갈림길 화면으로)
    setTimeout(() => {
        document.getElementById('shopScreen').classList.remove('active');
        document.getElementById('forkScreen').classList.add('active');
        
        // 다음 스테이지 진입 시 랜덤하게 플레이어 행동에 의해 감정이 바뀐 상황 가정
        player.emotion = Math.floor(Math.random() * 101); 
        player.hp = Math.max(20, player.hp - Math.floor(Math.random() * 40)); // 다음 스테이지 전투에서 대미지 입음
        
        addLog("다음 기억 스테이지 클리어! 삶의 갈림길이 다시 나타났습니다.");
        generateForkCards();
        updateUI();
    }, 1000);
}

// 최초 실행시 세팅
window.onload = function() {
    generateForkCards();
    updateUI();
};

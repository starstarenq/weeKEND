// bounce.js - 회고인생 피격 반사 및 랜덤 넉백(Bounce) 물리 연산 엔진
const BounceEngine = {
    /**
     * 플레이어에게 무작위 360도 방향으로 강력한 튕겨남(넉백) 물리 에너지를 인가합니다.
     * @param {Object} player - 메인 html의 플레이어 객체 (vx, vy 조작)
     * @param {number} force - 튕겨나가는 힘의 세기 (기본값 계수: 18)
     */
    applyRandomKnockback: function (player, force = 18) {
        // 1. 0에서 2*PI(360도) 사이의 임의의 랜덤 라디안 각도를 추출합니다.
        const randomAngle = Math.random() * Math.PI * 2;

        // 2. 삼각함수를 활용해 각도 값을 가속도 평면 벡터(X, Y 추진력)로 환산합니다.
        const vxImpulse = Math.cos(randomAngle) * force;
        const vyImpulse = Math.sin(randomAngle) * force;

        // 3. 메인 클라이언트의 플레이어 운동 속도 에너지를 순간적으로 덮어씌웁니다.
        player.vx = vxImpulse;
        player.vy = vyImpulse;
    },

    /**
     * 넉백 상태에서 튕겨나갈 때 가상의 지면 저항과 벽면 탄성 한계를 보정합니다.
     * (마찰력 저항 연산은 메인 프레임의 player.fric 값과 감쇠 결합됩니다)
     */
    constrainImpactVelocity: function (player) {
        // 급격한 속도 왜곡이나 오류로 인한 무한 튕김 현상을 방지하는 물리 안전 장치 수식
        const maxVelocity = 35;
        const currentSpeed = Math.hypot(player.vx, player.vy);

        if (currentSpeed > maxVelocity) {
            player.vx = (player.vx / currentSpeed) * maxVelocity;
            player.vy = (player.vy / currentSpeed) * maxVelocity;
        }
    }
};

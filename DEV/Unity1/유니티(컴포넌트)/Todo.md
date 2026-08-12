# 🎮 게임 컴포넌트 시스템 구축 할 일 목록 (Todo List)

`index.html`에서 로드하여 조립할 핵심 게임 엔진 컴포넌트 파일 생성 및 구현 목록입니다.

## 📂 폴더 구조 정의
```text
├── index.html
└── components/
    ├── AnimationSystem.js
    ├── GameLoop.js
    ├── CollisionSystem.js
    ├── PhysicsSystem.js
    └── RenderSystem.js
```

---

## 📝 컴포넌트 생성 및 구현 태스크

### [ ] 1. 루프 담당 (`components/GameLoop.js`)
*   [ ] `requestAnimationFrame` 기반의 메인 루프 구현
*   [ ] 일정한 델타 타임(Delta Time) 계산 및 고정 프레임레이트 관리
*   [ ] 게임의 일시정지(Pause) 및 재개(Resume) 기능 구현
*   [ ] 매 프레임마다 Update와 Render 이벤트를 전파하는 기능

### [ ] 2. 물리 담당 (`components/PhysicsSystem.js`)
*   [ ] 오브젝트의 위치(Position), 속도(Velocity), 가속도(Acceleration) 데이터 구조 정의
*   [ ] 중력(Gravity) 및 마찰력(Friction) 적용 기능 구현
*   [ ] 델타 타임을 반영한 오브젝트 이동 업데이트 로직 구현

### [ ] 3. 충돌 담당 (`components/CollisionSystem.js`)
*   [ ] AABB (Axis-Aligned Bounding Box) 사각형 충돌 감지 알고리즘 구현
*   [ ] 원형(Circle) 충돌 감지 알고리즘 구현
*   [ ] 충돌 발생 시 물리 엔진(`PhysicsSystem`)으로 반발력 또는 위치 보정 신호 전달 기능

### [ ] 4. 애니메이션 담당 (`components/AnimationSystem.js`)
*   [ ] 스프라이트 시트(Sprite Sheet) 파싱 및 프레임 데이터 관리
*   [ ] 경과 시간에 따른 프레임 전환(인덱스 업데이트) 로직 구현
*   [ ] 애니메이션 상태(예: Idle, Run, Jump) 전환 관리자(State Machine) 구현

### [ ] 5. 렌더링 담당 (`components/RenderSystem.js`)
*   [ ] HTML5 Canvas 컨텍스트(2D Context) 초기화 및 관리
*   [ ] 매 프레임 화면을 지우는 clear 기능 구현
*   [ ] `AnimationSystem`과 `PhysicsSystem`에서 넘겨받은 위치/프레임 데이터를 바탕으로 화면에 그리는 기능 (Draw)

### [ ] 6. 진입점 조립 (`index.html`)
*   [ ] `<canvas>` 엘리먼트 배치 및 스타일링
*   [ ] `type="module"`을 사용하여 생성한 5개 컴포넌트 스크립트 로드
*   [ ] 메인 인스턴스에서 각 컴포넌트를 초기화하고 `GameLoop`에 연결하여 연동 테스트

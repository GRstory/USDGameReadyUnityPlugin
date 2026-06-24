# Changelog

## [0.1.2] - 2026-06-24

### Added
- `gameReady:slopeAngleLimit`, `gameReady:stepHeight`, `gameReady:hasAudioSource` attribute 지원
- CharacterController 부착 시 Collider 자동 생략 및 colliderSize로 capsule 설정

## [0.1.1] - 2026-06-24

### Added
- `gameReady:isPlayer`, `gameReady:colliderSize`, `gameReady:isTrigger` attribute 지원
- 커스텀 옵션 기능 추가

## [0.1.0] - 2026-06-23

### Added
- `FilterNPCPrimsNode`: `gameReady:isNPC` attribute가 있는 prim 경로 수집
- `CreateNavMeshAgentNode`: NPC prim에 `NavMeshAgent` 컴포넌트 자동 부착
- `FilterColliderPrimsNode`: `gameReady:hasCollider` attribute 값(Box/Sphere/Capsule/Mesh) 수집
- `CreateColliderNode`: 지정된 타입의 Collider 컴포넌트 자동 부착
- `GameReady > Build USD Importer Graph` 메뉴: 커스텀 임포터 그래프 자산 생성

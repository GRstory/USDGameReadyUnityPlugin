# USDGameReady

Unity의 `com.unity.importer.usd` 패키지를 확장하여 USD 임포트 시 prim의 attribute를 읽어 게임에 필요한 컴포넌트를 자동으로 부착하는 플러그인입니다.

## 기능

USD prim에 다음 attribute를 작성하면 임포트 시 자동으로 적용됩니다.

### Collider / Rigidbody — UsdPhysics 우선, gameReady fallback

Collider와 Rigidbody는 **표준 UsdPhysics 스키마를 1순위**로 읽습니다.  
`_physics.usd` 같은 물리 레이어를 메인 USD 파일에 sublayer로 연결하면 자동으로 인식됩니다.  
UsdPhysics attribute가 없는 경우에만 `gameReady:*` 커스텀 attribute를 fallback으로 사용합니다.

```usda
# main.usd
(
    subLayers = [
        @./main_physics.usd@,   # PhysicsCollisionAPI, PhysicsRigidBodyAPI 등
        @./main_gameready.usd@  # gameReady:* 커스텀 attribute
    ]
)
```

### metersPerUnit 자동 스케일링

플러그인은 USD 스테이지의 `metersPerUnit` 메타데이터를 자동으로 읽어 collider 크기와 stepHeight에 적용합니다.  
씬 단위가 m(1.0)이든 cm(0.01)이든 별도 설정 없이 Unity 미터 단위로 정상 변환됩니다.

```usda
#usda 1.0
(
    metersPerUnit = 0.01   # cm 단위 씬
)
# colliderSize = (30, 0, 180) cm → Unity에서 (0.3, 0, 1.8) m 으로 자동 변환
```

---

### 지원 Attribute 목록

#### NPC / Player

| Attribute | 타입 | 설명 |
|---|---|---|
| `gameReady:isNPC` | bool | true면 `NavMeshAgent` (또는 Inspector에서 지정한 커스텀 컴포넌트) 부착 |
| `gameReady:isPlayer` | bool | true면 `CharacterController` (또는 Inspector에서 지정한 커스텀 컴포넌트) 부착 |
| `gameReady:slopeAngleLimit` | float | CharacterController의 `slopeLimit` 설정 (기본 컴포넌트일 때만) |
| `gameReady:stepHeight` | float | CharacterController의 `stepOffset` 설정, metersPerUnit 자동 적용 |

#### Collider (UsdPhysics 1순위 / gameReady fallback)

| Attribute | 타입 | 설명 |
|---|---|---|
| `PhysicsCollisionAPI` | UsdPhysics | Prim 타입(`Cube`/`Sphere`/`Capsule`/`Mesh`)으로 Collider 종류 자동 결정 |
| `physics:approximation` | token | Mesh Collider 근사 방식 (`convexHull` 등), `PhysicsMeshCollisionAPI` 필요 |
| `gameReady:hasCollider` | token | **fallback** — `"Box"` / `"Sphere"` / `"Capsule"` / `"Mesh"` 중 하나 |
| `gameReady:colliderSize` | float3 | **fallback** — Box=`(x,y,z)`, Sphere=`(radius,_,_)`, Capsule=`(radius,_,height)`, 씬 단위 기준 |
| `gameReady:colliderCenter` | float3 | **fallback** — Collider center offset, Unity XYZ 기준 씬 단위 (예: cm 씬에서 `(0, 100, 0)` = Unity `(0, 1, 0)` m) |
| `gameReady:isTrigger` | bool | true면 Collider의 `isTrigger` 켬 |

#### Rigidbody (UsdPhysics 1순위 / gameReady fallback)

| Attribute | 타입 | 설명 |
|---|---|---|
| `PhysicsRigidBodyAPI` | UsdPhysics | Rigidbody 부착 트리거 |
| `physics:kinematicEnabled` | bool | `isKinematic` 설정 |
| `physics:velocity` | float3 | 초기 `linearVelocity` |
| `physics:angularVelocity` | float3 | 초기 `angularVelocity` |
| `PhysicsMassAPI` + `physics:mass` | UsdPhysics | Rigidbody `mass` 설정 |
| `physics:centerOfMass` | float3 | `centerOfMass` 설정 |
| `gameReady:hasRigidbody` | bool | **fallback** — true면 Rigidbody 부착 |
| `gameReady:mass` | float | **fallback** — `mass` 설정 |
| `gameReady:isKinematic` | bool | **fallback** — `isKinematic` 설정 |
| `gameReady:useGravity` | bool | **fallback** — false면 `useGravity` 끔 |

#### AudioSource

| Attribute | 타입 | 설명 |
|---|---|---|
| `gameReady:hasAudioSource` | bool | true면 `AudioSource` 컴포넌트 부착 |

#### Interactable

| Attribute | 타입 | 설명 |
|---|---|---|
| `gameReady:isInteractable` | bool | true면 `GameReadyInteractable` 컴포넌트 부착 |
| `rel gameReady:interactTargets` | relationship | 이 오브젝트가 영향을 주는 대상 prim 경로 목록 (1:N 지원) |

`GameReadyInteractable` 컴포넌트는 `List<GameObject> targets` 필드를 가집니다.  
커스텀 스크립트에서 `GetComponent<GameReadyInteractable>().targets`로 참조를 가져가면 됩니다.

---

### Inspector 옵션 (per-asset)

`.usda` 파일 선택 후 Inspector에 표시됩니다.

| 옵션 | 타입 | 동작 |
|---|---|---|
| `GR-Enable NPC` | bool | NPC 처리 on/off |
| `GR-NPC Component` | Dropdown | `(Default)`=NavMeshAgent / 커스텀 MonoBehaviour 선택 |
| `GR-Enable Player` | bool | Player 처리 on/off |
| `GR-Player Component` | Dropdown | `(Default)`=CharacterController / 커스텀 MonoBehaviour 선택 |
| `GR-Enable Collider` | bool | Collider 처리 on/off |
| `GR-Enable Rigidbody` | bool | Rigidbody 처리 on/off |
| `GR-Enable AudioSource` | bool | AudioSource 처리 on/off |
| `GR-Enable Interactable` | bool | Interactable 처리 on/off |
| `GR-Enable Material` | bool | MeshRenderer에 머티리얼 자동 할당 on/off |
| `GR-Default Material` | Material | 할당할 Unity 머티리얼 (null이면 URP Lit / Standard 자동 사용) |

---

## 요구사항

- Unity 6000.0 이상
- `com.unity.importer.usd` 1.0.0-pre.2 이상

## 설치

Unity의 Package Manager → **Add package from git URL**:

```
https://github.com/GRstory/USDGameReadyUnityPlugin.git
```

## 사용법

### 1. 커스텀 임포터 그래프 생성

Unity 메뉴 **USDGameReady > Build USD Importer Graph** 클릭.  
→ `Assets/USDGameReady/usdGameReadyImporter.asset` 생성됨.

### 2. USD 파일에 그래프 적용

Project 창에서 `.usda` 파일 선택 → Inspector → **Graph** 필드를 `usdGameReadyImporter`로 변경 → **Apply**.

### 3. USD 파일 작성 예시

> **주의:** Unity USD 임포터는 `def Xform`과 `def Mesh` prim만 GameObject로 생성합니다.  
> `def Cube`, `def Sphere`, `def Capsule` 같은 USD 기하학 타입은 임포트되지 않습니다.

**gameReady 커스텀 attribute 방식 (m 단위 씬):**

```usda
#usda 1.0
(
    metersPerUnit = 1
    upAxis = "Y"
)

def Xform "World"
{
    def Xform "Goblin"
    {
        custom bool gameReady:isNPC = true
        custom token gameReady:hasCollider = "Capsule"
        custom float3 gameReady:colliderSize = (0.5, 0, 1.8)
    }

    def Xform "Hero"
    {
        custom bool gameReady:isPlayer = true
        custom float3 gameReady:colliderSize = (0.5, 0, 1.8)
        custom float gameReady:slopeAngleLimit = 45.0
        custom float gameReady:stepHeight = 0.3
        custom bool gameReady:hasAudioSource = true
    }

    def Mesh "Wall"
    {
        custom token gameReady:hasCollider = "Box"
        custom float3 gameReady:colliderSize = (2.0, 0.2, 3.0)
    }

    def Mesh "TriggerZone"
    {
        custom token gameReady:hasCollider = "Box"
        custom float3 gameReady:colliderSize = (5.0, 1.0, 5.0)
        custom bool gameReady:isTrigger = true
    }

    def Xform "Switch"
    {
        custom bool gameReady:isInteractable = true
        rel gameReady:interactTargets = [
            </World/Ceiling/Light1>,
            </World/Ceiling/Light2>
        ]
    }
}
```

**기존 씬에 gameReady 레이어 추가 (sublayer 방식, cm 단위 씬):**

```usda
# main.usd — 기존 파일, subLayers만 추가
(
    metersPerUnit = 0.01
    subLayers = [
        @./_gameready.usd@
    ]
)
```

```usda
# _gameready.usd — 새로 추가하는 게임레디 레이어
#usda 1.0
(
    metersPerUnit = 0.01
    upAxis = "Z"
)

over "World"
{
    def Xform "Hero"
    {
        custom bool   gameReady:isPlayer        = true
        custom float3 gameReady:colliderSize    = (30, 0, 180)   # cm → Unity 0.3, 0, 1.8 m 자동 변환
        custom float  gameReady:slopeAngleLimit = 45.0
        custom float  gameReady:stepHeight      = 5.0            # cm → Unity 0.05 m 자동 변환
    }

    def Xform "ElevatorButton"
    {
        custom bool   gameReady:isInteractable = true
        custom token  gameReady:hasCollider    = "Box"
        custom float3 gameReady:colliderSize   = (10, 10, 10)
    }
}
```

**UsdPhysics sublayer 방식 (권장):**

```usda
# main_physics.usd — DCC 툴(Houdini 등)이 자동 생성
def Sphere "Boulder" (
    prepend apiSchemas = ["PhysicsRigidBodyAPI", "PhysicsMassAPI", "PhysicsCollisionAPI"]
)
{
    bool physics:rigidBodyEnabled = true
    float physics:mass = 80.0
    bool physics:collisionEnabled = true
}
```

## 라이선스

MIT

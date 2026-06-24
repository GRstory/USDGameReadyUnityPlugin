# USDGameReady

Unity의 `com.unity.importer.usd` 패키지를 확장하여 USD 임포트 시 prim의 커스텀 attribute를 읽어 게임에 필요한 컴포넌트를 자동으로 부착하는 플러그인입니다.

## 기능

USD prim에 다음 커스텀 attribute를 작성하면 임포트 시 자동으로 적용됩니다.

### 지원 Custom Attribute 목록

| Attribute | 타입 | 설명 |
|---|---|---|
| `gameReady:isNPC` | bool | true면 NPC로 간주 — `NavMeshAgent` (또는 Inspector에서 지정한 커스텀 컴포넌트) 부착 |
| `gameReady:isPlayer` | bool | true면 Player로 간주 — `CharacterController` (또는 커스텀 컴포넌트) 부착 |
| `gameReady:hasCollider` | token | `"Box"` / `"Sphere"` / `"Capsule"` / `"Mesh"` 중 하나 — 해당 Collider 부착 |
| `gameReady:colliderSize` | float3 | Collider 사이즈 지정. Box=`(x,y,z)`, Sphere=`(radius,_,_)`, Capsule=`(radius,_,height)`. 없으면 `MeshFilter.sharedMesh.bounds`로 자동 산출 |
| `gameReady:isTrigger` | bool | true면 Collider의 `isTrigger` 켬. MeshCollider는 자동으로 `convex`도 켜짐 |

### Inspector 옵션 (per-asset)

`.usda` 파일 선택 후 Inspector에 표시됩니다.

| 옵션 | 타입 | 동작 |
|---|---|---|
| `GR-Enable NPC` | bool | NPC 처리 on/off |
| `GR-Enable Player` | bool | Player 처리 on/off |
| `GR-Enable Collider` | bool | Collider 처리 on/off |
| `GR-NPC Component` | Dropdown | `(Default)`=NavMeshAgent / 프로젝트의 사용자 정의 MonoBehaviour / CharacterController 중 선택 |
| `GR-Player Component` | Dropdown | `(Default)`=CharacterController / 동일한 드롭다운 |

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

### 3. USD 파일에 커스텀 attribute 작성

```usda
#usda 1.0

def Xform "World"
{
    def Xform "Goblin"
    {
        custom bool gameReady:isNPC = true
        custom token gameReady:hasCollider = "Capsule"
        custom float3 gameReady:colliderSize = (50, 0, 180)   # radius=50, height=180
    }

    def Xform "Hero"
    {
        custom bool gameReady:isPlayer = true
        custom token gameReady:hasCollider = "Capsule"
        # colliderSize 없음 → MeshFilter bounds로 자동 산출
    }

    def Mesh "Wall"
    {
        custom token gameReady:hasCollider = "Box"
        custom float3 gameReady:colliderSize = (200, 20, 300)
    }

    def Mesh "TriggerZone"
    {
        custom token gameReady:hasCollider = "Box"
        custom float3 gameReady:colliderSize = (500, 100, 500)
        custom bool gameReady:isTrigger = true
    }
}
```

## 라이선스

MIT

# USDGameReady

Unity의 `com.unity.importer.usd` 패키지를 확장하여 USD 임포트 시 prim의 커스텀 attribute를 읽어 게임에 필요한 컴포넌트를 자동으로 부착하는 플러그인입니다.

## 기능

- **NPC 자동 설정**: `gameReady:isNPC = true` attribute가 있는 prim에 `NavMeshAgent` 컴포넌트 자동 부착
- **Collider 자동 설정**: `gameReady:hasCollider` attribute 값(`"Box"` / `"Sphere"` / `"Capsule"` / `"Mesh"`)에 따라 해당 Collider 자동 부착

## 요구사항

- Unity 6000.0 이상
- `com.unity.importer.usd` 1.0.0-pre.2 이상

## 설치

Unity의 Package Manager에서 **Add package from git URL** 선택 후:

```
https://github.com/<유저명>/com.usdgameready.git
```

## 사용법

### 1. 커스텀 임포터 그래프 생성

Unity 메뉴에서 **USDGameReady > Build USD Importer Graph** 클릭. `Assets/USDGameReady/usdGameReadyImporter.asset` 자산이 생성됩니다.

### 2. USD 파일에 그래프 적용

Project 창에서 `.usda` 파일 선택 → Inspector → **Graph** 필드를 `usdGameReadyImporter`로 변경 → **Apply**.

### 3. USD 파일에 커스텀 attribute 작성

```usda
def Xform "Goblin"
{
    custom bool gameReady:isNPC = true
    custom token gameReady:hasCollider = "Capsule"
}

def Mesh "Wall"
{
    custom token gameReady:hasCollider = "Box"
}
```

## 라이선스

MIT

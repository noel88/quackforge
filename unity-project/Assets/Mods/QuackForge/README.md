# Assets/Mods/QuackForge — AssetBundle source

이 디렉토리의 하위 폴더에 **AssetBundle 로 묶을 대상** (prefab / material / texture / sprite) 을 배치합니다.

## 서브 폴더 규약

- `Weapons/` — 무기 prefab, Item 컴포넌트 설정. 각 prefab 에 AssetBundle 태그 `quackforge_weapons`.
- `Armors/` — 방어구 prefab. AssetBundle 태그 `quackforge_armors`.
- `Icons/` — 아이콘 스프라이트. AssetBundle 태그 `quackforge_icons`.

AssetBundle 태그 지정은 Unity 에디터의 **Project 창 → 파일 선택 → Inspector 하단 드롭다운** 에서.

## 빌드

Unity 에디터 상단 메뉴 `QuackForge > Build AssetBundles (All)` 실행 또는 `scripts/build-mod.sh` 로 CLI 빌드. 산출물은 `mod/quackforge/{mac,windows}/`.

## 주의

- Duckov 베이스 prefab 을 추출해서 올릴 때 **typeID 를 모드 전용 범위** (예: 200001~) 로 변경. 순정 아이템 ID 와 충돌 금지.
- 새 Material/Shader 는 반드시 **URP** 셰이더 사용. Built-in 셰이더는 게임에서 분홍색으로 깨짐.
- 이 디렉토리 자체는 git 에 포함되지만 **대용량 아트 에셋 (>10MB)** 은 별도 Git LFS 또는 외부 공유 경로 고려.

# Unity2D_SteamMultigame


## 1. 프로젝트 개요
+ 프로젝트명:
+ 플랫폼: PC 스팀
+ 장르: 2인 턴제 대결 게임
+ 개발 기간:
+ 사용 엔진: Unity_2022.3.60f1
+ 네트워크 : Mirror + Steamworks.NET
+ 개발 인원: 1인 개발

## 2. 게임 설명
  + 이 게임은 2인의 플레이어가 숨겨진 위치에서 턴마다 공격을 수행하고 움직이며, 심리전과 추리를 통해 상대방을 찾아 처치하는 전략적 대전 게임입니다.

  + 게임 방법:
     타일 기반 맵에서 상대방의 위치를 유추

     타일 파괴를 통해 간접적으로 상대의 존재를 확인

    아이템과 추리를 통해 상대를 처치

## 3. 주요 시스템 및 기능 설명
+ 네트워크 매니저 (CustomNetworkManager.cs)
  - Mirror 기반의 네트워크 연결을 설정하고, 플레이어들을 관리합니다.

+ Steam 로비 (SteamLobby.cs)
  - Steamworks.NET을 이용한 P2P 매치메이킹 시스템.
  - 방 생성 / 참가 / 나가기 / 방 정보 동기화 처리.
  - RoomListItem을 생성하여 유저에게 방 목록을 UI로 제공.
  - Steam Lobby 데이터를 Mirror에 전달함으로써 연동 완료.

+ 아이템 시스템
  - ItemSpawner.cs: 맵에 아이템을 랜덤으로 생성함.
  - Item.cs: 아이템의 ID, 이름, 설명, 스프라이트 등 정의.
  - ItemDatabase.cs: 아이템 종류들을 관리.
  - ItemPickup.cs: 플레이어가 아이템을 획득할 때 효과 발동을 처리.
  - ItemEffect.cs: 다양한 아이템 효과가 인터페이스(IItemEffect) 기반으로 구현됨.

+ 맵 / 셀 시스템
  - CellController.cs: 각 셀의 파괴 여부, 플레이어가 있는지 여부 등을 관리.
  - 플레이어는 셀 위에 숨겨져 있으며, 셀 파괴 등을 통해 상대를 추적 가능.

+ 플레이어
  - 플레이어 이동, 상태(이동, 공격, 피격, 사망), 애니메이션 처리.
  - Mirror에서 상태 동기화를 통해 멀티플레이어 환경 구성.

+ UI 시스템
  - RoomListItem.cs: Steam Lobby의 각 방 정보를 UI로 표시.
  - PlayerSlot.cs : Steam 유저의 정보를 UI로 표시(닉네임, 프로필 사진).
    
## 4. 기술 문서
 https://docs.google.com/presentation/d/1p8HLOtRiHHoiiGIYJMOrIhoHaxApWBR2cliikdRgvl4/edit?slide=id.p#slide=id.p

## 5. 구현 중 겪은 문제와 해결 방법

## 6. 학습 및 성과

## 7. 스크린샷 및 영상

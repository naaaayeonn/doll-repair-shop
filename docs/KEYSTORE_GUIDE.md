# KEYSTORE_GUIDE.md — Android 키스토어 이중 백업 가이드

> **이 파일 자체는 리포에 커밋해도 됩니다 (가이드 문서).**
> **실제 .jks 파일과 비밀번호는 절대 커밋하지 마세요.**

## ⚠️ 키스토어를 잃어버리면 앱 업데이트가 영원히 불가능합니다

## 백업 위치 (2곳 필수)
1. **팀 클라우드**: (예: 팀 구글 드라이브 공유 폴더 → `🔐 DollShop_Keystore` 폴더)
2. **개인 오프라인**: USB 드라이브 또는 개인 암호화 스토리지

## 백업해야 할 것
- [ ] `dollshop_release.jks` — 키스토어 파일
- [ ] 키스토어 비밀번호
- [ ] 키 별칭(Key Alias)
- [ ] 키 비밀번호 (키스토어 비밀번호와 다를 수 있음)

## 키스토어 생성 명령어 (참고)
```bash
keytool -genkey -v \
  -keystore dollshop_release.jks \
  -alias dollshop \
  -keyalg RSA \
  -keysize 2048 \
  -validity 10000
```

## Play App Signing 등록
Google Play Console → 앱 서명 → Play 앱 서명 사용 선택.
한 번 등록하면 Google이 서명 키를 관리하므로 키스토어 분실 위험이 줄어듭니다.
권장 설정입니다.

## 버전별 빌드 코드 규칙
| 시점 | versionName | versionCode |
|---|---|---|
| W4 내부 테스트 | 0.4.0 | 4 |
| W5 알파(클로즈드) | 0.9.0 | 9 |
| W9 RC | 1.0.0 | 100 |
| W10 출시 | 1.0.0 | 100 |
| 핫픽스 | 1.0.1 | 101 |

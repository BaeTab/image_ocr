# 릴리즈 가이드

새 버전을 배포하고, 사용자의 앱이 **자동 업데이트**되도록 하는 절차입니다.

## 한 줄 요약

```bash
./scripts/release.sh 1.0.1
```

이 명령이 버전 갱신 → 단일 exe 게시 → 커밋/태그 푸시 → GitHub Release 생성(exe 첨부)까지 한 번에 처리합니다.

## 왜 로컬에서 빌드하나?

이 앱은 **DevExpress**(유료, 라이선스 NuGet 피드 필요)를 사용합니다. GitHub Actions 러너에는
DevExpress 피드/라이선스가 없어 CI 빌드가 실패합니다. 그래서 릴리즈용 exe 는 **DevExpress 가
설치된 로컬 PC에서 빌드**해 릴리즈 자산으로 올립니다.

## 사전 조건

- .NET SDK (`dotnet`)
- `gh` CLI 로그인 (`gh auth status` 로 확인, 필요 시 `gh auth login`)
- DevExpress NuGet 피드 접근이 되는 PC (이 PC)
- Git Bash (스크립트는 bash)

## 자동 절차 (`scripts/release.sh`)

```bash
# 기본
./scripts/release.sh 1.0.1

# 릴리즈 노트를 직접 작성한 파일로 지정
./scripts/release.sh 1.0.1 docs/notes/1.0.1.md
```

스크립트가 수행하는 일:

1. `image_ocr/image_ocr.csproj` 의 `<Version>` / `<FileVersion>` / `<AssemblyVersion>` 을 갱신
2. `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true`
3. 버전 변경 커밋 → `vX.Y.Z` 태그 → `origin` 푸시
4. `gh release create vX.Y.Z ImageOcr.exe` (노트 파일 없으면 직전 태그 이후 커밋으로 자동 생성)

> **버전 규칙:** 반드시 이전보다 **높은** 버전을 주세요. 앱은 `AssemblyVersion` 과 릴리즈 태그
> (`vX.Y.Z`)를 비교해 더 높은 릴리즈가 있을 때만 업데이트를 제안합니다.

## 자동 업데이트 동작 방식

- 앱은 시작 시 `https://api.github.com/repos/BaeTab/image_ocr/releases/latest` 를 조회합니다.
- 최신 릴리즈 태그(`vX.Y.Z`)가 현재 버전보다 높고, 자산에 **`ImageOcr.exe`** 가 있으면 업데이트를 제안합니다.
- 수락 시: 새 exe 를 `%TEMP%` 로 내려받고 → 현재 프로세스 종료를 기다렸다가 → 교체 후 재실행합니다.
- 구현: [`image_ocr/Update/Updater.cs`](../image_ocr/Update/Updater.cs)

### 릴리즈 시 반드시 지킬 것
- 자산 파일 이름은 **정확히 `ImageOcr.exe`** 여야 합니다(업데이터가 이 이름을 찾음).
- 태그는 **`vX.Y.Z`** 형식(예: `v1.0.1`).

## 수동 절차 (스크립트 없이)

```bash
export TMP="C:\\temp\\gradle-tmp"; export TEMP="C:\\temp\\gradle-tmp"

# 1) csproj 버전 수정 (<Version>, <FileVersion>, <AssemblyVersion>)

# 2) 단일 exe 게시
dotnet publish image_ocr/image_ocr.csproj -c Release -r win-x64 \
  --self-contained true -p:PublishSingleFile=true

# 3) 커밋 & 태그
git add image_ocr/image_ocr.csproj
git commit -m "chore: 버전 1.0.1 릴리즈"
git tag v1.0.1 && git push origin HEAD && git push origin v1.0.1

# 4) 릴리즈 생성
gh release create v1.0.1 \
  "image_ocr/bin/Release/net8.0-windows10.0.19041.0/win-x64/publish/ImageOcr.exe" \
  --title "이미지 OCR v1.0.1" --notes "..."
```

## 롤백

- 잘못된 릴리즈: `gh release delete v1.0.1 --cleanup-tag`
- 사용자에게 이미 배포됐다면, **더 높은** 버전으로 수정본을 다시 릴리즈하는 편이 안전합니다
  (자동 업데이트가 다시 최신으로 끌어올림).

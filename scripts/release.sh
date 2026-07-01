#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# 이미지 OCR 릴리즈 스크립트
#
# 하는 일:
#   1) csproj 의 버전을 X.Y.Z 로 갱신
#   2) 단일 exe(self-contained) 게시
#   3) 버전 변경 커밋 + vX.Y.Z 태그 푸시
#   4) GitHub Release 생성 후 ImageOcr.exe 첨부
#
# 사용법 (Git Bash 에서):
#   ./scripts/release.sh 1.0.1
#   ./scripts/release.sh 1.0.1 path/to/notes.md   # 릴리즈 노트 파일 지정(선택)
#
# 사전 조건: dotnet SDK, gh(로그인됨), git remote origin 설정.
# ---------------------------------------------------------------------------
set -euo pipefail

VERSION="${1:-}"
NOTES_FILE="${2:-}"

if [[ -z "$VERSION" ]]; then
  echo "사용법: $0 <X.Y.Z> [notes.md]" >&2
  exit 1
fi
if [[ ! "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
  echo "버전 형식 오류: '$VERSION' (예: 1.0.1)" >&2
  exit 1
fi

# 저장소 루트로 이동(이 스크립트는 scripts/ 아래에 있음).
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

CSPROJ="image_ocr/image_ocr.csproj"
TAG="v$VERSION"
EXE="image_ocr/bin/Release/net8.0-windows10.0.19041.0/win-x64/publish/ImageOcr.exe"

# 이 PC 고정 이슈: Java/dotnet 루프백 우회를 위한 TMP/TEMP.
export TMP="C:\\temp\\gradle-tmp"
export TEMP="C:\\temp\\gradle-tmp"
mkdir -p /c/temp/gradle-tmp

echo "==> 사전 점검"
command -v dotnet >/dev/null || { echo "dotnet 없음" >&2; exit 1; }
command -v gh >/dev/null || { echo "gh 없음" >&2; exit 1; }
gh auth status >/dev/null 2>&1 || { echo "gh 로그인 필요: gh auth login" >&2; exit 1; }
if git rev-parse "$TAG" >/dev/null 2>&1; then
  echo "이미 존재하는 태그: $TAG" >&2; exit 1
fi

echo "==> 버전 갱신: $VERSION"
# <Version>, <FileVersion>, <AssemblyVersion> 세 줄을 치환.
sed -i -E "s#<Version>[0-9.]+</Version>#<Version>${VERSION}</Version>#" "$CSPROJ"
sed -i -E "s#<FileVersion>[0-9.]+</FileVersion>#<FileVersion>${VERSION}.0</FileVersion>#" "$CSPROJ"
sed -i -E "s#<AssemblyVersion>[0-9.]+</AssemblyVersion>#<AssemblyVersion>${VERSION}.0</AssemblyVersion>#" "$CSPROJ"

echo "==> 단일 exe 게시"
dotnet publish "$CSPROJ" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
[[ -f "$EXE" ]] || { echo "게시 산출물 없음: $EXE" >&2; exit 1; }

echo "==> 릴리즈 노트 준비"
NOTES_ARG=()
if [[ -n "$NOTES_FILE" && -f "$NOTES_FILE" ]]; then
  NOTES_ARG=(--notes-file "$NOTES_FILE")
else
  TMP_NOTES="$(mktemp)"
  LAST_TAG="$(git describe --tags --abbrev=0 2>/dev/null || true)"
  {
    echo "## 이미지 OCR $TAG"
    echo
    echo "### 변경 사항"
    if [[ -n "$LAST_TAG" ]]; then
      git log "${LAST_TAG}..HEAD" --pretty="- %s" | grep -vE "Co-Authored-By|Claude-Session" || echo "- 세부 변경 내역 없음"
    else
      echo "- 첫 릴리즈"
    fi
    echo
    echo "### 설치"
    echo "아래 **ImageOcr.exe** 를 내려받아 더블클릭하세요 (설치 불필요, 단일 실행 파일)."
  } > "$TMP_NOTES"
  NOTES_ARG=(--notes-file "$TMP_NOTES")
fi

echo "==> 버전 커밋 & 태그"
git add "$CSPROJ"
git commit -m "chore: 버전 ${VERSION} 릴리즈" || echo "(변경 없음 — 커밋 생략)"
git tag "$TAG"
git push origin HEAD
git push origin "$TAG"

echo "==> GitHub Release 생성"
gh release create "$TAG" "$EXE" --title "이미지 OCR $TAG" "${NOTES_ARG[@]}"

echo "==> 완료: https://github.com/BaeTab/image_ocr/releases/tag/$TAG"

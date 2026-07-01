# 이미지 OCR

이미지에서 텍스트를 추출하는 Windows 데스크톱 프로그램(.NET 8 · WinForms · DevExpress).
이미지를 **열거나 드래그앤드롭**하면 안의 글자를 텍스트로 뽑아 줍니다. 한국어 + 영어를 지원합니다.

## 다운로드 (설치 불필요)

[**최신 릴리즈**](https://github.com/BaeTab/image_ocr/releases/latest)에서 `ImageOcr.exe` 를 내려받아
더블클릭하세요. .NET 설치가 필요 없는 **단일 실행 파일**(self-contained)이며, 이후 새 버전이 나오면
**자동 업데이트**를 제안합니다.

## 주요 기능

- 📂 **이미지 열기** — 파일 대화상자(`Ctrl+O`) 또는 실행 시 파일 경로 인자
- 🖱️ **드래그앤드롭 / 📋 붙여넣기** — 창에 이미지 파일을 끌어다 놓거나 클립보드 이미지(`Ctrl+V`)
- 🔀 **엔진 선택 & 비교** — 두 OCR 엔진을 드롭다운에서 골라 결과 비교
  - **Tesseract**(기본) — 오픈소스, `kor+eng` 고정밀(tessdata_best) 데이터, 문서/긴 텍스트에 강함
  - **Windows 내장 OCR** — Windows 10/11 내장, 별도 설치 불필요, 빠름
- 🌐 **언어 선택** — `tessdata` 를 자동 감지해 Tesseract 인식 언어를 런타임에 전환
- ✂️ **영역 선택 OCR** — 이미지 위를 드래그해 사각형 영역만 인식(작게 클릭하면 해제)
- 🟩 **단어 하이라이트** — `단어 표시` 를 켜면 인식된 단어를 이미지 위에 박스로 오버레이
- 🎛️ **전처리 토글** — `흑백 · 대비 · 이진화 · 2x 확대` 조합으로 저해상/사진 정확도 향상
- 🗂️ **여러 이미지 일괄 처리** — 다중 선택 → 각 이미지 옆에 `*_ocr.txt` 저장
- 📄 결과 **복사** / **저장**(`.txt`, UTF-8, `Ctrl+S`)
- 💾 **설정 저장** — 엔진·언어·전처리·창 크기를 자동 기억
- 🔄 **자동 업데이트** — GitHub Releases 기반, 시작 시 새 버전 확인 후 원클릭 교체
- 지원 형식: `png, jpg, jpeg, bmp, gif, tif, tiff, webp`

## 단축키

| 키 | 동작 |
|---|---|
| `Ctrl+O` | 이미지 열기 |
| `Ctrl+V` | 클립보드에서 붙여넣기 |
| `F5` | 텍스트 추출 |
| `Ctrl+S` | 결과 저장 |

> 이미지 위에서 **마우스 드래그**로 영역을 선택하면 그 영역만 인식합니다.

## 소스에서 빌드

Visual Studio 에서 `image_ocr.sln` 을 열고 실행하거나, 명령줄에서:

```bash
# 빌드 (Windows OCR API 때문에 win-x64 로 빌드됨)
dotnet build image_ocr/image_ocr.csproj -c Release

# 실행 (선택적으로 이미지 경로를 인자로 주면 시작 시 자동 로드)
dotnet run --project image_ocr/image_ocr.csproj -- "C:\path\to\image.png"
```

> 이 PC 고정 이슈로 빌드 전 `TMP`/`TEMP` 를 `C:\temp\gradle-tmp` 로 지정해야 할 수 있습니다.
> 릴리즈 배포는 [`docs/RELEASE_GUIDE.md`](docs/RELEASE_GUIDE.md) 참고 (`./scripts/release.sh 1.2.0`).

## 엔진별 준비물

### Tesseract (기본)
실행 파일 옆 `tessdata\` 폴더(또는 단일 exe 내부에 임베드)의 `*.traineddata` 를 사용합니다.
기본 포함: `kor`, `eng` (tessdata_best).
- 다른 언어 추가: [tessdata_best](https://github.com/tesseract-ocr/tessdata_best)
  에서 `<lang>.traineddata` 를 받아 `image_ocr/tessdata/` 에 넣으면 프로그램이 자동 감지합니다.

### Windows 내장 OCR
언어 인식팩이 설치돼 있어야 합니다. 없으면 드롭다운에 `(사용 불가)` 로 표시됩니다.
- 설정 → 시간 및 언어 → 언어 및 지역 → 해당 언어 → 언어 옵션 → **광학 문자 인식(OCR)** 추가

## 프로젝트 구조

```
image_ocr/
├─ Program.cs              진입점 (+ --selftest 헤드리스 검증 모드)
├─ Form1.cs                UI 로직 (열기/드롭/영역선택/전처리/일괄/저장)
├─ Form1.Designer.cs       화면 레이아웃(2행 툴바)
├─ AppSettings.cs          설정 보존(%APPDATA%\ImageOcr\settings.json)
├─ Controls/
│  └─ ImageCanvas.cs       이미지 표시 · 영역 선택 · 단어 하이라이트
├─ Ocr/
│  ├─ IOcrEngine.cs        엔진 인터페이스 + OcrResult(단어 좌표 포함)
│  ├─ WindowsOcrEngine.cs  Windows.Media.Ocr 구현
│  ├─ TesseractOcrEngine.cs Tesseract 구현(런타임 언어 전환)
│  ├─ ImagePreprocessor.cs 흑백/대비/이진화/확대 전처리
│  └─ RuntimeAssets.cs     단일 exe 용 tessdata·네이티브 DLL 추출
├─ Update/
│  └─ Updater.cs           GitHub Releases 자동 업데이트
└─ tessdata/               kor.traineddata, eng.traineddata (best)
```

## 개발자용: 헤드리스 셀프테스트

GUI 없이 두 엔진을 한 번에 검증합니다. 이미지 경로가 없으면 샘플을 자동 생성합니다.

```bash
ImageOcr.exe --selftest <이미지경로> <결과txt경로>
```

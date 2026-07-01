# 이미지 OCR

이미지에서 텍스트를 추출하는 Windows 데스크톱 프로그램(.NET 8 · WinForms · DevExpress).
이미지를 **열거나 드래그앤드롭**하면 안의 글자를 텍스트로 뽑아 줍니다. 한국어 + 영어를 지원합니다.

## 주요 기능

- 📂 **이미지 열기** — 파일 대화상자(`Ctrl+O`) 또는 실행 시 파일 경로 인자
- 🖱️ **드래그앤드롭** — 창 아무 곳에나 이미지 파일을 끌어다 놓기
- 📋 **붙여넣기** — 클립보드 이미지/파일(`Ctrl+V`), 예: 캡처 도구로 복사한 화면
- 🔀 **엔진 선택 & 비교** — 두 OCR 엔진을 드롭다운에서 골라 결과 비교
  - **Windows 내장 OCR** — Windows 10/11 내장, 별도 설치·다운로드 불필요, 빠름
  - **Tesseract** — 오픈소스, `tessdata`의 학습데이터 사용, 문서/긴 텍스트에 강함
- 📄 결과 **복사** / **저장**(`.txt`, UTF-8, `Ctrl+S`)
- 지원 형식: `png, jpg, jpeg, bmp, gif, tif, tiff, webp`

## 실행

Visual Studio 에서 `image_ocr.sln` 을 열고 실행하거나, 명령줄에서:

```bash
# 빌드 (Windows OCR API 때문에 win-x64 로 빌드됨)
dotnet build image_ocr/image_ocr.csproj -c Release

# 실행 (선택적으로 이미지 경로를 인자로 주면 시작 시 자동 로드)
dotnet run --project image_ocr/image_ocr.csproj -- "C:\path\to\image.png"
```

> 이 PC 고정 이슈로 빌드 전 `TMP`/`TEMP` 를 `C:\temp\gradle-tmp` 로 지정해야 할 수 있습니다.

## 단축키

| 키 | 동작 |
|---|---|
| `Ctrl+O` | 이미지 열기 |
| `Ctrl+V` | 클립보드에서 붙여넣기 |
| `F5` | 텍스트 추출 |
| `Ctrl+S` | 결과 저장 |

## 엔진별 준비물

### Windows 내장 OCR
언어 인식팩이 설치돼 있어야 합니다. 없으면 드롭다운에 `(사용 불가)` 로 표시됩니다.
- 설정 → 시간 및 언어 → 언어 및 지역 → 해당 언어 → 언어 옵션 → **광학 문자 인식(OCR)** 추가
- 한국어 인식팩이 있으면 자동으로 한국어를 우선 사용합니다.

### Tesseract
실행 파일 옆 `tessdata\` 폴더의 `*.traineddata` 를 사용합니다. 기본 포함: `kor`, `eng`.
- 다른 언어를 추가하려면 [tessdata_fast](https://github.com/tesseract-ocr/tessdata_fast)
  (또는 정확도 우선이면 [tessdata_best](https://github.com/tesseract-ocr/tessdata_best))
  에서 `<lang>.traineddata` 를 받아 `image_ocr/tessdata/` 에 넣으면 됩니다.
  프로그램이 폴더의 학습데이터를 자동 감지해 `kor+eng` 처럼 합쳐 인식합니다.

## 프로젝트 구조

```
image_ocr/
├─ Program.cs              진입점 (+ --selftest 헤드리스 검증 모드)
├─ Form1.cs                UI 로직 (열기/드롭/붙여넣기/실행/복사/저장)
├─ Form1.Designer.cs       화면 레이아웃
├─ Ocr/
│  ├─ IOcrEngine.cs        엔진 공통 인터페이스 + OcrResult
│  ├─ WindowsOcrEngine.cs  Windows.Media.Ocr 구현
│  └─ TesseractOcrEngine.cs Tesseract 구현
└─ tessdata/               kor.traineddata, eng.traineddata
```

## 개발자용: 헤드리스 셀프테스트

GUI 없이 두 엔진을 한 번에 검증합니다. 이미지 경로가 없으면 샘플을 자동 생성합니다.

```bash
ImageOcr.exe --selftest <이미지경로> <결과txt경로>
```

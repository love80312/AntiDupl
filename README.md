![AntiDupl screenshot](https://ermig1979.github.io/AntiDupl/data/help/english/files/MainForm.png)

# AntiDupl.NET — ＡＩ個人化修正版本

這是基於 [ErmIg/AntiDupl](https://github.com/ermig1979/AntiDupl) 開源專案的ＡＩ個人化修正版本，主要分支為 `a`。

本專案提供圖片重複搜尋、相似圖片比對及瑕疵圖片檢查功能，並持續整合上游更新、改善操作介面、補充繁體中文翻譯及調整建置流程。

- **版本：** 2.4.0
- **主要分支：** `a`
- **目標平台：** Windows x64
- **主要技術：** C#、.NET 10、C++、Windows Forms、WPF
- **建置方案：** `src/AntiDupl.sln`

---

## 主要功能

### 1. 重複及相似圖片搜尋

搜尋電腦中的重複圖片及內容相似的圖片，不只比較檔案名稱，也會依圖片內容進行分析。

支援的常見圖片格式：

- JPEG、GIF、TIFF、BMP、PNG
- EMF、WMF、WEBP、EXIF、ICON
- JP2、PSD、DDS
- HEIF、HEIC、TGA
- AVIF、JXL

### 2. 圖片比對演算法

包含多種圖片比較方式：

- **SquaredSum：** 平方差異總和。
- **SSIM：** 結構相似性比較。
- **LocalColor：** 局部色彩差異比較。

其他比對功能：

- 位移圖片搜尋及位移補償。
- 可調整圖片差異門檻。
- 小於 1% 的差異門檻支援更細的設定級距。
- 忽略圖片邊緣指定寬度，降低邊框等因素造成的比對影響。

### 3. 重複檔案及硬連結辨識

改善重複檔案的分類與顯示：

- **Hard Link：** 識別指向相同檔案內容的硬連結。
- **Byte Identical：** 檔案位元組內容完全相同。
- **Pixel Identical：** 圖片像素相同，但檔案內容不同。
- 可設定略過硬連結的重複檔案。
- 硬連結結果會以明顯標記顯示，並調整排序優先級。

### 4. 瑕疵圖片檢查

除重複搜尋外，也可檢查部分圖片瑕疵，例如：

- 圖片模糊。
- 區塊化（Blockiness）。
- JPEG 結束標記遺失。
- 無法讀取或無法解碼的圖片。

無法解碼的圖片也能列為瑕疵結果，方便後續檢查。

### 5. 結果管理與檔案操作

- 支援一次新增多個搜尋資料夾。
- 支援忽略資料夾及檔案名稱篩選。
- 支援正規表示式檔名篩選。
- 支援結果復原及重做。
- 支援刪除、重新命名、移動及圖片替換。
- 刪除或替換操作使用資源回收筒處理，降低誤刪風險。
- 支援長路徑檔案的資源回收筒處理。

### 6. 圖片差異檢視

- 顯示圖片差異區域。
- 可調整差異標記的門檻及顯示方式。
- 支援垂直及水平圖片配對檢視。
- 支援圖片群組縮圖檢視。
- 可切換圖片對齊分隔線的方式。
- 支援顯示相鄰圖片。
- 可設定兩組外部圖片差異比較工具。

### 7. 介面與語言

專案包含兩種 Windows 桌面介面：

**Windows Forms**
- 垂直配對表格。
- 水平配對表格。
- 群組縮圖。
- 圖片差異標記及檢視設定。
- 淺色、深色及依照 Windows 系統設定的佈景主題。
- 繁體中文介面資源。

**WPF**
- 獨立的圖片搜尋及結果操作介面。
- 可設定的圖片處理動作。
- 深度搜尋相關模組整合。
- LightGlue、DINOv2 相關模型設定及狀態訊息。
- 繁體中文介面資源。

繁體中文資源持續補充中；不同介面及功能的翻譯完整度可能有所差異。

---

## 專案結構

| 路徑 | 用途 |
|---|---|
| `src/AntiDupl/` | C++ 圖片處理及比較核心 |
| `src/AntiDupl.NET.Core/` | .NET 核心介面與資料結構 |
| `src/AntiDupl.NET.WinForms/` | Windows Forms 介面 |
| `src/AntiDupl.NET.WPF/` | WPF 介面 |
| `src/AntiDupl.NET.Deep/` | 深度搜尋相關功能 |
| `src/AntiDupl.NET.Onnx/` | ONNX 模型及推論整合 |
| `src/AntiDupl.NET.Deep.Tests/` | 深度搜尋測試 |
| `data/resources/` | 程式語言及圖片等資源 |
| `docs/` | 文件及介面資源 |
| `cmd/` | 建置及封裝腳本 |
| `.github/workflows/` | GitHub Actions 工作流程 |

## 建置環境

需要準備：

- Windows x64。
- 支援 .NET 10 Windows Desktop 的 Visual Studio / MSBuild。
- Visual Studio C++ 桌面開發工具。
- NuGet。
- vcpkg。

### vcpkg 依賴項目

專案使用 `src/vcpkg.json` 管理原生依賴，包含：

- libjpeg-turbo
- openjpeg
- libwebp
- libheif
- simd
- libavif（dav1d）
- libjxl
- xxhash

vcpkg 使用專案指定的 baseline，建議保留原設定，不要隨意更新依賴版本。

### 建置步驟

1. 下載並安裝 Visual Studio，選擇 .NET 桌面及 C++ 桌面開發工具。
2. 安裝 Git。
3. 下載 vcpkg：

```bat
git clone https://github.com/microsoft/vcpkg.git
cd vcpkg
bootstrap-vcpkg.bat
vcpkg integrate install
```

4. 開啟方案：

```text
src\AntiDupl.sln
```

5. 選擇 `x64` 平台及需要的組態。
6. 還原 NuGet 套件及 vcpkg 依賴後進行建置。

### Publish 建置

專案提供 `AntiDuplPublishSingleFile` 發佈設定。

GitHub Actions 使用的主要建置命令：

```bat
msbuild src\AntiDupl.sln /p:Configuration=Publish /p:Platform=x64 -restore -t:Publish -p:PublishProfile="AntiDuplPublishSingleFile"
```

完成 Publish 後，執行：

```bat
cd cmd
MakePublish.cmd
```

封裝腳本會整理程式資源及原生 DLL，並產生：

- SingleFilePortable 自解壓縮執行檔。
- SingleFilePortable ZIP。
- SHA-256 雜湊資訊檔。

WinRAR 不存在時，會嘗試使用專案內附的 7-Zip 工具。

---

## GitHub Actions

工作流程：

```text
.github/workflows/A_CI.yml
```

目前工作流程設定：

- Windows Latest 執行環境。
- NuGet 套件還原。
- vcpkg 依賴整合。
- x64 Publish 建置。
- SingleFilePortable 封裝。
- 上傳 ZIP Artifact。

目前自動 Push 觸發分支設定為 `as`，另提供 `workflow_dispatch` 手動執行。

因此，若要建置 `a` 分支，請至 GitHub：

1. 開啟 Actions。
2. 選擇 AntiDupl Personal Build。
3. 點選 Run workflow。
4. 選擇 `a` 分支。
5. 執行建置。

`a_float` 也可用相同方式手動建置。

Artifact 名稱格式：

```text
AntiDupl.NET-<版本>-SingleFilePortable
```

---

## 分支說明

| 分支 | 用途 |
|---|---|
| `a` | 主要個人維護及建置分支 |
| `a_float` | 額外加入圖片懸浮放大預覽的功能分支 |
| `master` | ＡＩ個人化修正版本的其他整合分支 |

注意：圖片懸浮放大預覽目前屬於 `a_float` 的額外修改，尚未包含在 `a` 分支。

---

## 專案來源與授權

原始專案：

[ErmIg / AntiDupl](https://github.com/ermig1979/AntiDupl)

ＡＩ個人化修正版本：

[love80312 / AntiDupl](https://github.com/love80312/AntiDupl)

授權及著作權資訊請參閱專案根目錄的 `LICENSE`。

本版本包含上游程式碼及個人修改，功能與建置狀態應以各分支實際內容及 GitHub Actions 結果為準。

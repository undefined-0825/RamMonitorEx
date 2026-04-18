# MultiLayoutGridControl 仕様書（実装反映）

## 1. 目的
高速描画可能なマルチレイアウトグリッドの UserControl を C#（WinForms）で提供する。  
DataGridView は使用せず、完全カスタム描画とする。

---

## 2. 基本方針
- ダブルバッファリングを使用
- OnPaint で一括描画
- レイアウト計算と描画を分離
- データ（Row/Cell）と表示を分離
- 配置: `Controls/MultiLayoutGridControl/`

---

## 3. 実装クラス構成

### 3.1 MultiLayoutGridControl : UserControl
- 描画と入力イベント管理
- コンテキストメニュー管理
- リサイズ処理（セル幅）
- プロパティ画面呼び出し

#### 主なプロパティ
- `List<GridRow> Rows`
- `Color BackColor`（初期値: Black）
- `Color ForeColor`（初期値: White）
- `bool ShowGridLines`（初期値: true）
- `Color GridLineColor`（初期値: Gray）

#### 主なメソッド
- `RequestRedraw()`
- `CalculateLayout()`
- `DrawGrid(Graphics g)`
- `DrawGridLines(Graphics g)`

### 3.2 GridRow
- 行定義
- `int Height`
- `List<GridCell> Cells`
- `Rectangle Bounds`（内部キャッシュ）

### 3.3 GridCell
- セル定義
- `string Text`
- `Func<string>? ValueProvider`
- `int Width`
- `ContentAlignment HorizontalAlignment`
- `ContentAlignment VerticalAlignment`
- `Color? ForeColor`
- `Rectangle Bounds`（内部キャッシュ）
- `GetDisplayText()`

---

## 4. 描画仕様
描画順序:
1. 背景塗りつぶし
2. レイアウト計算
3. 行・セル描画（文字描画）
4. グリッド線描画（`ShowGridLines=true` の場合）

文字描画:
- `TextRenderer.DrawText` を使用

---

## 5. レイアウト仕様
- 各行・各セルは独立サイズ
- 列幅はセル単位で保持
- 行高さ・セル幅は動的変更可能
- セル/行の Rectangle を都度再利用（レイアウト計算で更新）

---

## 6. マウス操作
### 6.1 セル幅リサイズ
- セル右端ドラッグで幅変更
- ヒットテスト幅: 5px
- 最小幅: 30px

### 6.2 コンテキストメニュー（右クリック）
- `プロパティ...`
- `グリッド線を表示`（チェック切替）
- `背景色...`
- `文字色...`
- `グリッド線の色...`

---

## 7. プロパティ画面

### 7.1 MultiLayoutGridPropertiesForm（グリッド単位）
- 行一覧表示
- 行の追加 / 削除 / 編集
- 行の順序変更（上へ / 下へ）
- 行高さの変更

### 7.2 RowPropertiesForm（行単位）
- 列（セル）一覧表示
- 列の追加 / 削除 / 編集
- 列の順序変更（左へ / 右へ）

### 7.3 CellPropertiesForm（列単位）
- 固定文字列 / 可変値の切替
- テキスト変更
- 幅変更
- 水平/垂直配置変更
- 文字色変更（カスタム色 ON/OFF）

---

## 8. スタイル（文字色優先順位）
1. セル個別 `ForeColor`
2. コントロール `ForeColor`

---

## 9. データ更新
- `ValueProvider` が設定されている場合、描画時に値を取得
- 高頻度更新は `RequestRedraw()` を外部から呼び出して制御

---

## 10. DockPanel 連携
### 10.1 MultiLayoutGridPane : DockContent
- `GetPersistString()` は `MultiLayoutGridPane|{PaneName}`
- `PaneNameManager` で名前管理
- ファイルメニューから新規作成可能

### 10.2 メニュー項目
- `新規マルチレイアウトグリッド(&L)...`
- 名前入力ダイアログ経由で作成

---

## 11. XML シリアライズ対応
他ペイン同様、ワークスペース保存/復元に対応。

### 11.1 保存対象
- ペイン名
- グリッド設定
  - 背景色 / 前景色
  - グリッド線表示有無
  - グリッド線色
- 行設定
  - 行順序
  - 行高さ
- セル設定
  - セル順序
  - Text
  - IsVariable
  - Width
  - HorizontalAlignment
  - VerticalAlignment
  - ForeColor の有無と色

### 11.2 設定クラス
- `MultiLayoutGridPaneConfig`
- `MultiLayoutGridConfig`
- `MultiLayoutGridRowConfig`
- `MultiLayoutGridCellConfig`

---

## 12. 補足
- 可変値（`IsVariable=true`）の具体的なデータソースはアプリ側で注入可能
- 復元時の可変値は現在デモ用プロバイダー（乱数）で再設定
# ReadElf 機能仕様（初版）

## 1. 目的
ELF ファイルから以下の情報を抽出し、アプリ内で利用可能にする。
- シンボル名
- シンボルサイズ
- シンボルアドレス

本仕様は、`spec/ToDo.md` の「ELFファイルの解析」タスクの実装指針とする。

---

## 2. 対象範囲
### 2.1 対象
- ELF ヘッダの読み取り
- セクションヘッダの読み取り
- シンボルテーブル（`.symtab` / `.dynsym`）の読み取り
- 文字列テーブル（`.strtab` / `.dynstr`）によるシンボル名解決
- シンボル名・サイズ・アドレスの抽出

### 2.2 非対象（初版）
- DWARF 解析
- 再配置情報の詳細解析
- 書き込み/編集（読み取り専用）

---

## 3. 対応 ELF 形式
初版では以下を必須対応とする。
- ELF32 / ELF64
- リトルエンディアン（ELFDATA2LSB）

以下は将来対応（未対応）
- ビッグエンディアン（ELFDATA2MSB）

---

## 4. 読み取りフロー
1. ファイルをバイナリで開く
2. ELF ヘッダを読み取る
   - マジック (`0x7F 'E' 'L' 'F'`) 検証
   - クラス（32/64bit）判定
   - エンディアン判定
3. セクションヘッダテーブルを読み取る
4. セクション名文字列テーブル（`e_shstrndx`）を読み取り、セクション名解決
5. シンボルテーブル候補を探索（`.symtab`, `.dynsym`）
6. 各シンボルテーブルに紐づく文字列テーブルを読み取り（`sh_link`）
7. シンボルエントリを順次読み取り
8. 各エントリから「名前・サイズ・アドレス」を抽出
9. 表示用データに整形して返却

---

## 5. 抽出ルール
### 5.1 シンボル名
- `st_name` を文字列テーブルのオフセットとして解決
- 名前解決不可の場合は空文字または `"<noname>"` を使用

### 5.2 サイズ
- `st_size` をそのまま使用（整数）

### 5.3 アドレス
- `st_value` をそのまま使用
- 表示時は 16進表記（例: `0x00001234`）を推奨

### 5.4 フィルタ（初版推奨）
- `st_name == 0` かつサイズ0のエントリは表示から除外可能
- 必要に応じて `STT_FUNC` / `STT_OBJECT` のみ抽出できる拡張余地を残す

---

## 6. データモデル（案）
```csharp
public sealed class ElfParseResult
{
    public string FilePath { get; set; } = string.Empty;
    public bool IsElf { get; set; }
    public bool Is64Bit { get; set; }
    public bool IsLittleEndian { get; set; }
    public List<ElfSymbolInfo> Symbols { get; set; } = new();
}

public sealed class ElfSymbolInfo
{
    public string Name { get; set; } = string.Empty;
    public ulong Address { get; set; }
    public ulong Size { get; set; }
}
```

---

## 7. エラーハンドリング
以下は例外または失敗結果として返す。
- ELF マジック不一致
- クラス不正（ELF32/ELF64 以外）
- エンディアン未対応
- セクション/文字列テーブルの範囲外アクセス
- 破損ファイル（サイズ不足）

エラーメッセージには、可能な限り「どの段階で失敗したか」を含める。

---

## 8. 表示機能（ToDo連携）
抽出結果を一覧表示する。
- 列: `Name`, `Address`, `Size`
- ソート: 名前順/アドレス順（実装時に選択）
- ファイル読み込み後に即時反映

---

## 9. 実装分割（推奨）
1. `ElfReader`（バイナリ読み取りと基本検証）
2. `ElfSectionParser`（セクションヘッダ解析）
3. `ElfSymbolParser`（シンボル抽出）
4. `ElfParseResult` への統合
5. 画面表示層へ連携

---

## 10. 受け入れ条件（初版）
- 有効な ELF を読み込んでシンボル一覧を取得できる
- シンボル名・サイズ・アドレスが取得できる
- 不正ファイル時に異常終了せず、エラーが通知される
- `sample_elf_data.elf` で結果表示できる

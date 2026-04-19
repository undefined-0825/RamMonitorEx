# ReadElf 機能仕様（実装反映版）

## 1. 目的
ELF ファイルからシンボル情報を抽出し、UI上で選択・表示・利用可能にする。

抽出項目:
- シンボル名
- シンボルアドレス
- シンボルサイズ
- 取得元シンボルテーブル名（`.symtab` / `.dynsym` など）

---

## 2. 実装コンポーネント

### 2.1 ReadElfParser
ファイル: `ReadElf/ReadElfParser.cs`

主クラス:
- `ReadElfParser`
- `ReadElfResult`
- `ElfSymbolInfo`

主メソッド:
- `ReadElfResult Parse(string filePath)`

### 2.2 UI
- `Forms/ElfSymbolSelectionForm.cs`
  - ELFファイル選択、一覧表示、絞り込み、選択返却
- `Docking/ElfSymbolPane.cs`
  - 選択結果を表示する専用ペイン

---

## 3. 対応形式
- ELF32 / ELF64
- Little Endian（ELFDATA2LSB）

未対応:
- Big Endian（ELFDATA2MSB）

---

## 4. 解析フロー
1. ELFマジック検証
2. ELFクラス（32/64）判定
3. エンディアン判定
4. ELFヘッダ読み取り
5. セクションヘッダ読み取り
6. セクション名文字列テーブル解決（`e_shstrndx`）
7. `.symtab` / `.dynsym` を探索
8. `sh_link` で文字列テーブルを参照し、シンボル名を解決
9. シンボル情報を抽出
10. 重複除去・アドレス順整列

---

## 5. データモデル
```csharp
public sealed class ReadElfResult
{
    public string FilePath { get; set; }
    public bool Is64Bit { get; set; }
    public bool IsLittleEndian { get; set; }
    public List<ElfSymbolInfo> Symbols { get; set; }
}

public sealed class ElfSymbolInfo
{
    public string Name { get; set; }
    public ulong Address { get; set; }
    public ulong Size { get; set; }
    public string SourceTable { get; set; }
}
```

---

## 6. UI仕様

### 6.1 ELFシンボル選択画面（ElfSymbolSelectionForm）
- OpenFileDialog で ELF ファイル選択
- DataSet + DataTable + DataView + BindingSource を使用
- DataGridView表示列:
  - 選択（チェックボックス）
  - シンボル名
  - アドレス（16進）
  - サイズ
  - ソーステーブル
- シンボル名フィルタ（部分一致）
- LinkLabel:
  - 全て選択
  - 全て選択解除
- OK / キャンセル
- OK押下で選択シンボルを返却

### 6.2 専用ペイン（ElfSymbolPane）
- 選択済みシンボルを DockContent として表示
- 同様に DataView でフィルタ可能

---

## 7. 注意事項（0値シンボルについて）
`.dynsym` に含まれる外部参照シンボル（未定義シンボル）は、
- `Address = 0`
- `Size = 0`
となる場合がある。

これは関数かどうかではなく、ELF内で未定義（実体が別共有ライブラリ側）であることに起因する。

---

## 8. エラーハンドリング
以下は例外として扱う。
- ELFマジック不一致
- 未対応クラス
- 未対応エンディアン
- 範囲外アクセス（破損ファイルなど）
- ファイル未存在

---

## 9. 現状の未対応
- ElfSymbolPane のシンボル内容をワークスペースXMLへ完全保存/復元
- シンボル種別（FUNC/OBJECT）や未定義判定のUI表示列

---

## 10. 受け入れ条件
- ELFファイル選択後、シンボル一覧が表示される
- Name/Address/Size が取得できる
- シンボル名絞り込みが機能する
- チェック選択結果を呼び出し元へ返却できる
- 選択結果を専用ペインで表示できる

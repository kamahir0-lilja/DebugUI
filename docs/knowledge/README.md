# 技術知見

非自明な調査や、将来の修正で再び遭遇しやすい問題を残す。
期待する振る舞いの正本は [製品仕様](../specs/README.md)、資料の保持・更新ルールは [文書方針](../contributing/documentation-policy.md) にある。

| メモ | 内容 |
| --- | --- |
| [UI Toolkitのボタンとスクローラー](ui-toolkit-button-interactions.md) | Clickable、Capture解放、カスタムプロパティ、テーマの読み込み順 |
| [Editorパネルへのページ移動](ui-toolkit-editor-panels.md) | テーマ変数、フォーカス、複合入力、テキスト描画 |

確認状態と対象環境は各メモの冒頭を参照する。資料の配置換えだけで現行での再検証済みとはしない。

新しいメモや非自明な追記には [_template.md](_template.md) の項目を必要な範囲で使う。
小さなWHYが近接コメントとテストだけで十分に復元できるなら、独立したメモは作らない。

## 移行前のパスと現在の実装

古いメモの`src/...`は当時の配置として残している。現在の関連箇所は次のとおり。
クラスやAPIが同じ名前でも、過去の観察が現在も成立することを意味しない。

- 入力・ボタン補助: [DebugControls.cs](../../Packages/com.kamahir0.debug-ui/Runtime/Controls/DebugControls.cs)、[DebugControlHelpers.cs](../../Packages/com.kamahir0.debug-ui/Runtime/Controls/DebugControlHelpers.cs)。
- ページとスクローラー: [DebugPage.cs](../../Packages/com.kamahir0.debug-ui/Runtime/Navigation/DebugPage.cs)。
- Editor側の表示: [DebugMenuEditorWindow.cs](../../Packages/com.kamahir0.debug-ui/Editor/DebugMenuEditorWindow.cs)、[EditorPageNavigator.cs](../../Packages/com.kamahir0.debug-ui/Editor/EditorPageNavigator.cs)。
- テーマ: [Runtime/StyleSheets](../../Packages/com.kamahir0.debug-ui/Runtime/StyleSheets)、[Editor/StyleSheets](../../Packages/com.kamahir0.debug-ui/Editor/StyleSheets)。

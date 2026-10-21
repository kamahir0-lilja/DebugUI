# DebugUI 開発ドキュメント

仕様を決める情報、実装の知見、実際に検証した結果を分けて参照する。
すべてを毎回読む必要はない。

| 知りたいこと | 参照先 |
| --- | --- |
| 今回の目的と範囲 | 現在のユーザー依頼・Issue・PR。仕様本文を作業報告へ複製しない |
| 製品として期待する振る舞い | [仕様一覧](specs/README.md)のApproved / Implemented仕様 |
| 仕様を具体化・変更する手順 | [仕様ワークフロー](contributing/specification-workflow.md) |
| 文書の配置と知見の再利用 | [文書方針](contributing/documentation-policy.md) |
| 未採用の仕様案・変更理由 | [仕様変更](spec-changes/README.md) |
| Runtime / Editor共有設計の理由 | [ページ所有権のADR](adr/0001-runtime-editor-page-ownership.md) |
| 実操作と画像レビューによる完了確認 | [UI検証ワークフロー](contributing/ui-validation-workflow.md)、[レポートひな形](contributing/ui-validation-report-template.md) |
| 蓄積した技術調査を再利用する | [技術知見の索引](knowledge/README.md)。Reference資料で、各メモのVerificationと適用条件を確認 |
| 過去の検証結果を調べる | [2026-10-21の検証記録](evidence/2026-10-21-debug-ui-validation.md)。Historicalな実行結果 |
| 当時の説明や発表案を調べる | [発表資料](archive/presentations/debug-menu-ui-toolkit.md)。Archived資料で、現行仕様へ追従しない |
| パッケージの使い方・独立導入検証 | [README](../README.md) |

## 配置の見方

```text
docs/
  contributing/  現行の開発・検証手順とひな形
  specs/         製品の期待結果
  spec-changes/  仕様変更の提案・採用理由
  adr/           設計選択と理由
  knowledge/     適用条件付きの技術調査・知見
  evidence/      日付・対象状態を固定した検証の要約
  archive/       現行へ追従しない発表・過去資料
```

資料を追加・更新・退役させるときは [文書方針](contributing/documentation-policy.md) を参照する。
知見を読んだだけで製品仕様を変更したり、過去の検証結果で今回の変更を合格にしたりしない。

## スキルとの関係

仕様駆動の各活動は [refine-spec](../.agents/skills/refine-spec/SKILL.md)、
[review-spec](../.agents/skills/review-spec/SKILL.md)、
[implement-spec](../.agents/skills/implement-spec/SKILL.md)、
[review-code](../.agents/skills/review-code/SKILL.md) を必要な時だけ使う。
自動発見に対応しない環境でも、リンク先を読んで同じ手順を実行できる。

環境に `build-unity-editor-extensions` がある場合は、EditorWindow・Inspector・UI Toolkit等の
実装/レビューに使う。仕様の受け入れ条件を、Unity標準コントロール、フォーカス、テーマ、
実パネル上の検証へ落とし込むためのスキルであり、製品要件の正本ではない。
Runtimeの独自テーマまでEditor標準外観に変更する根拠にはしない。
スキルがない環境でも、仕様と検証手順はリポジトリ内で参照できる。

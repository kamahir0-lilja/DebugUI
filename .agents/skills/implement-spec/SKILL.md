---
name: implement-spec
description: DebugUIの採用済み仕様を実装し、部分確認から最終テスト・実操作・AI画像レビューまでつなぐ。
---

# 仕様から実装する

[仕様ワークフロー](../../../docs/contributing/specification-workflow.md)と
[UI検証ワークフロー](../../../docs/contributing/ui-validation-workflow.md)を参照する。

- 対象のApproved / Implemented仕様と受け入れ条件を確認し、対応するテスト・UIケースを選ぶ。
- 期待結果を仕様から選べない場合は提案とレビューへ戻る。通常の範囲内の判断なら同じ作業で解決して継続する。
- Editor側の実装には、利用できるbuild-unity-editor-extensionsの関連手順を使う。Runtime独自テーマをEditor標準へ置き換える根拠にはしない。
- 実装中も変更箇所を実際に操作・撮影して確認する。既存知見は現行版で適用条件を確かめ、必要な理由だけ実装付近へ残す。
- 最後にreview-codeと必要な検証を行い、要件IDと最終状態の証跡をレポートへ記録する。Unity終了確認まで行う。

仕様の採用、実装、検証成功は区別する。未取得の証跡を成功とせず、新しい機能・承認待ち・Git公開操作を段階移行だけで追加しない。

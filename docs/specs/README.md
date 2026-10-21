# 製品仕様

Approved / Implementedな仕様を、対象機能の実装・検証の判断基準にする。
状態や変更手順は [仕様ワークフロー](../contributing/specification-workflow.md) を参照する。

| 領域 | 正本 | 要件ID |
| --- | --- | --- |
| 初期化・破棄・ページ遷移・Runtime/Editorの所有権 | [ライフサイクルとページ](runtime-and-hosts.md) | DBG-LIFE、DBG-NAV、DBG-HOST、DBG-OPEN |
| 入力・動的グループ・テーマ・画面サイズ | [表示と操作](presentation-and-controls.md) | DBG-CTRL、DBG-GROUP、DBG-UI |
| Unityパッケージの導入・依存・サンプル | [配布](package-distribution.md) | DBG-PKG |

初期仕様は、既存の公開説明・過去の知見・コード・テストを照合して採用した主要操作の基準。
採用理由は [初期整理の記録](../spec-changes/0001-spec-driven-baseline.md) を参照する。
この整備ではUnityを再実行していないため、新しい検証済みの主張を加えずApprovedとする。
過去のテスト件数を固定した要件にしない。

一覧にない挙動や境界条件は未定義のままコードから決め打ちせず、必要になった作業の範囲で具体化する。
新規仕様は [_template.md](_template.md)、既存要件への意味の変更は [変更提案](../spec-changes/README.md) を使う。

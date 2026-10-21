# パッケージ配布

Status: Approved

## 目的と根拠

開発用プロジェクトを丸ごと取り込まず、Git URLからUPMパッケージを導入できるようにする。
根拠は [README](../../README.md)、[package.json](../../Packages/com.kamahir0.debug-ui/package.json)、
前回の[独立導入検証](../evidence/2026-10-21-debug-ui-validation.md)。
Agent Decision: 既存の導入手順を今後の変更で維持すべき契約として初期整理する。

## 振る舞い

### DBG-PKG-001

**導入単位と依存関係**

配布対象は `Packages/com.kamahir0.debug-ui/`、パッケージ名は `com.kamahir0.debug-ui`。
最低対応版はUnity 6000.3。UI Toolkit / Vector Graphicsの必要なモジュールを依存関係として宣言する。
Git URLのsubpathから導入した新規プロジェクトで、パッケージのコードと既定リソースを解決できる。
開発用プロジェクトのAddressables、Input System、Pipelineを配布パッケージの前提にしない。

### DBG-PKG-002

**サンプルと公開説明**

Basic Debug Menuサンプルをインポートし、独立した導入先でコンパイルできる。
公開README・ライセンス・変更履歴の参照が有効で、ルートとパッケージのREADMEは同じ内容を保つ。
配布に必要なUnityアセットと`.meta`を対応させ、同梱リソース参照を解決できる。

### DBG-PKG-003

**検証範囲の表示**

メタデータ検査の成功、Unityでのコンパイル/テスト成功、実画面の確認を区別する。
未実行の最低対応版・OS・Playerビルド・公開タグを検証済みとしない。
CIのライセンス設定不足でテスト前に停止した結果を、テスト成功へ読み替えない。

## 受け入れ条件と検証

| 要件ID | 確認方法 |
| --- | --- |
| DBG-PKG-001 | [prepare_consumer.py](../../.github/scripts/prepare_consumer.py)でGit導入先を生成し、そのUnityでコンパイルと回帰テストを実行。既定Resourcesの読込も確認 |
| DBG-PKG-002 | [validate_package.py](../../.github/scripts/validate_package.py)と導入先サンプルのコンパイル |
| DBG-PKG-003 | 実行結果・対象状態・未検証範囲を[レポート](../contributing/ui-validation-report-template.md)に記録し、CIログと照合 |

## 互換性・採用と実装状況

Unity最低版、公開API、GUID、Git導入URLのsubpath、サンプル名を変える場合は影響を整理して仕様を更新する。
この整備は新しい公開タグ・リリースを作らず、Unity 6000.3での動作を新たに保証する検証ではない。
採用根拠は [0001](../spec-changes/0001-spec-driven-baseline.md) を参照。

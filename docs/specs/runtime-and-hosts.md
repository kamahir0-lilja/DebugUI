# ライフサイクルとページ

Status: Approved

## 目的と範囲

ゲーム内のデバッグ項目をページで構成し、実行中のRuntimeまたはEditorウィンドウで操作する。
再初期化、再生再開、ページの往復で古い状態や表示の重複を残さない。
公開された主要操作の基準を扱い、未定義の全API引数や内部クラスの形まで固定しない。

## 根拠と判断

既存の [公開説明](../../README.md)、ライフサイクル/遷移テスト、
[全体構成の説明](../archive/presentations/debug-menu-ui-toolkit.md)を照合した初期基準。
Agent Decision: 既に説明・確認された利用体験を維持する期待結果として整理し、新機能や内部APIの互換性保証は追加しない。
採用記録は [0001](../spec-changes/0001-spec-driven-baseline.md)。

## 振る舞い

### DBG-LIFE-001

**初期化**

`DebugMenu.Initialize` はPlay ModeまたはPlayerでルートページを初期化し、初期表示は非表示とする。
初期化済みの再呼び出しはメニューを増やさず、最初のルートを保持する。
初期化が失敗した場合は途中状態を解放し、修正した入力で再試行できる。

### DBG-LIFE-002

**終了と再開**

`Shutdown` はメニューと共有状態を解放し、繰り返し呼べる。終了後は別のルートで再初期化できる。
メニューのGameObject破棄やDomain Reloadなしの再生再開で古い初期化状態を残さない。
Editorウィンドウを開いている場合も、再初期化したページへ接続し直せる。

### DBG-NAV-001

**通常ページの移動**

登録されたページへ移動し、「戻る」「ルートへ戻る」で履歴に対応するページとタイトルを表示する。
登録済みのページは往復して再利用でき、同じページへの再移動や循環した移動で履歴・表示が壊れない。
ルートで「戻る」を呼んでもルートを失わない。

### DBG-NAV-002

**一時ページ**

一時ページから戻ると、その表示要素を解放する。繰り返し開閉して表示ツリーに一時ページを蓄積しない。
この要件は一時ページの永続保存や通常の登録ページと同じキャッシュを保証しない。

### DBG-HOST-001

**Runtime / Editorの切り替え**

同じページを両方の表示先へ同時に接続しない。Editor側でページを取得したらRuntime側を非表示にし、
Runtimeの`Show`で戻すとランタイムのルート表示を確立する。
Editor側のスタイルや描画設定を戻した先へ意図せず残さない。
Play Mode外ではEditorウィンドウに利用条件の案内を表示する。

### DBG-OPEN-001

**表示とオープンボタン**

表示/非表示の連続要求は最後の要求に対応する状態へ落ち着く。
既定のオープンボタンは同一フレームに複数回生成を要求しても重複しない。
設定された複数回の入力でメニューを開け、ゲーム時間停止中もその入力を受け付ける。
配置の各選択肢でオープン操作の入口が画面内にある。

## 受け入れ条件と検証

| 要件ID | 確認方法 | 実操作ケース |
| --- | --- | --- |
| DBG-LIFE-001 | [Lifecycle tests](../../Packages/com.kamahir0.debug-ui/Tests/Editor/DebugMenuLifecycleTests.cs): DefaultResourcesAndPublicApiWork / RepeatedInitializeKeepsOneMenuAndOriginalPage / FailedConfigurationCleansUpAndCanRetry | UI-01、UI-09 |
| DBG-LIFE-002 | 同テスト: ShutdownAllowsImmediateReinitialization / DestroyingMenuReleasesStaticState / PlayModeRestartWithoutDomainReloadHasNoStaleState / EditorWindowReconnectsAfterShutdown | UI-09 |
| DBG-NAV-001 | [Navigation tests](../../Packages/com.kamahir0.debug-ui/Tests/Editor/DebugNavigationTests.cs): NavigationBackAndRootPreserveInstances / RepeatedPageNavigationDoesNotCorruptHistory / CyclicNavigationCanReturnToRoot | UI-06 |
| DBG-NAV-002 | 同テスト: TemporaryPagesAreReleasedAfterBack | UI-06 |
| DBG-HOST-001 | Lifecycle testsの再接続に加え、Editor→Runtimeの切り替え、文字・テーマの持ち越しを実画像で確認。自動テストだけでは全項目を覆わない | UI-09 |
| DBG-OPEN-001 | Navigation testsのRapidShowHideEndsInRequestedState / OpenButtonFactoryIsIdempotentBeforeStart / OpenButtonSettingsClampAndAllPositionsRemainOnScreenと、時間停止中の実入力 | UI-08 |

UIケースの操作・合否は [UI検証ワークフロー](../contributing/ui-validation-workflow.md) を参照する。
公開入口は [DebugMenu.cs](../../Packages/com.kamahir0.debug-ui/Runtime/DebugMenu.cs)、
所有権設計の理由は [ADR](../adr/0001-runtime-editor-page-ownership.md) を参照する。

## 互換性・未決事項

今回の整理で公開APIや保存値を変更しない。非Play時のすべてのAPI、
未登録名への遷移等の詳細なエラー契約は、この基準では新たに定義しない。
必要な機能変更で期待結果を選べない場合は仕様ワークフローへ戻る。

## 採用と実装状況

レビューと採用は [初期整理の記録](../spec-changes/0001-spec-driven-baseline.md) を参照。
既存テストと過去の実行記録は参照できるが、この文書追加に伴うUnity再実行はしていない。

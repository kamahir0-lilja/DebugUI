# 表示と操作

Status: Approved

## 目的と範囲

デバッグ項目を、値・ラベル・操作状態を取り違えずに編集できるようにする。
RuntimeはパッケージのDefault/Dark、EditorはUnityのLight/Darkと各表示先の文脈を維持する。
画面・色の完全なピクセル一致や、任意のカスタムテーマまでの動作保証は対象外。

## 根拠と判断

[入力/配置の既存テスト](../../Packages/com.kamahir0.debug-ui/Tests/Editor)、
[Editor側の知見](../knowledge/ui-toolkit-editor-panels.md)、
[ボタンの知見](../knowledge/ui-toolkit-button-interactions.md)、過去の[検証記録](../evidence/2026-10-21-debug-ui-validation.md)を照合した。
Agent Decision: 過去の回避策そのものではなく、その対処で守りたかった操作結果と可読性を採用する。

## 振る舞い

### DBG-CTRL-001

**値と通知**

提供する標準の入力コントロールでは、初期値の設定で変更通知を発生させず、
値を変更すると表示・取得値が一致し、その変更通知を一度送る。同じ値への設定で重複通知しない。
ToggleGroupの項目はキーボードのSubmitでも切り替えられる。
スライダーは指定した上下限へ収まる。

### DBG-GROUP-001

**動的グループ**

入れ子のFoldoutは表示と開閉状態が一致する。VirtualFoldoutは項目がなくなった際に空の表示を残さない。
動的項目の破棄は繰り返せ、複数の追加ハンドルを扱える。OrderedGroupは指定順序で表示する。

### DBG-UI-001

**狭い画面と長い内容**

メニュー幅と横並びの操作は表示領域に収まり、必要な操作を重ねない。
長い説明文は折り返して読める。タイトル等の省略が必要な場合も、入力・移動の入口を失わない。
長いページは最後までスクロールして末尾の操作を実行できる。
基本の検証サイズは320×568、390×844、768×1024、1280×720、1920×1080とする。

### DBG-UI-002

**テーマと状態**

Default/DarkおよびEditorのLight/Darkで、通常・hover・focus・無効・選択状態を意味に沿って見分けられる。
必要な文字が背景と同化せず、操作状態の変化でラベルや入力欄が意図せず移動しない。
Editorでは標準の入力・フォーカスの操作感を維持する。Runtime独自の色までEditorと同一にする要件ではない。

### DBG-UI-003

**複合入力と進捗**

Vector/Rect/Boundsのラベルと各成分の対応が分かり、複数行の項目を一行分へ潰さない。
進捗バーの内部領域がコントロールの高さに合い、0/50/100%で進捗と文字を識別できる。

### DBG-UI-004

**保存位置とリサイズ**

ウィンドウをドラッグでき、保存元と異なる画面サイズへ復元・縮小してもヘッダーへアクセスできる。
`ResetPosition`で既定位置へ戻せる。

## 受け入れ条件と検証

| 要件ID | 自動検証・実操作 | UIケース |
| --- | --- | --- |
| DBG-CTRL-001 | [DebugControlTests](../../Packages/com.kamahir0.debug-ui/Tests/Editor/DebugControlTests.cs): FieldsRoundTripAndNotifyExactlyOnce / SliderClampsAtEndpoints / ToggleItemSupportsKeyboardSubmit。実入力でも値と通知を照合 | UI-02、UI-03 |
| DBG-GROUP-001 | 同テスト: OrderedGroupIsStableAndDisposalIsIdempotent / VirtualFoldoutTracksMultipleHandles。開閉・追加・全削除を操作 | UI-04 |
| DBG-UI-001 | [DebugLayoutTests](../../Packages/com.kamahir0.debug-ui/Tests/Editor/DebugLayoutTests.cs): HorizontalButtonsWrapWithoutOverlapAtNarrowWidths / LongLabelsWrapAndStayInsideTheirPage / MenuWidthFitsBothCompactAndWideViewports。末尾ボタンを操作 | UI-01、UI-05 |
| DBG-UI-002 | 両テーマで通常/hover/focus/無効/選択の実画像を確認。Unity標準の操作感と比較 | UI-01〜UI-04、UI-09 |
| DBG-UI-003 | DebugLayoutTestsのProgressBarUsesItsControlHeightと、複合入力の下部までの画像/編集確認 | UI-03 |
| DBG-UI-004 | DebugLayoutTestsのSavedPositionOutsideViewportIsRecovered / ViewportShrinkKeepsSavedMenuAccessibleに加え、ドラッグ・リセットを操作 | UI-07 |

検証方法・画像の判定基準は [UI検証ワークフロー](../contributing/ui-validation-workflow.md) を参照する。
固定色、フォーカス無効化、遅延refresh等の実装方式はこの仕様で指定しない。

## 互換性・採用と実装状況

公開コントロールAPI・保存値・既存の独自テーマをこの整備で変更しない。
採用根拠は [0001](../spec-changes/0001-spec-driven-baseline.md) を参照。
今回はUnityを再実行していない。画像比較の一括ランナーや全版での同一描画が実装済みであることを意味しない。

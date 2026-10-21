# Lilja Debug UI

[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](https://github.com/kamahir0-lilja/DebugUI/blob/main/LICENSE)
![unity-version](https://img.shields.io/badge/unity-6000.3+-blue.svg)

Unity向けのランタイムデバッグメニューと、同じページを表示するEditorウィンドウです。
UI Toolkitを使用します。Unity **6.3（6000.3）以上**とGitが必要です。
同梱SVGはUnity 6.3以降のネイティブSVGインポーターを使用します。
UI ToolkitとVector Graphicsの組み込みモジュールは依存関係として自動追加されます。

## インストール

Unityの **Window > Package Manager > [+] > Install package from git URL...**
（バージョンにより **Add package from git URL...**）に以下を入力します。

```text
https://github.com/kamahir0-lilja/DebugUI.git?path=/Packages/com.kamahir0.debug-ui
```

`Packages/manifest.json` の既存の `dependencies` に追加する場合：

```json
"com.kamahir0.debug-ui": "https://github.com/kamahir0-lilja/DebugUI.git?path=/Packages/com.kamahir0.debug-ui"
```

バージョンを固定する場合は、URL末尾に `#` と**実在する40文字のコミットSHA**を追加します。
下記の `<commit-sha>` は置換用の表記です。そのまま入力しないでください。

```text
https://github.com/kamahir0-lilja/DebugUI.git?path=/Packages/com.kamahir0.debug-ui#<commit-sha>
```

タグで固定する場合も同じ位置に `#タグ名` を指定します。
[公開済みタグ一覧](https://github.com/kamahir0-lilja/DebugUI/tags)に存在するタグを使用してください。
パッケージ内の `version` はGitタグを自動作成しません。

## 最初のメニュー

Package Managerで **Lilja Debug UI > Samples > Basic Debug Menu > Import** を選び、
インポートした `DebugMenuExample` をシーンのGameObjectに追加してPlayを開始してください。
以下のコードを `DebugMenuExample.cs` として保存しても動作します。

```csharp
using UnityEngine;

namespace Lilja.DebugUI.Samples
{
    public sealed class DebugMenuExample : MonoBehaviour
    {
        private void Start()
        {
            if (!Debug.isDebugBuild) return;
            DebugMenu.Initialize(new GenericDebugPage("Debug", builder =>
            {
                builder.Label("Lilja Debug UI");
                builder.PrimaryButton("Log message", () => Debug.Log("Debug action"));
                builder.NavigationButton("System", page =>
                {
                    page.Label($"Unity {Application.unityVersion}");
                    page.Label($"Platform: {Application.platform}");
                });
            }));
            DebugMenuOpenButton.Instantiate();
            DebugMenu.Show();
        }
    }
}
```

この例はEditorとDevelopment Buildで有効です。ルートと子ページを作成し、
メニューと開くボタンを自動生成します。UIDocumentやPanelSettingsの手動設定は不要です。
メニューの外側をクリックすると閉じ、画面左下のボタンを0.75秒以内に3回押すと開きます。

## 表示・ページ・ライフサイクル

```csharp
DebugMenu.Show();
DebugMenu.Hide();
DebugMenu.NavigateTo("System"); // Configure内で登録済みのページ名
DebugMenu.Back();
DebugMenu.BackToRoot();
```

`DebugMenu.Initialize(rootPage)` はPlay ModeまたはPlayerから呼び出します。
初期化済みの場合、再呼び出しは何もしません。メニューと開くボタンはシーン遷移後も残ります。
ルートを変更する場合や終了する場合は `DebugMenu.Shutdown()` を呼び出してください。
Shutdown後は `IsInitialized` がfalseになり、再びInitializeできます。
初期化の失敗時やメニューのGameObject破棄時、Domain Reloadを無効にしたPlay再実行時も状態を解放します。
`Shutdown()` は開くボタンを削除しません。不要なら `DebugMenuOpenButton` のGameObjectを別途破棄してください。

独自の `DebugPage` で `Configure(IDebugUIBuilder builder)` をオーバーライドすることもできます。
`Initialize(rootPage, panelSettings)` の第2引数で独自のPanelSettingsを指定できます。

## Editorウィンドウ

**Lilja > DebugUI > Debug Menu Editor Window** を開き、Play中にメニューを初期化してください。
Editorウィンドウとゲーム画面は同じページを共有し、操作する側へページの所有権を移します。
ゲーム画面で `DebugMenu.Show()` を呼ぶとランタイム側へ戻ります。

## 開発・検証

リポジトリ直下は開発用Unityプロジェクトです。使用するEditorは `ProjectSettings/ProjectVersion.txt` を参照してください。
配布対象は `Packages/com.kamahir0.debug-ui/` のみです。

リポジトリでの作業は[AGENTS.md](https://github.com/kamahir0-lilja/DebugUI/blob/main/AGENTS.md)と
[UI検証ワークフロー](https://github.com/kamahir0-lilja/DebugUI/blob/main/docs/contributing/ui-validation-workflow.md)に従います。
実装中は変更箇所を実操作・画像で確認し、完了前に自動テストとAI画像レビューを行います。
仕様・既存知見・活動スキルの入口は[開発ドキュメント](https://github.com/kamahir0-lilja/DebugUI/blob/main/docs/README.md)を参照してください。
新しい振る舞いや既存仕様の変更は、受け入れ条件を整理・レビューしてから実装します。

開発プロジェクトの `Assets/Development/Scenes/DebugUIValidation.unity` を開いてPlayすると、
各入力型、ページ遷移、動的グループ、200行のスクロールを手動確認できます。
画面サイズを変えた確認、Editorウィンドウとの切り替えにも使用できます。
自動テストは値の通知、ライフサイクル、ナビゲーション、狭い画面のレイアウトを検査します。
実行済みの範囲と制約は[検証記録](https://github.com/kamahir0-lilja/DebugUI/blob/main/docs/evidence/2026-10-21-debug-ui-validation.md)を参照してください。

```sh
python3 .github/scripts/check_docs.py
python3 .github/scripts/validate_package.py
python3 .github/scripts/prepare_consumer.py --output /tmp/debug-ui-consumer --unity-version 6000.6.4f1
```

`--output` には存在しないディレクトリを指定してください。
このコマンドは現在のパッケージを一時Gitリポジトリにコピーし、そのGit URLから導入する独立した
`Consumer` プロジェクトを作成します。開発プロジェクトの依存関係を引き継がず、サンプルもコンパイル対象へ追加します。
`Consumer` をUnityで開き、Test RunnerのEditModeテストを実行してください。
テストはPlay Modeへの出入りを含みます。

GitHub Actionsはメタデータ検査に加え、Unity 6000.3.12f1と6000.6.4f1でこのGit導入と回帰テストを実行します。
UnityジョブにはリポジトリSecretsの `UNITY_LICENSE`、または `UNITY_SERIAL`・`UNITY_EMAIL`・`UNITY_PASSWORD` による
[GameCIのライセンス設定](https://game.ci/docs/github/activation/)が必要です。
ライセンス未設定の場合は理由を表示して失敗します。

## ライセンス・変更履歴

[MIT License](https://github.com/kamahir0-lilja/DebugUI/blob/main/Packages/com.kamahir0.debug-ui/LICENSE) / [Changelog](https://github.com/kamahir0-lilja/DebugUI/blob/main/Packages/com.kamahir0.debug-ui/CHANGELOG.md)

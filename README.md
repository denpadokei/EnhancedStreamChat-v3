# Enhanced Stream Chat

[![Build and Release](https://github.com/denpadokei/EnhancedStreamChat-v3/actions/workflows/build-and-release.yml/badge.svg)](https://github.com/denpadokei/EnhancedStreamChat-v3/actions/workflows/build-and-release.yml)

Beat Saber のメニューとプレイ中の両方に Twitch チャットを表示する MOD。上流は Auros/EnhancedStreamChat-v3。現在はこのフォークで Beat Saber 1.39.0 向けにメンテナンスしている。

## 機能

- Twitch チャットをゲーム内のフローティングスクリーンに表示する。メニュー用とプレイ中用で位置・回転を別々に保存
- 全 Unicode 対応。日本語を含むフォントを UserData のフォントアセットから読み込み、チャット受信時のスパイクを抑える
- エモート / 絵文字 / バッジをインライン画像として描画する。アニメーション GIF に対応し、取得した画像はキャッシュされる
- ビッツ付きエモートはビット数を色付きで併記する
- 送信者ごとの名前色、`/me` 形式のアクションメッセージ、システムメッセージの色分けに対応
- メッセージ削除、チャットクリア、フォロー、チャンネルポイントの引き換えをシステムメッセージとして表示
- チャット欄の幅・高さ・フォントサイズ・配色・表示順をゲーム内の設定画面から変更できる。表示レイヤーも UI / HMD のみ / 手動から選択可能
- CatCore の Web アプリから Twitch へのログインとチャンネルの参加・退出を行う

## 動作環境

| 項目 | 内容 |
| --- | --- |
| Beat Saber | 1.39.0 |
| 依存 MOD | BSIPA 4.3.5 以上 / BeatSaberMarkupLanguage 1.12.3 以上 / BS Utils 1.14.2 以上 / CatCore 1.1.0 以上 / SiraUtil 3.1.12 以上 / CameraUtils 1.0.7 以上 |

## インストール

1. Beat Saber を 1.39.0 に合わせる (BSManager などのバージョン管理ツールを推奨)
2. BSIPA と上記の依存 MOD をインストールする
3. [Releases](https://github.com/denpadokei/EnhancedStreamChat-v3/releases) から `EnhancedStreamChat-10.1.0-bs1.39.0.zip` をダウンロードし、`Plugins` フォルダをゲームフォルダへ上書きコピーする

## 使い方

1. ゲームを起動すると CatCore の設定 Web アプリが既定のブラウザで開く。ここで Twitch にログインし、表示したいチャンネルへ参加する
2. チャット欄の右下の歯車アイコンを掴むと設定画面が開く。`Layout` / `Colors` / `Login/Channels` / `About` のタブで色・サイズ・レイヤーなどを変更でき、変更は即座に反映される
3. `Login/Channels` タブの `click here` で CatCore の設定画面を開き直せる。IRC 接続が切れた場合は `ReConnect` ボタンで接続し直す
4. チャット欄を移動させたい場合は `Layout` タブで `Allow Movement` を有効にするとハンドルが表示され、ドラッグした位置がメニュー / プレイ中それぞれに保存される

### フォント

日本語のスパイクを避けるため、フォントはあらかじめ Unity のフォントアセットとして読み込む。次の場所に配置すること。

- `UserData/ESC` … メインに使うフォントファイル (ttf など)
- `UserData/FontAssets/Main` … メインフォントの `.assets` (アセットバンドル)
- `UserData/FontAssets/FallBack` … フォールバックフォントの `.assets`

`UserData/FontAssets/Main` にアセットバンドルがあればそれを最優先で使い、無ければ `UserData/ESC` 内のフォントファイル、それも無ければ設定したシステムフォントの順に解決する。フォールバックには `UserData/FontAssets/FallBack` の内容に加えて OS の Meiryo が自動で追加される。

同梱のフォント情報を `UserData` に置かないとスパイクが発生するため、必ず配置すること。オリジナルのフォントアセットの作成方法は [ESCFontProject](https://github.com/denpadokei/ESCFontProject) を参照。

## 設定

ゲーム内の設定画面、または `UserData/EnhancedStreamChat.json` で変更する。

| 項目 | 型 | 既定値 | 説明 |
| --- | --- | --- | --- |
| SystemFontName | string | `Segoe UI` | `UserData/ESC` 内のフォントファイル名の照合と、システムフォントの取得に使う名前 |
| ChatWidth | int | 120 | チャット欄の幅 |
| ChatHeight | int | 140 | チャット欄の高さ |
| FontSize | float | 3.4 | 本文のフォントサイズ |
| BackgroundColor | Color (RGBA) | 0.3, 0.3, 0.3, 1 | チャット欄の背景色 |
| TextColor | Color (RGBA) | 1, 1, 1, 1 | 本文の色 |
| AccentColor | Color (RGBA) | 0.57, 0.28, 1, 1 | 強調メッセージ左端のアクセント色 |
| HighlightColor | Color (RGBA) | 0.57, 0.28, 1, 0.06 | 強調メッセージ (フォロー・ポイント引き換えなど) の背景色 |
| AllowMovement | bool | false | チャット欄のハンドルを表示してドラッグ移動を許可する |
| SyncOrientation | bool | false | メニューとプレイ中の位置・回転を同期する |
| ReverseChatOrder | bool | false | 新しいメッセージを上から並べる |
| Menu_ChatPosition | Vector3 | 0, 3.75, 2.5 | メニューでのチャット欄の位置 |
| Menu_ChatRotation | Vector3 | 325, 0, 0 | メニューでのチャット欄の回転 (オイラー角) |
| Song_ChatPosition | Vector3 | 0, 3.75, 2.5 | プレイ中のチャット欄の位置 |
| Song_ChatRotation | Vector3 | 325, 0, 0 | プレイ中のチャット欄の回転 (オイラー角) |
| Layer | `UI` / `HMDOnly` / `Manual` | `UI` | チャット欄を描画するレイヤー |
| UILayer | int | 5 | `Layer` が `Manual` のときに使うレイヤー番号 |

## ビルド

```powershell
git clone https://github.com/denpadokei/EnhancedStreamChat-v3.git
cd EnhancedStreamChat-v3
dotnet build .\EnhancedStreamChat.csproj -c Release
```

`BeatSaberDir` に Beat Saber のインストール先を設定すると、ゲームの参照アセンブリを解決できます。

GitHub Actions の `build-and-release.yml` が push ごとにビルドし、`X.X.X` 形式のタグを push するとリリースを作成します。

## クレジット

- オリジナル作者: brian91292 ([Auros/EnhancedStreamChat-v3](https://github.com/Auros/EnhancedStreamChat-v3))
- チャットバックエンド: [ErisApps/CatCore](https://github.com/ErisApps/CatCore)

## ライセンス

MIT License (Copyright (c) 2020 brian91292)。詳細は [LICENSE](LICENSE) を参照。

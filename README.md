# EnhancedStreamChat v3
Enhanced Stream Chat is a rich text chat integration mod for Beat Saber. It supports emotes, emojis, badges, and any other image that may come inline in a chat message through any of the supported services.  
  
# このMODについて  
このEnhancedStreamChatは日本語を受信したときにスパイクを起こさないように日本人向けに改良されたEnhancedStreamChatです。  
スパイクの原因はフォントをキャッシュする動作にあるためあらかじめUnityでFont Assetを作成することで回避しています。  
そのため同梱しているフォント情報をUserDataに置かないとスパイクが発生してしまうので必ずフォントを配置してください。  
  
また、各自でオリジナルのフォントアセットを作成し読み込むことも可能です。  
詳しくは[こちら](https://github.com/denpadokei/ESCFontProject)  
# Basic Configuration (for users)
1. Install CatCore **2.0.0-alpha1** (or newer) from https://github.com/denpadokei/CatCore/releases
2. Grab the latest EnhancedStreamChat release from https://github.com/denpadokei/EnhancedStreamChat-v3/releases
3. Extract the latest EnhancedStreamChat zip to your `Beat Saber` directory
4. Once you launch the game, the CatCore settings web app will open in your default browser. Use this to login, join/leave channels, and configure various settings.

> EnhancedStreamChat 11.0.0 targets Beat Saber **1.40.8** and requires CatCore **2.x** (`^2.0.0-alpha1`).
> CatCore 2.0 replaced the decommissioned legacy Twitch PubSub with EventSub, so an older CatCore will not be loaded with this version.

# Important Stuff
- This is still a WIP
- That means if you find something is missing or incomplete, it's probably because it's not done.
- If you find any bugs (crashes/freezes) make an issue, but again keep in mind this isn't done.

# Dependecy(依存mod)

```
    BSIPA
    BeatSaberMarkupLanguage
    BS Utils
    CatCore
    SiraUtil
    CameraUtils
  ```

# Build (開発者向け)

1. Beat Saber **1.40.8** をインストールする
2. CatCore **2.0.0-alpha1** の `Libs/CatCore.dll` を `Beat Saber\Libs\CatCore.dll` に配置する
   (https://github.com/denpadokei/CatCore/releases)
3. `dotnet build EnhancedStreamChat.csproj -c Release` を実行する
4. 配布物は `bin\Release\net48\Artifact` に出力される (`Plugins\EnhancedStreamChat.dll`, `UserData\FontAssets\...`)

GitHub Actions (`.github\workflows\build.yml`) が push / pull request ごとに自動ビルドし、
`EnhancedStreamChat-<version>-bs<gameVersion>.zip` を artifact としてアップロードする。
タグ付けとリリース作成は手動で行う。

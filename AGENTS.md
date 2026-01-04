# AGENTS

このリポジトリで作業するエージェント／開発者向けの最小限ガイドです。

## プロジェクト概要

- アプリ名: **JustNotification**
- 目的: Windows の通知を VR 上で表示する
- ソリューション: `JustNotification.sln`
- 主プロジェクト: `JustNotification/`
- 現在の作業ブランチ: `xsoverlay`（デフォルト: `dev`）

## 環境・ビルド

- 必須: .NET 8.0 SDK（net8.0-windows10.0.19041.0 をターゲット）
- ビルド: `dotnet build --configuration Debug`
- 実行バイナリ: `JustNotification/bin/Debug/net8.0-windows10.0.19041.0/JustNotificationPlus.exe`
- ログ: 同ディレクトリ配下の `logs/*.log`

## 通知取得の既知事項

- `UserNotificationListener` の一部 API が環境によって `NotImplementedException` (E_NOTIMPL) を返すことがある。
- `ShowNotification` は `UserNotification.AppInfo` アクセスを `try-catch` でラップ済み。再発時は周辺ログを確認。

## トラブルシューティング

1) ビルドで `apphost.exe` をコピーできないエラー
   - 実行中の `JustNotificationPlus.exe` を終了して再ビルド（例: `Stop-Process -Name "JustNotificationPlus"`）。
2) 通知が届かない
   - ログに `UserNotificationListener.GetNotificationsAsync is not supported` が出ていないか確認。
   - `Settings.Default.interval` のポーリング間隔も確認。

## コミット・ブランチ運用

- 現在の変更は `xsoverlay` ブランチで作業し、リモートへ push する。
- コミット例: `fix: ...`, `docs: ...`

## ドキュメント

- 変更履歴: `CHANGELOG.md`
- README: 簡易説明のみ。必要に応じて更新。

## 今後の改善メモ

- 未実装/非対応環境向けの通知モックやフォールバック検討。
- ログローテーションの追加検討。

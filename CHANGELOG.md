# Changelog

## [Unreleased]

### Fixed

- Fixed `NotImplementedException` errors in notification polling on some environments
  - Added exception handling in `ShowNotification` method to gracefully handle cases where `UserNotification.AppInfo` property is not implemented
  - Prevents log spam and allows the application to continue functioning on unsupported environments
  - Related logs: Multiple `NotImplementedException` errors recorded from 2025-12-28 onwards

## Version History

### Initial Release

- VR上でWindowsの通知を確認できるシンプルなアプリケーション

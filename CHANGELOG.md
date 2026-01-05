# Changelog

## [Unreleased]

### Added

- Enhanced debug logging for easier troubleshooting
  - Added detailed logging for notification content (app name, title, body, timeout)
  - Added logging for XSOverlay notification sending with success/failure status
  - Added logging for NamedPipe notification sending with success/failure status
  - Added polling loop status logging (interval, retrieved count, new notifications)
  - Added notification ID tracking logs
  - Added JSON payload trace logging
  - Improved exception logs with notification IDs for context

### Fixed

- Fixed `NotImplementedException` errors in notification polling on some environments
  - Added exception handling in `ShowNotification` method to gracefully handle cases where `UserNotification.AppInfo` property is not implemented
  - Prevents log spam and allows the application to continue functioning on unsupported environments
  - Related logs: Multiple `NotImplementedException` errors recorded from 2025-12-28 onwards
  - Verified fix: No ERROR-level logs for NotImplementedException after 2025-12-28; properly logged as WARN level

## Version History

### Initial Release

- VR上でWindowsの通知を確認できるシンプルなアプリケーション

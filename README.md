<div align="center">

# 🧹 System Cleaner

A lightweight Windows system cleanup tool built with **.NET 8 WinForms**.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?style=for-the-badge&logo=windows)
![Language](https://img.shields.io/badge/C%23-100%25-239120?style=for-the-badge&logo=csharp)
![License](https://img.shields.io/badge/License-MIT-success?style=for-the-badge)

</div>

---

## Overview

System Cleaner helps users keep Windows tidy and responsive by providing safe cleanup actions, RAM optimization via Microsoft Sysinternals RAMMap, automatic scheduling, and tray-based background execution.

## Highlights

- Clean Windows Temp and user temp folders
- Empty Recycle Bin
- Clean Prefetch files
- Run RAMMap commands for memory optimization
- Schedule automatic runs
- Minimize to tray with notifications
- Detailed activity log with timestamps
- Automatic RAMMap download when missing

## Screenshots

Replace the files in `Assets/` with your own screenshots.

![Main UI](Assets/screenshot-main.png)
![Tray Icon](Assets/tray.png)
![Demo GIF](Assets/demo.gif)

## Architecture

```text
MainForm
├── CleanerService
├── RamMapService
├── SchedulerService
└── TrayIconService
         │
         └── Windows API / Sysinternals RAMMap
```

## Tech Stack

- .NET 8
- C# WinForms
- Windows API (P/Invoke)
- Async / Await
- Manual Dependency Injection
- Microsoft Sysinternals RAMMap

## Folder Structure

```text
SystemCleaner
├── Forms
├── Services
├── Assets
├── .github
└── README.md
```

## Getting Started

```bash
git clone https://github.com/tuanpv62/system-cleaner.git
cd system-cleaner
dotnet build
dotnet run
```

## Notes for Recruiters

This project demonstrates:
- desktop application design
- service-based structure
- Windows integration
- background scheduling
- safe file operations
- clean UX with tray icon and logs

## Roadmap

- Dark mode
- Export logs
- Update checker
- Multi-language UI
- Better settings management
- Portable release package

## License

MIT License

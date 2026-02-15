# VK-Common-BE

![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-512BD4)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen)

このリポジトリは、**モダンなバックエンドアーキテクチャの知識体系を構築・学習すること**を最大の目的としています。
単なるライブラリ集ではなく、DDD (ドメイン駆動設計) や Clean Architecture の原則に基づいた「実用的なビルディングブロック」の実装パターンを探求・蓄積しています。

## 📦 現在の主要実装

### 🗄️ [VK.Blocks.Persistence.EFCore](src/BuildingBlocks/Persistence/EFCore/README.md)

Entity Framework Core をベースとした、高機能かつ堅牢な永続化層モジュールです。
バルク操作と監査ログの自動整合 (Hybrid Auditing) や、スレッドセーフなメタデータキャッシュなど、パフォーマンスと保守性を両立させる工夫を凝らしています。

👉 **[詳細な設計思想とドキュメントはこちら](src/BuildingBlocks/Persistence/EFCore/README.md)**

---

Copyright (c) 2026 Kenkon Ro

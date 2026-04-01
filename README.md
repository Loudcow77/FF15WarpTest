# FF15WarpTest
<a name="japanese"></a>

## 概要
本プロジェクトは、ゲームプレイプログラミングの技術習得を目的とした自主制作の演習作品です。スクウェア・エニックスのAAAタイトル『ファイナルファンタジーXV』における代表的なアクション「ウォープストライク」を選び、そのゲームプレイシステムをUnityとC#でゼロから再実装しました。
ビジュアルアセットの再現ではなく、ゲームプレイシステムのロジックのリバースエンジニアリングが目的です。ターゲット選定の仕組み、キャラクターの移動処理、攻撃トリガーのタイミング、そしてそれらが一体となって「気持ちよい操作感」を生み出す仕組みを理解すること — これらの問いに答えるために、ターゲット検出・移動補間・アニメーションタイミング・ゲームフィールについて深く考察しました。

![altgif](https://github.com/user-attachments/assets/73cd4679-9181-441d-af77-96a4ab20eb30)

## ウォープストライクとは
**『FFXV』におけるウォープストライクは以下の操作で構成されます：**
1. 遠距離の敵またはウォープポイントへのロックオン
2. 高速で対象へ向かって飛翔（キャラクターが空間を「ワープ」するような表現）
3. 到達と同時に攻撃を発動

このメカニクスは同作の流動的でスピード感のある戦闘を象徴するシグネチャーアクションであり、ゲームプレイ研究の対象として非常に適した題材です。

## 実装システム
**ターゲット検出・ロックオンシステム**
- Physics オーバーラップと角度チェックを組み合わせ、設定可能な範囲と視野角コーン内の有効ターゲットをスキャン
- 単純な距離ではなく、プレイヤーの照準方向に最も近いターゲットを優先選択（距離と画面中心への近さを考慮）
- ウォープ中、攻撃が解決されるまでロックされたターゲット参照を維持
- ロックオン中のターゲットが死亡・範囲外に出た場合のターゲット無効化処理をグレースフルに処理

**ワープ移動システム**
- ワープトリガー時、一定速度の移動ではなく**非線形補間（LerpUnclamped / AnimationCurve）**でキャラクターをターゲットへ射出 — 「速く飛んで着地でスナップする」特徴的な移動感を再現
- ワープ中は物理演算から**切り離し（kinematic化）**し、飛行中のコリジョン干渉を防止
- 到達時にコリジョンと重力を再有効化し、物理シミュレーションにクリーンに復帰
- ワープ移動時間は距離に応じて変化：短距離はスナッピー、長距離は迫力あるスピード感

**到達時の攻撃処理**
- コルーチンベースのタイミングシステムにより、移動カーブの終点と同期した正確な到達フレームで攻撃ヒットボックスを有効化
- ダメージ適用はヒットボックストリガーから分離 — ヒットボックスは重複検出のみを担当し、別途コンバットシステムがダメージを解決（疎結合設計）
- 到達後の数フレームで着地アニメーションロックを設け、即座のボタン連打による動作の乱れを防止

**カメラ・プレイヤーフィール**
- わずかな追従ラグをつけてカメラがワープ軌道に追従 — 方向感覚を失わずにスピード感を維持
- シェーダーパラメータ制御によるモーションブラー / スピードエフェクトをワープ中に有効化
- 前のアクションが完了するわずか前からワープコマンドを受け付けるインプットバッファにより、流れるようなコンボチェーンを実現

## アーキテクチャ・設計方針
| パターン | 使用箇所 |
| ----------- | ----------- |
| ステートマシン | プレイヤー状態：待機 / 移動 / ワープチャージ中 / ワープ中 / 攻撃中 / 着地中 |
| コルーチンベースのタイミング | Update()ポーリングを使わずワープ移動と攻撃トリガーを同期 |
| Observer / イベント | 攻撃解決が移動コードへの直接参照なしにコンバットシステムへ通知 |
| AnimationCurve | Inspectorから調整可能な非線形ワープ速度プロファイル |
| コンポーネント分離 | TargetDetector・WarpMover・AttackResolver・CameraControllerを独立して実装 |

本プロジェクトで最も難しかったのは移動の「気持ちよさ」の再現でした。FF15のワープはテレポートでもダッシュでもなく、特有の弧・速度プロファイル・着地のスナップが合わさって「強さ」を感じさせます。参照に近い感覚を実現するまでに補間カーブと到達タイミングを何度も調整しました。この反復チューニングのプロセスから、コードとゲームフィールの関係について多くを学びました。

| 技術スタック | |
| ----------- | ----------- |
| エンジン | Unity (LTS) |
| 言語 | C# |
| 移動処理 | AnimationCurve駆動のLerp、Rigidbody kinematicトグル |
| 検出処理 | Physics.OverlapSphere ＋ 内積による角度チェック |
| タイミング制御 | コルーチンベース、移動カーブと同期 |
| バージョン管理 | Git |

## 学んだこと
- ゲームプレイメカニクスのリバースエンジニアリング手法 — ゆっくりとプレイしながらサブシステムに分解し、それぞれを独立して再構築する
- 移動フィールにおける非線形補間の重要性 — 線形Lerpは機械的に感じられ、カーブ駆動の移動は生き生きと感じられる
- 一時的に物理演算から切り離して移動処理を行い、復帰時にシミュレーションを壊さない方法
- 攻撃ヒットボックスをダメージロジックから分離すべき理由 — 検出と解決の分離により、両システムを独立して反復しやすくなる
- ゲームフィールのチューニングの技術 — 着地タイミング・カメララグ・インプットバッファのわずかな変化が、メカニクスの「気持ちよさ」に対して予想以上に大きな影響を持つ

<a name="english"></a>
## Overview
This project is a focused gameplay programming exercise: taking a well-known, polished mechanic from a AAA title — the warp-strike from Final Fantasy XV — and rebuilding it from scratch to understand exactly how it works under the hood.
Rather than copying visual assets, the goal was to reverse-engineer the gameplay systems logic: how does the game select a valid warp target, how does the player character travel to it, when does the attack trigger, and how does it all feel fluid in motion? Answering these questions required thinking carefully about target detection, character movement interpolation, animation timing, and game feel.
Built entirely solo as a technical portfolio piece.

![altgif](https://github.com/user-attachments/assets/73cd4679-9181-441d-af77-96a4ab20eb30)

## What is the Warp-Strike Mechanic?
**In Final Fantasy XV, the warp-strike allows the player to:**
1. Lock onto an enemy or warp point at range
2. Launch toward it at high speed (the character visually "warps" through space)
3. Deliver an attack upon arrival

It is a signature mechanic that defines the game's fluid, high-speed combat — making it a rich target for gameplay mechanic study.

## Systems Implemented
**Target Detection & Lock-On System**
- Scans for valid targets within a configurable range and field-of-view cone using Physics overlap and angle checks
- Selects the most appropriate target based on distance and screen-centre proximity — prioritising the target closest to the player's aim direction, not simply the nearest
- Maintains a locked target reference that persists through the warp motion until the attack resolves
- Cleanly handles target invalidation — if the locked target dies or leaves range mid-warp, the system falls back gracefully

**Warp Movement System**
- On warp trigger, the player character is launched toward the target using non-linear interpolation (LerpUnclamped / AnimationCurve) rather than a fixed-speed translation — creating the characteristic fast-then-snap movement feel
- Movement is decoupled from physics during the warp arc to prevent collision interference mid-flight
- Collision and gravity are re-enabled on arrival, returning the character to the physics simulation cleanly
- Warp travel time is distance-aware: short warps feel snappy, long-range warps carry more dramatic speed

**Attack Resolution on Arrival**
- An attack hitbox is activated at the exact arrival frame using a coroutine-based timing system synchronised with the movement curve's end point
- Damage application is separated from the hitbox trigger — the hitbox simply detects overlap, and a separate combat system resolves the damage — keeping systems decoupled
- A brief landing animation lock prevents player input for a few frames on arrival, preventing jank from immediate button mashing after the warp

**Camera & Player Feel**
- Camera follows the warp trajectory with a slight lag to preserve the sense of speed without disorientation
- A motion blur / speed effect activates during warp travel via shader parameter control
- Input buffer accepts the warp command slightly before the previous action completes, allowing fluid combo chaining

## Architecture & Design Decisions
| Pattern | Where Used |
| ----------- | ----------- |
| State Machine | Player states: Idle / Moving / WarpCharging / Warping / Attacking / Landing |
| Coroutine-Based Timing | Warp movement arc and attack trigger synchronised without Update() polling |
| Observer / Events | Attack resolution notifies combat system without direct coupling to movement code |
| AnimationCurve | Non-linear warp speed profile configured in the Inspector for easy tuning |
| Component Separation | TargetDetector, WarpMover, AttackResolver, and CameraController are independent |

The hardest part of this project was getting the movement feel right. The warp in FF15 doesn't feel like a teleport or a dash — it has a specific arc, a specific speed profile, and a specific arrival snap that makes it feel powerful. Achieving this required several iterations on the interpolation curve and arrival timing before it felt close to the reference. This iterative tuning process taught me a lot about the relationship between code and game feel.

| Technical Stack | |
| ----------- | ----------- |
| Engine | Unity (LTS) |
| Language | C# |
| Movement | AnimationCurve-driven Lerp, Rigidbody kinematic toggle |
| Detection | Physics.OverlapSphere + dot product angle check |
| Timing | Coroutine-based, synchronised with movement curve |
| Version Control | Git |

## What I Learned
- How to reverse-engineer a gameplay mechanic by playing it slowly and breaking it into discrete sub-systems, then rebuilding each one independently
- The importance of non-linear interpolation for movement feel — a linear lerp feels mechanical; a curve-driven movement feels alive
- How to decouple movement from physics temporarily without breaking the simulation on re-entry
- Why attack hitboxes should be separate from damage logic — separating detection from resolution makes both systems easier to iterate on independently
- The craft behind game feel tuning: small changes to arrival timing, camera lag, and input buffering have an outsized effect on whether a mechanic feels satisfying or frustrating

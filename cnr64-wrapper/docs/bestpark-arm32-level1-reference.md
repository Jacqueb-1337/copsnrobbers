# Best Park: verified genuine ARMv7 Level 1 reference

Date: October 29, 2026. This is original-game reference evidence, NOT
evidence that the PV7 ARM64 wrapper runs Cocos2d-x.

## Environment

Genuine Android 4.4.2 ARMv7 emulator on Windows using Google's classic
emulator-arm.exe 28.0.25; serial emulator-5580. Software GLES2 via
SwiftShader. Best Park diagnostic APK v1.2.1 and matching version-16 OBB.
Frida ARM 12.4.8 server and isolated Python 3.7 client. Unmodified native
libgame.so executes directly on the emulated ARM CPU.

The emulator screenshot is portrait 320x480 with game content rotated,
but input taps use the landscape 480x320 logical coordinate space.

## Verified reproducible level-1 path

1. Launch game, permit initialization and dismiss legacy Google Games
   sign-in overlay. If Quit confirmation opens, use its NO button
   (reference input tap roughly x=320, y=190). Back can trigger Quit.
2. Title Play: ADB tap x=249, y=253.
3. Opening story: repeated taps near x=420, y=185 with 0.8-second gaps
   (about fifteen in total), checking screenshots between batches.
4. Select Destination: first unlocked destination x=135, y=210.
5. Select 2 Characters: Mordecai and Rigby are ALREADY selected by
   default. Their frames and Play are brightly highlighted. Tapping
   character buttons TOGGLES them off; doing so disabled Play and caused
   our earlier misleading no-transition observation. If accidentally
   deselected, retoggle Mordecai at x=115,y=145 and Rigby x=215,y=145
   as necessary. Verify both highlighted and Play yellow.
6. Character selection Play: x=243,y=290. Level 1 loads and renders.
   Verified screen shows two character health bars, score HUD, courtyard
   grass, obstacles, and a visible game character.
7. Gameplay tracer ran during actual Level 1 with a test tap at
   x=315,y=205. The frame still rendered afterward. It is NOT confirmed
   that this particular tap moved the character.

Allow the slow emulator to settle before treating a tap as ineffective.

## Verified ARM32 internal function interception

The source tools are pv7_arm32_function_agent.js,
pv7_arm32_function_trace.py, and pv7_arm32_trace_correlate.py.

Capture                              | Profile   | Hooks | Active funcs | Counted | Samples in/out | Errors
-------------------------------------|-----------|------:|-------------:|--------:|---------------:|------:
Title Play -> cutscene               | scene     |    90 |           12 |      15 |        15 / 15 |      0
Destination 1 -> character selection | scene     |   110 |           16 |      22 |        22 / 22 |      0
Character button toggle              | character |    31 |            4 |      12 |        12 / 12 |      0
Character Play -> Level 1            | scene     |   110 |           27 |     262 |      136 / 133 |      0
Active Room update (~10s)            | gameplay  |    86 |            4 |  11,625 |        97 / 97 |      3

Important function observations:
- CharacterSelectScene::onCharSelect was directly intercepted.
- CharacterSelectScene::create/init/createCharButton and
  CCDirector::replaceScene/setNextScene establish menu transitions.
- CutscenePlayer::create and CCTransitionFade callbacks establish
  the title -> story sequence.
- In the running first level, Room::update(float) ran 265 times;
  Entity::update(float) 8,456; Entity::removeNonExistants() 264;
  and a C++ entity pointer std::remove specialization 2,640.
- Three gameplay hooks failed at their ARM entry points:
  BossRoom::addXpToPlayer, HordeRoom::addXpToPlayer and
  PlayerComboSystem::update. These are Frida instrumentation failures,
  NOT evidence that those game routines are unsupported.
- 136 entry samples versus 133 returns in the level-entry capture
  simply indicates some functions had not yet returned or their
  returns were not observed before the bounded capture ended.

## Source-of-truth and limitations

Original local APK's native libgame.so was extracted privately for
offline ELF32 ARM Thumb symbol lookup (15,115 symbols), without changing
the original archive. Fifteen local JSONL sessions were merged into
PV7-BestPark-Level1-ARM32-reference.json and matching .md under
the isolated C: PV7 ARM32Classic working directory. The report sums
12,568 selected function interceptions across independent sessions;
it is NOT the total number of calls in the game. Earlier sessions
without count summaries remain sample-only.

Raw traces, APKs, OBBs, extracted game libraries, screenshots, device
logs, and process arguments stay local to the owner's PC and are
not uploaded to GitHub.

Frida Stalker still remains disabled after earlier zero-event captures
and responsiveness issues. This is a bounded exported-function hook
strategy, not exhaustive execution coverage.

## Next implementation tasks for PV7

1. Instrument the SEPARATE Cocos Dynarmic guest from JNI_OnLoad to
   JNI renderer entry, first frame and service call boundaries.
2. Implement or validate game-specific JNI, Bionic/libc object calls,
   GLES/EGL APIs, private guest OBB/data/files/cache access and input.
3. Normalize symbol addresses, call-site PCs, threads and event types
   against the observed original ARMv7 reference and identify differences.
4. Reproduce title -> story -> destination -> characters -> Level 1
   under the PV7 wrapper without changing its Unity/CNR path.

Production Cocos support in PV7 remains unverified and disabled.

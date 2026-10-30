/*
 * PV7 ARM32 reference function tracer (Frida 12.4.8).
 *
 * Uses selected exported guest ARM32 functions; NO Frida Stalker and
 * NO calls into the game. The optional PV7_TRACE_CONFIG is injected by
 * the host collector (fixed validated profile and bounded limits).
 * Stable render profile preserves the original tracing baseline.
 */
var mod = Process.findModuleByName('libgame.so');
if (mod === null) {
  send({kind: 'fatal', detail: 'libgame.so not loaded'});
} else {
  var config = typeof PV7_TRACE_CONFIG === 'undefined'
    ? {profile: 'render', maxHooks: 90, sampleLimit: 1000}
    : PV7_TRACE_CONFIG;
  var profile = config.profile;
  var cap = Math.max(1, Math.min(120, config.maxHooks || 90));
  var maxSamples = Math.max(1, Math.min(5000, config.sampleLimit || 1000));
  var selectors = {
    render: {
      classes: /CCDirector|CCFileUtils|CCTextureCache|CCSpriteFrameCache|CCScene|FrameHandler|JNI_OnLoad|Java_org_cocos2dx_lib_Cocos2dxRenderer_nativeRender/,
      methods: /drawScene|mainLoop|runWithScene|replaceScene|setNextScene|sharedDirector|addImage|getFileData|fullPathForFilename|addSpriteFramesWithFile|spriteFrameByName|CCScene|FrameHandler|JNI_OnLoad|nativeRender/
    },
    scene: {
      classes: /CCDirector|CCScene|CCTransition|SLMenu|AreaMenu|Room|CutscenePlayer|Select|Screen|LevelSelect/,
      methods: /runWithScene|replaceScene|pushScene|popScene|setNextScene|init|onEnter|onExit|create|start|show|hide|transition|loadLevel|update|endCallback/
    },
    assets: {
      classes: /CCFileUtils|CCTextureCache|CCTexture2D|CCSpriteFrameCache|CCBReader|ZipFile|CCImage|SoundPlayer|CCSpriteBatchNode/,
      methods: /fullPath|isFileExist|getFileData|addImage|addSpriteFrame|spriteFrameByName|load|initWith|read|open|playSound|create/
    },
    input: {
      classes: /Cocos2dxRenderer_nativeTouches|Cocos2dxRenderer_nativeKey|CCEventDispatcher|CCLayer|CCMenu|CCMenuItem|Room|AreaMenuContainer|SLMenuDelegate|KeyboardNotificationLayer/,
      methods: /nativeTouches|nativeKey|touches|ccTouch|ccTouches|keyBack|dispatchTouch|touchBegan|touchEnded|touchMoved/
    },
    gameplay: {
      classes: /Room|TutorialRoom|Player|Enemy|CharacterSkills|Character|Entity|ItemDestroyRoom|CutscenePlayer|FrameHandler|GameScene/,
      methods: /update|onLevel|createEnemy|addXp|onTutorial|init|create|attack|spawn|move|damage|die|skill|onEnter|onExit/
    }
  };
  var selector = selectors[profile];
  if (!selector) {
    send({kind: 'fatal', detail: 'Unknown trace profile: ' + profile});
  } else {
    var exports = Module.enumerateExportsSync('libgame.so').filter(function (e) {
      return e.type === 'function';
    });
    var candidates = [];
    var seen = {};
    var eligible = exports.filter(function (e) {
      return e.name.indexOf('_ZTh') !== 0 &&
        selector.classes.test(e.name) && selector.methods.test(e.name);
    });
    // On a tiny emulated ARM CPU, favor game-specific functions over generic
    // framework constructors. Keep the stable render profile unmodified.
    if (profile === 'scene' || profile === 'gameplay') {
      function priority(name) {
        if (/CutscenePlayer|TutorialRoom|AreaMenuContainer|SLMenu|LevelSelect|Room|Player|Enemy|CharacterSkills/.test(name)) return 0;
        if (/CCDirector/.test(name)) return 1;
        return 2;
      }
      eligible = eligible.map(function (e, i) {
        return {entry: e, originalIndex: i, priority: priority(e.name)};
      }).sort(function (a, b) {
        return a.priority - b.priority || a.originalIndex - b.originalIndex;
      }).map(function (x) {return x.entry;});
    }
    eligible.forEach(function (e) {
      if (candidates.length >= cap) return;
      var address = e.address.toString();
      if (seen[address]) return;
      seen[address] = true;
      candidates.push(e);
    });
    var counts = {}, names = {}, sampled = 0, hooked = 0, failures = 0;
    var base = mod.base, listeners = [];
    candidates.forEach(function (e) {
      var name = e.name, offset = e.address.sub(base).toString();
      names[offset] = name;
      try {
        var listener = Interceptor.attach(e.address, {
          onEnter: function (args) {
            var count = (counts[offset] || 0) + 1;
            counts[offset] = count;
            this.logged = false;
            if (sampled < maxSamples && (count <= 10 || count % 200 === 0)) {
              sampled++;
              this.logged = true;
              var caller = this.returnAddress;
              var owner = Process.findModuleByAddress(caller);
              var raw = [];
              for (var i = 0; i < 4; i++) raw.push(args[i].toString());
              send({
                kind: 'call', profile: profile, name: name, offset: offset,
                count: count, thread: Process.getCurrentThreadId(),
                caller: caller.toString(),
                caller_module: owner ? owner.name : 'unknown',
                caller_offset: owner ? caller.sub(owner.base).toString() : null,
                args: raw
              });
            }
          },
          onLeave: function (ret) {
            if (this.logged) {
              send({kind: 'return', profile: profile, name: name,
                    offset: offset, return_raw: ret.toString()});
            }
          }
        });
        listeners.push(listener);
        hooked++;
      } catch (error) {
        failures++;
        if (failures <= 10) {
          send({kind: 'hook_error', symbol: name, error: String(error)});
        }
      }
    });
    send({
      kind: 'ready', profile: profile, arch: Process.arch,
      base: base.toString(), exports: exports.length, cap: cap,
      candidate_count: candidates.length, hooked: hooked,
      failures: failures, sample_limit: maxSamples,
      selected: candidates.map(function (e) {
        return {name: e.name, offset: e.address.sub(base).toString()};
      })
    });
    rpc.exports = {
      snapshot: function () {
        return {profile: profile, hooks: hooked, counts: counts,
                names: names, sampled: sampled, failures: failures};
      },
      mark: function (name) {
        send({kind: 'phase', profile: profile, name: String(name)});
        return true;
      }
    };
  }
}

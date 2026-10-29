/*
 * PV7 ARM32 reference function tracer (Frida 12.x-compatible JavaScript).
 *
 * Observes a curated set of exported functions from the running, unmodified
 * ARM guest libgame.so. No Stalker, symbol replacement, or arbitrary
 * function invocation. All arguments/return values are raw machine words.
 *
 * The static symbol inventory is much larger than the selected hook set.
 * This is sampled call coverage, NOT an exhaustive ARM instruction trace.
 */
var mod = Process.findModuleByName('libgame.so');
if (mod === null) {
  send({kind: 'fatal', detail: 'libgame.so not loaded'});
} else {
  var ex = Module.enumerateExportsSync('libgame.so').filter(function (x) {
    return x.type === 'function';
  });
  var wanted = [
    'CCDirector', 'CCFileUtils', 'CCTextureCache', 'CCSpriteFrameCache',
    'CCScene', 'FrameHandler', 'JNI_OnLoad',
    'Java_org_cocos2dx_lib_Cocos2dxRenderer_nativeRender'
  ];
  var focus = /drawScene|mainLoop|runWithScene|replaceScene|setNextScene|sharedDirector|addImage|getFileData|fullPathForFilename|addSpriteFramesWithFile|spriteFrameByName|CCScene|FrameHandler|JNI_OnLoad|nativeRender/;
  var candidates = ex.filter(function (e) {
    if (!focus.test(e.name)) return false;
    for (var i = 0; i < wanted.length; i++) {
      if (e.name.indexOf(wanted[i]) >= 0) return true;
    }
    return false;
  }).slice(0, 90);

  var counts = {}, sampled = 0, hooked = 0, failures = 0;
  var maxSamples = 1000, base = mod.base;
  candidates.forEach(function (e) {
    var name = e.name, offset = e.address.sub(base).toString();
    try {
      Interceptor.attach(e.address, {
        onEnter: function (args) {
          var count = (counts[offset] || 0) + 1;
          counts[offset] = count;
          this.logged = false;
          if (sampled < maxSamples && (count <= 10 || count % 200 === 0)) {
            sampled++;
            this.logged = true;
            var caller = this.returnAddress;
            var owner = Process.findModuleByAddress(caller);
            var regs = [];
            for (var i = 0; i < 4; i++) regs.push(args[i].toString());
            send({
              kind: 'call', name: name, offset: offset, count: count,
              thread: Process.getCurrentThreadId(),
              caller: caller.toString(),
              caller_module: owner ? owner.name : 'unknown',
              caller_offset: owner ? caller.sub(owner.base).toString() : null,
              args: regs
            });
          }
        },
        onLeave: function (rv) {
          if (this.logged) {
            send({kind: 'return', name: name, offset: offset,
                  return_raw: rv.toString()});
          }
        }
      });
      hooked++;
    } catch (error) {
      failures++;
      if (failures <= 10) {
        send({kind: 'hook_error', symbol: name, error: String(error)});
      }
    }
  });

  send({
    kind: 'ready', arch: Process.arch, base: base.toString(),
    exports: ex.length, candidate_count: candidates.length,
    hooked: hooked, failures: failures, sample_limit: maxSamples,
    sample_names: candidates.slice(0, 15).map(function (e) {return e.name;})
  });
  rpc.exports = {
    snapshot: function () {
      return {hooks: hooked, counts: counts, sampled: sampled,
              failures: failures};
    }
  };
}

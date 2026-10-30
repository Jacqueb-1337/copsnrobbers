/* PV7 reference instrumentation: tested against Android 9 x86 native bridge.
   Observes host JNI/Android native API boundaries, not guest instruction-level
   ARM call graph. The MAX_EVENTS cap is per-call: counts continue after cap. */
const MAX_EVENTS = 15000;
const targets = {
  'libc.so':['open','openat','fopen','read','write','close','mmap','munmap',
             'pthread_create','pthread_mutex_lock','pthread_mutex_unlock',
             'clock_gettime','nanosleep','__system_property_get','stat','fstat','access'],
  'libdl.so':['dlopen','dlsym','dlclose','android_dlopen_ext'],
  'liblog.so':['__android_log_print','__android_log_write'],
  'libEGL.so':['eglCreateContext','eglMakeCurrent','eglSwapBuffers','eglCreateWindowSurface'],
  'libGLESv2.so':['glClear','glDrawArrays','glDrawElements','glBindTexture',
                  'glTexImage2D','glUseProgram','glGetError','glViewport']
};
let observed = 0, dropped = 0, seq = 0;
const buffer = [];
const hooked = [];
const counts = {};
for (const [library, names] of Object.entries(targets)) {
  const m = Process.findModuleByName(library);
  if (m === null) continue;
  for (const name of names) {
    const address = m.findExportByName(name);
    if (address === null) continue;
    try {
      Interceptor.attach(address, {
        onEnter(args) {
          this.sequence = ++seq;
          this.symbol = name;
          observed++;
          counts[library+'!'+name] = (counts[library+'!'+name] || 0) + 1;
          if (this.sequence <= MAX_EVENTS) {
            const rawArgs = [];
            for (let i = 0; i < 5; i++) rawArgs.push(args[i].toString());
            buffer.push({kind:'enter', seq:this.sequence, api:name, library,
              monotonic_order:this.sequence, time:Date.now(),
              thread_id:Process.getCurrentThreadId(),
              return_address:this.returnAddress.toString(), args_raw:rawArgs});
          } else dropped++;
        },
        onLeave(rv) {
          if (this.sequence && this.sequence <= MAX_EVENTS) {
            buffer.push({kind:'return',seq:this.sequence, api:this.symbol,
              time:Date.now(),return_raw:rv.toString()});
          }
        }
      });
      hooked.push(library+'!'+name);
    } catch (error) {}
  }
}
send({kind:'ready',arch:Process.arch,hooked,coverage:
  'host Android APIs only; guest ARM internal calls not instrumented'});
const interval = setInterval(function () {
  while (buffer.length) send({kind:'batch',events:buffer.splice(0,250)});
  send({kind:'summary', observed, dropped, counts});
}, 1000);
rpc.exports = {
  flush() {
    clearInterval(interval);
    while (buffer.length) send({kind:'batch',events:buffer.splice(0,250)});
    return {observed,dropped,hooked:hooked.length,counts};
  }
};
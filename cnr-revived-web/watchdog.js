const fs = require('fs');
const { spawn } = require('child_process');

const pidFile = './server.pid';
const logFile = './server.log';

const MIN_RESTART_DELAY = 1000;
const MAX_RESTART_DELAY = 30000;

let restartDelay = MIN_RESTART_DELAY;
let lastStartTime = 0;

fs.writeFileSync(pidFile, process.pid.toString());

let currentChild = null;

process.on('exit', () => {
  try { fs.unlinkSync(pidFile); } catch (_) {}
});
process.on('SIGTERM', () => {
  if (currentChild) currentChild.kill('SIGTERM');
  process.exit(0);
});
process.on('SIGINT', () => {
  if (currentChild) currentChild.kill('SIGTERM');
  process.exit(0);
});

function startServer() {
  lastStartTime = Date.now();
  const logFd = fs.openSync(logFile, 'a');

  const child = spawn('node', ['index.js', 'play.jacqueb.me'], {
    stdio: ['ignore', logFd, logFd]
  });
  currentChild = child;

  const ts = new Date().toISOString();
  fs.appendFileSync(logFile, `[${ts}] [watchdog] server started (PID ${child.pid})\n`);

  child.on('close', (code) => {
    currentChild = null;
    fs.closeSync(logFd);
    const ts2 = new Date().toISOString();
    fs.appendFileSync(logFile, `[${ts2}] [watchdog] server exited (code ${code}), restarting in ${restartDelay}ms\n`);

    const uptime = Date.now() - lastStartTime;
    if (uptime > 60000) {
      restartDelay = MIN_RESTART_DELAY;
    } else {
      restartDelay = Math.min(restartDelay * 2, MAX_RESTART_DELAY);
    }

    setTimeout(startServer, restartDelay);
  });
}

startServer();

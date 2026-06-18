const fs = require('fs');
const { spawn } = require('child_process');

const pidFile = './server.pid';
const logFile = './server.log';
const serverArg = process.env.CNR_SERVER_ARG || '';

if (fs.existsSync(pidFile)) {
  const pid = fs.readFileSync(pidFile, 'utf8').trim();
  try {
    process.kill(pid, 0);
    console.log(`Server already running (PID: ${pid})`);
    console.log('Run "npm run stop" first to stop it');
    process.exit(1);
  } catch (_) {
    fs.unlinkSync(pidFile);
  }
}

const logFd = fs.openSync(logFile, 'a');

const child = spawn('node', ['watchdog.js'], {
  detached: true,
  stdio: ['ignore', logFd, logFd]
});

child.unref();

fs.closeSync(logFd);

console.log(`Watchdog started in background (PID: ${child.pid})`);
console.log(`Logs: ${logFile}`);
console.log('Run "npm run logs" to view logs');
console.log('Run "npm run stop" to stop server');

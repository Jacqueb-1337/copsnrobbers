const fs = require('fs');

const pidFile = './server.pid';

if (!fs.existsSync(pidFile)) {
  console.log('Server is NOT running (no PID file)');
  process.exit(0);
}

const pid = fs.readFileSync(pidFile, 'utf8').trim();

try {
  process.kill(pid, 0);
  console.log(`Server is RUNNING (PID: ${pid})`);

  const stats = fs.statSync(pidFile);
  const uptime = Math.floor((Date.now() - stats.mtimeMs) / 1000);
  console.log(`Started: ${Math.floor(uptime / 60)} minutes ago`);
} catch (err) {
  if (err.code === 'ESRCH') {
    console.log(`Server is NOT running (stale PID file: ${pid})`);
    fs.unlinkSync(pidFile);
  } else {
    console.error('Error checking process:', err.message);
  }
}

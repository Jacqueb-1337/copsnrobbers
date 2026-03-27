const fs = require('fs');

const pidFile = './server.pid';

if (!fs.existsSync(pidFile)) {
  console.log('Server is not running (no PID file)');
  console.log('Run "npm run start:bg" to start it');
  process.exit(1);
}

const pid = fs.readFileSync(pidFile, 'utf8').trim();

try {
  process.kill(pid, 'SIGTERM');
  fs.unlinkSync(pidFile);
  console.log(`Server stopped (PID: ${pid})`);
} catch (err) {
  if (err.code === 'ESRCH') {
    console.log(`Process ${pid} not found (stale PID file)`);
    fs.unlinkSync(pidFile);
  } else {
    console.error('Error stopping server:', err.message);
    process.exit(1);
  }
}

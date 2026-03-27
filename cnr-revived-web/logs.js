const fs = require('fs');

const logFile = './server.log';

if (!fs.existsSync(logFile)) {
  console.log('No log file yet');
  console.log('Server may not have been started or no activity yet');
  process.exit(0);
}

const content = fs.readFileSync(logFile, 'utf8');
const lines = content.split('\n');
const last50 = lines.slice(-50).join('\n');

console.log(last50);

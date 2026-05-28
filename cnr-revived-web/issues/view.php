<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width,initial-scale=1"/>
<title>Issue — CNR Revival</title>
<link rel="stylesheet" href="style.css"/>
</head>
<body>
<div class="page">

  <header class="site-header">
    <div class="logo">CNR</div>
    <div>
      <h1>Issue Tracker</h1>
      <div class="sub">Cops n Robbers Revival &mdash; <a href="https://play.jacqueb.me">play.jacqueb.me</a></div>
    </div>
    <div class="header-back" style="display:flex;gap:6px;align-items:center">
      <a href="index.html" class="btn btn-ghost btn-sm">&larr; All Issues</a>
      <button class="btn btn-ghost btn-sm" id="btn-admin-toggle">Admin</button>
    </div>
  </header>

  <div id="view-root">
    <div class="loading-center"><span class="loader"></span></div>
  </div>

</div>

<!-- ── Edit Issue Modal ────────────────────────────────────────────────── -->
<div class="modal-overlay hidden" id="modal-edit">
  <div class="modal">
    <button class="modal-close" id="modal-close-edit">&times;</button>
    <h2>Edit Issue</h2>
    <div class="field">
      <label for="ei-title">Title</label>
      <input type="text" id="ei-title" maxlength="200"/>
    </div>
    <div class="field">
      <label for="ei-body">Description</label>
      <textarea id="ei-body" rows="8"></textarea>
    </div>
    <div class="form-row">
      <div class="field">
        <label for="ei-mod">Related mod</label>
        <select id="ei-mod"><option value="">None</option></select>
      </div>
      <div class="field">
        <label for="ei-ver">Version</label>
        <select id="ei-ver"><option value="">Any version</option></select>
      </div>
    </div>
    <div class="field">
      <label>Tags</label>
      <div class="tag-input-wrap" id="ei-tag-wrap">
        <input type="text" id="ei-tag-input" placeholder="press Enter to add" maxlength="32"/>
      </div>
    </div>
    <div style="display:flex;gap:8px;justify-content:flex-end;margin-top:4px">
      <button class="btn btn-ghost" id="btn-cancel-edit">Cancel</button>
      <button class="btn btn-primary" id="btn-submit-edit">Save Changes</button>
    </div>
    <div id="ei-error" style="color:var(--danger);font-size:13px;display:none"></div>
  </div>
</div>

<!-- ── Edit Comment Modal ─────────────────────────────────────────────── -->
<div class="modal-overlay hidden" id="modal-edit-comment">
  <div class="modal">
    <button class="modal-close" id="modal-close-ec">&times;</button>
    <h2>Edit Comment</h2>
    <div class="field">
      <label for="ec-body">Comment</label>
      <textarea id="ec-body" rows="5"></textarea>
    </div>
    <div style="display:flex;gap:8px;justify-content:flex-end;margin-top:4px">
      <button class="btn btn-ghost" id="btn-cancel-ec">Cancel</button>
      <button class="btn btn-primary" id="btn-submit-ec">Save</button>
    </div>
  </div>
</div>

<!-- ── Admin Login Modal ─────────────────────────────────────────────────── -->
<div class="modal-overlay hidden" id="modal-admin">
  <div class="modal" style="max-width:360px">
    <button class="modal-close" id="modal-close-admin">&times;</button>
    <h2>Admin Login</h2>
    <div class="field">
      <label for="al-user">Username</label>
      <input type="text" id="al-user" autocomplete="username"/>
    </div>
    <div class="field">
      <label for="al-pass">Password</label>
      <input type="password" id="al-pass" autocomplete="current-password"/>
    </div>
    <div style="display:flex;gap:8px;justify-content:flex-end;margin-top:4px">
      <button class="btn btn-ghost" id="btn-cancel-admin">Cancel</button>
      <button class="btn btn-primary" id="btn-submit-admin">Login</button>
    </div>
    <div id="al-error" style="color:var(--danger);font-size:13px;display:none"></div>
  </div>
</div>

<!-- ── Toast ──────────────────────────────────────────────────────────── -->
<div id="toast"></div>

<script>
// ── Helpers ───────────────────────────────────────────────────────────────
const API = 'api.php';
let isAdmin = false;

async function checkAdminStatus() {
  try {
    const d = await fetch('login.php?action=status').then(r => r.json());
    isAdmin = d.admin === true;
  } catch { isAdmin = false; }
  const btn = document.getElementById('btn-admin-toggle');
  if (!btn) return;
  if (isAdmin) {
    btn.textContent = 'Logout';
    btn.style.color = 'var(--accent)';
    btn.style.borderColor = 'rgba(74,222,128,0.3)';
  } else {
    btn.textContent = 'Admin';
    btn.style.color = '';
    btn.style.borderColor = '';
  }
}

function esc(s) {
  return String(s ?? '')
    .replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;')
    .replace(/"/g,'&quot;').replace(/'/g,'&#39;');
}

function fmtDate(ts) {
  const d = new Date(ts * 1000);
  const diff = Date.now() - d.getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 1) return 'just now';
  if (mins < 60) return mins + 'm ago';
  const hrs = Math.floor(mins / 60);
  if (hrs < 24) return hrs + 'h ago';
  const days = Math.floor(hrs / 24);
  if (days < 30) return days + 'd ago';
  return d.toLocaleDateString();
}

function statusBadge(s) {
  const labels = { open: 'open', confirmed: 'confirmed bug', wip: 'in progress', resolved: 'resolved', wontfix: "won't fix", closed: 'closed' };
  return `<span class="status-dot ${esc(s)}">${labels[s] || s}</span>`;
}

let toastTimer;
function toast(msg, isError = false) {
  const el = document.getElementById('toast');
  el.textContent = msg;
  el.className = 'show' + (isError ? ' error' : '');
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => { el.className = ''; }, 3400);
}

async function apiFetch(action, params = {}) {
  const qs = Object.entries(params).map(([k,v]) => v ? '&'+k+'='+encodeURIComponent(v) : '').join('');
  const r  = await fetch(API + '?action=' + action + qs);
  return r.json();
}
async function apiPost(action, data) {
  const r = await fetch(API + '?action=' + action, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
  });
  return r.json();
}

function getTokens() {
  try { return JSON.parse(localStorage.getItem('cnr_issue_tokens') || '{}'); } catch { return {}; }
}
function getIssueToken(id) { return getTokens()[id] || null; }

function getCommentTokens() {
  try { return JSON.parse(localStorage.getItem('cnr_comment_tokens') || '{}'); } catch { return {}; }
}
function saveCommentToken(id, token) {
  const t = getCommentTokens(); t[id] = token;
  localStorage.setItem('cnr_comment_tokens', JSON.stringify(t));
}
function getCommentToken(id) { return getCommentTokens()[id] || null; }

// ── Issue ID from URL ─────────────────────────────────────────────────────
const issueId = new URLSearchParams(location.search).get('id');

// ── Mod list ──────────────────────────────────────────────────────────────
let mods = [];
async function loadMods() {
  const data = await apiFetch('get_mods');
  mods = data.mods || [];
}

function populateModSelect(selId, currentMod) {
  const sel = document.getElementById(selId);
  sel.innerHTML = '<option value="">None</option>';
  mods.forEach(m => {
    sel.insertAdjacentHTML('beforeend',
      `<option value="${esc(m.id)}"${m.id===currentMod?' selected':''}>${esc(m.name)}</option>`);
  });
}

function populateVerSelect(selId, modId, currentVer) {
  const sel = document.getElementById(selId);
  sel.innerHTML = '<option value="">Any version</option>';
  const mod = mods.find(m => m.id === modId);
  (mod?.versions || []).forEach(v => {
    sel.insertAdjacentHTML('beforeend',
      `<option value="${esc(v)}"${v===currentVer?' selected':''}>${esc(v)}</option>`);
  });
  sel.disabled = !modId;
}

// ── Diff ───────────────────────────────────────────────────────────────────
function lineDiff(oldText, newText) {
  const oldLines = oldText.split('\n');
  const newLines = newText.split('\n');
  let html = '';
  const maxLines = Math.max(oldLines.length, newLines.length);
  for (let i = 0; i < maxLines; i++) {
    const o = oldLines[i];
    const n = newLines[i];
    if (o === undefined) {
      html += `<div class="diff-add">+ ${esc(n)}</div>`;
    } else if (n === undefined) {
      html += `<div class="diff-del">- ${esc(o)}</div>`;
    } else if (o !== n) {
      html += `<div class="diff-del">- ${esc(o)}</div><div class="diff-add">+ ${esc(n)}</div>`;
    } else {
      html += `<div class="diff-ctx">  ${esc(o)}</div>`;
    }
  }
  return html;
}

// ── CS source diff (LCS-based, context-collapsed) ────────────────────────
function csFileDiff(oldText, newText) {
  const CONTEXT  = 4;
  const oldLines = (oldText  || '').split('\n');
  const newLines = (newText || '').split('\n');
  const m = oldLines.length, n = newLines.length;

  // Build LCS DP table (capped to avoid OOM on very large files)
  function computeOps() {
    if (m * n > 3000000) {
      // Fallback: show new file as all-added
      return newLines.map(l => ({ t: '+', v: l }));
    }
    const dp = [];
    for (let i = 0; i <= m; i++) dp[i] = new Int32Array(n + 1);
    for (let i = 1; i <= m; i++)
      for (let j = 1; j <= n; j++)
        dp[i][j] = oldLines[i-1] === newLines[j-1]
          ? dp[i-1][j-1] + 1
          : Math.max(dp[i-1][j], dp[i][j-1]);
    const ops = [];
    let i = m, j = n;
    while (i > 0 || j > 0) {
      if (i > 0 && j > 0 && oldLines[i-1] === newLines[j-1]) {
        ops.push({ t: '=', v: oldLines[i-1] }); i--; j--;
      } else if (j > 0 && (i === 0 || dp[i][j-1] >= dp[i-1][j])) {
        ops.push({ t: '+', v: newLines[j-1] }); j--;
      } else {
        ops.push({ t: '-', v: oldLines[i-1] }); i--;
      }
    }
    return ops.reverse();
  }

  const ops = computeOps();

  // Mark context lines to show (within CONTEXT of a change)
  const show = new Array(ops.length).fill(false);
  for (let i = 0; i < ops.length; i++) {
    if (ops[i].t !== '=') {
      for (let j = Math.max(0, i - CONTEXT); j <= Math.min(ops.length - 1, i + CONTEXT); j++)
        show[j] = true;
    }
  }
  // Always show first/last few lines
  for (let i = 0; i < Math.min(CONTEXT, ops.length); i++) show[i] = true;
  for (let i = Math.max(0, ops.length - CONTEXT); i < ops.length; i++) show[i] = true;

  let html = '', skipCount = 0;
  for (let i = 0; i < ops.length; i++) {
    const op = ops[i];
    if (op.t === '=' && !show[i]) {
      skipCount++;
    } else {
      if (skipCount > 0) {
        html += `<div class="diff-skip">\u22ef ${skipCount} unchanged line${skipCount !== 1 ? 's' : ''} \u22ef</div>`;
        skipCount = 0;
      }
      if      (op.t === '+') html += `<div class="diff-add">+&nbsp;${esc(op.v)}</div>`;
      else if (op.t === '-') html += `<div class="diff-del">-&nbsp;${esc(op.v)}</div>`;
      else                   html += `<div class="diff-ctx">&nbsp;&nbsp;${esc(op.v)}</div>`;
    }
  }
  if (skipCount > 0)
    html += `<div class="diff-skip">\u22ef ${skipCount} unchanged line${skipCount !== 1 ? 's' : ''} \u22ef</div>`;
  return html;
}

async function renderCsDiff(commentId, newFilename) {
  const container = document.getElementById('cs-diff-' + commentId);
  if (!container) return;
  try {
    const issue = currentIssue;
    if (!issue || !issue.related_mod || !issue.related_version) {
      container.innerHTML = '<p style="color:var(--muted);font-size:12px">No related mod/version set on this issue — cannot compute diff.</p>';
      return;
    }
    const mod     = issue.related_mod;
    const ver     = issue.related_version;
    const oldPath = `../mods/${encodeURIComponent(mod)}/${encodeURIComponent(mod)}-${encodeURIComponent(ver)}.cs`;
    const newPath = `uploads/${encodeURIComponent(newFilename)}`;
    const [oldRes, newRes] = await Promise.all([fetch(oldPath), fetch(newPath)]);
    if (!oldRes.ok) throw new Error(`Old file not found: ${esc(mod)}-${esc(ver)}.cs`);
    if (!newRes.ok) throw new Error(`New file not found`);
    const [oldText, newText] = await Promise.all([oldRes.text(), newRes.text()]);
    const diffHtml = csFileDiff(oldText, newText);
    container.innerHTML =
      `<div style="font-size:11px;color:var(--muted);margin-bottom:6px">` +
      `Diff: <code>${esc(mod)}-${esc(ver)}.cs</code> &rarr; <code>${esc(newFilename)}</code></div>` +
      `<div class="diff-block cs-diff-block">${diffHtml}</div>`;
  } catch (e) {
    container.innerHTML = `<span style="color:var(--danger);font-size:12px">Diff error: ${esc(e.message)}</span>`;
  }
}

async function uploadCsFile(file, commentId) {
  const fd = new FormData();
  fd.append('file', file);
  fd.append('comment_id', commentId);
  return fetch('api.php?action=upload_attachment', { method: 'POST', body: fd }).then(r => r.json());
}

// ── Image compression + upload ────────────────────────────────────────────
async function compressIfNeeded(file, maxMB = 5) {
  if (file.size <= maxMB * 1024 * 1024 || !file.type.startsWith('image/')) return file;
  return new Promise(resolve => {
    const img = new Image();
    const url = URL.createObjectURL(file);
    img.onload = () => {
      URL.revokeObjectURL(url);
      const canvas = document.createElement('canvas');
      let { width, height } = img;
      const maxDim = 2048;
      if (width > maxDim || height > maxDim) {
        const ratio = Math.min(maxDim / width, maxDim / height);
        width = Math.round(width * ratio); height = Math.round(height * ratio);
      }
      canvas.width = width; canvas.height = height;
      canvas.getContext('2d').drawImage(img, 0, 0, width, height);
      const tryQ = q => canvas.toBlob(blob => {
        if (blob.size > maxMB * 1024 * 1024 && q > 0.3) tryQ(+(q - 0.1).toFixed(1));
        else resolve(new File([blob], file.name.replace(/\.[^.]+$/, '.jpg'), { type: 'image/jpeg' }));
      }, 'image/jpeg', q);
      tryQ(0.85);
    };
    img.src = url;
  });
}
async function uploadAttachment(file, issueId, commentId) {
  const f  = await compressIfNeeded(file);
  const fd = new FormData();
  fd.append('file', f);
  if (issueId)   fd.append('issue_id',   issueId);
  if (commentId) fd.append('comment_id', commentId);
  return fetch('api.php?action=upload_attachment', { method: 'POST', body: fd }).then(r => r.json());
}

// ── Comment attachment thumbnails (display only, immediate delete) ─────────
function renderAttachments(attachments) {
  if (!attachments || !attachments.length) return '';
  const thumbs = attachments.map(a =>
    `<div class="attachment-thumb">` +
    `<a href="uploads/${esc(a.filename)}" target="_blank" rel="noopener">` +
    `<img src="uploads/${esc(a.filename)}" alt="screenshot" loading="lazy"/></a>` +
    (isAdmin ? `<button class="attachment-delete" data-attid="${esc(a.id)}" data-immediate="1" title="Delete">&times;</button>` : '') +
    `</div>`
  ).join('');
  return `<div class="attachment-grid">${thumbs}</div>`;
}

// ── Render attachment thumbnails ──────────────────────────────────────────
let pendingIssueUploads   = [];   // [{id, file, previewUrl}]
let pendingIssueDeletions = new Set();
let issueCanModify        = false;

function renderAttachmentGrid(attachments, canModify) {
  const existing = (attachments || []).map(a => {
    const pd = pendingIssueDeletions.has(a.id);
    return `<div class="attachment-thumb${pd ? ' pending-delete' : ''}">` +
      `<a href="uploads/${esc(a.filename)}" target="_blank" rel="noopener">` +
      `<img src="uploads/${esc(a.filename)}" alt="screenshot" loading="lazy"/></a>` +
      (isAdmin ? `<button class="attachment-delete" data-attid="${esc(a.id)}" title="${pd ? 'Undo' : 'Delete'}">${pd ? '↩' : '×'}</button>` : '') +
      `</div>`;
  }).join('');
  const pendingHTML = pendingIssueUploads.map(p =>
    `<div class="attachment-thumb pending-upload">` +
    `<img src="${p.previewUrl}" alt="pending" loading="lazy"/>` +
    `<button class="attachment-delete" data-tempid="${p.id}" title="Remove">&times;</button>` +
    `</div>`
  ).join('');
  const addBtn = canModify
    ? `<button class="attachment-add-btn" type="button" id="btn-add-attach" title="Add screenshot">+</button>` +
      `<input type="file" id="attach-file-input" accept="image/*" multiple style="display:none"/>` : '';
  const n = pendingIssueUploads.length + pendingIssueDeletions.size;
  const saveBtn = n > 0
    ? `<button class="btn btn-primary btn-sm" id="btn-save-attachments" style="align-self:center">Save (${n})</button>` : '';
  const hasContent = (attachments && attachments.length) || pendingIssueUploads.length || canModify;
  return hasContent ? `<div class="attachment-grid" id="issue-attach-grid">${existing}${pendingHTML}${addBtn}${saveBtn}</div>` : '';
}

function refreshAttachmentGrid() {
  const grid = document.getElementById('issue-attach-grid');
  if (!grid) return;
  const tmp = document.createElement('div');
  tmp.innerHTML = renderAttachmentGrid(currentIssue ? (currentIssue.attachments || []) : [], issueCanModify);
  if (tmp.firstChild) grid.replaceWith(tmp.firstChild);
}

// ── Tag input widget ──────────────────────────────────────────────────────
let eiTags = [];
function renderEiTags() {
  const wrap  = document.getElementById('ei-tag-wrap');
  const input = document.getElementById('ei-tag-input');
  Array.from(wrap.querySelectorAll('.tag-chip')).forEach(e => e.remove());
  eiTags.forEach((t, i) => {
    const chip = document.createElement('span');
    chip.className = 'tag-chip';
    chip.innerHTML = `${esc(t)}<button type="button" data-i="${i}">&times;</button>`;
    wrap.insertBefore(chip, input);
  });
}
function addEiTag(raw) {
  const t = raw.toLowerCase().replace(/[^a-z0-9 _\-]/g,'').trim();
  if (t && !eiTags.includes(t) && eiTags.length < 10) eiTags.push(t);
  renderEiTags();
}
document.getElementById('ei-tag-wrap').addEventListener('click', e => {
  if (e.target.dataset.i !== undefined) { eiTags.splice(+e.target.dataset.i,1); renderEiTags(); }
  else document.getElementById('ei-tag-input').focus();
});
document.getElementById('ei-tag-input').addEventListener('keydown', e => {
  if (e.key==='Enter'||e.key===',') { e.preventDefault(); addEiTag(e.target.value); e.target.value=''; }
  else if (e.key==='Backspace'&&e.target.value===''&&eiTags.length) { eiTags.pop(); renderEiTags(); }
});
document.getElementById('ei-tag-input').addEventListener('blur', e => {
  if (e.target.value.trim()) { addEiTag(e.target.value); e.target.value=''; }
});

// ── Version cascade in edit form ──────────────────────────────────────────
document.getElementById('ei-mod').addEventListener('change', function() {
  populateVerSelect('ei-ver', this.value, '');
});

// ── Render full issue page ────────────────────────────────────────────────
let currentIssue = null;
let currentComments = [];
let currentHistory = [];
let currentCommentHistory = {};

async function loadIssue() {
  if (!issueId) {
    document.getElementById('view-root').innerHTML =
      `<div class="empty"><div class="empty-icon">❌</div><p>No issue ID provided.</p></div>`;
    return;
  }
  const data = await apiFetch('get_issue', { id: issueId });
  if (data.error) {
    document.getElementById('view-root').innerHTML =
      `<div class="empty"><div class="empty-icon">❌</div><p>${esc(data.error)}</p></div>`;
    return;
  }
  currentIssue          = data.issue;
  currentComments       = data.comments || [];
  currentHistory        = data.history  || [];
  currentCommentHistory = data.comment_history || {};
  document.title        = `#${currentIssue.number} ${currentIssue.title} — CNR Issues`;
  renderIssue();
}

function renderIssue() {
  const issue      = currentIssue;
  const token      = getIssueToken(issue.id);
  const isOwner    = !!token;
  const canModify  = isAdmin || isOwner;
  issueCanModify   = canModify;
  const tags     = (issue.tags || []).map(t => `<span class="tag-pill">${esc(t)}</span>`).join('');
  const modBadge = issue.related_mod
    ? `<span class="issue-mod-badge">${esc(issue.related_mod)}${issue.related_version?' v'+esc(issue.related_version):''}</span>` : '';

  // History section
  let historyHTML = '';
  if (currentHistory.length > 0) {
    const entries = currentHistory.map((h, i) => {
      const titleChanged = h.old_title !== h.new_title;
      const bodyChanged  = h.old_body  !== h.new_body;
      return `<div style="margin-bottom:14px">
        <div style="font-size:12px;color:var(--muted);margin-bottom:6px">Edit ${i+1} &mdash; ${fmtDate(h.edited_at)}</div>
        ${titleChanged ? `<div style="font-size:12px;color:var(--muted);margin-bottom:4px">Title change:</div>
          <div class="diff-block">
            <div class="diff-del">- ${esc(h.old_title)}</div>
            <div class="diff-add">+ ${esc(h.new_title)}</div>
          </div>` : ''}
        ${bodyChanged ? `<div style="font-size:12px;color:var(--muted);margin:6px 0 4px">Body diff:</div>
          <div class="diff-block">${lineDiff(h.old_body, h.new_body)}</div>` : ''}
      </div>`;
    }).join('');
    historyHTML = `
      <details class="history-section card" style="padding:16px;margin-top:16px">
        <summary><span class="history-toggle-icon">▶</span> &nbsp;Edit history (${currentHistory.length})</summary>
        <div style="margin-top:14px">${entries}</div>
      </details>`;
  }

  // Actions
  let actionsHTML = '';
  if (canModify) {
    const allStatuses = [
      { v: 'open',      l: 'Open' },
      { v: 'confirmed', l: 'Confirmed Bug' },
      { v: 'wip',       l: 'In Progress' },
      { v: 'resolved',  l: 'Resolved' },
      { v: 'wontfix',   l: "Won't Fix" },
      { v: 'closed',    l: 'Closed' },
    ];
    const opts = allStatuses.filter(s => s.v !== issue.status)
      .map(s => `<option value="${s.v}">${s.l}</option>`).join('');
    actionsHTML = `
      <div class="actions-row">
        <button class="btn btn-ghost btn-sm" id="btn-edit-issue">Edit</button>
        <select id="sel-status" style="font-size:12px;padding:4px 8px;background:var(--surface-2);border:1px solid var(--border);border-radius:var(--radius-sm);color:var(--text);cursor:pointer">
          <option value="">Change status…</option>
          ${opts}
        </select>
        <button class="btn btn-danger btn-sm" id="btn-delete-issue">Delete</button>
      </div>`;
  }

  // Comments
  // Render comment list and schedule async diff renders
  const commentsHTML = currentComments.map(c => renderComment(c)).join('');
  const root = document.getElementById('view-root');
  root.innerHTML = `
    <div class="card issue-body-card">
      <div class="issue-header">
        <div class="issue-header-top">
          <h2>${esc(issue.title)}</h2>
          ${statusBadge(issue.status)}
        </div>
        <div class="meta-row">
          <span>#${issue.number}</span>
          <span>Opened ${fmtDate(issue.created_at)}</span>
          ${issue.updated_at !== issue.created_at ? `<span>· Updated ${fmtDate(issue.updated_at)}</span>` : ''}
          ${modBadge}
          ${tags}
        </div>
      </div>
      <div class="issue-body">${esc(issue.body)}</div>
      ${renderAttachmentGrid(issue.attachments || [], canModify)}
      ${actionsHTML}
    </div>

    ${historyHTML}

    <div class="comments-section">
      <h3>${currentComments.length} Comment${currentComments.length !== 1 ? 's' : ''}</h3>
      <div id="comment-list">${commentsHTML}</div>

      <!-- Add comment -->
      <div class="card" style="margin-top:12px">
        <h3 style="margin:0 0 14px;font-size:14px;color:var(--muted)">Add a comment</h3>
        <div class="field">
          <textarea id="add-comment-body" placeholder="Write your comment…" rows="4"></textarea>
        </div>
        <div class="field" style="margin-top:10px">
          <input type="email" id="add-comment-email" placeholder="Email (optional) — subscribe to updates"/>
        </div>
        <div style="display:flex;align-items:center;gap:8px;margin-top:10px;flex-wrap:wrap">
          <button class="btn btn-ghost btn-sm" type="button" id="btn-comment-attach">+ Screenshot</button>
          <button class="btn btn-ghost btn-sm" type="button" id="btn-comment-cs">+ .cs Source</button>
          <input type="file" id="comment-file-input" accept="image/*" multiple style="display:none"/>
          <input type="file" id="comment-cs-input" accept=".cs" style="display:none"/>
          <span id="comment-attach-names" style="font-size:12px;color:var(--muted)"></span>
          <label id="label-show-diff" style="font-size:12px;color:var(--muted);display:none;align-items:center;gap:4px">
            <input type="checkbox" id="chk-show-diff"/> Show as source diff
          </label>
          <button class="btn btn-primary btn-sm" id="btn-add-comment" style="margin-left:auto">Post Comment</button>
        </div>
      </div>

      <!-- Subscribe bar -->
      <div class="subscribe-bar">
        <label for="sub-email">Email updates:</label>
        <input type="email" id="sub-email" placeholder="your@email.com"/>
        <button class="btn btn-ghost btn-sm" id="btn-subscribe">Subscribe</button>
      </div>
      <div style="color:var(--muted);font-size:12px;margin-top:6px;text-align:right" id="sub-count"></div>
    </div>`;

  attachViewHandlers(issue, token);

  // Schedule async CS diff rendering for any diff comments
  currentComments.forEach(c => {
    if (c.is_diff) {
      const csAtt = (c.attachments || []).find(a => a.filename.endsWith('.cs'));
      if (csAtt) setTimeout(() => renderCsDiff(c.id, csAtt.filename), 0);
    }
  });
}

let data_subscriber_count = 0;
async function loadIssueAndRender() {
  await loadMods();
  const data = await apiFetch('get_issue', { id: issueId });
  if (data.error) {
    document.getElementById('view-root').innerHTML =
      `<div class="empty"><p>Error: ${esc(data.error)}</p></div>`;
    return;
  }
  currentIssue          = data.issue;
  currentComments       = data.comments || [];
  currentHistory        = data.history  || [];
  currentCommentHistory = data.comment_history || {};
  data_subscriber_count = data.subscriber_count || 0;
  document.title        = `#${currentIssue.number} ${currentIssue.title} — CNR Issues`;
  renderIssue();
  // Update subscriber count display
  const scEl = document.getElementById('sub-count');
  if (scEl && data_subscriber_count > 0) {
    scEl.textContent = data_subscriber_count + ' subscriber' + (data_subscriber_count !== 1 ? 's' : '');
  }
}

function renderComment(c) {
  const token      = getCommentToken(c.id);
  const isOwner    = !!token;
  const canModify  = isAdmin || isOwner;
  const edited     = c.updated_at !== c.created_at
    ? `<span class="comment-edited-badge">(edited)</span>` : '';
  const answerBadge = c.is_answer
    ? `<span class="answer-badge">&#10003; Answer</span>` : '';

  const history  = currentCommentHistory[c.id] || [];
  let histHTML   = '';
  if (history.length > 0) {
    const entries = history.map((h, i) =>
      `<div style="margin-bottom:10px">
         <div style="font-size:11px;color:var(--muted);margin-bottom:4px">Edit ${i+1} &mdash; ${fmtDate(h.edited_at)}</div>
         <div class="diff-block">${lineDiff(h.old_body, h.new_body)}</div>
       </div>`).join('');
    histHTML = `<details style="margin-top:8px">
      <summary style="font-size:12px;color:var(--muted);cursor:pointer">Show edit history</summary>
      <div style="margin-top:8px">${entries}</div>
    </details>`;
  }

  // Diff block: rendered async after mount
  let diffBlock = '';
  if (c.is_diff) {
    const csAtt = (c.attachments || []).find(a => a.filename.endsWith('.cs'));
    diffBlock = csAtt
      ? `<div class="diff-loading" id="cs-diff-${esc(c.id)}"><span class="loader"></span> Computing diff&hellip;</div>`
      : `<div style="color:var(--muted);font-size:12px;margin-top:8px">No .cs attachment found for diff.</div>`;
  }

  const markAnswerBtn = isAdmin && !c.is_answer
    ? `<button class="btn btn-ghost btn-sm" data-cid="${esc(c.id)}" data-action="mark-answer">&#10003; Mark as Answer</button>` : '';

  const actions = canModify ? `<div class="comment-actions">
    ${markAnswerBtn}
    <button class="btn btn-ghost btn-sm" data-cid="${esc(c.id)}" data-action="edit-comment">Edit</button>
    <button class="btn btn-danger btn-sm" data-cid="${esc(c.id)}" data-action="delete-comment">Delete</button>
  </div>` : '';

  const cardClass = c.is_answer ? 'comment-card answer-comment' : 'comment-card';
  return `<div class="${cardClass}" id="comment-${esc(c.id)}">
    <div class="comment-header">
      <span>${fmtDate(c.created_at)} ${edited}</span>
      ${answerBadge}
    </div>
    ${c.body ? `<div class="comment-body">${esc(c.body)}</div>` : ''}
    ${diffBlock}
    ${c.is_diff ? '' : renderAttachments(c.attachments || [])}
    ${histHTML}
    ${actions}
  </div>`;
}

// ── Event handlers ────────────────────────────────────────────────────────
function attachViewHandlers(issue, token) {

  // Edit issue
  document.getElementById('btn-edit-issue')?.addEventListener('click', () => {
    document.getElementById('ei-title').value = issue.title;
    document.getElementById('ei-body').value  = issue.body;
    populateModSelect('ei-mod', issue.related_mod || '');
    populateVerSelect('ei-ver', issue.related_mod || '', issue.related_version || '');
    eiTags = [...(issue.tags || [])]; renderEiTags();
    document.getElementById('modal-edit').classList.remove('hidden');
  });

  // Status select
  document.getElementById('sel-status')?.addEventListener('change', async function() {
    const status = this.value;
    if (!status) return;
    this.value = '';
    const res = await apiPost('set_status', { id: issue.id, token, status });
    if (res.error) { toast(res.error, true); return; }
    toast('Status updated.');
    loadIssueAndRender();
  });

  // Issue attachment — add button
  document.getElementById('btn-add-attach')?.addEventListener('click', () =>
    document.getElementById('attach-file-input').click());
  document.getElementById('attach-file-input')?.addEventListener('change', async function() {
    const files = Array.from(this.files);
    if (!files.length) return;
    this.disabled = true;
    for (const f of files) {
      const res = await uploadAttachment(f, issue.id, '');
      if (res.error) toast('Upload error: ' + res.error, true);
    }
    this.value = ''; this.disabled = false;
    toast('Screenshot uploaded.');
    loadIssueAndRender();
  });

  // Attachment delete (delegated)
  document.getElementById('view-root').addEventListener('click', async e => {
    const btn = e.target.closest('.attachment-delete');
    if (!btn || !isAdmin) return;
    e.preventDefault();
    if (!confirm('Delete this attachment?')) return;
    const res = await apiPost('delete_attachment', { id: btn.dataset.attid });
    if (res.error) { toast(res.error, true); return; }
    toast('Attachment deleted.');
    loadIssueAndRender();
  });

  // Comment screenshot button
  document.getElementById('btn-comment-attach').addEventListener('click', () =>
    document.getElementById('comment-file-input').click());
  document.getElementById('comment-file-input').addEventListener('change', function() {
    const names = Array.from(this.files).map(f => f.name).join(', ');
    document.getElementById('comment-attach-names').textContent = names;
  });

  // Comment .cs source button
  document.getElementById('btn-comment-cs').addEventListener('click', () =>
    document.getElementById('comment-cs-input').click());
  document.getElementById('comment-cs-input').addEventListener('change', function() {
    const f = this.files[0];
    if (!f) return;
    document.getElementById('comment-attach-names').textContent = f.name;
    const lbl = document.getElementById('label-show-diff');
    lbl.style.display = 'flex';
    document.getElementById('chk-show-diff').checked = true;
  });

  // Delete issue
  document.getElementById('btn-delete-issue')?.addEventListener('click', async () => {
    if (!confirm('Delete this issue? This cannot be undone.')) return;
    const res = await apiPost('delete_issue', { id: issue.id, token });
    if (res.error) { toast(res.error, true); return; }
    toast('Issue deleted.');
    setTimeout(() => { location.href = 'index.html'; }, 1200);
  });

  // Add comment
  document.getElementById('btn-add-comment').addEventListener('click', async () => {
    const body     = document.getElementById('add-comment-body').value.trim();
    const email    = document.getElementById('add-comment-email').value.trim();
    const imgFiles = Array.from(document.getElementById('comment-file-input').files);
    const csFiles  = Array.from(document.getElementById('comment-cs-input').files);
    const showDiff = document.getElementById('chk-show-diff').checked && csFiles.length > 0;
    if (!body && !csFiles.length) { toast('Comment cannot be empty.', true); return; }
    const res = await apiPost('add_comment', { issue_id: issue.id, body, email, is_diff: showDiff ? 1 : 0 });
    if (res.error) { toast(res.error, true); return; }
    saveCommentToken(res.id, res.token);
    for (const f of imgFiles) {
      const ar = await uploadAttachment(f, '', res.id);
      if (ar.error) toast('Attachment: ' + ar.error, true);
    }
    for (const f of csFiles) {
      const ar = await uploadCsFile(f, res.id);
      if (ar.error) toast('Source upload: ' + ar.error, true);
    }
    document.getElementById('add-comment-body').value   = '';
    document.getElementById('add-comment-email').value  = '';
    document.getElementById('comment-file-input').value = '';
    document.getElementById('comment-cs-input').value   = '';
    document.getElementById('comment-attach-names').textContent = '';
    document.getElementById('label-show-diff').style.display = 'none';
    toast('Comment posted.');
    loadIssueAndRender();
  });

  // Edit / delete comment (delegated)
  let editingCommentId = null;
  document.getElementById('comment-list').addEventListener('click', e => {
    const btn = e.target.closest('[data-action]');
    if (!btn) return;
    const cid    = btn.dataset.cid;
    const action = btn.dataset.action;
    const ctoken = getCommentToken(cid);
    if (!isAdmin && !ctoken) { toast('You don\'t own this comment.', true); return; }
    if (action === 'delete-comment') {
      if (!confirm('Delete this comment?')) return;
      apiPost('delete_comment', { id: cid, token: ctoken || '' }).then(res => {
        if (res.error) { toast(res.error, true); return; }
        toast('Comment deleted.');
        loadIssueAndRender();
      });
    } else if (action === 'edit-comment') {
      const card = document.getElementById('comment-' + cid);
      const bodyEl = card.querySelector('.comment-body');
      editingCommentId = cid;
      document.getElementById('ec-body').value = bodyEl ? bodyEl.textContent : '';
      document.getElementById('modal-edit-comment').classList.remove('hidden');
    } else if (action === 'mark-answer') {
      if (!isAdmin) return;
      apiPost('mark_answer', { comment_id: cid }).then(res => {
        if (res.error) { toast(res.error, true); return; }
        toast('Marked as answer.');
        loadIssueAndRender();
      });
    }
  });

  // Submit comment edit
  document.getElementById('btn-submit-ec').addEventListener('click', async () => {
    if (!editingCommentId) return;
    const body   = document.getElementById('ec-body').value.trim();
    const ctoken = getCommentToken(editingCommentId);
    if (!body) { toast('Comment cannot be empty.', true); return; }
    const res = await apiPost('edit_comment', { id: editingCommentId, token: ctoken, body });
    if (res.error) { toast(res.error, true); return; }
    document.getElementById('modal-edit-comment').classList.add('hidden');
    toast('Comment updated.');
    loadIssueAndRender();
  });

  // Subscribe
  document.getElementById('btn-subscribe').addEventListener('click', async () => {
    const email = document.getElementById('sub-email').value.trim();
    if (!email) { toast('Enter an email address.', true); return; }
    const res = await apiPost('subscribe', { issue_id: issue.id, email });
    if (res.error) { toast(res.error, true); return; }
    document.getElementById('sub-email').value = '';
    toast(res.message || 'Subscribed!');
  });
}

// ── Modal close handlers ──────────────────────────────────────────────────
document.getElementById('modal-close-edit').addEventListener('click', () =>
  document.getElementById('modal-edit').classList.add('hidden'));
document.getElementById('btn-cancel-edit').addEventListener('click', () =>
  document.getElementById('modal-edit').classList.add('hidden'));
document.getElementById('modal-edit').addEventListener('click', e => {
  if (e.target === document.getElementById('modal-edit'))
    document.getElementById('modal-edit').classList.add('hidden');
});

document.getElementById('modal-close-ec').addEventListener('click', () =>
  document.getElementById('modal-edit-comment').classList.add('hidden'));
document.getElementById('btn-cancel-ec').addEventListener('click', () =>
  document.getElementById('modal-edit-comment').classList.add('hidden'));

// ── Save issue edits ──────────────────────────────────────────────────────
document.getElementById('btn-submit-edit').addEventListener('click', async () => {
  const title = document.getElementById('ei-title').value.trim();
  const body  = document.getElementById('ei-body').value.trim();
  const mod   = document.getElementById('ei-mod').value;
  const ver   = document.getElementById('ei-ver').value;
  const errEl = document.getElementById('ei-error');
  errEl.style.display = 'none';
  if (!title) { errEl.textContent = 'Title required.'; errEl.style.display = 'block'; return; }
  if (!body)  { errEl.textContent = 'Description required.'; errEl.style.display = 'block'; return; }
  const token = getIssueToken(issueId);
  const res   = await apiPost('edit_issue', {
    id: issueId, token, title, body,
    related_mod: mod, related_version: ver, tags: eiTags
  });
  if (res.error) { errEl.textContent = res.error; errEl.style.display = 'block'; return; }
  document.getElementById('modal-edit').classList.add('hidden');
  toast('Issue updated.');
  loadIssueAndRender();
});

// ── Admin login modal handlers ─────────────────────────────────────────────────
document.getElementById('btn-admin-toggle').addEventListener('click', () => {
  if (isAdmin) {
    fetch('login.php?action=logout').then(() => location.reload());
  } else {
    document.getElementById('modal-admin').classList.remove('hidden');
    setTimeout(() => document.getElementById('al-user').focus(), 50);
  }
});
document.getElementById('modal-close-admin').addEventListener('click', () =>
  document.getElementById('modal-admin').classList.add('hidden'));
document.getElementById('btn-cancel-admin').addEventListener('click', () =>
  document.getElementById('modal-admin').classList.add('hidden'));
document.getElementById('modal-admin').addEventListener('click', e => {
  if (e.target === document.getElementById('modal-admin'))
    document.getElementById('modal-admin').classList.add('hidden');
});
document.getElementById('al-pass').addEventListener('keydown', e => {
  if (e.key === 'Enter') document.getElementById('btn-submit-admin').click();
});
document.getElementById('btn-submit-admin').addEventListener('click', async () => {
  const user  = document.getElementById('al-user').value.trim();
  const pass  = document.getElementById('al-pass').value;
  const errEl = document.getElementById('al-error');
  errEl.style.display = 'none';
  if (!user || !pass) { errEl.textContent = 'Enter username and password.'; errEl.style.display = 'block'; return; }
  const submitBtn = document.getElementById('btn-submit-admin');
  submitBtn.disabled = true; submitBtn.textContent = 'Logging in…';
  const res = await fetch('login.php', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username: user, password: pass })
  }).then(r => r.json()).catch(() => ({ error: 'Network error.' }));
  submitBtn.disabled = false; submitBtn.textContent = 'Login';
  if (res.error) { errEl.textContent = res.error; errEl.style.display = 'block'; return; }
  location.reload();
});

// ── Attachment handlers (delegated on view-root, set up once) ──────────────
document.getElementById('view-root').addEventListener('click', async e => {
  const delBtn = e.target.closest('.attachment-delete');
  if (delBtn) {
    e.preventDefault(); e.stopPropagation();
    const tempid    = delBtn.dataset.tempid;
    const attid     = delBtn.dataset.attid;
    const immediate = delBtn.dataset.immediate;
    if (immediate) {
      if (!confirm('Delete this attachment?')) return;
      const res = await apiPost('delete_attachment', { id: attid });
      if (res.error) { toast(res.error, true); return; }
      toast('Attachment deleted.');
      loadIssueAndRender();
      return;
    }
    if (tempid) {
      const p = pendingIssueUploads.find(u => u.id === tempid);
      if (p) URL.revokeObjectURL(p.previewUrl);
      pendingIssueUploads = pendingIssueUploads.filter(u => u.id !== tempid);
    } else if (attid) {
      if (pendingIssueDeletions.has(attid)) pendingIssueDeletions.delete(attid);
      else pendingIssueDeletions.add(attid);
    }
    refreshAttachmentGrid();
    return;
  }
  if (e.target.closest('#btn-add-attach')) {
    document.getElementById('attach-file-input')?.click();
    return;
  }
  if (e.target.closest('#btn-save-attachments')) {
    const btn = e.target.closest('#btn-save-attachments');
    btn.disabled = true; btn.textContent = 'Saving…';
    for (const id of pendingIssueDeletions) {
      const res = await apiPost('delete_attachment', { id });
      if (res.error) toast('Delete error: ' + res.error, true);
    }
    for (const p of pendingIssueUploads) {
      const res = await uploadAttachment(p.file, currentIssue.id, '');
      if (res.error) toast('Upload error: ' + res.error, true);
      URL.revokeObjectURL(p.previewUrl);
    }
    pendingIssueDeletions.clear();
    pendingIssueUploads = [];
    toast('Attachments saved.');
    loadIssueAndRender();
  }
});
document.getElementById('view-root').addEventListener('change', e => {
  if (e.target.id === 'attach-file-input') {
    Array.from(e.target.files).forEach(f => {
      const previewUrl = URL.createObjectURL(f);
      pendingIssueUploads.push({ id: 'tmp-' + Date.now() + '-' + Math.random(), file: f, previewUrl });
    });
    e.target.value = '';
    refreshAttachmentGrid();
  }
});

// ── Init ───────────────────────────────────────────────────────────────────────────
if (!issueId) {
  document.getElementById('view-root').innerHTML =
    `<div class="empty"><p>No issue ID in URL.</p><p><a href="index.html">Back to list</a></p></div>`;
} else {
  checkAdminStatus().then(() => loadIssueAndRender());
}
</script>
</body>
</html>

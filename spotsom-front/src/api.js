export const BASE = import.meta.env.VITE_API_URL || 'http://127.0.0.1:5000';

async function req(path, opts = {}) {
  let res;
  try {
    res = await fetch(BASE + path, opts);
  } catch {
    throw new Error('Não foi possível conectar à API. Verifique se ela está rodando e se o CORS está liberado.');
  }
  if (!res.ok) {
    let msg = '';
    try {
      const t = await res.text();
      try { const j = JSON.parse(t); msg = j.message || j.title || j.error || t; } catch { msg = t; }
    } catch { /* ignora */ }
    throw new Error(msg || `Erro ${res.status}`);
  }
  if (res.status === 204) return null;
  const t = await res.text();
  return t ? JSON.parse(t) : null;
}

function isUnknownEntity(value) {
  return value && typeof value === 'object'
    && typeof value.name === 'string'
    && value.name.trim().toLowerCase() === 'unknown';
}

export function hideUnknown(value) {
  if (Array.isArray(value)) {
    return value
      .filter((item) => !isUnknownEntity(item))
      .map(hideUnknown);
  }
  if (!value || typeof value !== 'object') return value;
  if (isUnknownEntity(value)) return undefined;

  return Object.fromEntries(
    Object.entries(value)
      .map(([key, item]) => [key, hideUnknown(item)])
      .filter(([, item]) => item !== undefined)
  );
}

const json = (method, body) => ({
  method,
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(body),
});

// A documentação não mostra o campo de ID nos itens; tentamos os nomes mais comuns
// e, para músicas, extraímos o ID do audioUrl (/api/musics/player/{id}).
export function getId(item) {
  if (!item) return undefined;
  const id = item.id ?? item.Id ?? item.ID;
  if (id != null) return id;
  const m = item.audioUrl?.match(/(\d+)\s*$/);
  return m ? Number(m[1]) : undefined;
}

const slash = (res) => (res === 'musics' || res === 'playlists' ? '/' : '');

export const api = {
  list: async (res, page = 1, size = 10) => hideUnknown(await req(`/api/${res}/page=${page}/size=${size}`)),
  get: (res, id) => req(`/api/${res}/${id}`),
  create: (res, body) => req(`/api/${res}/create`, json('POST', body)),
  update: (res, id, body) => req(`/api/${res}/${id}/edit${slash(res)}`, json('PUT', body)),
  remove: (res, id) => req(`/api/${res}/${id}`, { method: 'DELETE' }),
  addMusic: (res, id, musicId) =>
    req(`/api/${res}/${id}/${res === 'albums' ? 'addmusic' : 'add'}`, json('PUT', { musicId })),
  upload: (file) => {
    const f = new FormData();
    f.append('Audio', file);
    return req('/api/musics/create', { method: 'POST', body: f });
  },
  audioSrc: (id) => `${BASE}/api/musics/player/${id}`,
};

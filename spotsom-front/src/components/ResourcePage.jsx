import { useCallback, useEffect, useState } from 'react';
import { api, getId } from '../api';
import { RESOURCES } from '../config';
import Form from './Form';

const PAGE_SIZE = 10;

const show = (v) =>
  Array.isArray(v)
    ? v.length ? v.map((x) => (typeof x === 'object' ? x.name ?? JSON.stringify(x) : x)).join(', ') : '—'
    : v && typeof v === 'object' ? v.name ?? JSON.stringify(v) : v === '' || v == null ? '—' : String(v);

export default function ResourcePage({ res, onPlay }) {
  const cfg = RESOURCES[res];
  const [page, setPage] = useState(1);
  const [data, setData] = useState(null);
  const [err, setErr] = useState('');
  const [modal, setModal] = useState(null);

  const load = useCallback(async () => {
    try { setErr(''); setData(await api.list(res, page, PAGE_SIZE)); }
    catch (e) { setErr(e.message); setData({ items: [], totalPages: 0 }); }
  }, [res, page]);

  useEffect(() => { load(); }, [load]);

  const done = () => { setModal(null); load(); };

  async function remove(item) {
    if (!window.confirm(`Excluir ${cfg.single} “${item.name}”?`)) return;
    try {
      await api.remove(res, getId(item));
      if (data.items.length === 1 && page > 1) setPage(page - 1); else load();
    } catch (e) { setErr(e.message); }
  }

  async function details(item) {
    try { setModal({ type: 'detail', info: await api.get(res, getId(item)) }); }
    catch (e) { setErr(e.message); }
  }

  async function upload(e) {
    const file = e.target.files[0];
    e.target.value = '';
    if (!file) return;
    try { setErr(''); await api.upload(file); load(); } catch (e2) { setErr(e2.message); }
  }

  const items = data?.items || [];
  const totalPages = data?.totalPages || 0;

  return (
    <section>
      <header className="page-head">
        <h1>{cfg.label}</h1>
        {cfg.upload ? (
          <label className="btn">
            Enviar MP3
            <input type="file" accept="audio/mpeg,.mp3" hidden onChange={upload} />
          </label>
        ) : (
          <button className="btn" onClick={() => setModal({ type: 'create' })}>Novo {cfg.single}</button>
        )}
      </header>

      {err && <p className="error" role="alert">{err}</p>}
      {!data && <p className="muted">Carregando…</p>}
      {data && items.length === 0 && !err && (
        <p className="empty">
          {cfg.upload
            ? 'Nenhuma música ainda. Cadastre antes um artista, um gênero e um álbum (ID 1) e depois envie um MP3.'
            : `Nenhum ${cfg.single} cadastrado. Use o botão acima para criar o primeiro.`}
        </p>
      )}

      <ul className="list">
        {items.map((item, i) => (
          <li key={getId(item) ?? i} className="row">
            {res === 'musics' && (
              <button className="play" aria-label={`Tocar ${item.name}`} onClick={() => onPlay({ id: getId(item), name: item.name })}>▶</button>
            )}
            <div className="row-main">
              <strong>{item.name}</strong>
              {res === 'musics' && <span className="muted">{[item.duration, show(item.artists)].filter((x) => x && x !== '—').join(' • ')}</span>}
            </div>
            <div className="row-actions">
              <button className="link" onClick={() => details(item)}>Detalhes</button>
              {cfg.addMusic && <button className="link" onClick={() => setModal({ type: 'add', item })}>Adicionar música</button>}
              <button className="link" onClick={() => setModal({ type: 'edit', item })}>Editar</button>
              <button className="link danger" onClick={() => remove(item)}>Excluir</button>
            </div>
          </li>
        ))}
      </ul>

      {totalPages > 1 && (
        <nav className="pager" aria-label="Paginação">
          <button className="btn ghost" disabled={page <= 1} onClick={() => setPage(page - 1)}>Anterior</button>
          <span>Página {page} de {totalPages}</span>
          <button className="btn ghost" disabled={page >= totalPages} onClick={() => setPage(page + 1)}>Próxima</button>
        </nav>
      )}

      {modal?.type === 'create' && (
        <Form title={`Novo ${cfg.single}`} fields={cfg.create} submitLabel="Criar" onClose={() => setModal(null)}
          onSubmit={async (b) => { await api.create(res, b); done(); }} />
      )}
      {modal?.type === 'edit' && (
        <Form title={`Editar ${cfg.single}`} fields={cfg.edit} initial={{ name: modal.item.name }} submitLabel="Salvar" onClose={() => setModal(null)}
          onSubmit={async (b) => { await api.update(res, getId(modal.item), b); done(); }} />
      )}
      {modal?.type === 'add' && (
        <Form title={`Adicionar música a “${modal.item.name}”`} submitLabel="Adicionar" onClose={() => setModal(null)}
          fields={[{ key: 'musicId', label: 'Música', type: 'select', source: 'musics' }]}
          onSubmit={async (b) => { await api.addMusic(res, getId(modal.item), b.musicId); done(); }} />
      )}
      {modal?.type === 'detail' && (
        <div className="overlay" onClick={() => setModal(null)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>{modal.info?.name ?? 'Detalhes'}</h2>
            <dl>
        {Object.entries(modal.info || {})
          .filter(([k, v]) => 
            !(v && typeof v === 'object' && typeof v.name === 'string' && v.name.trim().toLowerCase() === 'unknown')
          )
          .filter(([k]) => !['id', 'audioUrl'].includes(k.toLowerCase()))
          .map(([k, v]) => (
                <div key={k}><dt>{k}</dt><dd>{show(v)}</dd></div>
              ))}
            </dl>
            <div className="actions"><button className="btn" onClick={() => setModal(null)}>Fechar</button></div>
          </div>
        </div>
      )}
    </section>
  );
}

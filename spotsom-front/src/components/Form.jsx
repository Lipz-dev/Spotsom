import { useEffect, useState } from 'react';
import { api, getId } from '../api';

export default function Form({ title, fields, initial = {}, submitLabel, onSubmit, onClose }) {
  const [values, setValues] = useState(initial);
  const [opts, setOpts] = useState({});
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState('');

  useEffect(() => {
    fields.filter((f) => f.source).forEach(async (f) => {
      try {
        const r = await api.list(f.source, 1, 100);
        setOpts((o) => ({ ...o, [f.key]: r.items || [] }));
      } catch (e) { setErr(e.message); }
    });
  }, []); // eslint-disable-line

  const set = (k, v) => setValues((s) => ({ ...s, [k]: v }));
  const toggle = (k, id) => {
    const cur = values[k] || [];
    set(k, cur.includes(id) ? cur.filter((x) => x !== id) : [...cur, id]);
  };

  async function submit(e) {
    e.preventDefault();
    setBusy(true); setErr('');
    try {
      const body = {};
      fields.forEach((f) => {
        const v = values[f.key];
        body[f.key] = f.type === 'select' ? Number(v) : f.type === 'multi' ? v || [] : v ?? '';
      });
      await onSubmit(body);
    } catch (e2) { setErr(e2.message); setBusy(false); }
  }

  return (
    <div className="overlay" onClick={onClose}>
      <form className="modal" onClick={(e) => e.stopPropagation()} onSubmit={submit}>
        <h2>{title}</h2>
        {fields.map((f) => (
          <div className="field" key={f.key}>
            <label htmlFor={f.key}>{f.label}</label>
            {f.type === 'text' && (
              <input id={f.key} required maxLength={f.max} value={values[f.key] ?? ''} onChange={(e) => set(f.key, e.target.value)} autoFocus />
            )}
            {f.type === 'select' && (
              <select id={f.key} required value={values[f.key] ?? ''} onChange={(e) => set(f.key, e.target.value)}>
                <option value="" disabled>Selecione…</option>
                {(opts[f.key] || []).map((o) => <option key={getId(o)} value={getId(o)}>{o.name}</option>)}
              </select>
            )}
            {f.type === 'multi' && (
              <div className="chips">
                {(opts[f.key] || []).length === 0 && <span className="muted">Nada cadastrado ainda.</span>}
                {(opts[f.key] || []).map((o) => {
                  const id = getId(o);
                  const on = (values[f.key] || []).includes(id);
                  return (
                    <button type="button" key={id} className={'chip' + (on ? ' on' : '')} aria-pressed={on} onClick={() => toggle(f.key, id)}>
                      {o.name}
                    </button>
                  );
                })}
              </div>
            )}
          </div>
        ))}
        {err && <p className="error" role="alert">{err}</p>}
        <div className="actions">
          <button type="button" className="btn ghost" onClick={onClose}>Cancelar</button>
          <button className="btn" disabled={busy}>{busy ? 'Salvando…' : submitLabel}</button>
        </div>
      </form>
    </div>
  );
}

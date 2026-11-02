import { useState } from 'react';
import { api } from './api';
import { RESOURCES } from './config';
import ResourcePage from './components/ResourcePage';

export default function App() {
  const [tab, setTab] = useState('musics');
  const [track, setTrack] = useState(null);

  return (
    <div className="app">
      <aside className="side">
        <div className="logo">Spotsom</div>
        <nav>
          {Object.entries(RESOURCES).map(([key, c]) => (
            <button key={key} className={'nav' + (tab === key ? ' on' : '')} aria-current={tab === key} onClick={() => setTab(key)}>
              {c.label}
            </button>
          ))}
        </nav>
      </aside>

      <main className="main">
        <ResourcePage key={tab} res={tab} onPlay={setTrack} />
      </main>

      <footer className="player">
        {track ? (
          <>
            <div className="now"><span className="muted-dark">Tocando</span><strong>{track.name}</strong></div>
            <audio key={track.id} controls autoPlay src={api.audioSrc(track.id)} />
          </>
        ) : (
          <span className="muted-dark">Escolha uma música com ▶ para ouvir.</span>
        )}
      </footer>
    </div>
  );
}

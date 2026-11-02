// Descrição de cada recurso da API: campos de criação/edição e ações extras.
export const RESOURCES = {
  musics: {
    label: 'Músicas',
    single: 'música',
    upload: true,
    edit: [
      { key: 'name', label: 'Nome', type: 'text', max: 100 },
      { key: 'albumId', label: 'Álbum', type: 'select', source: 'albums' },
      { key: 'genreIds', label: 'Gêneros', type: 'multi', source: 'genres' },
      { key: 'artistIds', label: 'Artistas', type: 'multi', source: 'artists' },
    ],
  },
  albums: {
    label: 'Álbuns',
    single: 'álbum',
    create: [
      { key: 'name', label: 'Nome', type: 'text', max: 25 },
      { key: 'artistId', label: 'Artista', type: 'select', source: 'artists' },
    ],
    edit: [{ key: 'name', label: 'Nome', type: 'text', max: 25 }],
    addMusic: true,
  },
  artists: {
    label: 'Artistas',
    single: 'artista',
    create: [{ key: 'name', label: 'Nome', type: 'text', max: 25 }],
    edit: [{ key: 'name', label: 'Nome', type: 'text', max: 25 }],
  },
  genres: {
    label: 'Gêneros',
    single: 'gênero',
    create: [{ key: 'name', label: 'Nome', type: 'text', max: 15 }],
    edit: [{ key: 'name', label: 'Nome', type: 'text', max: 15 }],
  },
  playlists: {
    label: 'Playlists',
    single: 'playlist',
    create: [
      { key: 'name', label: 'Nome', type: 'text', max: 25 },
      { key: 'userId', label: 'Usuário', type: 'select', source: 'users' },
    ],
    edit: [{ key: 'name', label: 'Nome', type: 'text', max: 25 }],
    addMusic: true,
  },
  users: {
    label: 'Usuários',
    single: 'usuário',
    create: [{ key: 'name', label: 'Nome', type: 'text', max: 25 }],
    edit: [{ key: 'name', label: 'Nome', type: 'text', max: 25 }],
  },
};

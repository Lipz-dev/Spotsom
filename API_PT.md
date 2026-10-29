# Documentação da API Spotsom

A Spotsom API gerencia usuários, artistas, gêneros, álbuns, músicas e playlists.

## Informações gerais

- **Base URL:** `http://127.0.0.1:5000`
- **Formato das requisições JSON:** `application/json`
- **Formato das respostas:** JSON, exceto o endpoint de reprodução de áudio
- **Paginação:** definida pelos parâmetros `page` e `pageSize`, ambos maiores que zero

## Executar o projeto

Na raiz da API, execute:

```powershell
cd Spotsom.Api
dotnet run
```

A API ficará disponível em `http://127.0.0.1:5000`. O banco SQLite usado é `Spotsom.db` e pode ser criado ou atualizado automaticamente na inicialização.

## Respostas HTTP comuns

| Código | Significado |
|---|---|
| `200 OK` | Requisição concluída com sucesso |
| `201 Created` | Recurso criado com sucesso |
| `204 No Content` | Recurso excluído com sucesso |
| `400 Bad Request` | Dados enviados inválidos ou operação não permitida |
| `404 Not Found` | Recurso ou registro não encontrado |

## Estrutura de paginação

Os endpoints com paginação retornam:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 10,
  "totalItems": 0,
  "totalPages": 0
}
```

---

## Usuários

### Listar usuários

```http
GET /api/users/page={page}/size={pageSize}
```

Exemplo: `GET /api/users/page=1/size=10`

### Buscar usuário por ID

```http
GET /api/users/{id}
```

### Criar usuário

```http
POST /api/users/create
Content-Type: application/json

{
  "name": "João"
}
```

O nome deve ser obrigatório e pode ter até 25 caracteres.

### Atualizar usuário

```http
PUT /api/users/{id}/edit
Content-Type: application/json

{
  "name": "João Silva"
}
```

### Excluir usuário

```http
DELETE /api/users/{id}
```

---

## Artistas

### Listar artistas

```http
GET /api/artists/page={page}/size={pageSize}
```

### Buscar artista por ID

```http
GET /api/artists/{id}
```

### Criar artista

```http
POST /api/artists/create
Content-Type: application/json

{
  "name": "Travis Scott"
}
```

O nome deve ser obrigatório e pode ter até 25 caracteres.

Resposta de exemplo:

```json
{
  "name": "Travis Scott",
  "musics": []
}
```

### Atualizar artista

```http
PUT /api/artists/{id}/edit
Content-Type: application/json

{
  "name": "Novo nome"
}
```

### Excluir artista

```http
DELETE /api/artists/{id}
```

---

## Gêneros

### Listar gêneros

```http
GET /api/genres/page={page}/size={pageSize}
```

### Buscar gênero por ID

```http
GET /api/genres/{id}
```

### Criar gênero

```http
POST /api/genres/create
Content-Type: application/json

{
  "name": "Rock"
}
```

O nome deve ser obrigatório e pode ter até 15 caracteres.

### Atualizar gênero

```http
PUT /api/genres/{id}/edit
Content-Type: application/json

{
  "name": "Rock alternativo"
}
```

### Excluir gênero

```http
DELETE /api/genres/{id}
```

---

## Álbuns

### Listar álbuns

```http
GET /api/albums/page={page}/size={pageSize}
```

### Buscar álbum por ID

```http
GET /api/albums/{id}
```

### Criar álbum

```http
POST /api/albums/create
Content-Type: application/json

{
  "name": "Novo álbum",
  "artistId": 1
}
```

- `name`: obrigatório, máximo de 25 caracteres.
- `artistId`: ID do artista existente.

### Atualizar álbum

```http
PUT /api/albums/{id}/edit
Content-Type: application/json

{
  "name": "Nome atualizado"
}
```

### Adicionar música ao álbum

```http
PUT /api/albums/{id}/addmusic
Content-Type: application/json

{
  "musicId": 1
}
```

A mesma música não pode ser adicionada duas vezes ao mesmo álbum.

### Excluir álbum

```http
DELETE /api/albums/{id}
```

---

## Músicas

### Listar músicas

```http
GET /api/musics/page={page}/size={pageSize}
```

### Buscar música por ID

```http
GET /api/musics/{id}
```

Resposta de exemplo:

```json
{
  "name": "music.mp3",
  "duration": "00:03:30",
  "releaseDate": "2026-10-29",
  "artists": ["Travis Scott"],
  "genres": ["Rock"],
  "album": "Novo álbum",
  "audioUrl": "/api/musics/player/1"
}
```

### Reproduzir ou baixar o áudio

```http
GET /api/musics/player/{id}
```

A resposta contém o arquivo de áudio com o tipo `audio/mpeg` e processamento de requisições de intervalo (`Range`) habilitado. Isso permite que players compatíveis solicitem partes específicas do arquivo, como durante o avanço ou retorno da reprodução. O endereço `audioUrl` retornado pela música pode ser usado diretamente para acessar esse endpoint.

### Enviar música

```http
POST /api/musics/create
Content-Type: multipart/form-data

Campo: Audio
Arquivo: arquivo MP3
```

Exemplo usando `curl`:

```powershell
curl.exe -X POST "http://127.0.0.1:5000/api/musics/create" `
  -F "Audio=@Spotsom.Api\Storage\music002.mp3"
```

No envio atual, os metadados são associados ao álbum, artista e gênero com ID `1`, eles são valores padrões já alocados no Database pela função SeedingSqlite() do DataExtensions.cs.

### Atualizar música

```http
PUT /api/musics/{id}/edit/
Content-Type: application/json

{
  "name": "Nome da música",
  "albumId": 1,
  "genreIds": [1, 2],
  "artistIds": [1, 2]
}
```

- `albumId`: ID de um álbum existente.
- `genreIds`: lista de IDs de gêneros existentes.
- `artistIds`: lista de IDs de artistas existentes.

### Excluir música

```http
DELETE /api/musics/{id}
```

---

## Playlists

### Listar playlists

```http
GET /api/playlists/page={page}/size={pageSize}
```

### Buscar playlist por ID

```http
GET /api/playlists/{id}
```

### Criar playlist

```http
POST /api/playlists/create
Content-Type: application/json

{
  "name": "Minha playlist",
  "userId": 1
}
```

- `name`: obrigatório, máximo de 25 caracteres.
- `userId`: ID de um usuário existente.

### Atualizar playlist

```http
PUT /api/playlists/{id}/edit/
Content-Type: application/json

{
  "name": "Novo nome"
}
```

### Adicionar música à playlist

```http
PUT /api/playlists/{id}/add
Content-Type: application/json

{
  "musicId": 1
}
```

A mesma música não pode ser adicionada duas vezes à mesma playlist.

### Excluir playlist

```http
DELETE /api/playlists/{id}
```

---

## Exemplos rápidos

### Criar dados iniciais para testar

```powershell
Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:5000/api/users/create" -ContentType "application/json" -Body '{"name":"Usuário Teste"}'
Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:5000/api/artists/create" -ContentType "application/json" -Body '{"name":"Artista Teste"}'
Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:5000/api/genres/create" -ContentType "application/json" -Body '{"name":"Rock"}'
Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:5000/api/albums/create" -ContentType "application/json" -Body '{"name":"Álbum Teste","artistId":1}'
```

Depois, envie uma música:

```powershell
curl.exe -X POST "http://127.0.0.1:5000/api/musics/create" -F "Audio=@Spotsom.Api\Storage\music002.mp3"
```

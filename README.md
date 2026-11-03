# Spotsom

Spotsom é uma aplicação web para gerenciar um catálogo musical. O projeto reúne uma API REST em ASP.NET Core com um frontend em React, permitindo organizar músicas, artistas, gêneros, álbumes, playlists e usuários.

## Funcionalidades

- Cadastro, consulta, edição e exclusão de músicas, artistas, gêneros, álbumes, playlists e usuários.
- Upload de arquivos de áudio para a biblioteca.
- Reprodução das músicas diretamente pelo player integrado.
- Associação entre músicas, artistas, gêneros, álbumes e playlists.
- Listagem paginada dos recursos cadastrados.
- Adição de músicas a álbumes e playlists.
- Persistência dos dados em SQLite.
- Interface responsiva e navegação por categorias.
- Execução conjunta da API e do frontend com Docker Compose.

## Tecnologias utilizadas

### Backend

- ASP.NET Core (.NET 10)
- Minimal APIs
- Entity Framework Core
- SQLite
- TagLibSharp
- Docker

### Frontend

- React 18
- Vite
- JavaScript
- CSS
- Fetch API

## Arquitetura

O projeto segue uma arquitetura cliente-servidor:

```text
Navegador
   |
   v
Frontend React/Vite
   |
   | HTTP/JSON
   v
API ASP.NET Core
   |
   +--> SQLite (dados)
   |
   +--> Sistema de arquivos (arquivos de áudio)
```

- O frontend envia requisições REST para os endpoints de `/api`.
- Os endpoints recebem e validam as operações, delegando regras de negócio aos serviços.
- O `SpotsomContext` usa Entity Framework Core para acessar o banco SQLite.
- Os arquivos enviados são armazenados no diretório `wwwroot/audio` e disponibilizados pela API para reprodução.
- No ambiente Docker, os dados do banco e os arquivos de áudio são mantidos em volumes separados.

## Instalação

### Pré-requisitos

- .NET SDK 10
- Node.js 18 ou superior
- npm
- Docker e Docker Compose (opcional, para a execução em containers)

### Executar com Docker Compose

Na raiz do projeto:

```bash
docker compose up --build
```

A aplicação ficará disponível em:

- Frontend: http://localhost:5173
- API: http://localhost:5000

Para encerrar os containers:

```bash
docker compose down
```

### Executar localmente

1. Inicie a API:

```bash
cd Spotsom.Api
dotnet restore
dotnet run
```

A API será iniciada em http://127.0.0.1:5000. Na primeira execução, as migrations são aplicadas automaticamente.

2. Em outro terminal, instale e inicie o frontend:

```bash
cd spotsom-front
npm install
npm run dev
```

O frontend será iniciado em http://localhost:5173.

### Configuração opcional do frontend

Por padrão, o frontend usa `http://127.0.0.1:5000`. Para apontar para outra API, crie um arquivo `spotsom-front/.env` com:

```env
VITE_API_URL=http://127.0.0.1:5000
```

Depois, reinicie o servidor de desenvolvimento do frontend.

### Build de produção do frontend

```bash
cd spotsom-front
npm run build
```

Os arquivos de distribuição serão gerados na pasta `spotsom-front/dist`.

## Exemplos visuais da aplicação

> As imagens abaixo representam os principais fluxos da interface. Ao publicar capturas reais do projeto em `docs/images`, os recursos Markdown podem substituir este esquema visual.

### Catálogo de músicas

```text
+--------------------------------------------------------------+
| SPOTSOM        |  Músicas selecionadas |  Músicas           |
|                +------------------------+                    |
| Musicas        |  Nome                  |  [play] [editar]   |
| Albuns         |  Nome                  |  [play] [editar]   |
| Artistas       |  Nome                  |  [play] [editar]   |
| Generos        |  Nome                  |  [play] [editar]   |
| Playlists      +------------------------+                    |
| Usuarios                                                     |
|                     [Adicionar música]  [Buscar...]          |
+--------------------------------------------------------------+
```

### Player de áudio

```text
+--------------------------------------------------------------+
| Tocando: Nome da música                                      |
| [play] -----------===========----------------  02:14 / 03:40 |
+--------------------------------------------------------------+
```

### Gestão de playlists

```text
+---------------------------+----------------------------------+
| PLAYLISTS                 | Nome da playlist                 |
| - Favoritas               | + Adicionar música               |
| - Para trabalhar          |                                  |
| - Viagens                 | [música 1] [remover]             |
| + Nova playlist           | [música 2] [remover]             |
+---------------------------+----------------------------------+
```

## API

A documentação detalhada dos endpoints está disponível em:

- [API em português](API_PT.md)
- [API em inglês](API_EN.md)

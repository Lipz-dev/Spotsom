# Spotsom API Documentation

The Spotsom API manages users, artists, genres, albums, songs, and playlists.

## General Information

- **Base URL:** `http://127.0.0.1:5000`
- **JSON request format:** `application/json`
- **Response format:** JSON, except for the audio playback endpoint
- **Pagination:** defined by the `page` and `pageSize` parameters, both of which must be greater than zero

## Running the Project

From the API directory, run:

```powershell
cd Spotsom.Api
dotnet run
```

The API will be available at `http://127.0.0.1:5000`. The SQLite database used is `Spotsom.db`, which may be created or updated automatically when the application starts.

## Common HTTP Responses

| Status | Meaning |
|---|---|
| `200 OK` | Request completed successfully |
| `201 Created` | Resource created successfully |
| `204 No Content` | Resource deleted successfully |
| `400 Bad Request` | Invalid data was sent or the operation is not allowed |
| `404 Not Found` | Resource or record not found |

## Pagination Structure

Endpoints that support pagination return:

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

## Users

### List users

```http
GET /api/users/page={page}/size={pageSize}
```

Example: `GET /api/users/page=1/size=10`

### Get a user by ID

```http
GET /api/users/{id}
```

### Create a user

```http
POST /api/users/create
Content-Type: application/json

{
  "name": "John"
}
```

The name is required and may contain up to 25 characters.

### Update a user

```http
PUT /api/users/{id}/edit
Content-Type: application/json

{
  "name": "John Smith"
}
```

### Delete a user

```http
DELETE /api/users/{id}
```

---

## Artists

### List artists

```http
GET /api/artists/page={page}/size={pageSize}
```

### Get an artist by ID

```http
GET /api/artists/{id}
```

### Create an artist

```http
POST /api/artists/create
Content-Type: application/json

{
  "name": "Travis Scott"
}
```

The name is required and may contain up to 25 characters.

Example response:

```json
{
  "name": "Travis Scott",
  "musics": []
}
```

### Update an artist

```http
PUT /api/artists/{id}/edit
Content-Type: application/json

{
  "name": "New name"
}
```

### Delete an artist

```http
DELETE /api/artists/{id}
```

---

## Genres

### List genres

```http
GET /api/genres/page={page}/size={pageSize}
```

### Get a genre by ID

```http
GET /api/genres/{id}
```

### Create a genre

```http
POST /api/genres/create
Content-Type: application/json

{
  "name": "Rock"
}
```

The name is required and may contain up to 15 characters.

### Update a genre

```http
PUT /api/genres/{id}/edit
Content-Type: application/json

{
  "name": "Alternative Rock"
}
```

### Delete a genre

```http
DELETE /api/genres/{id}
```

---

## Albums

### List albums

```http
GET /api/albums/page={page}/size={pageSize}
```

### Get an album by ID

```http
GET /api/albums/{id}
```

### Create an album

```http
POST /api/albums/create
Content-Type: application/json

{
  "name": "New album",
  "artistId": 1
}
```

- `name`: required; maximum of 25 characters.
- `artistId`: ID of an existing artist.

### Update an album

```http
PUT /api/albums/{id}/edit
Content-Type: application/json

{
  "name": "Updated name"
}
```

### Add a song to an album

```http
PUT /api/albums/{id}/addmusic
Content-Type: application/json

{
  "musicId": 1
}
```

The same song cannot be added twice to an album.

### Delete an album

```http
DELETE /api/albums/{id}
```

---

## Songs

### List songs

```http
GET /api/musics/page={page}/size={pageSize}
```

### Get a song by ID

```http
GET /api/musics/{id}
```

Example response:

```json
{
  "name": "music.mp3",
  "duration": "00:03:30",
  "releaseDate": "2026-10-29",
  "artists": ["Travis Scott"],
  "genres": ["Rock"],
  "album": "New album",
  "audioUrl": "/api/musics/player/1"
}
```

### Play or download audio

```http
GET /api/musics/player/{id}
```

The response contains the audio file with the `audio/mpeg` content type and range request processing enabled. This allows compatible players to request specific parts of the file, for example while seeking forward or backward during playback. The `audioUrl` returned by the song can be used directly to access this endpoint.

### Upload a song

```http
POST /api/musics/create
Content-Type: multipart/form-data

Field: Audio
File: MP3 file
```

Example using `curl`:

```powershell
curl.exe -X POST "http://127.0.0.1:5000/api/musics/create" `
  -F "Audio=@Spotsom.Api\Storage\music002.mp3"
```

In the current implementation, metadata is associated with the album, artist, and genre with ID `1`. Therefore, these records must exist before uploading a song.

### Update a song

```http
PUT /api/musics/{id}/edit/
Content-Type: application/json

{
  "name": "Song name",
  "albumId": 1,
  "genreIds": [1, 2],
  "artistIds": [1, 2]
}
```

- `albumId`: ID of an existing album.
- `genreIds`: list of IDs for existing genres.
- `artistIds`: list of IDs for existing artists.

### Delete a song

```http
DELETE /api/musics/{id}
```

---

## Playlists

### List playlists

```http
GET /api/playlists/page={page}/size={pageSize}
```

### Get a playlist by ID

```http
GET /api/playlists/{id}
```

### Create a playlist

```http
POST /api/playlists/create
Content-Type: application/json

{
  "name": "My playlist",
  "userId": 1
}
```

- `name`: required; maximum of 25 characters.
- `userId`: ID of an existing user.

### Update a playlist

```http
PUT /api/playlists/{id}/edit/
Content-Type: application/json

{
  "name": "New name"
}
```

### Add a song to a playlist

```http
PUT /api/playlists/{id}/add
Content-Type: application/json

{
  "musicId": 1
}
```

The same song cannot be added twice to a playlist.

### Delete a playlist

```http
DELETE /api/playlists/{id}
```

---

## Quick Examples

### Create initial data for testing

```powershell
Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:5000/api/users/create" -ContentType "application/json" -Body '{"name":"Test User"}'
Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:5000/api/artists/create" -ContentType "application/json" -Body '{"name":"Test Artist"}'
Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:5000/api/genres/create" -ContentType "application/json" -Body '{"name":"Rock"}'
Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:5000/api/albums/create" -ContentType "application/json" -Body '{"name":"Test Album","artistId":1}'
```

Then upload a song:

```powershell
curl.exe -X POST "http://127.0.0.1:5000/api/musics/create" -F "Audio=@Spotsom.Api\Storage\music002.mp3"
```

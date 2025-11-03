### Ejecución

```bash
> docker compose down -v && docker compose up --build --scale songs-service=3
```

# TFU 4

### Patrones a implementar:

#### Disponibilidad

- [x] Health endpoint monitoring - Cambiar /ping por health checks nativos de asp.net

Para ver los health checks fallando, puede ejecutar

```bash
docker compose stop db
```

- [x] Rate limiting - Rate limiting por servicio como en la tfu 2

Se limita cada dirección IP a una tasa base de 1 petición por segundo,
permitiendo ráfagas (bursts) de hasta 10 peticiones. Cualquier petición por
encima de la ráfaga es rechazada.

[Ver Configuración](./proxy/nginx.conf)

#### Rendimiento

- [x] Cache-aside - Ya implementado para la tfu 3 en [SongCacheService](./services/songs/src/services/SongCacheService.cs).
- [x] CQRS - Ya implementado para la TFU 3. El SongCacheService separa las
      lecturas (de un caché en memoria) de las escrituras (a diffs que se sincronizan eventualmente).

#### Seguridad

- [x] Gateway offloading - El proxy se encarga de manejar HTTPs, los servicios internos manejan HTTP.

Se ha configurado el proxy Nginx para que actúe como punto de terminación SSL/TLS.
Maneja todo el cifrado HTTPS (en https://localhost:8443) y se comunica con los
servicios internos (auth, songs, playlists) usando HTTP simple.

También redirige automáticamente todo el tráfico HTTP (en http://localhost:8080)
a HTTPS.

[Ver Configuración](./proxy/nginx.conf)

- [x] Federated identity - Ya implementado para tfu 3. Auth service crea jwt, los otros servicios confían en él.

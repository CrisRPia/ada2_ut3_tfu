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

- [ ] Rate limiting - Rate limiting por servicio como en la tfu 2

#### Rendimiento

- [x] Cache-aside - Ya implementado para la tfu 3 en [SongCacheService](./services/songs/src/services/SongCacheService.cs).
- [x] CQRS - Ya implementado para la tfu 3 con un cache y escritura en diffs en [SongCacheService](./services/songs/src/services/SongCacheService.cs).

#### Seguridad

- [ ] Gateway offloading - El proxy se encarga de manejar HTTPs, los servicios internos manejan HTTP.
- [x] Federated identity - Ya implementado para tfu 3. Auth service crea jwt, los otros servicios confían en él.

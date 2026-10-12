# Decisiones

- **Sin base de datos ni cuentas:** el conocimiento es JSON incrustado; nada del usuario se guarda ni se registra (tampoco el cuerpo de las peticiones).
- **Motor por reglas, no IA:** cada veredicto es explicable y trazable a una fuente.
- **Prudencia:** los resultados solo pueden empeorar con reglas de contexto; desconocido ≠ seguro; >50 % desconocido → «No lo sé».
- **13+:** vocabulario directo sin infantilizar. Edad fuera de 13–18 → mensaje que remite a un pediatra/dermatólogo.
- **Seguridad web:** CSP estricta (sin scripts ni estilos en línea), cabeceras de seguridad, límite 60 peticiones/min por IP, cuerpo máximo 64 KB, DOM construido con `textContent`.
- **Gratis:** Azure F1, GitHub Actions, tipografías autoalojadas, sin CDN ni analítica.
- **Pruebas:** xUnit. En el entorno de desarrollo sin NuGet se verificaron con un arnés propio; CI ejecuta `dotnet test` real.
- **Comparación con Java/Spring:** Program.cs ≈ `@RestController`s; los records ≈ DTOs/`record`; `AddSingleton` ≈ `@Bean`/`@Component` singleton.

# Despliegue gratuito en Azure

Coste: **0 €** con App Service plan **F1 (Free)**. Límites: 60 min de CPU al día, se duerme tras inactividad (la primera visita tarda unos segundos), dominio `*.azurewebsites.net` (sin dominio propio ni TLS personalizado).

1. Portal de Azure → *Crear recurso* → **Web App**. Publicar: Código · Pila: **.NET 10** · SO: Linux · Plan: **Free F1**.
2. En la Web App → *Obtener perfil de publicación* y copia su contenido.
3. En GitHub → Settings → Secrets → Actions: crea `AZURE_WEBAPP_PUBLISH_PROFILE` (el XML) y la variable `AZURE_WEBAPP_NAME`.
4. Actions → **Desplegar en Azure** → *Run workflow* (es manual para que nada se publique sin querer).
5. Comprueba `https://<nombre>.azurewebsites.net/api/salud`.

Si .NET 10 no aparece aún como pila en tu región, publica *self-contained* (cambia `dotnet publish` en el workflow a `--self-contained -r linux-x64`).

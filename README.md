# Letra pequeña (AppSkinCare)

Web **informativa y gratuita** para personas de **13 a 18 años**: pegas la lista de ingredientes (INCI) de un producto de cuidado de la piel y te explica, ingrediente por ingrediente, si puede darte problemas según tu edad y tu piel. También tiene consejos para problemas comunes, una rutina corta y un juego de mitos.

- Sin cuentas, sin cookies, sin base de datos: lo que se escribe se evalúa y se olvida.
- Lo que no conoce lo dice con claridad (nunca lo da por seguro).
- Cada ficha, consejo y mito enlaza a su fuente. Ver [docs/FUENTES-Y-LIMITES.md](docs/FUENTES-Y-LIMITES.md).
- Stack: **ASP.NET Core (.NET 10)** minimal API + HTML/CSS/JS estático. Cero paquetes NuGet de terceros en producción.

## Ejecutar

```bash
dotnet run --project src/AppSkinCare.Web      # http://localhost:5000 (ver consola)
dotnet test                                   # 69 pruebas
```

Las páginas HTML se generan con `python3 tools/generar-paginas.py` (solo si cambias cabecera, pie o textos de `problemas.json`). El resultado ya está en el repositorio.

## Estructura

| Carpeta | Qué hay |
|---|---|
| `src/AppSkinCare.Core` | Motor sin dependencias: parser INCI, catálogo, evaluador, generador de rutina. Los ingredientes están en `Data/ingredientes/*.json` (incrustados). |
| `src/AppSkinCare.Web` | API (`/api/evaluar`, `/api/rutina`, `/api/ingredientes`, `/api/categorias`, `/api/salud`) y `wwwroot`. |
| `tests/AppSkinCare.Tests` | xUnit. |
| `docs/` | Decisiones, fuentes y límites, despliegue gratuito en Azure, cómo añadir ingredientes. |

## Añadir un ingrediente

Ver [docs/ANADIR-INGREDIENTES.md](docs/ANADIR-INGREDIENTES.md). Toda fuente nueva debe existir en `wwwroot/data/fuentes.json` (hay una prueba que lo comprueba).

## Licencias

Código: elige la licencia que prefieras antes de publicarlo. Tipografías Unbounded y Figtree: SIL Open Font License (en `wwwroot/fonts`).

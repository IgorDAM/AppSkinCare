# Añadir un ingrediente

1. Elige el fichero de `src/AppSkinCare.Core/Data/ingredientes/` (`activos`, `base`, `solares`, `conservantes-fragancias`, `formulacion`).
2. Añade un objeto con: `id` (único, minúsculas), `nombre`, `inci` (nombres como salen en etiqueta, sin repetir en otros), `categoria`, `resumen`, `porQue`, `nivel13a14`, `nivel15a18` (`neutro|ok|precaucion|evitar`) y `fuentes` (ids de `fuentes.json`).
3. Opcionales: `patrones` (regex), `irritante` + `grupo`, `medicamento`, `limites` (por porcentaje), `reglas` (según contexto: atopia, sensible, acne, manchas, isotretinoina…), `notaConcentracion`.
4. Las reglas solo pueden **empeorar** el nivel, nunca mejorarlo.
5. Si no hay fuente fiable, **no lo añadas**: es preferible que la web diga «no lo conozco».
6. Ejecuta `dotnet test`: comprueba nombres duplicados y que las fuentes existan.

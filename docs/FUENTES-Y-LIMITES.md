# Fuentes y límites

## Fuentes
La lista completa, con enlaces, está en `src/AppSkinCare.Web/wwwroot/data/fuentes.json` y se muestra en la página «Sobre la web». Cada ingrediente, problema y mito cita ids de esa lista.

La fuente `informe` es un resumen propio de investigación del proyecto (sin enlace). No es una fuente médica: donde es posible, se respalda con una guía clínica, una norma de la UE o una revisión.

## Qué se dejó fuera por no estar confirmado (y se dice en la web)
1. **Marcas y productos con nombre.** No se valoran; solo la lista de ingredientes que pega la persona.
2. **Normativa de China, Corea y Brasil** que aparecía en la investigación inicial: sin fuente oficial verificada.
3. **Recomendación francesa sobre fenoxietanol en zona del pañal:** no verificada y fuera del rango de edad.
4. **Etnia / color de piel como variable de decisión:** no hay reglas confirmadas que justifiquen cambiar el veredicto. Solo se aconseja protección solar para todas las pieles.

## Límites del motor
- Se conocen 74 grupos de ingredientes (258 nombres INCI). Lo demás se declara «no conocido».
- Sin porcentaje en la etiqueta no se sabe la concentración: se avisa.
- No conoce alergias individuales ni tratamientos médicos (salvo la casilla de isotretinoína).
- Niveles de 13–14 años: columna «10–14» del informe (la más prudente); 15–18: columna «14–18».
- Texto educativo; no es diagnóstico.

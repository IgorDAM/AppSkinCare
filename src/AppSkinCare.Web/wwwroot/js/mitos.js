import { h, fuentesPorId, listaFuentes } from "/js/common.js";

const caja = document.getElementById("quiz");
const [mitos, fuentes] = await Promise.all([
  fetch("/data/mitos.json").then(r => r.json()),
  fuentesPorId()
]);

// Barajamos para que no salga siempre en el mismo orden.
const orden = [...mitos].sort(() => Math.random() - 0.5);
let i = 0;
let aciertos = 0;

function pregunta() {
  const m = orden[i];
  const feedback = h("div", { "aria-live": "polite" });
  const respuestas = h("div", { class: "quiz-respuestas" });

  const responder = dice => {
    const acierto = dice === m.verdad;
    if (acierto) aciertos++;
    respuestas.querySelectorAll("button").forEach(b => { b.disabled = true; });
    const f = h("div", { class: "quiz-feedback " + (acierto ? "n-ok" : "n-evitar") },
      h("p", null, h("strong", null, acierto ? "¡Bien! " : "Casi. "),
        m.verdad ? "Es verdad. " : "Es un mito. ", m.explicacion),
      listaFuentes(m.fuentes, fuentes),
      h("button", { class: "boton", type: "button", id: "siguiente" }, i + 1 < orden.length ? "Siguiente" : "Ver resultado"));
    feedback.replaceChildren(f);
    const sig = f.querySelector("#siguiente");
    sig.addEventListener("click", () => { i++; i < orden.length ? pregunta() : final(); });
    sig.focus();
  };

  respuestas.append(
    h("button", { class: "boton boton-claro", type: "button" }, "Verdad"),
    h("button", { class: "boton boton-claro", type: "button" }, "Mito"));
  respuestas.children[0].addEventListener("click", () => responder(true));
  respuestas.children[1].addEventListener("click", () => responder(false));

  caja.replaceChildren(
    h("p", { class: "quiz-progreso" }, `Pregunta ${i + 1} de ${orden.length}`),
    h("p", { class: "quiz-afirmacion", id: "afirmacion", tabindex: "-1" }, m.afirmacion),
    respuestas, feedback);
  document.getElementById("afirmacion").focus({ preventScroll: true });
}

function final() {
  const reinicio = h("button", { class: "boton", type: "button" }, "Jugar otra vez");
  reinicio.addEventListener("click", () => { i = 0; aciertos = 0; orden.sort(() => Math.random() - 0.5); pregunta(); });
  caja.replaceChildren(
    h("p", { class: "quiz-afirmacion", tabindex: "-1", id: "fin" }, `Has acertado ${aciertos} de ${orden.length}.`),
    h("p", null, "Si algo te ha sorprendido, repásalo en la lista de abajo."),
    reinicio,
    h("div", { class: "grupo" }, 
      orden.map(m => h("div", { class: "paso" }, h("h4", null, (m.verdad ? "Verdad: " : "Mito: ") + m.afirmacion), h("p", null, m.explicacion)))));
  document.getElementById("fin").focus({ preventScroll: true });
}

pregunta();

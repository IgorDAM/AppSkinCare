import { h, api } from "/js/common.js";

const formulario = document.getElementById("formulario");
const salida = document.getElementById("resultado");
const boton = document.getElementById("generar");
const error = document.getElementById("error");

formulario.addEventListener("submit", async ev => {
  ev.preventDefault();
  error.hidden = true;
  const f = formulario.elements;
  const perfil = {
    edad: Number(f.edad.value),
    tipoPiel: f.tipoPiel.value,
    acne: f.acne.value,
    sensible: f.sensible.checked,
    atopia: f.atopia.checked,
    manchas: f.manchas.checked,
    isotretinoina: f.isotretinoina.checked,
    pielClara: f.pielClara.checked,
    muchoSol: f.muchoSol.checked
  };
  boton.disabled = true;
  boton.textContent = "Preparando…";
  try {
    pintar(await api("/api/rutina", perfil));
  } catch (e) {
    error.replaceChildren(h("p", null, e.message));
    error.hidden = false;
  } finally {
    boton.disabled = false;
    boton.textContent = "Ver mi rutina";
  }
});

function paso(p) {
  return h("div", { class: "paso" },
    h("h4", null, p.titulo, p.opcional ? h("span", { class: "opcional" }, "opcional") : null),
    h("p", null, p.que),
    p.buscar?.length ? [h("p", { class: "etiqueta-lista" }, "Busca"), h("ul", null, p.buscar.map(x => h("li", null, x)))] : null,
    p.evitar?.length ? [h("p", { class: "etiqueta-lista" }, "Evita"), h("ul", null, p.evitar.map(x => h("li", null, x)))] : null);
}

function bloque(clase, titulo, items) {
  if (!items?.length) return null;
  return h("div", { class: clase }, h("h4", null, titulo), h("ul", { class: "lista-simple" }, items.map(x => h("li", null, x))));
}

function pintar(r) {
  salida.replaceChildren(h("div", { class: "contenedor" },
    h("h2", { id: "resultado-titulo" }, r.titulo),
    h("p", null, r.resumen),
    h("div", { class: "dos-columnas" },
      h("section", null, h("h3", null, "Por la mañana"), r.manana.map(paso)),
      h("section", null, h("h3", null, "Por la noche"), r.noche.map(paso))),
    h("div", { class: "dos-columnas" },
      h("div", { class: "panel" },
        bloque("bloque-evitar", "Mejor evita", r.evita),
        bloque("lista", "Consejos", r.consejos)),
      h("div", { class: "panel" }, bloque("bloque-medico", "Ve al médico/a o al dermatólogo/a si…", r.medicoSi))),
    h("p", { class: "aviso-legal" }, r.avisoLegal),
    h("p", null, "¿Ya tienes productos? ", h("a", { href: "/" }, "Revisa su etiqueta"), " para ver si encajan con esta rutina.")));
  salida.hidden = false;
  salida.setAttribute("aria-labelledby", "resultado-titulo");
  salida.scrollIntoView({ behavior: matchMedia("(prefers-reduced-motion: reduce)").matches ? "auto" : "smooth" });
  salida.focus({ preventScroll: true });
}

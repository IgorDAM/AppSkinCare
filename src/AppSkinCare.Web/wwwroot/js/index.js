import { h, api, fuentesPorId, pastillaNivel, listaFuentes } from "/js/common.js";

const formulario = document.getElementById("formulario");
const salida = document.getElementById("resultado");
const boton = document.getElementById("revisar");
const error = document.getElementById("error");
const rutinaCaja = document.getElementById("rutina-caja");
const anadir = document.getElementById("anadir-producto");

const EJEMPLOS = {
  limpiador: {
    inci: "Aqua, Sodium Cocoyl Isethionate, Cocamidopropyl Betaine, Glycerin, Sodium Chloride, Panthenol, Citric Acid, Phenoxyethanol",
    aclara: true
  },
  acne: {
    inci: "Aqua, Benzoyl Peroxide 5%, Carbomer, Glycerin, Propylene Glycol, Sodium Hydroxide, Parfum, Limonene",
    aclara: false
  },
  hidratante: {
    inci: "Aqua, Glycerin, Cetearyl Alcohol, Ceramide NP, Ceramide AP, Ceramide EOP, Panthenol, Squalane, Tocopherol, Xanthan Gum",
    aclara: false
  }
};

const VEREDICTOS = {
  buena: { clase: "v-buena", signo: "✓", palabra: "Sin alertas" },
  parcial: { clase: "v-parcial", signo: "?", palabra: "Sin alertas, con dudas" },
  precaucion: { clase: "v-precaucion", signo: "!", palabra: "Con cuidado" },
  evitar: { clase: "v-evitar", signo: "✕", palabra: "Mejor evitar" },
  sinDatos: { clase: "v-sindatos", signo: "?", palabra: "No lo sé" }
};

document.querySelectorAll("[data-ejemplo]").forEach(b => b.addEventListener("click", () => {
  const e = EJEMPLOS[b.dataset.ejemplo];
  if (!e) return;
  document.getElementById("inci").value = e.inci;
  formulario.elements.seAclara.checked = e.aclara;
  document.getElementById("inci").focus();
}));

anadir?.addEventListener("click", () => {
  const n = rutinaCaja.querySelectorAll("textarea").length;
  if (n >= 5) return;
  const id = "rutina-" + (n + 1);
  rutinaCaja.insertBefore(
    h("div", { class: "campo" },
      h("label", { for: id }, "Producto " + (n + 1) + " de tu rutina"),
      h("textarea", { id, name: "rutina", placeholder: "Pega aquí sus ingredientes", rows: "3" })),
    anadir.parentElement);
  if (n + 1 >= 5) anadir.hidden = true;
  document.getElementById(id).focus();
});

formulario.addEventListener("submit", async ev => {
  ev.preventDefault();
  error.hidden = true;
  const f = formulario.elements;
  const pedido = {
    producto: { inci: f.inci.value },
    rutina: [...rutinaCaja.querySelectorAll("textarea")]
      .map(t => t.value.trim()).filter(Boolean).map(inci => ({ inci })),
    contexto: {
      edad: Number(f.edad.value),
      tipoPiel: f.tipoPiel.value,
      sensible: f.sensible.checked,
      atopia: f.atopia.checked,
      acne: f.acne.checked,
      manchas: f.manchas.checked,
      isotretinoina: f.isotretinoina.checked,
      seAclara: f.seAclara.checked
    }
  };

  boton.disabled = true;
  boton.textContent = "Revisando…";
  try {
    const [resultado, fuentes] = await Promise.all([api("/api/evaluar", pedido), fuentesPorId()]);
    pintar(resultado, fuentes);
  } catch (e) {
    error.replaceChildren(h("p", null, e.message));
    error.hidden = false;
    error.focus();
  } finally {
    boton.disabled = false;
    boton.textContent = "Revisar etiqueta";
  }
});

function pintar(r, fuentes) {
  const v = VEREDICTOS[r.veredicto] ?? VEREDICTOS.sinDatos;
  const nodos = [];

  nodos.push(h("div", { class: "resultado-cabecera" },
    h("div", { class: "sello nuevo " + v.clase, "aria-hidden": "true" },
      h("span", { class: "sello-icono" }, v.signo),
      h("span", { class: "sello-palabra" }, v.palabra)),
    h("div", { class: "resultado-texto" },
      h("h2", { id: "resultado-titulo" }, r.titulo),
      h("p", null, r.resumen),
      h("p", { class: "peque" }, `Reconozco ${r.reconocidos} de ${r.total} ingredientes.`))));

  if (r.alertas.length) {
    nodos.push(h("ul", { class: "alertas" }, r.alertas.map(a =>
      h("li", { class: "alerta " + (a.nivel === "evitar" ? "n-evitar" : "n-precaucion") },
        h("p", null, pastillaNivel(a.nivel), " ", a.texto)))));
  }

  if (r.desconocidos.length) {
    nodos.push(h("section", { class: "desconocidos", "aria-labelledby": "desc-titulo" },
      h("h3", { id: "desc-titulo" }, `No conozco ${r.desconocidos.length} ${r.desconocidos.length === 1 ? "ingrediente" : "ingredientes"}`),
      h("p", null, r.mensajeDesconocidos),
      h("ul", null, r.desconocidos.map(d =>
        h("li", null, d.texto, d.sugerencia ? h("span", { class: "peque" }, ` — ¿quizá quieres decir «${d.sugerencia}»? Es solo una pista; no lo he dado por bueno.`) : null)))));
  }

  const orden = [
    ["evitar", "Para evitar"],
    ["precaucion", "Con cuidado"],
    ["ok", "Lo que suma"],
    ["neutro", "Sin alerta"]
  ];
  for (const [nivel, titulo] of orden) {
    const items = unirRepetidos(r.ingredientes.filter(i => i.nivel === nivel));
    if (!items.length) continue;
    const filas = items.map(i => fila(i, fuentes));
    const grupo = h("section", { class: "grupo" },
      h("h3", null, titulo, h("span", { class: "cuenta" }, String(items.length))));
    if (nivel === "neutro") {
      grupo.append(h("details", null, h("summary", null, "Ver los " + items.length + " ingredientes sin alerta"), h("div", null, filas)));
    } else {
      grupo.append(...filas);
    }
    nodos.push(grupo);
  }

  if (r.avisos.length) {
    nodos.push(h("section", { class: "grupo" },
      h("h3", null, "Antes de usarlo"),
      h("ul", { class: "avisos" }, r.avisos.map(a => h("li", null, a)))));
  }
  nodos.push(h("p", { class: "aviso-legal" }, r.avisoLegal));

  salida.replaceChildren(h("div", { class: "contenedor" }, ...nodos));
  salida.hidden = false;
  salida.setAttribute("aria-labelledby", "resultado-titulo");
  salida.scrollIntoView({ behavior: matchMedia("(prefers-reduced-motion: reduce)").matches ? "auto" : "smooth" });
  salida.focus({ preventScroll: true });
}

function fila(i, fuentes) {
  const n = i.nivel;
  const detalle = i.detalle || listaFuentes(i.fuentes, fuentes)
    ? h("details", null, h("summary", null, "Más información"),
        i.detalle ? h("p", null, i.detalle) : null,
        listaFuentes(i.fuentes, fuentes))
    : null;
  return h("article", { class: "fila n-" + n },
    h("div", { class: "fila-cabecera" },
      h("h4", null, i.nombre),
      pastillaNivel(n),
      i.porcentaje != null ? h("span", { class: "peque" }, i.porcentaje + " %") : null),
    i.textos?.length > 1 ? h("p", { class: "peque" }, "En la etiqueta: " + i.textos.join(", ")) : null,
    h("p", null, i.porQue),
    i.ayudaCon?.length ? h("p", { class: "ayuda" }, "Puede ayudar con: " + i.ayudaCon.join(", ")) : null,
    detalle);
}

// Varios nombres de etiqueta pueden ser el mismo ingrediente (p. ej. Ceramide NP / AP / EOP): una sola tarjeta.
function unirRepetidos(lista) {
  const porId = new Map();
  for (const i of lista) {
    const previo = porId.get(i.ingredienteId);
    if (!previo) { porId.set(i.ingredienteId, { ...i, textos: [i.texto] }); continue; }
    previo.textos.push(i.texto);
    previo.porcentaje ??= i.porcentaje;
  }
  return [...porId.values()];
}

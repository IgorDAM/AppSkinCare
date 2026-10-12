import { h, api, fuentesPorId, pastillaNivel, listaFuentes } from "/js/common.js";

const q = document.getElementById("q");
const categoria = document.getElementById("categoria");
const lista = document.getElementById("lista");
const estado = document.getElementById("estado");

const [categorias, fuentes] = await Promise.all([api("/api/categorias"), fuentesPorId()]);
for (const c of categorias) categoria.append(h("option", { value: c }, c));

let temporizador;
let ultima = 0;
async function buscar() {
  const id = ++ultima;
  const ruta = `/api/ingredientes?q=${encodeURIComponent(q.value.trim())}&categoria=${encodeURIComponent(categoria.value)}&max=60`;
  try {
    const datos = await api(ruta);
    if (id !== ultima) return;
    estado.textContent = datos.length
      ? `${datos.length} ${datos.length === 1 ? "resultado" : "resultados"}`
      : "No tengo ese ingrediente en mi base. Eso no significa que sea seguro ni que sea malo: simplemente no lo conozco.";
    lista.replaceChildren(...datos.map(tarjeta));
  } catch (e) {
    if (id === ultima) estado.textContent = e.message;
  }
}

function tarjeta(i) {
  return h("article", { class: "fila n-" + (i.nivel15a18 === "evitar" || i.nivel13a14 === "evitar" ? "evitar" : i.nivel15a18 === "precaucion" || i.nivel13a14 === "precaucion" ? "precaucion" : i.nivel15a18 === "ok" ? "ok" : "neutro") },
    h("div", { class: "fila-cabecera" }, h("h4", null, i.nombre), h("span", { class: "peque" }, i.categoria)),
    h("p", null, i.resumen),
    h("div", { class: "ing-niveles" },
      h("span", null, "13–14 años: ", pastillaNivel(i.nivel13a14)),
      h("span", null, "15–18 años: ", pastillaNivel(i.nivel15a18))),
    h("details", null,
      h("summary", null, "Por qué y fuentes"),
      i.porQue13a14 === i.porQue15a18
        ? h("p", null, i.porQue15a18)
        : [h("p", null, h("strong", null, "13–14: "), i.porQue13a14), h("p", null, h("strong", null, "15–18: "), i.porQue15a18)],
      i.detalle ? h("p", null, i.detalle) : null,
      i.inci?.length ? h("p", { class: "peque" }, "Aparece en etiquetas como: " + i.inci.join(", ")) : null,
      listaFuentes(i.fuentes, fuentes)));
}

q.addEventListener("input", () => { clearTimeout(temporizador); temporizador = setTimeout(buscar, 250); });
categoria.addEventListener("change", buscar);
document.getElementById("buscador").addEventListener("submit", e => { e.preventDefault(); buscar(); });
buscar();

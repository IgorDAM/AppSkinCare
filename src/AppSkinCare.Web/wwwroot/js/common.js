// Utilidades compartidas. Todo el texto se inserta con textContent: nada de innerHTML.

export function h(etiqueta, atributos, ...hijos) {
  const e = document.createElement(etiqueta);
  for (const [k, v] of Object.entries(atributos ?? {})) {
    if (v === false || v == null) continue;
    if (k === "class") e.className = v;
    else if (k === "text") e.textContent = v;
    else e.setAttribute(k, v === true ? "" : v);
  }
  for (const hijo of hijos.flat()) {
    if (hijo == null || hijo === false) continue;
    e.append(hijo.nodeType ? hijo : document.createTextNode(String(hijo)));
  }
  return e;
}

export async function api(ruta, cuerpo) {
  let respuesta;
  try {
    respuesta = await fetch(ruta, cuerpo === undefined ? undefined : {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(cuerpo)
    });
  } catch {
    throw new Error("No he podido conectar. Comprueba tu conexión e inténtalo de nuevo.");
  }
  if (respuesta.status === 429) throw new Error("Has hecho muchas consultas seguidas. Espera un minuto y vuelve a probar.");
  let datos = null;
  try { datos = await respuesta.json(); } catch { /* sin cuerpo */ }
  if (!respuesta.ok) throw new Error(datos?.detail ?? datos?.title ?? "Algo ha fallado. Inténtalo de nuevo en un rato.");
  return datos;
}

let fuentesCache;
export async function fuentesPorId() {
  if (!fuentesCache) {
    try {
      const lista = await (await fetch("/data/fuentes.json")).json();
      fuentesCache = new Map(lista.map(f => [f.id, f]));
    } catch { fuentesCache = new Map(); }
  }
  return fuentesCache;
}

export const NIVELES = {
  evitar: { clase: "n-evitar", texto: "Evitar", signo: "✕" },
  precaucion: { clase: "n-precaucion", texto: "Con cuidado", signo: "!" },
  ok: { clase: "n-ok", texto: "Bien", signo: "✓" },
  neutro: { clase: "n-neutro", texto: "Sin alerta", signo: "·" }
};

export function pastillaNivel(nivel) {
  const n = NIVELES[nivel] ?? NIVELES.neutro;
  return h("span", { class: "nivel " + n.clase }, h("span", { "aria-hidden": "true" }, n.signo), n.texto);
}

export function listaFuentes(ids, mapa) {
  const items = (ids ?? []).map(id => mapa.get(id)).filter(Boolean);
  if (!items.length) return null;
  return h("ul", { class: "lista-fuentes" }, items.map(f =>
    h("li", null, f.url ? h("a", { href: f.url, rel: "noopener noreferrer", target: "_blank" }, f.titulo) : f.titulo)));
}

// ---- menú y tema ----

function temaActual() {
  const t = document.documentElement.getAttribute("data-theme");
  if (t) return t;
  return matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}

function iniciarCabecera() {
  const menu = document.getElementById("menu");
  const boton = document.querySelector(".menu-boton");
  if (menu && boton) {
    boton.addEventListener("click", () => {
      const abierto = menu.classList.toggle("abierto");
      boton.setAttribute("aria-expanded", String(abierto));
    });
  }

  const tema = document.querySelector(".tema-boton");
  if (tema) {
    const pintar = () => { tema.textContent = temaActual() === "dark" ? "Modo claro" : "Modo oscuro"; };
    pintar();
    tema.addEventListener("click", () => {
      const nuevo = temaActual() === "dark" ? "light" : "dark";
      document.documentElement.setAttribute("data-theme", nuevo);
      try { localStorage.setItem("tema", nuevo); } catch { /* da igual */ }
      pintar();
    });
  }

  const ruta = location.pathname.replace(/\/index\.html$/, "/");
  document.querySelectorAll("#menu a").forEach(a => {
    if (new URL(a.href).pathname === ruta) a.setAttribute("aria-current", "page");
  });
}

iniciarCabecera();

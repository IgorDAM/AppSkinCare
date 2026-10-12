import { api } from "/js/common.js";

const sitio = document.getElementById("cuantos");
if (sitio) {
  try {
    const s = await api("/api/salud");
    sitio.textContent = String(s.ingredientes);
  } catch { /* se queda el texto de reserva */ }
}

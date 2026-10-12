// Se carga en <head> y sin "defer": pone el tema guardado antes de pintar para evitar parpadeos.
(function () {
  try {
    var t = localStorage.getItem("tema");
    if (t === "light" || t === "dark") document.documentElement.setAttribute("data-theme", t);
  } catch (e) { /* sin almacenamiento: se usa el tema del sistema */ }
  document.documentElement.classList.add("js");
})();

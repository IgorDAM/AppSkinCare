#!/usr/bin/env python3
"""Genera las páginas HTML de wwwroot (cabecera y pie compartidos) y la de «Problemas» a partir de data/problemas.json.

Uso:  python3 tools/generar-paginas.py
No hay nada que instalar. Las páginas generadas SÍ se suben al repositorio, para que la web
funcione sin ejecutar este script."""
import html
import json
from pathlib import Path

RAIZ = Path(__file__).resolve().parent.parent / "src" / "AppSkinCare.Web" / "wwwroot"
REPO = "https://github.com/IgorDAM/AppSkinCare"
e = html.escape

MENU = [
    ("/", "Revisar etiqueta"),
    ("/rutina.html", "Mi rutina"),
    ("/problemas.html", "Problemas de piel"),
    ("/mitos.html", "¿Mito o verdad?"),
    ("/ingredientes.html", "Ingredientes"),
    ("/sobre.html", "Sobre la web"),
]


def pagina(titulo, descripcion, cuerpo, script=None, robots=None):
    menu = "\n".join(f'      <a href="{u}">{e(t)}</a>' for u, t in MENU)
    mod = f'<script type="module" src="/js/{script}.js"></script>' if script else '<script type="module" src="/js/common.js"></script>'
    meta_robots = f'\n<meta name="robots" content="{robots}">' if robots else ""
    return f"""<!doctype html>
<html lang="es">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>{e(titulo)}</title>
<meta name="description" content="{e(descripcion)}">{meta_robots}
<meta name="color-scheme" content="light dark">
<link rel="preload" href="/fonts/unbounded-latin-wght-normal.woff2" as="font" type="font/woff2" crossorigin>
<link rel="preload" href="/fonts/figtree-latin-wght-normal.woff2" as="font" type="font/woff2" crossorigin>
<link rel="icon" href="/favicon.svg" type="image/svg+xml">
<link rel="stylesheet" href="/css/site.css">
<script src="/js/theme.js"></script>
</head>
<body>
<a class="saltar" href="#contenido">Saltar al contenido</a>
<header class="cabecera">
  <div class="contenedor cabecera-in">
    <a class="marca" href="/"><span class="marca-sello" aria-hidden="true">Aa</span>Letra pequeña</a>
    <button class="menu-boton" type="button" aria-expanded="false" aria-controls="menu">Menú</button>
    <nav class="menu" id="menu" aria-label="Principal">
{menu}
    </nav>
    <button class="tema-boton" type="button">Modo oscuro</button>
  </div>
</header>
<main id="contenido">
{cuerpo}
</main>
<footer class="pie">
  <div class="contenedor">
    <p>Información educativa para personas de 13 a 18 años. No es un diagnóstico y no sustituye a un médico/a ni a un dermatólogo/a.</p>
    <p>No hay cuentas ni cookies, y no guardamos lo que escribes. <a href="/sobre.html">Cómo funciona y qué no sé</a> · <a href="{REPO}" rel="noopener noreferrer">Código abierto</a></p>
  </div>
</footer>
{mod}
</body>
</html>
"""


def escribir(nombre, contenido):
    (RAIZ / nombre).write_text(contenido, encoding="utf-8", newline="\n")
    print("escrito", nombre)


# ---------------------------------------------------------------- inicio

def selector_edad(id_="edad"):
    ops = "\n".join(f'<option value="{n}"{" selected" if n == 15 else ""}>{n} años</option>' for n in range(13, 19))
    return f'<select id="{id_}" name="edad">\n{ops}\n</select>'


def tipo_piel():
    return """<select id="tipoPiel" name="tipoPiel">
<option value="normal">Normal / no lo sé</option>
<option value="seca">Seca (tira, se descama)</option>
<option value="grasa">Grasa (brilla)</option>
<option value="mixta">Mixta (brilla solo en la frente y la nariz)</option>
</select>"""


def ficha(nombre, texto, marcado=False):
    return f'<label class="ficha"><input type="checkbox" name="{nombre}"{" checked" if marcado else ""}><span>{e(texto)}</span></label>'


INDEX = f"""
<div class="contenedor hero">
  <div>
    <h1>Lee la letra pequeña.</h1>
    <p class="hero-lead">Pega los ingredientes de tu crema, limpiador o protector y te cuento qué hace cada uno. Sin cuentas. Sin guardar nada.</p>
    <p class="privacidad-linea"><span aria-hidden="true">🔒</span><span>Lo que escribes se revisa y se olvida. No se guarda ni se comparte.</span></p>
    <div class="caminos">
      <a class="camino" href="/rutina.html"><strong>No tengo nada aún</strong><span>Te propongo una rutina corta para empezar.</span></a>
      <a class="camino" href="/problemas.html"><strong>Tengo un problema en la piel</strong><span>Granos, manchas, sequedad, sol…</span></a>
      <a class="camino" href="/mitos.html"><strong>¿Mito o verdad?</strong><span>Un juego rápido con cosas que seguro has oído.</span></a>
    </div>
  </div>

  <div class="panel panel-etiqueta">
    <form id="formulario" novalidate>
      <div class="campo">
        <label for="inci">Ingredientes del producto</label>
        <textarea id="inci" name="inci" required placeholder="Está en la parte de atrás del envase, después de «Ingredients». Cópialo tal cual."></textarea>
        <p class="ejemplos peque">
          Prueba con un ejemplo:
          <button type="button" class="boton-texto" data-ejemplo="limpiador">limpiador</button>
          <button type="button" class="boton-texto" data-ejemplo="acne">crema para granos</button>
          <button type="button" class="boton-texto" data-ejemplo="hidratante">hidratante</button>
        </p>
      </div>

      <div class="dos-columnas campos-pareja">
        <div class="campo"><label for="edad">Tu edad</label>{selector_edad()}</div>
        <div class="campo"><label for="tipoPiel">Tu tipo de piel</label>{tipo_piel()}</div>
      </div>

      <fieldset class="campo">
        <legend>¿Te pasa alguna de estas cosas? <span class="peque">(opcional)</span></legend>
        <div class="fichas">
          {ficha("acne", "Granos")}
          {ficha("manchas", "Marcas tras los granos")}
          {ficha("sensible", "Piel sensible")}
          {ficha("atopia", "Eccema o piel atópica")}
          {ficha("isotretinoina", "Tomo isotretinoína")}
        </div>
      </fieldset>

      <fieldset class="campo">
        <legend>¿Cómo se usa el producto?</legend>
        <div class="fichas">{ficha("seAclara", "Se aclara con agua (limpiador, champú, gel)")}</div>
      </fieldset>

      <details class="campo">
        <summary>Comparar con mi rutina (opcional)</summary>
        <div id="rutina-caja" class="rutina-caja">
          <div class="campo">
            <label for="rutina-1">Producto 1 de tu rutina</label>
            <textarea id="rutina-1" name="rutina" rows="3" placeholder="Pega aquí sus ingredientes"></textarea>
          </div>
          <p><button type="button" class="boton-texto" id="anadir-producto">+ Añadir otro producto</button></p>
        </div>
        <p class="peque">Así puedo avisarte si juntas varios productos que irritan.</p>
      </details>

      <div class="acciones">
        <button class="boton" type="submit" id="revisar">Revisar etiqueta</button>
      </div>
      <div class="error" id="error" role="alert" tabindex="-1" hidden></div>
    </form>
  </div>
</div>
<section class="resultado" id="resultado" tabindex="-1" hidden></section>
"""

escribir("index.html", pagina(
    "Letra pequeña · Revisa los ingredientes de tu producto",
    "Pega la lista de ingredientes de un producto de cuidado de la piel y te explico qué hace cada uno, para personas de 13 a 18 años.",
    INDEX, "index"))

# ---------------------------------------------------------------- rutina

RUTINA = f"""
<div class="contenedor cabecera-pagina">
  <h1>Mi rutina</h1>
  <p>Cuéntame cómo es tu piel y te propongo una rutina corta. Menos es más.</p>
</div>
<div class="contenedor">
  <div class="panel">
    <form id="formulario" novalidate>
      <div class="dos-columnas campos-pareja">
        <div class="campo"><label for="edad">Tu edad</label>{selector_edad()}</div>
        <div class="campo"><label for="tipoPiel">Tu tipo de piel</label>{tipo_piel()}</div>
      </div>
      <div class="campo">
        <label for="acne">¿Tienes granos?</label>
        <select id="acne" name="acne">
          <option value="no">No</option>
          <option value="leve">Pocos (algunos granitos o puntos negros)</option>
          <option value="moderado">Bastantes (algunos inflamados)</option>
          <option value="intenso">Muchos, grandes, dolorosos o que dejan cicatriz</option>
        </select>
      </div>
      <fieldset class="campo">
        <legend>¿Algo más? <span class="peque">(opcional)</span></legend>
        <div class="fichas">
          {ficha("manchas", "Marcas tras los granos")}
          {ficha("sensible", "Piel sensible")}
          {ficha("atopia", "Eccema o piel atópica")}
          {ficha("isotretinoina", "Tomo isotretinoína")}
          {ficha("pielClara", "Me quemo con facilidad")}
          {ficha("muchoSol", "Paso mucho tiempo al aire libre")}
        </div>
      </fieldset>
      <div class="acciones"><button class="boton" type="submit" id="generar">Ver mi rutina</button></div>
      <div class="error" id="error" role="alert" hidden></div>
    </form>
  </div>
</div>
<section class="resultado contenedor resultado-suelto" id="resultado" tabindex="-1" hidden></section>
"""
escribir("rutina.html", pagina(
    "Mi rutina · Letra pequeña",
    "Una rutina corta de cuidado de la piel según tu edad y tu tipo de piel.",
    RUTINA, "rutina"))

# ---------------------------------------------------------------- problemas (estático desde JSON)

problemas = json.loads((RAIZ / "data" / "problemas.json").read_text(encoding="utf-8"))
fuentes = {f["id"]: f for f in json.loads((RAIZ / "data" / "fuentes.json").read_text(encoding="utf-8"))}


def lista(items):
    return "<ul>" + "".join(f"<li>{e(x)}</li>" for x in items) + "</ul>"


def fuentes_html(ids):
    li = []
    for i in ids:
        f = fuentes[i]  # si falta una fuente, que falle la generación: así no se publica una cita rota
        t = e(f["titulo"])
        li.append(f'<li><a href="{e(f["url"])}" rel="noopener noreferrer">{t}</a></li>' if f.get("url") else f"<li>{t}</li>")
    return '<ul class="lista-fuentes">' + "".join(li) + "</ul>"


indice = "".join(f'<li><a href="#{p["id"]}">{e(p["titulo"])}</a></li>' for p in problemas)
bloques = []
for p in problemas:
    bloques.append(f"""<details class="panel problema" id="{p['id']}">
  <summary>{e(p['titulo'])}<span class="gancho">{e(p['gancho'])}</span></summary>
  <p>{e(p['queEs'])}</p>
  <div class="problema-cuerpo">
    <div class="bloque-ayuda"><h4>Qué ayuda</h4>{lista(p['ayuda'])}</div>
    <div class="bloque-evitar"><h4>Qué evitar</h4>{lista(p['evitar'])}</div>
  </div>
  <div class="bloque-medico"><h4>Ve al médico/a si…</h4>{lista(p['medicoSi'])}</div>
  <details><summary class="peque">Fuentes</summary>{fuentes_html(p['fuentes'])}</details>
</details>""")

PROBLEMAS = f"""
<div class="contenedor cabecera-pagina">
  <h1>Problemas de piel</h1>
  <p>Consejos generales para lo más común a tu edad. Si algo te preocupa, el médico/a o el dermatólogo/a sabe más que esta web.</p>
</div>
<div class="contenedor">
  <ul class="problemas-indice">{indice}</ul>
  {"".join(bloques)}
  <p class="aviso-legal">Son consejos generales de cuidado diario. No sirven para diagnosticar nada. Si el problema es fuerte, dura mucho o te preocupa, consúltalo.</p>
</div>
"""
escribir("problemas.html", pagina(
    "Problemas de piel · Letra pequeña",
    "Consejos claros para granos, manchas, piel seca, eccema, sol y más, para personas de 13 a 18 años.",
    PROBLEMAS))

# ---------------------------------------------------------------- mitos

MITOS = """
<div class="contenedor cabecera-pagina">
  <h1>¿Mito o verdad?</h1>
  <p>Cosas que seguro has oído sobre la piel. Elige y te cuento por qué.</p>
</div>
<div class="contenedor">
  <div class="panel quiz" id="quiz">
    <noscript><p>Este juego necesita JavaScript. Mientras tanto: casi todo lo que se cuenta sobre «trucos» para los granos es mito. Lo que funciona es limpiar suave, hidratar y proteger del sol.</p></noscript>
  </div>
</div>
"""
escribir("mitos.html", pagina(
    "¿Mito o verdad? · Letra pequeña",
    "Un juego rápido para descubrir qué es mito y qué es verdad en el cuidado de la piel.",
    MITOS, "mitos"))

# ---------------------------------------------------------------- ingredientes

ING = """
<div class="contenedor cabecera-pagina">
  <h1>Ingredientes</h1>
  <p>Busca un ingrediente por su nombre (en español o como sale en la etiqueta) y mira qué sé de él.</p>
</div>
<div class="contenedor">
  <form id="buscador" class="filtros" role="search">
    <div class="campo"><label for="q">Nombre</label><input type="search" id="q" name="q" placeholder="Por ejemplo: glicerina, niacinamide, retinol" autocomplete="off"></div>
    <div class="campo"><label for="categoria">Tipo</label><select id="categoria"><option value="">Todos</option></select></div>
  </form>
  <p id="estado" class="peque" aria-live="polite"></p>
  <div id="lista" class="tarjetas"></div>
</div>
"""
escribir("ingredientes.html", pagina(
    "Ingredientes · Letra pequeña",
    "Consulta qué hace cada ingrediente de cosmética y si es adecuado a tu edad.",
    ING, "ingredientes"))

# ---------------------------------------------------------------- sobre

FUENTES_LI = "".join(
    (f'<li><a href="{e(f["url"])}" rel="noopener noreferrer">{e(f["titulo"])}</a> <span class="peque">({e(f.get("tipo",""))})</span></li>'
     if f.get("url") else f'<li>{e(f["titulo"])} <span class="peque">(resumen propio)</span></li>')
    for f in fuentes.values())

SOBRE = f"""
<div class="contenedor cabecera-pagina">
  <h1>Sobre la web</h1>
  <p>Qué hace, cómo lo hace y, sobre todo, qué no sabe.</p>
</div>
<div class="contenedor prosa">
  <h2>Qué es</h2>
  <p>Una web informativa para personas de 13 a 18 años. Lees la lista de ingredientes de un producto y te explico, ingrediente por ingrediente, si puede darte problemas a tu edad y a tu tipo de piel. También hay consejos para problemas comunes y una rutina corta.</p>
  <p>No hay cuentas, ni cookies, ni publicidad. Lo que pegas se revisa en el momento y no se guarda.</p>

  <h2>Cómo decide</h2>
  <ul>
    <li>Cada ingrediente tiene una ficha con su nivel (bien, sin alerta, con cuidado o evitar) para 13–14 años y para 15–18, y la razón.</li>
    <li>Las reglas salen de guías de sociedades médicas, normas de la Unión Europea y revisiones científicas. Las fuentes aparecen en cada ficha.</li>
    <li>Ahora mismo conozco <strong id="cuantos">varios</strong> grupos de ingredientes.</li>
    <li>El orden de la lista y los porcentajes (si los indica la etiqueta) se tienen en cuenta. Muchas veces la etiqueta no dice cuánto lleva de cada cosa, y entonces te lo digo.</li>
    <li>Si juntas varios productos que irritan, te aviso.</li>
  </ul>

  <h2>Lo que no sé</h2>
  <p>Cuando no conozco un ingrediente, te lo digo con su nombre y no lo cuento como «seguro». <strong>Que un ingrediente no salga como problema no significa que sea seguro</strong>: puede ser que no lo conozca. Si no reconozco suficientes ingredientes, el resultado es «No lo sé», no «Bien».</p>
  <p>Tampoco puedo saber:</p>
  <ul>
    <li>Si eres alérgico/a a algo concreto.</li>
    <li>Cuánto lleva realmente de cada ingrediente si la etiqueta no lo dice.</li>
    <li>Cómo está hecha la fórmula completa: un mismo ingrediente se comporta distinto según el producto.</li>
    <li>Qué te ha recetado tu médico/a.</li>
  </ul>

  <h2>Lo que he dejado fuera</h2>
  <p>He preferido dejar fuera lo que no he podido confirmar, y te lo cuento:</p>
  <ul>
    <li><strong>Marcas.</strong> La web no valora marcas ni productos con nombre. Las marcas cambian las fórmulas sin avisar y no he podido confirmar ninguna información de forma fiable. Por eso trabajo con la lista de ingredientes que tú pegas.</li>
    <li><strong>Normas fuera de la Unión Europea.</strong> En mi investigación aparecieron normas de China, Corea y Brasil, pero no he podido confirmarlas con una fuente oficial, así que no las uso.</li>
    <li><strong>Una recomendación francesa sobre el fenoxietanol en la zona del pañal.</strong> Tampoco he podido confirmarla, y además no tiene que ver con tu edad.</li>
    <li><strong>Etnia o color de piel.</strong> No he encontrado reglas confirmadas que justifiquen cambiar el resultado según la etnia, así que no te lo pregunto. Lo que sí es seguro: <em>todas</em> las pieles necesitan protección solar.</li>
  </ul>
  <p>Si tienes una fuente fiable sobre alguno de estos puntos, se puede añadir: el proyecto es de código abierto.</p>

  <h2>De dónde sale la información</h2>
  <p>Cada ficha de ingrediente, cada consejo y cada mito enlaza a su fuente. Estas son todas. Las que dicen «resumen propio» son notas de investigación del proyecto: no son una fuente médica, y por eso siempre que he podido las he respaldado con una de las demás.</p>
  <ul class="lista-fuentes">{FUENTES_LI}</ul>
  <p class="peque">Los enlaces llevan a páginas externas. Algunas revistas piden registro o pago para leer el artículo completo; las normas y guías de sociedades médicas suelen ser gratuitas.</p>

  <h2>Es gratis</h2>
  <p>Usarla no cuesta nada y no hay nada de pago escondido. Funciona en un alojamiento gratuito, sin anuncios, y todo el código es abierto.</p>

  <h2>Lo que esta web no es</h2>
  <p>No es un médico ni un dermatólogo. No diagnostica nada y no sustituye una consulta. Si algo te duele, te preocupa o no mejora, habla con un adulto de confianza y con un profesional sanitario.</p>

  <h2>Código y datos</h2>
  <p>Todo el código y la base de ingredientes están en <a href="{REPO}" rel="noopener noreferrer">GitHub</a>. Las tipografías (Unbounded y Figtree) tienen licencia SIL Open Font License.</p>
</div>
"""
escribir("sobre.html", pagina(
    "Sobre la web · Letra pequeña",
    "Cómo funciona esta web, qué no sabe y qué hemos dejado fuera por no poder confirmarlo.",
    SOBRE, "sobre"))

# ---------------------------------------------------------------- 404

NOT_FOUND = """
<div class="contenedor cabecera-pagina">
  <h1>Esta página no existe</h1>
  <p>Puede que el enlace esté mal escrito. Prueba a volver al inicio.</p>
  <p><a class="boton" href="/">Ir al inicio</a></p>
</div>
"""
escribir("404.html", pagina("No encontrada · Letra pequeña", "Esta página no existe.", NOT_FOUND, robots="noindex"))

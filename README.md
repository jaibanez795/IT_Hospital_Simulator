```markdown
# Game Design Document (GDD)

## "IT del Hospital" *(Título de trabajo)*

---

## 1. Concepto General

| Elemento | Descripción |
|----------|-------------|
| **Género** | Simulador / Estrategia Social / Comedia |
| **Tono** | Humor, sátira laboral, casual |
| **Perspectiva** | Tercera persona (isométrico) o primera persona |
| **Plataforma** | PC (Steam), potencialmente móvil |
| **Jugadores** | Single-player (con potencial multijugador futuro) |
| **Inspiraciones** | Papers Please, Untitled Goose Game, Job Simulator, The Stanley Parable |

---

## 2. Premisa y Narrativa

Eres el nuevo técnico de IT en un hospital público. Tu contrato es indefinido, tu sueldo es mediocre, y tu motivación es **nula**. Tu objetivo no es ser el mejor empleado... es sobrevivir cada jornada haciendo **lo mínimo indispensable** sin que te corran.

El hospital es un ecosistema social complejo: doctores estresados, enfermeras que lo saben todo, administrativos chismosos, y supervisores que buscan cualquier excusa para reportarte. Tu verdadera habilidad no es la tecnología... es **la política de oficina**.

---

## 3. Loop Principal de Juego

```
INICIO DEL DÍA
     │
     ▼
Revisar tickets del día (obligatorios mínimos)
     │
     ▼
Decidir: ¿Trabajo rápido o socializo?
     │
     ├──► Resolver tickets (mantiene tu "métrica" segura)
     │
     ├──► Socializar (ganas aliados, info, favores)
     │
     ├──► Esconderte (descansas, pero hay riesgo)
     │
     └──► Actividades personales (celular, snacks, siesta)
     │
     ▼
Eventos aleatorios (supervisor aparece, emergencia IT, chisme útil)
     │
     ▼
FIN DEL DÍA → Evaluación → Siguiente día
```

---

## 4. Mecánicas Principales

### 4.1 Sistema de Tickets

Los tickets son tu "trabajo real". Deben ser **simples y rápidos** para que no sean el foco del juego, sino una obligación molesta:

- **Cambiar cable de red** — Click en el cable correcto (3 opciones de color)
- **Reiniciar computadora** — Mantener botón presionado 3 segundos
- **Resetear contraseña** — Escribir 4 caracteres cualquiera y confirmar
- **"No me jala el internet"** — Desconectar y reconectar un cable (drag & drop)
- **Instalar programa** — Barra de progreso donde solo presionas "Siguiente, siguiente, aceptar"
- **Cambiar tóner** — Mini puzzle de sacar cartucho y meter nuevo

**Reglas de tickets:**

- Tienes una **cola de tickets diaria** (3-6 por día)
- Debes resolver un **mínimo** para no levantar sospechas (ej: 2 de 5)
- Resolver más tickets = menos tiempo para socializar
- Resolver menos = riesgo de que el supervisor te busque

---

### 4.2 Sistema Social / Relaciones

El corazón del juego. Cada NPC del hospital tiene:

- **Nivel de amistad** (0-5 estrellas)
- **Personalidad** (chismoso, serio, cómplice, soplón)
- **Beneficio al ser aliado**
- **Ubicación habitual y horario**

#### NPCs Clave:

| Personaje | Rol | Beneficio de Amistad |
|-----------|-----|---------------------|
| **Doña Lupe** (Intendencia) | Sabe dónde están todos | Te avisa cuando viene el supervisor |
| **Dr. Ramírez** (Médico) | Respetado por todos | Te cubre diciendo que "estás en su consultorio arreglando algo" |
| **Paty** (Recepción) | Chismosa nivel experto | Te da info sobre auditorías, juntas, visitas sorpresa |
| **Carlos** (Almacén) | Relajado, esconde cosas | Te presta un cuarto para descansar |
| **Ing. Mendoza** (Tu supervisor) | Antagonista principal | Si subes su relación, te deja en paz... un rato |
| **Mariana** (Recursos Humanos) | Neutral/peligrosa | Si la caes bien, ignora reportes menores |
| **Don Beto** (Vigilancia) | Controla cámaras | Te avisa de zonas seguras o borra grabaciones |

#### Cómo subir amistad:

- **Platicar** en momentos libres (diálogos con opciones)
- **Hacer favores** personales (arreglar el celular de alguien, bajar una película)
- **Compartir** snacks, café, chismes
- **Resolver su ticket** de forma prioritaria
- **NO resolver su ticket** si te piden que lo dejes para otro día (complicidad)

---

### 4.3 Sistema de Habilidades

Las habilidades suben con el uso, estilo RPG pasivo:

| Habilidad | Cómo sube | Efecto |
|-----------|-----------|--------|
| **Labia** | Platicando con NPCs | Más opciones de diálogo, convences más fácil |
| **Sigilo** | Escondiéndote exitosamente | Menos probabilidad de que te descubran |
| **Velocidad IT** | Resolviendo tickets | Terminas tickets más rápido (más tiempo libre) |
| **Percepción** | Sobreviviendo encuentros con el supervisor | Ves alertas de peligro antes |
| **Carisma** | Haciendo favores | La gente te busca para darte info sin pedirla |
| **Chanchullo** | Haciendo tratos bajo la mesa | Acceso a atajos, items especiales |

---

### 4.4 Sistema de Riesgo / Sospecha

Hay un **medidor de sospecha** que funciona como tu "vida":

```
[██████████░░░░░░░░░░] 50% Sospecha
```

- **Sube** cuando: no resuelves tickets, te cacha el supervisor sin hacer nada, un NPC soplón te reporta, llegas tarde
- **Baja** cuando: resuelves tickets, un aliado te cubre, asistes a junta (aunque no pongas atención), haces algo visible

**Si llega al 100%:** Acta administrativa → 3 actas = **GAME OVER (despido)**

---

### 4.5 Sistema de Escondites

El mapa del hospital tiene zonas donde puedes "desaparecer":

- **Cuarto de servidores** — Siempre disponible, pero el supervisor lo revisa seguido
- **Azotea** — Hay que desbloquearla (amistad con vigilancia)
- **Almacén** — Necesitas amistad con Carlos
- **Cafetería** — Semi-segura, pero todos te ven
- **Estacionamiento** — Riesgoso, si te ven ahí es sospechoso
- **Consultorio vacío** — Temporal, puede llegar alguien

Cada escondite tiene un **nivel de seguridad** y un **tiempo máximo** antes de que sea riesgoso quedarse.

---

## 5. Estructura del Día

Un día en el juego dura **~10-15 minutos reales**:

| Hora (in-game) | Fase | Descripción |
|----------------|------|-------------|
| 8:00 | Llegada | Checa tu entrada (llegar tarde = sospecha +10) |
| 8:00-9:00 | Mañana temprana | Pocos supervisores, buen momento para socializar |
| 9:00-12:00 | Horario activo | Tickets llegan, supervisor ronda, máximo riesgo |
| 12:00-13:00 | Comida | Tiempo libre, socialización fuerte |
| 13:00-16:00 | Tarde | Menos vigilancia, pero tickets pendientes pesan |
| 16:00 | Salida | Checa salida, resumen del día |

---

## 6. Progresión y Objetivos

### Corto plazo (diario):
- Sobrevivir el día sin acta
- Resolver el mínimo de tickets
- Avanzar una relación

### Mediano plazo (semanal):
- Desbloquear nuevo escondite
- Conseguir un aliado clave
- Sobrevivir auditoría sorpresa

### Largo plazo (historia):
- **Conseguir tu base (plaza definitiva)** — El juego tiene un arco narrativo donde tu objetivo final es asegurar tu puesto permanente
- Crear una **red de aliados** tan fuerte que básicamente nadie te molesta
- Eventos finales: auditoría externa, cambio de jefe, reestructuración

---

## 7. Eventos Aleatorios

Para mantener cada día fresco:

- **"¡Cayó el sistema!"** — Emergencia donde TODOS buscan al de IT (debes actuar rápido o sospecha sube mucho)
- **Visita del director general** — Todos fingen trabajar, tú también debes
- **Chisme bomba** — Un NPC te cuenta algo que puedes usar como leverage
- **Corte de luz** — Oportunidad para desaparecer sin consecuencias
- **Junta obligatoria** — Pierdes tiempo pero baja sospecha
- **Nuevo empleado** — NPC temporal que puede ser aliado o soplón
- **Ticket imposible** — "Mi computadora habla sola" (humor absurdo)

---

## 8. Mapa del Hospital

```
┌─────────────────────────────────────────┐
│  AZOTEA (escondite nivel 3)             │
├─────────┬───────────┬───────────────────┤
│ Oficina │ Consulto- │  Cuarto de        │
│ de IT   │ rios      │  Servidores       │
│ (base)  │           │  (escondite lv1)  │
├─────────┼───────────┼───────────────────┤
│ Recep-  │ Pasillos  │  Almacén          │
│ ción    │ principa- │  (escondite lv2)  │
│         │ les       │                   │
├─────────┼───────────┼───────────────────┤
│ Cafete- │ Recursos  │  Estaciona-       │
│ ría     │ Humanos   │  miento           │
└─────────┴───────────┴───────────────────┘
```

---

## 9. Monetización (si aplica)

- **Modelo premium**: Compra única en Steam ($10-15 USD)
- **DLCs potenciales**: Nuevos hospitales (hospital privado, clínica rural), nuevos personajes
- **Cosméticos**: Skins para tu personaje (bata, gafete personalizado, stickers en laptop)

---

## 10. Estilo Visual Sugerido

- **Low-poly colorido** o estilo cartoon (como Overcooked o Two Point Hospital)
- Expresiones exageradas en NPCs
- UI minimalista con toques de "sistema operativo viejo" (ventanas tipo Windows XP para los tickets)
- Colores del hospital: blancos, verdes, azules, con toques de personalidad en cada zona

---

## 11. Audio

- **Música**: Lo-fi relajado durante momentos tranquilos, música tensa cuando el supervisor está cerca
- **Efectos**: Sonidos de hospital (intercomunicador, pitidos), teclado, notificaciones de tickets
- **Voces**: Murmullos o expresiones cortas (sin voz completa, estilo Animal Crossing)

---

## 12. Nombre Final (Sugerencias)

- *IT Survival: Hospital Edition*
- *Mínimo Esfuerzo*
- *El Técnico*
- *Ctrl+Alt+Descansar*
- *Ticket Pendiente*
- *No Estoy En Mi Lugar*

---

## Próximos Pasos

1. **Validar el concepto**: ¿El tono es más mexicano/latino o universal?
2. **Definir el alcance del prototipo**: Un día completo con 2-3 NPCs y el sistema de tickets
3. **Elegir motor**: Unity sería ideal por la comunidad y recursos
4. **Arte conceptual**: Definir el estilo visual antes de modelar
```

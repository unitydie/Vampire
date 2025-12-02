Prosjekttittel:
«Online Survivors» (arbeidstittel)

Idé:
Et lite arkadespill der spillerne prøver å overleve bølger av monstre. Fokus er på:

Enkelt og intuitivt kontrolloppsett

Automatiske angrep fra figuren

Følelsen av kaos og fremgang når figuren blir sterkere over tid

Målet er å ha en fungerende prototype.

Konsept:

Sjanger: arkade-action / roguelite med top-down-perspektiv

Plattform: PC (Windows)

Kamera: ovenfra, litt på skrå

Spillerne befinner seg på en arena hvor fiender hele tiden dukker opp. Når fiender dør, får man erfaringskrystaller som fyller opp erfaringsbaren. Målet er å overleve så lenge som mulig. Når en spiller dør, sendes han/hun tilbake til lobbyen.

Verktøy og teknologi:

Spillmotor: Unity

Programmeringsspråk: C#

Arkitektur: ECS (Entity Component System) for å håndtere mange objekter

Nettverk: Photon Fusion (planlagt for fremtidig flerspiller)

UI og kontroller: Unity UI + tastatur (WASD) eller virtuell joystick

Grafikk: enkle 2D-spriter eller low-poly 3D-modeller

Ferdige kjerne-mekanikker:

Spillfiguren:

Bevegelse med tastatur eller joystick

Automatisk angrep når fiende er i rekkevidde

HP og erfaringsbar vises i UI

Figuren kan dø

Fiender:

Spawner rundt spilleren, men ikke rett på ham/henne

Beveger seg mot nærmeste spiller

Angriper spilleren til enten fienden eller spilleren dør

ECS / DOTS-systemer:

Kamp mellom spillere og fiender

Opptjening av erfaringskrystaller

Død av spiller og fiender

Spawning av fiender

UI:

HP-bar som minker når spilleren tar skade

Erfaringsbar som fylles når krystaller samles inn

Visuell stil:

Enkel og oversiktlig for å skille spillere, fiender og dropp

Kan være 2D top-down-spriter eller enkel 3D

Status / MVP:
Alt som er implementert:

Bevegelse og automatisk angrep for spillfiguren

Fiender som spawner, går mot spiller og gjør skade

Dropp av erfaringskrystaller

HP- og erfaringsbar i UI

System for død av spiller og fiender

Kamp- og erfaringssystem via DOTS

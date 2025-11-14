# Vampire
1. Prosjekttittel

«Online Survivors» (arbeidstittel)

2. Idé

Et lite kooperativt arkadespill der flere spillere sammen prøver å overleve endeløse bølger av monstre.
Fokuset er på:

enkelt og intuitivt kontrolloppsett

automatiske angrep fra figuren

følelsen av kaos og at figuren blir sterkere over tid

mulighet til å spille med venner over nett

Målet er ikke å lage et kjempestort spill, men et lite, fungerende prototype-spill med flerspiller.

3. Konsept

Sjanger: arkade-action / roguelite med top-down-perspektiv

Plattform: PC (Windows), eventuell Android hvis det blir tid

Kamera: ovenfra, litt på skrå, likt som i spill av typen Vampire Survivors

Spillerne befinner seg på en arena der fiender hele tiden dukker opp.
Når fiender dør får man erfaring og går opp i nivå, og kan velge oppgraderinger.
Målet er å overleve så lenge som mulig. Når en spiller dør, sendes han/hun tilbake til lobbyen.

4. Verktøy og teknologi

Spillmotor: Unity

Programmeringsspråk: C#

Nettverksbibliotek: Photon Fusion (til lobbyer, rom og synkronisering av spillere/fiender)

Arkitektur: ECS (Entity Component System) for å håndtere mange objekter (spillere, fiender, prosjektiler) og for bedre ytelse

UI og kontroller: Unity UI + virtuell joystick (f.eks. Simple Input) eller tastatur (WASD)

Versjonskontroll (valgfritt): Git / GitHub

Grafikk: enkle 2D-spriter eller low-poly 3D-modeller / ikoner

5. Design og kjerne-mekanikker
5.1 Spillfiguren

Spilleren styrer figuren med joystick (mobil) eller tastatur (PC):

figuren beveger seg fritt rundt på arenaen

når en fiende kommer innenfor angrepsradius, angriper figuren automatisk den nærmeste fienden

fra døde fiender faller det:

erfaringskrystaller – fyller opp erfaringsbaren

helsedrikker – gjenoppretter HP

Når nok erfaring er samlet, går figuren opp et nivå.

5.2 Nivåsystem og oppgraderinger

Når man går opp et nivå, vises et oppgraderingsmeny øverst på skjermen (uten å sette spillet på pause).
Spilleren velger én av flere oppgraderinger, for eksempel:

økt angrepshastighet

økt skade

mer maks-HP

raskere bevegelse

Slik bygges følelsen av at figuren blir sterkere jo lengre man overlever.

5.3 Brukergrensesnitt

På skjermen ser spilleren:

HP-bar øverst på skjermen – minker når spilleren tar skade

erfaringsbar ved siden av HP-baren, samme stil men annen farge

eventuelt en timer eller hvilken bølge man er på

For andre spillere vises:

posisjonen deres på kartet

angrep/prosjektiler de skyter

små HP-barer over figurene deres

6. Fiender (AI)

Fiendene må være synkronisert likt for alle spillere.

Oppførsel:

Spawning:

fiender spawner utenfor spillernes synsfelt

et tilfeldig punkt velges rundt en tilfeldig spiller

det sjekkes at fienden ikke dukker opp rett oppå en spiller

Bevegelse:

fienden beveger seg mot nærmeste spiller

Kamp:

angriper spilleren til enten fienden dør eller spilleren dør

Alt dette skal vises likt hos alle klienter i flerspilleren.

7. Flerspiller og lobby

Før spillet starter kommer spilleren til en lobby.

Funksjonen til lobbyen:

Opprette rom / spill

spilleren trykker på en knapp for å lage et nytt rom

rommet får automatisk en unik, tilfeldig ID

Bli med i rom

spilleren skriver inn rom-ID og kobler seg til et eksisterende spill

Brukergrensesnittet i lobbyen lages i Unity, og knappene kaller Photon-funksjoner for å opprette og bli med i rom.

Regel i spillet:
Hvis en spiller dør i løpet av en runde, sendes han/hun tilbake til lobbyen, og kan ikke gå inn i det samme rommet igjen.

8. Visuell stil

Enkel og oversiktlig stil, for å spare tid på grafikk

To muligheter:

2D top-down-spriter

eller enkel 3D med kamera ovenfra

Viktig at man lett skiller:

spillere

fiender

prosjektiler

dropp (krystaller og potions)

9. Omfang og prioritering
Minimumsversjon (MVP)

Dette er det jeg prioriterer å få ferdig:

Én arena / ett kart

Bevegelse og automatisk angrep for spillfiguren

Fiender som spawner, går mot spilleren og gjør skade

Dropp av erfaringskrystaller og helsedrikker

Nivåsystem med valg av én oppgradering

HP-bar og erfaringsbar

Lobby der man kan lage rom og koble seg til med rom-ID

Synkronisering av spillere og fiender over nett (Photon Fusion + ECS)

Ekstra hvis det blir tid

Flere typer fiender

Flere typer våpen og oppgraderinger

Visuelle effekter (partikler, lysglimt, enkle animasjoner)

Lyd og musikk

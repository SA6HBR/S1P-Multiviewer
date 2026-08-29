# Mätvärden i Explore-vyn (`comboBox1`)

Förklaring av de 29 valen i rullgardinsmenyn i Explore-fliken, vad de visar och
när de är relevanta att titta på beroende på om du mäter en **antenn**
(vanligtvis enports, bara S11 är meningsfullt) eller ett **filter**
(tvåports, både S11 och S21 är meningsfulla).

## Snabbguide

**Mäter du en antenn?** Börja med `VSWR S11` eller `LogMag S11` (samma
information, olika enheter). Vill du designa en matchningsnätverk, titta även
på `Resistance_R S11` + `Reactance_X S11` (serie-form) eller
`Conductance_G S11` + `Susceptance_B S11` (parallell-form).

**Mäter du ett filter?** Börja med `LogMag S11 + S21` - visar insertion
loss (S21, rött) och return loss (S11, blått) i samma diagram, precis det
klassiska filter-diagrammet. Använd `PhaseDegrees S21` om du behöver räkna
fram group delay.

## S11 - reflektion (antenn *och* filterets ingång)

S11 beskriver hur mycket av signalen som studsar tillbaka från porten du
matar in i. För en antenn är detta i praktiken **det enda du mäter** (en
antenn har bara en port). För ett filter beskriver S11 hur väl filtrets
ingång är anpassad till 50 Ω - ett filter kan ha perfekt insertion loss men
ändå vara dåligt anpassat (reflekterar signal bakåt mot sändaren).

| Val | Vad det visar | Antenn | Filter |
|---|---|---|---|
| `VSWR S11` | Voltage Standing Wave Ratio, 1.0 = perfekt, stiger mot oändligheten vid total reflektion. Standardmåttet inom amatörradio. | **Huvudmått.** Mål ofta <2:1, gärna <1.5:1 i bandet du vill använda. | Användbart för ingångsanpassningen, men `LogMag`/`ReturnLoss_RL` är vanligare i filterdatablad. |
| `LogMag S11` | `20·log10(|S11|)` i dB. Negativt tal - desto mer negativt, desto bättre anpassning (mindre reflekteras). Det du ser rakt av på en NanoVNA/VNA:s S11-graf. | **Huvudmått**, alternativ till VSWR. -10 dB ≈ VSWR 1.9, -20 dB ≈ VSWR 1.2. | **Huvudmått** för ingångsanpassning i passbandet. Bör ligga under ca -15 dB i passbandet för ett bra filter. |
| `ReturnLoss_RL S11` | Samma formel som `LogMag S11` men med omvänt tecken (positivt tal). Vanligare i professionella databladskontext. | Samma information som ovan, positivt tal i stället. | Samma som ovan - vanlig i filterdatablad ("Return Loss > 15 dB"). |
| `Magnitude S11` | `|Γ|`, linjär reflektionskoefficient 0-1. 0 = perfekt anpassning, 1 = total reflektion. | Sällan huvudvyn, men bra underlag om du ska räkna vidare för hand. | Samma - mest ett beräkningsunderlag. |
| `LinMag S11` | Samma som `Magnitude S11` (linjär `|Γ|`, 0-1), beräknad via den golvskyddade dB-vägen - i praktiken samma tal. | Sällan huvudvyn. | Beräkningsunderlag. |
| `ReflectedPower S11` | `|Γ|²` - andelen av inmatad effekt som studsar tillbaka (0-1, eller ange `incidentPower` för faktisk effekt). | Bra för att förstå faktisk effektförlust vid dålig matchning, t.ex. hur många watt som reflekteras vid hög uteffekt. | Samma - relevant vid högeffektfilter där reflekterad effekt kan vara ett termiskt/skyddsproblem. |
| `ImpedanceMagnitude S11` | `|Z|`, beloppet av impedansen i ohm. | Snabb koll: ligger den nära 50 Ω? | Mindre vanligt, men kan avslöja om filtrets ingångsimpedans är rimlig. |
| `Resistance_R S11` | Resistiva delen av impedansen (serie-form), ohm. | **Viktigt vid matchning.** Vid resonans vill man ofta ha R nära 50 Ω (eller den impedans matningskabeln har). | Diagnostik av ingångsimpedansen om filtret inte beter sig som förväntat. |
| `Reactance_X S11` | Reaktiva delen (serie-form), ohm. Positivt = induktivt, negativt = kapacitivt. | **Viktigt vid matchning.** Vid resonans ska X vara nära 0. Avgör hur mycket induktans/kapacitans som behövs för att trimma antennen till resonans. | Samma diagnostik som R ovan. |
| `Conductance_G S11` | Konduktansen (parallell-form av admittansen), siemens. | Användbart om du designar en parallell-/stubmatchning i stället för serie-matchning. | Sällan förstahandsvalet, men samma nytta som för antenn. |
| `Susceptance_B S11` | Susceptansen (parallell-form), siemens. | Samma som ovan - parallellmatchning. | Sällan förstahandsvalet. |

## S21 - transmission (bara relevant för tvåportsmätningar)

S21 beskriver hur mycket signal som når port 2 från port 1. **En vanlig
enportsantenn har ingen S21** - det här blocket gäller filter, förstärkare,
dämpsatser, kablar, eller om du medvetet mäter kopplingen/isoleringen mellan
två antenner (då är "port 1" ena antennen och "port 2" den andra).

| Val | Vad det visar | Antenn | Filter |
|---|---|---|---|
| `LogMag S21` | `20·log10(|S21|)` i dB. Negativt = förlust (normalfallet för ett passivt filter/kabel), positivt = förstärkning. | Bara relevant om du mäter isolering/koppling mellan två antenner - då vill man ofta ha S21 så negativt (låg koppling) som möjligt. | **Huvudmått.** Visar insertion loss i passbandet (nära 0 dB = bra) och rejektion i stoppbandet (mycket negativt = bra). |
| `InsertionLoss S21` | Samma formel som `LogMag S21` men med omvänt tecken (positivt tal, "hur mycket förlust"). Standardterm i filterdatablad. | Samma info, sällan relevant för ren antennmätning. | **Huvudmått**, vanlig databladsterm ("Insertion Loss < 1 dB i passbandet"). |
| `Magnitude S21` | `|S21|`, linjär transmissionskoefficient 0-1 (eller >1 vid aktiv förstärkning). | Sällan huvudvyn. | Beräkningsunderlag. |
| `LinMag S21` | Samma som `Magnitude S21`, beräknad via den golvskyddade dB-vägen - i praktiken samma tal. | Sällan huvudvyn. | Beräkningsunderlag. |
| `TransmittedPower S21` | `|S21|²` - andelen av inmatad effekt som når fram genom nätverket. | Kopplings-/isoleringsmätning mellan antenner: hur stor andel av sänd effekt som "läcker" till den andra antennen. | Effektbudget - hur mycket effekt som faktiskt går igenom filtret, relevant vid högeffekt. |
| `PhaseDegrees S21` / `PhaseRadians S21` | Transmissionsfasen. | Sällan relevant för en enkel antennmätning. | Används för att räkna fram **group delay** (fasens lutning mot frekvens) - viktigt för filter som ska hantera bredbandiga signaler eller data utan att förvränga dem i tid. |
| `VSWR S21` | **Experimentell, inte fysikaliskt meningsfull.** Samma VSWR-formel som för S11 applicerad på `|S21|`, av nyfikenhet. VSWR är ett reflektionsbegrepp (stående våg mellan infallande och reflekterad våg på *samma* ledning) - S21 är ingen reflektion, så talet motsvarar inget verkligt fysikaliskt fenomen. | Använd inte som mätunderlag. | Använd inte som mätunderlag - `LogMag S21`/`InsertionLoss S21` är de rätta måtten. |

## Fasmått på S11 (`PhaseDegrees S11` / `PhaseRadians S11`)

Reflektionskoefficientens fasvinkel. Mindre använt som förstahandsmått för
antenner (VSWR/LogMag/R/X ger oftast det man behöver), men relevant om du:
- Räknar fram gruppfördröjning i en antennsystems ingång.
- Designar en matchningsnätverk för hand och behöver fasinformationen från
  Smith-diagrammet (samma information som visas där, fast som siffror).

## Kombinerade vyer (`... S11 + S21`)

Åtta val ritar **båda** kurvorna i samma diagram - S11 i blått, S21 i rött,
med förklaring/legend: `Magnitude S11 + S21`, `LinMag S11 + S21`,
`LogMag S11 + S21`, `PhaseDegrees S11 + S21`, `PhaseRadians S11 + S21`,
`ReflectedPower S11 + TransmittedPower S21`,
`ReturnLoss_RL S11 + InsertionLoss S21`, och `VSWR S11 + S21`. Innehållet är
samma formler som i respektive S11/S21-rad ovan, bara ritade tillsammans.

Mest användbart för filter, där du vill se insertion loss och return loss
samtidigt (`LogMag S11 + S21` är den klassiska filterkarakteriseringsgrafen).
För en ren antennmätning är den kombinerade vyn sällan relevant eftersom S21
normalt saknas helt.

**`VSWR S11 + S21` innehåller den experimentella S21-varianten** (se
varningen i S21-tabellen ovan) - S11-kurvan (blå) är ett giltigt VSWR, men
S21-kurvan (röd) är det inte. Använd `LogMag S11 + S21` eller
`ReturnLoss_RL S11 + InsertionLoss S21` i stället om du vill ha en
fysikaliskt meningsfull kombinerad vy.

## Praktisk sammanfattning

- **Snabb antennkoll:** `VSWR S11` eller `LogMag S11`.
- **Matchningsnätverksdesign för antenn:** `Resistance_R S11` +
  `Reactance_X S11` (serie), eller `Conductance_G S11` + `Susceptance_B S11`
  (parallell/stub).
- **Filterkarakterisering:** `LogMag S11 + S21` som förstahandsval, sedan
  `PhaseDegrees S21` om group delay behövs.
- **Effektbudget/högeffekt:** `ReflectedPower S11` och `TransmittedPower S21`.
- **Undvik som mätunderlag:** `VSWR S21` (experimentell, se varning ovan).

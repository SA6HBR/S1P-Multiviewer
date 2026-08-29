# Guide: Antennanalys med S11 (Reflection)

Denna guide beskriver hur man tolkar mätvärden vid analys och trimning av en antenn i ett 50-ohms system. Vid antennmätning är det primärt **Port 1 (S11)** som används.

## 1. Huvudindikatorer för Antennprestanda

| Parameter | Utmärkt | Acceptabelt | Justering krävs |
| :--- | :--- | :--- | :--- |
| **VSWR** | 1.0 till 1.3 | 1.3 till 2.0 | > 2.0 (Risk för radioskada) |
| **LogMag (S11)** | < -18 dB | -10 till -18 dB | > -10 dB |
| **Reflected Power**| < 1.5% | 1.5% till 10% | > 10% studsar tillbaka |
| **Resistance (R)** | 45 - 55 $\Omega$ | 30 - 70 $\Omega$ | < 20 eller > 100 $\Omega$ |
| **Reactance (X)** | ~ 0 $\Omega$ | < +/- 10 $\Omega$ | Hög reaktans = ej i resonans |

---

## 2. Parameterguide för Antenntrimning

### Resonans och Anpassning
*   **LogMag (S11):** Visar var antennen är "i resonans" (där grafen dippar som lägst). Målet är en djup dipp vid den önskade arbetsfrekvensen.
*   **VSWR:** Det vanligaste måttet. En VSWR på 1.5:1 betyder att ca 4% av effekten reflekteras. De flesta moderna sändare drar ner effekten (foldback) om VSWR går över 2.0.
*   **ReturnLoss_RL:** Ju högre värde desto bättre. 20 dB innebär att antennen är mycket väl anpassad.

### Impedansanalys (Varför stämmer den inte?)
*   **Resistance_R:** Om resistansen är 50 $\Omega$ vid dippen är antennen perfekt anpassad. 
    *   *För låg R (< 50 $\Omega$):* Antennen kan sitta för nära mark/metall eller vara för kort.
    *   *För hög R (> 50 $\Omega$):* Kan tyda på dålig jordplan eller att antennen sitter för högt.
*   **Reactance_X:** Berättar om antennen är elektriskt för lång eller för kort.
    *   **Positivt värde (+j):** Antennen är induktiv (för lång).
    *   **Negativt värde (-j):** Antennen är kapacitiv (för kort).
    *   **Mål:** X = 0 vid arbetsfrekvensen.

### Övriga parametrar
*   **PhaseDegrees:** Vid resonans passerar fasen ofta genom 0 grader. En snabb fasändring indikerar en antenn med hög Q-faktor (smalbandig).
*   **ImpedanceMagnitude ($|Z|$):** Det totala motståndet (vektorsumman av R och X). Ska vara 50 $\Omega$.
*   **ReflectedPower:** Ett mycket pedagogiskt värde för att förklara förlust. Visar i Watt eller Procent hur mycket effekt som "går till spillo" som värme i radion istället för att sändas ut.

---

## 3. Praktisk trimningsguide

### Hur flyttar jag resonansfrekvensen?
1.  **Antennen resonerar för lågt i frekvens:** Antennen är fysiskt **för lång**. Klipp eller korta av elementet.
2.  **Antennen resonerar för högt i frekvens:** Antennen är fysiskt **för kort**. Förläng elementet eller lägg till en spole (loading coil).

### Hur tolkar jag kurvan?
*   **Bred och grund dipp:** Antennen är förlåtande och fungerar över ett brett område, men kanske inte är super-effektiv. Vanligt för t.ex. gummi-antenner på handapparater.
*   **Smal och djup dipp:** Antennen är mycket effektiv på en specifik frekvens (högt Q-värde), men tål inte att man flyttar sig långt i frekvens innan VSWR sticker iväg.

### Miljöns påverkan
Antenner påverkas extremt mycket av omgivningen. En mätning inomhus vid en dator ger helt andra värden än en mätning utomhus på en mast.
*   **Handheld-effekt:** Om du håller i en handradio påverkar din kropp S11-värdet (du fungerar som motvikt/jordplan).
*   **Närfältsstörningar:** Metallföremål inom en våglängds avstånd drar ner resistansen och flyttar resonansen.


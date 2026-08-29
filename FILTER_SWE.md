# Guide: Filteranalys med S-parametrar (S11 & S21)

Denna guide beskriver hur man tolkar de olika mätvärdena vid analys av ett filter (exempelvis ett lågpassfilter) i ett 50-ohms system.

## 1. Övergripande Filterprestanda

| Område | S21 (Transmission) | S11 (Reflektion) | Status |
| :--- | :--- | :--- | :--- |
| **Passband** | -0.1 till -1.0 dB | < -20 dB | **Perfekt:** Signalen går igenom, minimal reflektion. |
| **Passband (Gränsfall)**| -1.0 till -3.0 dB | -10 till -15 dB | **Acceptabelt:** Viss dämpning och reflektion. |
| **Cutoff (f_c)** | -3.0 dB | ~ -7 dB | **Gränsen:** Här börjar filtret blockera signalen. |
| **Stopband** | < -40 dB | > -0.5 dB | **Effektivt:** Signalen spärras och reflekteras tillbaka. |

---

## 2. Parameterguide (S11 & S21)

Här följer en förklaring av alla tillgängliga grafer och deras förväntade värden för ett fungerande filter.

### Magnitud & Logaritmisk skala
*   **LogMag (S21):** Visar filtrets dämpning. 0 dB är ingen förlust. -40 dB betyder att endast 1/10000 av effekten går igenom.
*   **LogMag (S11):** Visar hur bra filtret "matchar" 50 Ohm. Ju lägre (mer negativt) värde, desto bättre.
*   **ReturnLoss_RL:** Den positiva motsvarigheten till LogMag (S11). 20 dB return loss är mycket bra; 0 dB betyder total reflektion.
*   **LinMag (S21):** Spänningsförhållandet (0 till 1.0). Vid cutoff ($f_c$) är detta värde exakt $0.707$.
*   **GammaMagnitude:** Reflektionskoefficientens magnitud (0 till 1.0). Ska vara nära 0 i passbandet och nära 1 i stopbandet.

### Impedans & Admittans (S11-baserat)
*   **ImpedanceMagnitude ($|Z|$):** Ska ligga så nära **50 $\Omega$** som möjligt i passbandet.
*   **Resistance_R:** Den reala delen av impedansen. Bör vara **50 $\Omega$** i passbandet. Sjunker ofta mot 0 i stopbandet för LC-filter.
*   **Reactance_X:** Den imaginära delen (induktans/kapacitans). Bör vara **0 $\Omega$** i passbandet för en ren resistiv matchning.
*   **Conductance_G:** Reciproka av resistansen. För 50 $\Omega$ är målet **20 mS** (milliSiemens).
*   **Susceptance_B:** Den imaginära delen av admittansen. Bör vara **0 mS** i passbandet.

### Effekt & Stående våg
*   **VSWR:** (Voltage Standing Wave Ratio). 1.0 är perfekt. Värden under 1.5 betraktas som bra matchning i ett radiosystem.
*   **ReflectedPower:** Anger hur stor del av effekten som studsar tillbaka. 
    *   -20 dB S11 = 0.01 (1% reflektion).
    *   -10 dB S11 = 0.10 (10% reflektion).

### Fas
*   **PhaseDegrees / PhaseRadians:** Visar fasförskjutningen. I passbandet ser man ofta en linjär nedåtgående trend (fasfördröjning). Plötsliga hopp i stopbandet beror oftast på brus när signalen blir för svag för att mätas noggrant.

---

## 3. Praktiska tips vid analys

1.  **Identifiera Bruset:** Om `LogMag (S21)` planar ut på t.ex. -70 dB och ser "gräsigt" ut, har du nått nätverksanalysatorns dynamiska omfång (brusgolv).
2.  **Kolla Rippel:** Om `LogMag (S21)` böljar upp och ner i passbandet tyder det på dålig design eller felaktiga komponentvärden som orsakar interna reflektioner.
3.  **Symmetri:** För de flesta passiva filter är S11 och S22 nästan identiska. Om dina mätningar visar stora skillnader kan filtret vara trasigt eller felmonterat.
4.  **Energibalans:** Om både S11 och S21 är låga samtidigt (t.ex. S11 = -20 dB och S21 = -20 dB) försvinner effekten i filtret som värme. Detta tyder på komponenter med lågt Q-värde eller hög resistans i spolarna.


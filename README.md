# ARDroneControl

## Indice

1. [Panoramica del progetto](#panoramica-del-progetto)  
2. [Requisiti](#requisiti)  
3. [Istruzioni per l'installazione](#istruzioni-per-linstallazione)  
4. [Architettura del sistema](#architettura-del-sistema)  
5. [Funzionalità principali](#funzionalità-principali)  
6. [Modalità di utilizzo](#modalità-di-utilizzo)  
7. [Limitazioni note](#limitazioni-note)  
8. [Video dimostrativi](#video-dimostrativi)  

---

## Panoramica del progetto

**ARDroneControl** è un sistema immersivo sviluppato in Unity che consente di **riconoscere e controllare uno o più droni DJI Robomaster TT** utilizzando il visore **Meta Quest 3**, sfruttando realtà aumentata e intelligenza artificiale.

Il riconoscimento dei droni avviene tramite un modello di **object detection YOLOv9**, integrato nel visore con **Unity Sentis**. Il controllo dei droni è gestito tramite **comandi UDP** inviati a un modulo **ESP8266**, che funge da ponte con i droni.

L’interazione è naturale e intuitiva: l’utente può selezionare visivamente un drone semplicemente puntandolo con il controller del visore e comandarlo attraverso un’interfaccia immersiva.

---

## Requisiti

### Software

- **Unity**: `2022.3.58f1 LTS`
- **Meta XR All-in-One SDK**: `76.0.1` (Unity Asset Store)
- **Oculus XR Plugin**: `4.5.0`
- **Unity Sentis**: `2.1.1`
- **Input System**: `1.1.2`

> ⚠️ Assicurati di abilitare XR Plugin Management e impostare Android come piattaforma di build.

### Hardware

- Visore **Meta Quest 3**
- Uno o più droni **DJI Robomaster TT**
- Modulo **ESP8266** configurato come Access Point
- Connessione Wi-Fi diretta tra visore e ESP8266 (offline)

---

## Istruzioni per l'installazione

1. Clona il progetto da GitHub:
   ```bash
   git clone https://github.com/RaffCurcio/ARDroneControl.git
   ```
2. Apri il progetto in **Unity 2022.3.58f1**.

3. Vai su `File > Build Settings`, seleziona **Android**, quindi clicca su **Switch Platform**.

4. Installa i seguenti pacchetti dal **Package Manager**:
   - Meta XR All-in-One SDK
   - Unity Sentis
   - Oculus XR Plugin
   - Input System

5. Collega il visore **Meta Quest 3** alla rete Wi-Fi creata dall’**ESP8266**.

6. Accendi i droni DJI e assicurati che si connettano all’**ESP8266**.

7. Costruisci e installa l'app tramite **Build & Run** o **ADB**.

---

## Architettura del sistema

- **YOLOv9-c (ONNX, float16)**: modello leggero addestrato per riconoscere i droni DJI Robomaster TT.  
- **Unity Sentis**: inferenza del modello direttamente sul visore, in locale.  
- **WebCamTexture**: cattura il video passthrough dalla fotocamera del Quest 3.  
- **ESP8266**: ponte Wi-Fi per inoltrare comandi UDP ai droni.  
- **Protocollo UDP**: usato per trasmettere comandi come `takeoff`, `land`, `rc 0 50 0 0`, ecc.

---

## Funzionalità principali

- Rilevamento dei droni in tempo reale tramite YOLOv9  
- Interfaccia immersiva con selezione visiva dei droni  
- Controllo singolo o simultaneo di più droni  
- Comunicazione locale tramite UDP, senza necessità di connessione internet  
- Interazione naturale tramite i controller del visore VR  

---

## Modalità di utilizzo

| Azione             | Comando del controller        |
|--------------------|-------------------------------|
| Decollo            | Pulsante **A**                |
| Atterraggio        | Pulsante **B**                |
| Movimento X/Y      | **Stick destro**              |
| Altezza / Rotazione| **Stick sinistro**            |
| Selezione drone    | Puntare e premere **grilletto** |

> 🔁 Il sistema esegue un’inferenza ogni 3 secondi per bilanciare precisione e prestazioni.

---

## Limitazioni note

- Rilevamento efficace solo entro ~2 metri e con buona illuminazione  
- Il drone deve essere visibile e fermo per poter essere selezionato  
- Il modello non segue oggetti in movimento continuo  
- Il visore può avvicinarsi ai limiti computazionali se si aumenta la frequenza di inferenza  
- Test eseguiti solo in ambienti indoor  

---

## Video dimostrativi

*(Inserisci i link ai video dimostrativi quando disponibili)*

- ▶️ **Selezione e decollo**  
![Video dimostrativo](Media/sing.gif)

- ▶️ **Controllo simultaneo**  
![Video dimostrativo](Media/cont.gif)
---



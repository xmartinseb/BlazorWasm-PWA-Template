# Blazor PWA šablona klient-server 
🚧 WORK IN PROGRESS

Jedná se o šablonu pro webové projekty Blazor WASM s tradiční architekturou klient-server:
- Klientská část využívá WASM a Javascript runtime, obsahuje example pages s využitím chytrých stavových komponent, caches apod.
- Serverová část poskytuje klienta v rámci index.html, obsahuje controllery a serverovou logiku

Podobná šablona byla poskytována přímove Visual Studio, ale ve verzi .NET 8 byla odebrána. Tato šablona ji nahrazuje a poskytuje spoustu návrhových vzorů navíc.

## Caches
Jedna z výhod Blazor WASM aplikace je, že přirozeně využívá browser jako plnohodnotné prostředí se vším všudy. Blazor umožňuje s
těmito úložišti manipulovat přes Javascript runtime, což je z pohledu aplikace dobrý základ, ale příliš low level. Definujeme proto několik typů
cache tříd:

### LocalStorage
- Local storage je stringové úložiště typu klíč-hodnota přímo v browseru.
- Je sdílené mezi všemi záložkami téhož webu, nesmaže se při zavření prohlížeče a je tak permanentní
- Vhodné na uživatelské preference a jiná dlouhodobá data
- Třída je postavena přímo nad JS Runtime a definuje jen GetAsync/SetAsync, jinak žádnou další logiku 

### SessionStorage
- Session storage je také stringové úložiště typu klíč-hodnota přímo v browseru.
- Patří pouze jedné záložce, není sdíleno. Při zavření záložky zaniká
- Vhodné na cachování krátkodobých dat, např. stav formuláře, načtená čísla apod.
- Třída je postavena přímo nad JS Runtime a definuje jen GetAsync/SetAsync, jinak žádnou další logiku
- Mohlo by se zdát, že session storage lze nahradit prostou in-memory cache přímo v C# bez vazby na Javascript.
  - Bylo by to jednodušší a výkonnější
  - Ale session storage má další zásadní výhody: 
    - díky vazbě na záložku přežije refresh F5
    - díky vazbě na záložku přežije přesměrování na externí stránku (např. platební bránu) a zpět
    - díky integraci do browseru se dá zobrazit a kontrolovat v consoli

V každém případě nepatří do klienských úlošišť nezašifrované citlivé informace! Ty nesmí opustit server!

### BrowserTtlCache\<TStorage>
- Na rozdíl od výše popsaných úložišť nedělá pouze Get a Set, ale nahlíží na uložené hodnoty jako na záznamy cache. 
- Ukládá si metadata: čas uložení a TTL
- Při čtení dat zohledňuje TTL
- O level výše: není postaven přímo nad JS Runtime, ale využívá LocalStorage nebo SessionStorage (dle generického parametru)
  - Je-li použito LocalStorage, je to cache sdílená mezi záložkami téhož webu a přežije restart prohlížeče
  - Je-li použito SessionStorage, je to cache jen pro jednu záložku, která zanikne zavřením prohlížeče

## Komponenty a data
### StateSwitch
- Přepínač stavů (loading, loaded (bool isReloading), error)
- Dle stavu zobrazuje různý obsah. Žádná další logika

### ComponentWithGuiState\<TDataLoaded>
- Základní typ pro komponentu, která potřebuje přepínat stavy (obsahuje GuiState). Obsahuje jednotný kód k přepínání stavů
- Definuje loading message a proceduru k načtení dat
- Metoda LoadAndSetGuiStateAsync přepíná stavy a načítá data (volá načítací proceduru)

### DataComponent\<TData>
- Obsahuje StateSwitch a dědí z ComponentWithGuiState, umí tak přepínat stavy a vizuálně to zobrazovat
- Může volitelně obsahovat Timer, který periodicky načítá nová data
- Může volitelně využívat cache s TTL (postavena nad browser storage)
- Obvykle není třeba manuálně definovat cache key. Defaultně se použije FullName typu TData
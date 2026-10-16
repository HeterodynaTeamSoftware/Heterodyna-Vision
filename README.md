# Heterodyna Vision

**Heterodyna Vision** to desktopowy program dla Windows do obsługi kamer CCTV/IP, kamer USB i rejestratorów. Łączy wykrywanie urządzeń w sieci, konfigurację RTSP i ONVIF, podgląd wielu kamer, nagrywanie oraz podstawową detekcję ruchu w jednym programie.

Aktualna wersja kodu w repozytorium: **v01.001.0059**.

Program powstał z myślą o sytuacji, w której w jednej sieci działa kilka różnych kamer i rejestratorów, często różnych producentów. Zamiast osobno szukać adresów RTSP, sprawdzać porty, testować ONVIF i uruchamiać kolejne narzędzia, większość tych rzeczy można zrobić bezpośrednio z Heterodyna Vision.

## Najważniejsze funkcje

### Obsługa kamer sieciowych i rejestratorów

Program pozwala dodawać kamery ręcznie albo wyszukiwać je w sieci lokalnej. Dla każdej kamery można zapisać m.in. nazwę, lokalizację, rejestrator, adres IP lub host, port, dane logowania, format źródła, producenta i adres strumienia.

Obsługiwane źródła i formaty:

- RTSP,
- ONVIF,
- HTTP Snapshot,
- MJPEG,
- RTMP,
- HLS,
- HTTP/MP4,
- USB / DirectShow,
- tryb AUTO.

### Skanowanie sieci

Heterodyna Vision potrafi przeskanować wskazaną podsieć i sprawdzić urządzenia na wybranych portach. Można podać pojedynczy adres, prefiks podsieci albo zakres w zapisie CIDR, np. `192.168.2.0/24`.

Program próbuje również rozpoznać urządzenia ONVIF w sieci lokalnej za pomocą WS-Discovery.

### Automatyczne wykrywanie protokołu

Tryb **Auto prot.** analizuje podany adres i dostępne porty. Jeżeli wykryje RTSP albo HTTP, podpowiada najbardziej prawdopodobny sposób połączenia. Dla RTSP można dodatkowo uruchomić test gotowych szablonów adresów.

### RTSP auto

Program ma zestaw popularnych ścieżek RTSP i może kolejno je sprawdzać. Ułatwia to uruchomienie kamer, dla których znamy adres IP i dane logowania, ale nie znamy dokładnej ścieżki strumienia.

Wbudowane profile obejmują m.in.:

- Generic,
- Hikvision / HiWatch / OEM,
- Dahua / BCS / Imou / OEM,
- Xiongmai / XM,
- Reolink,
- TP-Link Tapo / VIGI,
- Axis,
- Foscam,
- Samsung Wisenet / Hanwha.

Lista profili jest zapisywana w `profiles/vendors.json` i może być dalej rozwijana.

### ONVIF

Obsługa ONVIF obejmuje:

- wyszukiwanie urządzeń,
- pobieranie profili multimedialnych,
- wybór profilu main/sub,
- pobieranie URI strumienia,
- obsługę `PasswordDigest` i `PasswordText`,
- zapis wybranego tokenu i nazwy profilu w konfiguracji kamery.

### Kamery USB

Kamery USB są obsługiwane przez DirectShow. Program może:

- wyświetlić dostępne urządzenia wideo,
- odczytać obsługiwane rozdzielczości i FPS,
- wykonać testową klatkę,
- dodać kamerę USB do zwykłej listy kamer.

### Widok wielu kamer

Panel główny może pracować w kilku układach:

- 1 kamera,
- 4 kamery,
- 8 kamer,
- 16 kamer,
- 32 kamery,
- 64 kamery.

Kamery można przypisywać do slotów, zapisywać układ i uruchamiać wszystkie przypisane źródła jednocześnie.

### Detekcja ruchu i alarm

Na aktywnych podglądach można włączyć detekcję ruchu. Program porównuje kolejne klatki i po przekroczeniu progu zapisuje zdarzenie do lokalnego logu.

Dostępne są również:

- alarm dźwiękowy,
- zapis zdarzeń ruchu,
- nagrywanie uruchamiane po wykryciu ruchu,
- automatyczne zatrzymanie nagrywania po okresie bez kolejnego ruchu.

Nagrania trafiają do katalogu `data/recordings/` z podziałem na kamerę i dzień.

### Nagrywanie ręczne

Wybraną kamerę można nagrywać ręcznie do pliku MP4. Program wykorzystuje FFmpeg i dla źródeł, które na to pozwalają, zapisuje strumień bez ponownego kodowania (`-c copy`).

### Historia nagrań

Wbudowane okno historii pozwala:

- przeglądać zapisane nagrania,
- otworzyć wybrany plik,
- otworzyć katalog z nagraniami,
- otworzyć log ruchu,
- usuwać stare nagrania według wieku.

### Import i eksport konfiguracji

Listę kamer można zapisać do pliku JSON i później ponownie zaimportować. Jest to wygodne przy przenoszeniu konfiguracji między komputerami albo przed większymi zmianami w ustawieniach.

**Uwaga:** eksport konfiguracji może zawierać dane logowania do kamer. Takiego pliku nie należy publikować w repozytorium ani udostępniać osobom trzecim.

### Konta użytkowników

Przy pierwszym uruchomieniu program pozwala utworzyć lokalne konto administratora. Dane użytkowników są przechowywane lokalnie w katalogu `data/`.

Repozytorium celowo **nie zawiera** plików `data/users.json`, konfiguracji kamer, logów ani nagrań z działającej instalacji.

## FFmpeg i ffprobe

Część funkcji wideo wymaga `ffmpeg.exe` i `ffprobe.exe`. Dotyczy to m.in. podglądu klatek, testów strumieni, kamer USB oraz nagrywania.

Program potrafi znaleźć FFmpeg:

- w folderze wskazanym w oknie **Biblioteki**,
- obok programu,
- w katalogu `bin`,
- w `tools/ffmpeg`,
- w `PATH` systemu Windows.

Jeżeli FFmpeg nie zostanie znaleziony, program pokaże diagnostykę ścieżek zamiast udawać, że funkcja działa.

## Wymagania do uruchomienia gotowej wersji

- Windows 10 lub Windows 11,
- procesor/system x64,
- `ffmpeg.exe` i `ffprobe.exe` dla funkcji korzystających z FFmpeg.

Gotowy build Windows jest publikowany osobno jako plik:

`HeterodynaVision_win-x64.zip`

Binarnego wydania nie trzymamy w historii Git. Repozytorium służy do publikacji kodu, a gotowe paczki trafiają do **GitHub Releases**.

## Kompilacja ze źródeł

Do kompilacji potrzebny jest **.NET 8 SDK** na Windows.

```powershell
cd src\HeterodynaVision
dotnet restore
dotnet build HeterodynaVision.csproj -c Release
```

Publikacja wersji self-contained dla Windows x64:

```powershell
dotnet publish HeterodynaVision.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=false `
  -o .\publish\win-x64
```

Po publikacji pliki aplikacji znajdą się w `publish\win-x64`.

## Struktura kodu

```text
src/HeterodynaVision/
├── Discovery/   # skanowanie sieci i WS-Discovery / ONVIF
├── Models/      # modele kamer, użytkowników i slotów
├── Properties/  # informacje o wersji i assembly
├── Security/    # lokalne logowanie użytkowników
├── Services/    # RTSP, ONVIF, FFmpeg, nagrywanie, USB, ruch
├── Storage/     # konfiguracja kamer, slotów i ścieżek narzędzi
├── UI/          # interfejs Windows Forms
├── Vendors/     # profile producentów i szablony RTSP
├── Program.cs
└── HeterodynaVision.csproj
```

## O źródłach w tym repozytorium

Kod tej wersji został odzyskany z oficjalnego builda **v01.001.0059** oraz dołączonego pliku PDB. Odtworzenie wykonano z własnej kompilacji projektu przy użyciu dekompilatora .NET.

Zachowane zostały klasy, metody, przestrzenie nazw oraz nazwy lokalnych zmiennych dostępne w PDB. Nie da się natomiast odzyskać 1:1 komentarzy, pierwotnego formatowania ani części informacji, które kompilator usuwa podczas budowania programu.

Przed publikacją usunięto wyłącznie dane runtime z paczki programu, w szczególności lokalne konto użytkownika i pliki, które mogłyby zawierać dane dostępowe. Logika aplikacji nie została w tym procesie celowo zmieniona.

## Dane lokalne i prywatność

Katalog `data/` może zawierać:

- konta użytkowników,
- hashe i sole haseł,
- listę kamer,
- loginy i hasła kamer,
- zapisany układ slotów,
- ścieżkę do FFmpeg,
- log detekcji ruchu,
- nagrania.

Dlatego `data/` jest wyłączony z repozytorium przez `.gitignore`.

## Wersja

**Heterodyna Vision v01.001.0059**

## Autor

**Heterodyna Team Software**  
https://heterodynateamsoftware.pl

## Licencja

Kod jest publikowany na licencji **GNU General Public License v3.0 only**.

`GPL-3.0-only`

Pełny tekst licencji znajduje się w pliku [`LICENSE`](LICENSE).

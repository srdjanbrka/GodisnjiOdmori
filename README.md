# Evidencija i odobravanje zahteva za godišnji odmor

Seminarski projekat iz predmeta **Razvoj višeslojnog softvera** za školsku 2025/26. godinu.

Aplikacija omogućava zaposlenom da podnese zahtev za godišnji odmor, dok kadrovska služba pregleda i obrađuje zahteve. Prilikom odobravanja sistem preko REST servisa učitava dozvoljeni broj istovremeno odsutnih zaposlenih iz istog sektora i automatski primenjuje poslovno pravilo.

## Funkcionalnosti

- prijava u sistem kao zaposleni ili kadrovska služba;
- prikaz podataka u skladu sa ulogom korisnika;
- podnošenje, izmena i brisanje zahteva zaposlenog;
- pretraga zahteva po zaposlenom, sektoru ili statusu;
- pregled i štampanje pojedinačnog zahteva;
- odobravanje ili odbijanje zahteva od strane kadrovske službe;
- automatsko postavljanje zahteva na čekanje kada je prekoračen limit odsutnih;
- provera ispravnosti perioda godišnjeg odmora;
- automatsko računanje radnih dana od ponedeljka do petka;
- zaštita od neovlašćenog pristupa i istovremenih izmena istog zahteva.

## Korisničke uloge

| Uloga | Mogućnosti |
|---|---|
| Zaposleni | Vidi samo svoje zahteve, podnosi novi zahtev, menja ili briše zahtev koji nije odobren i štampa detalje zahteva. |
| Kadrovska služba | Vidi zahteve svih zaposlenih, pretražuje evidenciju, pregleda zaposlene i odobrava ili odbija zahteve. |

## Poslovno pravilo

> Ako bi odobravanjem zahteva broj odsutnih zaposlenih iz istog sektora u izabranom periodu bio veći od dozvoljenog broja **X**, onda se zahtev automatski postavlja u status **„Na čekanju“**.

Vrednost **X** nalazi se u eksternom XML fajlu [`3_RESTServis/App_Data/Parametri.xml`](3_RESTServis/App_Data/Parametri.xml) i aplikacija je učitava preko REST servisa. Početna vrednost parametra je `2`.

Pravilo se proverava za svaki radni dan traženog perioda. Ako bi broj odsutnih makar jednog dana bio veći od vrednosti X, zahtev se ne odobrava, već dobija status **„Na čekanju“**.

## Arhitektura aplikacije

Rešenje je organizovano u četiri sloja:

| Projekat | Uloga |
|---|---|
| `1_SlojPodataka` | Modeli, pristup SQL Server bazi i izvršavanje upita. |
| `2_PoslovnaLogika` | Validacija perioda, računanje radnih dana i primena poslovnog pravila. |
| `3_RESTServis` | Web API koji čita parametar X iz XML fajla. |
| `4_MVC` | ASP.NET MVC korisnički interfejs, prijava i autorizacija. |
| `Testovi` | Konzolne provere poslovne logike. |

Tok odobravanja zahteva:

1. Kadrovska služba bira opciju **Proveri i odobri**.
2. Poslovna logika poziva REST adresu `/api/parametri`.
3. REST servis vraća vrednost X iz XML fajla.
4. Sistem proverava odobrena odsustva u istom sektoru za svaki traženi dan.
5. Zahtev dobija status **Odobren** ili **Na čekanju**.

## Korišćene tehnologije

- C# i .NET Framework 4.8;
- ASP.NET MVC 5;
- ASP.NET Web API;
- Microsoft SQL Server;
- ADO.NET;
- XML;
- Razor, HTML i CSS;
- Forms Authentication i autorizacija na osnovu uloga.

## Preduslovi

Za pokretanje su potrebni:

- Visual Studio 2022;
- workload **ASP.NET and web development**;
- **.NET Framework 4.8 SDK** i **.NET Framework 4.8 targeting pack**;
- SQL Server 2022 ili kompatibilna verzija;
- SQL Server Management Studio.

## Pokretanje projekta

### 1. Kreiranje baze

1. Otvoriti SQL Server Management Studio.
2. Povezati se na lokalni SQL Server.
3. Otvoriti [`Baza/01_Baza.sql`](Baza/01_Baza.sql).
4. Izvršiti celu skriptu komandom **Execute**.

Skripta kreira bazu `GodisnjiOdmori`, potrebne tabele, indekse, proceduru i početne podatke. Ako baza već postoji, skripta je neće prepisati.

Ako se SQL Server ne nalazi na adresi `localhost`, u fajlu [`4_MVC/Web.config`](4_MVC/Web.config) treba promeniti `Data Source` u connection stringu `Odmori`.

### 2. Otvaranje i prevođenje rešenja

1. Otvoriti [`GodisnjiOdmori.sln`](GodisnjiOdmori.sln) u Visual Studio-u.
2. Desnim klikom na solution izabrati **Restore NuGet Packages**.
3. Otvoriti **Build > Configuration Manager** i proveriti da je uključena opcija **Build** za sve projekte.
4. Izabrati **Build > Rebuild Solution**.

Uspešan rezultat je `5 succeeded, 0 failed`.

### 3. Podešavanje projekata za pokretanje

1. Desni klik na solution i izabrati **Configure Startup Projects**.
2. Izabrati **Multiple startup projects**.
3. Za projekte `Servis` i `MVC` postaviti **Action = Start**.
4. Za projekte `Podaci`, `Logika` i `Testovi` ostaviti **Action = None**.
5. Pokrenuti aplikaciju tasterom **F5**.

Adrese aplikacije:

- MVC aplikacija: [http://localhost:5100/](http://localhost:5100/)
- REST parametar: [http://localhost:5101/api/parametri](http://localhost:5101/api/parametri)

Otvaranje samo `http://localhost:5101/` može prikazati IIS grešku `403.14`, jer REST projekat nema početnu HTML stranicu. To ne predstavlja problem ako adresa `/api/parametri` vraća XML podatak.

## Demo nalozi

| Uloga | Korisničko ime | Lozinka |
|---|---|---|
| Zaposleni | `zaposleni` | `ZaposleniDemo2026!` |
| Kadrovska služba | `kadrovska` | `OdmorDemo2026!` |

Demo nalog zaposlenog povezan je sa zaposlenom **Anom Petrović** (`ZaposleniID = 1`). Nalozi služe isključivo za lokalnu demonstraciju projekta.

## Validacija perioda

Prilikom podnošenja ili izmene zahteva primenjuju se sledeće provere:

- datum početka ne može biti pre današnjeg datuma;
- datum završetka ne može biti pre datuma početka;
- period mora sadržati najmanje jedan radni dan;
- period može trajati najviše 366 dana;
- dozvoljene su godine od 2000. do 2100.

Radni dani se računaju od ponedeljka do petka. Državni praznici nisu obuhvaćeni ovim prototipom.

## Demonstracija poslovnog pravila

Za brzu demonstraciju ograničenja X:

1. Pokrenuti [`Baza/02_DemoLimit.sql`](Baza/02_DemoLimit.sql) u SSMS-u.
2. Skripta dodaje dva odobrena odsustva zaposlenih iz sektora **Razvoj softvera** za narednu radnu nedelju.
3. U rezultatu skripte pročitati početak i kraj demo perioda, zatim se prijaviti kao zaposleni i podneti zahtev za isti period.
4. Prijaviti se kao kadrovska služba i izabrati **Proveri i odobri**.
5. Pošto je `X = 2`, novi zahtev se automatski postavlja u status **Na čekanju**.

Skripta je bezbedna za ponovno pokretanje jer proverava da li demo redovi sa oznakom `DEMO_LIMIT_X` već postoje i neće ih dodati ponovo.

## Testiranje poslovne logike

Projekat `Testovi` sadrži 14 konzolnih provera za:

- granične vrednosti parametra X;
- brojanje odsutnih zaposlenih po danu;
- izbegavanje dvostrukog brojanja zaposlenog;
- računanje radnih dana;
- obrnut period, datum u prošlosti i period koji sadrži samo vikend;
- neispravnu vrednost parametra X.

Za pokretanje testova privremeno postaviti projekat `Testovi` kao **Startup Project** i pokrenuti ga bez debagovanja. Očekivani završni ispis je:

```text
Ukupno uspesnih provera: 14
```

## Dokumentacija

Kompletan seminarski rad dostupan je u Word formatu:

- [`Dokumentacija/Seminarski_rad_Godisnji_odmori.docx`](Dokumentacija/Seminarski_rad_Godisnji_odmori.docx)

## Struktura repozitorijuma

```text
GodisnjiOdmori/
├── 1_SlojPodataka/
├── 2_PoslovnaLogika/
├── 3_RESTServis/
│   └── App_Data/Parametri.xml
├── 4_MVC/
├── Baza/
│   ├── 01_Baza.sql
│   └── 02_DemoLimit.sql
├── Dokumentacija/
│   └── Seminarski_rad_Godisnji_odmori.docx
├── Testovi/
├── .gitignore
├── GodisnjiOdmori.sln
└── README.md
```

## Napomene

- Projekat je namenjen lokalnom akademskom demonstriranju.
- Autentikacija koristi dva demonstraciona naloga definisana u konfiguraciji aplikacije.
- Parametar X menja se u XML fajlu bez izmene i ponovnog prevođenja poslovne logike.
- REST servis mora biti pokrenut da bi odobravanje zahteva moglo da primeni poslovno pravilo.

## Autor

**Srđan Nedić**  
Školska godina 2025/26.

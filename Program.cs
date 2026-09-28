using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SD_gokart
{
    // A versenyzők/ügyfelek adatait tároló osztály
    class Szemely
    {
        // A versenyző személyes adatai (csak olvasható tulajdonságok)
        public string Vezeteknev { get; }
        public string Keresztnev { get; }
        public DateTime SzuletesiIdo { get; }
        public bool Elmulte18 { get; }
        public string VersenyzoAzonosito { get; }
        public string EmailCim { get; }

        // Konstruktor egy személy adatainak beállítására és a származtatott mezők kiszámítására
        public Szemely(string vezeteknev, string keresztnev, DateTime szuletesiIdo)
        {
            Vezeteknev = vezeteknev;
            Keresztnev = keresztnev;
            SzuletesiIdo = szuletesiIdo;

            // Elmúlt-e 18 éves az aktuális dátumhoz képest
            Elmulte18 = KiszamoltKor() >= 18;

            // Ékezetmentesített nevek előállítása a kért azonosító és email formátumhoz
            string v = EkezetMentesit(vezeteknev);
            string k = EkezetMentesit(keresztnev);

            // Egyedi versenyző-azonosító (pl. GO-KovacsDenes-19741204)
            VersenyzoAzonosito = $"GO-{v}{k}-{szuletesiIdo:yyyyMMdd}";

            // Generált email cím (pl. kovacs.denes@gmail.com)
            EmailCim = $"{v.ToLower()}.{k.ToLower()}@gmail.com";
        }

        // Kiszámítja a személy pontos korát a mai naphoz képest
        private int KiszamoltKor()
        {
            int kor = DateTime.Today.Year - SzuletesiIdo.Year;
            // Ha idén még nem volt születésnapja, 1 évvel kevesebb
            if (SzuletesiIdo.AddYears(kor) > DateTime.Today) kor--;
            return kor;
        }

        // Kicseréli az ékezetes karaktereket ékezetmentes megfelelőikre
        private static string EkezetMentesit(string text)
        {
            string[] ekezetes = { "á", "é", "í", "ó", "ö", "ő", "ú", "ü", "ű", "Á", "É", "Í", "Ó", "Ö", "Ő", "Ú", "Ü", "Ű" };
            string[] mentes = { "a", "e", "i", "o", "o", "o", "u", "u", "u", "A", "E", "I", "O", "O", "O", "U", "U", "U" };

            for (int i = 0; i < ekezetes.Length; i++)
                text = text.Replace(ekezetes[i], mentes[i]);

            return text;
        }
    }

    // Egy adott pályabérlést (foglalást) képviselő osztály
    class Foglalas
    {
        public List<Szemely> Versenyzok { get; } // A foglaláshoz tartozó személyek listája
        public DateTime Datum { get; }          // A foglalás napja
        public int KezdoOra { get; }            // Kezdő óra (pl. 8 = 8:00)
        public int Idotartam { get; }           // Foglalás időtartama (1 vagy 2 óra)

        public Foglalas(List<Szemely> versenyzok, DateTime datum, int kezdoOra, int idotartam)
        {
            Versenyzok = versenyzok;
            Datum = datum;
            KezdoOra = kezdoOra;
            Idotartam = idotartam;
        }
    }

    internal class Program
    {
        static void Main()
        {
            Random random = new Random();
            bool fut = true;

            // 1. Fejléc és a gokarthelyszín adatainak kiírása
            Console.WriteLine("=== SD Gokartpálya ===");
            Console.WriteLine("Cím: 6065 Lakitelek, Csokonai utca 6. | Tel: +36-70-725-7247 | Web: SD-gokart.hu\n");

            // 2. Nevek beolvasása a megadott fájlokból
            string[] vezeteknevek = BeolvasNeveket("vezeteknevek.txt");
            string[] keresztnevek = BeolvasNeveket("keresztnevek.txt");

            // 3. Véletlenszerű versenyzők generálása (10–150 fő)
            List<Szemely> szemelyek = GeneraljSzemelyeket(random, vezeteknevek, keresztnevek);
            Console.WriteLine($"Generált versenyzők száma: {szemelyek.Count}\n");

            // 4. 5 db kezdő alapfoglalás legyártása a hónap hátralévő napjaira
            List<Foglalas> foglalasok = GeneraljFoglalasokat(random, szemelyek, 5);

            // 5. Az 5 alapfoglalás részleteinek megjelenítése
            Console.WriteLine("=== 5 ALAP FOGLALÁS ===");
            foreach (var foglalas in foglalasok)
                MegjelenitFoglalas(foglalas);

            Console.WriteLine();
            Console.WriteLine("=== HAVI FOGLALÁSI TÁBLÁZAT ===");
            KiirTablazat(foglalasok);

            Console.WriteLine();

            // 6. Főmenü ciklus
            while (fut)
            {
                Console.WriteLine("Válasszon egy műveletet:");
                Console.WriteLine();
                Console.WriteLine("1 - Foglalások listázása (táblázat)");
                Console.WriteLine("2 - Új foglalás");
                Console.WriteLine("3 - Foglalás módosítása");
                Console.WriteLine("4 - Foglalás törlése");
                Console.WriteLine("5 - Kilépés");

                string bemenet = Console.ReadLine();
                int valasz;
                bool sikerult = int.TryParse(bemenet, out valasz);

                if (!sikerult)
                {
                    Console.WriteLine("Hibás választás!");
                }
                else if (valasz == 1)
                {
                    KiirTablazat(foglalasok);
                }
                else if (valasz == 2)
                {
                    UjFoglalas(foglalasok, szemelyek);
                }
                else if (valasz == 3)
                {
                    FoglalasModositasa(foglalasok);
                }
                else if (valasz == 4)
                {
                    FoglalasTorlese(foglalasok);
                }
                else if (valasz == 5)
                {
                    Console.WriteLine("Viszontlátásra!");
                    fut = false;
                }
                else
                {
                    Console.WriteLine("Hibás választás!");
                }
            }
        }

        // Módosítja egy létező foglalás dátumát és időpontját a sorszáma alapján
        static void FoglalasModositasa(List<Foglalas> foglalasok)
        {
            Console.WriteLine();
            Console.WriteLine("=== FOGLALÁS MÓDOSÍTÁSA ===");

            if (foglalasok.Count == 0)
            {
                Console.WriteLine("Nincs módosítható foglalás!");
                return;
            }

            // Meglévő foglalások felsorolása sorszámmal
            for (int i = 0; i < foglalasok.Count; i++)
            {
                Console.WriteLine($"{i + 1}. foglalás -> {foglalasok[i].Datum:yyyy.MM.dd} | {foglalasok[i].KezdoOra}:00 - {foglalasok[i].KezdoOra + foglalasok[i].Idotartam}:00 | {foglalasok[i].Versenyzok.Count} fő");
            }

            Console.Write("Válassza ki a módosítani kívánt foglalás sorszámát: ");
            int sorszam;
            if (!int.TryParse(Console.ReadLine(), out sorszam) || sorszam < 1 || sorszam > foglalasok.Count)
            {
                Console.WriteLine("Hibás sorszám!\n");
                return;
            }

            Foglalas kivalasztott = foglalasok[sorszam - 1];

            // Új dátum bekérése és ellenőrzése
            DateTime ujDatum;
            Console.Write("Új dátum (éééé.hh.nn): ");
            while (!DateTime.TryParse(Console.ReadLine(), out ujDatum))
                Console.Write("Hibás dátum, próbálja újra (éééé.hh.nn): ");

            // Új kezdő óra bekérése (nyitvatartás: 8:00 - 19:00)
            int ujKezdoOra;
            Console.Write("Új kezdő óra (8-18): ");
            while (!int.TryParse(Console.ReadLine(), out ujKezdoOra) || ujKezdoOra < 8 || ujKezdoOra > 18)
                Console.Write("Hibás óra, próbálja újra (8-18): ");

            // Új időtartam bekérése (1 vagy 2 óra)
            int ujIdotartam;
            Console.Write("Új időtartam (1 vagy 2 óra): ");
            while (!int.TryParse(Console.ReadLine(), out ujIdotartam) || (ujIdotartam != 1 && ujIdotartam != 2) || ujKezdoOra + ujIdotartam > 19)
                Console.Write("Hibás időtartam, próbálja újra (1 vagy 2): ");

            // Foglalás felülírása az új adatokkal, a meglévő személyi lista megtartásával
            foglalasok[sorszam - 1] = new Foglalas(kivalasztott.Versenyzok, ujDatum, ujKezdoOra, ujIdotartam);

            Console.WriteLine("\nFoglalás sikeresen módosítva!\n");
        }

        // Töröl egy meglévő foglalást a sorszáma alapján
        static void FoglalasTorlese(List<Foglalas> foglalasok)
        {
            Console.WriteLine();
            Console.WriteLine("=== FOGLALÁS TÖRLÉSE ===");

            if (foglalasok.Count == 0)
            {
                Console.WriteLine("Nincs törölhető foglalás!");
                return;
            }

            // Meglévő foglalások kilistázása
            for (int i = 0; i < foglalasok.Count; i++)
            {
                Console.WriteLine($"{i + 1}. foglalás -> {foglalasok[i].Datum:yyyy.MM.dd} | {foglalasok[i].KezdoOra}:00 - {foglalasok[i].KezdoOra + foglalasok[i].Idotartam}:00 | {foglalasok[i].Versenyzok.Count} fő");
            }

            Console.Write("Adja meg a törlendő foglalás sorszámát: ");
            int sorszam;
            if (int.TryParse(Console.ReadLine(), out sorszam) && sorszam >= 1 && sorszam <= foglalasok.Count)
            {
                foglalasok.RemoveAt(sorszam - 1);
                Console.WriteLine("\nFoglalás sikeresen törölve!\n");
            }
            else
            {
                Console.WriteLine("\nHibás sorszám!\n");
            }
        }

        // Új manuális foglalás felvétele a felhasználótól bekért adatok alapján
        static void UjFoglalas(List<Foglalas> foglalasok, List<Szemely> szemelyek)
        {
            const int maxFerohely = 20;

            Console.WriteLine();
            Console.WriteLine("=== ÚJ FOGLALÁS ===");

            // Elérhető személyek kilistázása az azonosítójukkal
            Console.WriteLine("Elérhető versenyzők:");
            foreach (Szemely szemely in szemelyek)
                Console.WriteLine($"  {szemely.VersenyzoAzonosito}  -  {szemely.Vezeteknev} {szemely.Keresztnev}");
            Console.WriteLine();

            // Létszám bekérése (5-20 fő)
            int letszam;
            Console.Write("Hány fő szeretne foglalni (5-20)? ");
            while (!int.TryParse(Console.ReadLine(), out letszam) || letszam < 5 || letszam > 20)
                Console.Write("Hibás létszám, próbálja újra (5-20): ");

            // A résztvevők kiválasztása azonosító alapján
            List<Szemely> versenyzok = new List<Szemely>();
            for (int i = 0; i < letszam; i++)
            {
                Szemely kivalasztott = null;

                while (kivalasztott == null)
                {
                    Console.Write($"{i + 1}. résztvevő azonosítója: ");
                    string azonosito = Console.ReadLine();

                    kivalasztott = szemelyek.FirstOrDefault(sz => sz.VersenyzoAzonosito == azonosito);

                    if (kivalasztott == null)
                        Console.WriteLine("Nincs ilyen azonosító, próbálja újra!");
                    else if (versenyzok.Contains(kivalasztott))
                    {
                        Console.WriteLine("Ez a versenyző már szerepel a foglalásban, válasszon másikat!");
                        kivalasztott = null;
                    }
                }

                versenyzok.Add(kivalasztott);
            }

            // Dátum bekérése
            DateTime datum;
            Console.Write("Dátum (éééé.hh.nn): ");
            while (!DateTime.TryParse(Console.ReadLine(), out datum))
                Console.Write("Hibás dátum, próbálja újra (éééé.hh.nn): ");

            // Kezdő óra bekérése
            int kezdoOra;
            Console.Write("Kezdő óra (8-18): ");
            while (!int.TryParse(Console.ReadLine(), out kezdoOra) || kezdoOra < 8 || kezdoOra > 18)
                Console.Write("Hibás óra, próbálja újra (8-18): ");

            // Időtartam bekérése
            int idotartam;
            Console.Write("Időtartam órában (1 vagy 2): ");
            while (!int.TryParse(Console.ReadLine(), out idotartam)
                || (idotartam != 1 && idotartam != 2)
                || kezdoOra + idotartam > 19)
                Console.Write("Hibás időtartam, próbálja újra (1 vagy 2): ");

            // Szabad hely ellenőrzése az érintett idősáv(ok)ban
            int szabadHely = maxFerohely;
            for (int ora = kezdoOra; ora < kezdoOra + idotartam; ora++)
            {
                int mostFoglalt = foglalasok
                    .Where(f =>
                        f.Datum.Date == datum.Date &&
                        ora >= f.KezdoOra &&
                        ora < f.KezdoOra + f.Idotartam)
                    .Sum(f => f.Versenyzok.Count);

                szabadHely = Math.Min(szabadHely, maxFerohely - mostFoglalt);
            }

            // Ha nincs elegendő szabad hely, elutasítjuk a foglalást
            if (letszam > szabadHely)
            {
                Console.WriteLine($"Sajnos ebben az időpontban csak {Math.Max(szabadHely, 0)} fő számára van hely, a foglalás nem sikerült!");
                return;
            }

            // Sikeres foglalás mentése
            foglalasok.Add(new Foglalas(versenyzok, datum, kezdoOra, idotartam));
            Console.WriteLine("\nSikeres foglalás!\n");
        }

        // Beolvassa és tisztítja a neveket a megadott `.txt` fájlból
        static string[] BeolvasNeveket(string fajlNev)
        {
            return File.ReadAllText(fajlNev, Encoding.UTF8)
                .Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(nev => nev.Trim(' ', '\''))
                .ToArray();
        }

        // Generál 10 és 150 közötti véletlenszerű személyt a megadott névlistákból
        static List<Szemely> GeneraljSzemelyeket(Random random, string[] vezeteknevek, string[] keresztnevek)
        {
            var szemelyek = new List<Szemely>();
            int darab = random.Next(10, 151);

            DateTime legkorabbi = new DateTime(1960, 1, 1);
            int napokSzama = (DateTime.Today - legkorabbi).Days;

            for (int i = 0; i < darab; i++)
            {
                string vezeteknev = vezeteknevek[random.Next(vezeteknevek.Length)];
                string keresztnev = keresztnevek[random.Next(keresztnevek.Length)];
                DateTime szuletesiIdo = legkorabbi.AddDays(random.Next(napokSzama));

                szemelyek.Add(new Szemely(vezeteknev, keresztnev, szuletesiIdo));
            }

            return szemelyek;
        }

        // Legyártja a kért számú alapfoglalást a hónap hátralévő napjaira, figyelve a létszámkorlátokra
        static List<Foglalas> GeneraljFoglalasokat(Random random, List<Szemely> szemelyek, int darabszam)
        {
            const int maxFerohely = 20;
            var foglalasok = new List<Foglalas>();

            DateTime maiNap = DateTime.Today;
            int honapVegeigHatralevoNapok = DateTime.DaysInMonth(maiNap.Year, maiNap.Month) - maiNap.Day;

            for (int i = 0; i < darabszam; i++)
            {
                Foglalas ujFoglalas = null;
                int probalkozasokSzama = 0;

                // Max 50 próbálkozás, hogy találjunk érvényes idősávot
                while (ujFoglalas == null && probalkozasokSzama < 50)
                {
                    probalkozasokSzama++;

                    DateTime datum = maiNap.AddDays(random.Next(0, honapVegeigHatralevoNapok + 1));
                    int idotartam = random.Next(1, 3);
                    int kezdoOra = random.Next(8, 19 - idotartam + 1);

                    // Szabad kapacitás mérése az idősávokban
                    int szabadHely = maxFerohely;
                    for (int ora = kezdoOra; ora < kezdoOra + idotartam; ora++)
                    {
                        int mostFoglalt = foglalasok
                            .Where(f =>
                                f.Datum.Date == datum.Date &&
                                ora >= f.KezdoOra &&
                                ora < f.KezdoOra + f.Idotartam)
                            .Sum(f => f.Versenyzok.Count);

                        szabadHely = Math.Min(szabadHely, maxFerohely - mostFoglalt);
                    }

                    if (szabadHely <= 0) continue; // Tele van, új időpontot választunk

                    // Létszám igazítása a szabad helyekhez
                    int letszam = random.Next(5, 21);
                    letszam = Math.Min(letszam, szabadHely);
                    letszam = Math.Min(letszam, szemelyek.Count);

                    List<Szemely> versenyzok = ValasztVeletlenSzemelyeket(random, szemelyek, letszam);
                    ujFoglalas = new Foglalas(versenyzok, datum, kezdoOra, idotartam);
                }

                if (ujFoglalas != null)
                    foglalasok.Add(ujFoglalas);
            }

            return foglalasok;
        }

        // Kiválaszt a megadott listából egyedi (nem ismétlődő) személyeket
        static List<Szemely> ValasztVeletlenSzemelyeket(Random random, List<Szemely> szemelyek, int darabszam)
        {
            var maradek = new List<Szemely>(szemelyek);
            var kivalasztottak = new List<Szemely>();

            for (int i = 0; i < darabszam && maradek.Count > 0; i++)
            {
                int index = random.Next(maradek.Count);
                kivalasztottak.Add(maradek[index]);
                maradek.RemoveAt(index);
            }

            return kivalasztottak;
        }

        // Részletesen kiírja egy adott foglalás adatait és résztvevőit a konzolra
        static void MegjelenitFoglalas(Foglalas foglalas)
        {
            int befejezoOra = foglalas.KezdoOra + foglalas.Idotartam;

            Console.WriteLine($"Létszám: {foglalas.Versenyzok.Count} fő");
            foreach (Szemely versenyzo in foglalas.Versenyzok)
                Console.WriteLine($"  - {versenyzo.Vezeteknev} {versenyzo.Keresztnev} ({versenyzo.VersenyzoAzonosito})");

            Console.WriteLine($"Dátum: {foglalas.Datum:yyyy.MM.dd}");
            Console.WriteLine($"Foglalás: {foglalas.KezdoOra}:00 - {befejezoOra}:00 ({foglalas.Idotartam} óra)");
            Console.WriteLine(new string('-', 45));
        }

        // A táblázat egy cellájának kirajzolása színes háttérrel a foglaltság függvényében (Zöld = 0 fő, Sárga = 1-19 fő, Piros = 20 fő - betelt)
        static void KiirSzinesCella(int letszam, int szelesseg)
        {
            ConsoleColor szin;
            if (letszam == 0) szin = ConsoleColor.Green;
            else if (letszam >= 20) szin = ConsoleColor.Red;
            else szin = ConsoleColor.Yellow;

            Console.BackgroundColor = szin;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(Kozepre(letszam.ToString(), szelesseg));
            Console.ResetColor(); // Színek alaphelyzetbe állítása
        }

        // Szöveg középre igazítása egy megadott oszlopszélességen belül
        static string Kozepre(string szoveg, int szelesseg)
        {
            if (szoveg.Length >= szelesseg) return szoveg;

            int uresHely = szelesseg - szoveg.Length;
            int balOldal = uresHely / 2;
            int jobbOldal = uresHely - balOldal;

            return new string(' ', balOldal) + szoveg + new string(' ', jobbOldal);
        }

        // Havi foglalási táblázat kirajzolása napokkal és órás idősávokkal
        static void KiirTablazat(List<Foglalas> foglalasok)
        {
            const int datumSzelesseg = 12;
            const int oraSzelesseg = 7;
            int oraSavokSzama = 19 - 8; // 8:00 - 19:00 közötti 11 idősáv

            DateTime maiNap = DateTime.Today;
            DateTime honapVege = new DateTime(maiNap.Year, maiNap.Month, DateTime.DaysInMonth(maiNap.Year, maiNap.Month));

            // Táblázat vízszintes elválasztó vonala
            string elvalaszto = "+" + new string('-', datumSzelesseg)
                + string.Concat(Enumerable.Repeat("+" + new string('-', oraSzelesseg), oraSavokSzama))
                + "+";

            // Fejléc kiíratása
            Console.WriteLine(elvalaszto);
            Console.Write("|" + Kozepre("Dátum", datumSzelesseg));
            for (int ora = 8; ora < 19; ora++)
                Console.Write("|" + Kozepre($"{ora}-{ora + 1}", oraSzelesseg));
            Console.WriteLine("|");
            Console.WriteLine(elvalaszto);

            // Adatsorok kiírása napról napra a hónap végéig
            DateTime nap = maiNap;
            while (nap <= honapVege)
            {
                Console.Write("|" + Kozepre(nap.ToString("yyyy.MM.dd"), datumSzelesseg));

                for (int ora = 8; ora < 19; ora++)
                {
                    // Létszám kiszámítása az adott nap adott órájában
                    int letszam = foglalasok
                        .Where(f =>
                            f.Datum.Date == nap.Date &&
                            ora >= f.KezdoOra &&
                            ora < f.KezdoOra + f.Idotartam)
                        .Sum(f => f.Versenyzok.Count);

                    Console.Write("|");
                    KiirSzinesCella(letszam, oraSzelesseg);
                }

                Console.WriteLine("|");
                nap = nap.AddDays(1);
            }

            Console.WriteLine(elvalaszto);
        }
    }
}
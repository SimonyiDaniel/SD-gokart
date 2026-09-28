using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SD_gokart
{
    class Szemely
    {
        public string Vezeteknev { get; }
        public string Keresztnev { get; }
        public DateTime SzuletesiIdo { get; }
        public bool Elmulte18 { get; }
        public string VersenyzoAzonosito { get; }
        public string EmailCim { get; }

        public Szemely(string vezeteknev, string keresztnev, DateTime szuletesiIdo)
        {
            Vezeteknev = vezeteknev;
            Keresztnev = keresztnev;
            SzuletesiIdo = szuletesiIdo;
            Elmulte18 = KiszamoltKor() >= 18;

            string v = EkezetMentesit(vezeteknev);
            string k = EkezetMentesit(keresztnev);
            VersenyzoAzonosito = $"GO-{v}{k}-{szuletesiIdo:yyyyMMdd}";
            EmailCim = $"{v.ToLower()}.{k.ToLower()}@gmail.com";
        }

        // Kort a mai naphoz képest, korrekten (figyelembe veszi, ha még nem volt idén szülinap)
        private int KiszamoltKor()
        {
            int kor = DateTime.Today.Year - SzuletesiIdo.Year;
            if (SzuletesiIdo.AddYears(kor) > DateTime.Today) kor--;
            return kor;
        }

        private static string EkezetMentesit(string text)
        {
            string[] ekezetes = { "á", "é", "í", "ó", "ö", "ő", "ú", "ü", "ű", "Á", "É", "Í", "Ó", "Ö", "Ő", "Ú", "Ü", "Ű" };
            string[] mentes = { "a", "e", "i", "o", "o", "o", "u", "u", "u", "A", "E", "I", "O", "O", "O", "U", "U", "U" };

            for (int i = 0; i < ekezetes.Length; i++)
                text = text.Replace(ekezetes[i], mentes[i]);

            return text;
        }
    }

    class Foglalas
    {
        public List<Szemely> Versenyzok { get; } // egy foglaláshoz 5-20 fő tartozhat
        public DateTime Datum { get; }
        public int KezdoOra { get; }
        public int Idotartam { get; } // 1 vagy 2 óra

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

            Console.WriteLine("=== SD Gokartpálya ===");
            Console.WriteLine("Cím: 6065 Lakitelek, Csokonai utca 6. | Tel: +36-70-725-7247 | Web: SD-gokart.hu\n");

            string[] vezeteknevek = BeolvasNeveket("vezeteknevek.txt");
            string[] keresztnevek = BeolvasNeveket("keresztnevek.txt");

            List<Szemely> szemelyek = GeneraljSzemelyeket(random, vezeteknevek, keresztnevek);
            Console.WriteLine($"Generált versenyzők száma: {szemelyek.Count}\n");

            List<Foglalas> foglalasok = GeneraljFoglalasokat(random, szemelyek, 5);

            Console.WriteLine("=== 5 ALAP FOGLALÁS ===");
            foreach (var foglalas in foglalasok)
                MegjelenitFoglalas(foglalas);

            Console.WriteLine();
            Console.WriteLine("=== HAVI FOGLALÁSI TÁBLÁZAT ===");
            KiirTablazat(foglalasok);

            for (int i = 0; i < 1; i++)
                Console.WriteLine();

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

                }
                else if (valasz == 4)
                {

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

        // Új foglalás manuális felvétele: kilistázza a versenyzőket, azonosító alapján
        // kiválasztjuk a résztvevőket, majd bekérjük a dátumot és az időpontot.
        static void UjFoglalas(List<Foglalas> foglalasok, List<Szemely> szemelyek)
        {
            const int maxFerohely = 20;

            Console.WriteLine();
            Console.WriteLine("=== ÚJ FOGLALÁS ===");

            // Versenyzők kilistázása, hogy legyen miből választani
            Console.WriteLine("Elérhető versenyzők:");
            foreach (Szemely szemely in szemelyek)
                Console.WriteLine($"  {szemely.VersenyzoAzonosito}  -  {szemely.Vezeteknev} {szemely.Keresztnev}");
            Console.WriteLine();

            // Létszám bekérése
            int letszam;
            Console.Write("Hány fő szeretne foglalni (5-20)? ");
            while (!int.TryParse(Console.ReadLine(), out letszam) || letszam < 5 || letszam > 20)
                Console.Write("Hibás létszám, próbálja újra (5-20): ");

            // A résztvevők azonosítóinak bekérése, egyesével
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

            // Időtartam bekérése (nem léphet túl 19:00-án)
            int idotartam;
            Console.Write("Időtartam órában (1 vagy 2): ");
            while (!int.TryParse(Console.ReadLine(), out idotartam)
                || (idotartam != 1 && idotartam != 2)
                || kezdoOra + idotartam > 19)
                Console.Write("Hibás időtartam, próbálja újra (1 vagy 2): ");

            // Szabad hely ellenőrzése az összes érintett órában
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

            if (letszam > szabadHely)
            {
                Console.WriteLine($"Sajnos ebben az időpontban csak {Math.Max(szabadHely, 0)} fő számára van hely, a foglalás nem sikerült!");
                return;
            }

            foglalasok.Add(new Foglalas(versenyzok, datum, kezdoOra, idotartam));
            Console.WriteLine();
            Console.WriteLine("Sikeres foglalás!");
            Console.WriteLine();
        }

        // Nevek beolvasása fájlból (vesszővel/sortöréssel elválasztva)
        static string[] BeolvasNeveket(string fajlNev)
        {
            return File.ReadAllText(fajlNev, Encoding.UTF8)
                .Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(nev => nev.Trim(' ', '\''))
                .ToArray();
        }

        // 10-150 véletlenszerű versenyző legyártása
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

        // Néhány alap foglalás legyártása, a mai naptól a hónap végéig eső napokra.
        // Egy adott órában (bármely nap) legfeljebb "maxFerohely" fő lehet összesen -
        // ezért minden új foglalás előtt megnézzük, mennyi szabad hely van még az érintett órákban.
        
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

                // Próbálkozunk, amíg nem találunk időpontot, ahol van még szabad hely (max 50 próbálkozás)
                while (ujFoglalas == null && probalkozasokSzama < 50)
                {
                    probalkozasokSzama++;

                    DateTime datum = maiNap.AddDays(random.Next(0, honapVegeigHatralevoNapok + 1));
                    int idotartam = random.Next(1, 3); // 1 vagy 2 óra
                    int kezdoOra = random.Next(8, 19 - idotartam + 1); // legkésőbb 19:00-ig ér véget

                    // Megnézzük, az érintett órák közül a legszűkebb helyen mennyi hely maradt
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

                    if (szabadHely <= 0) continue; // ebben az időpontban tele van, próbálunk másikat

                    int letszam = random.Next(5, 21); // 5-20 fő szeretnénk
                    letszam = Math.Min(letszam, szabadHely); // de csak annyi fér el, amennyi hely van
                    letszam = Math.Min(letszam, szemelyek.Count); // és annyi versenyző sincs több

                    List<Szemely> versenyzok = ValasztVeletlenSzemelyeket(random, szemelyek, letszam);
                    ujFoglalas = new Foglalas(versenyzok, datum, kezdoOra, idotartam);
                }

                if (ujFoglalas != null)
                    foglalasok.Add(ujFoglalas);
            }

            return foglalasok;
        }
        

        // Kiválaszt "darabszam" db egyedi (nem ismétlődő) személyt a listából, véletlenszerűen.
        static List<Szemely> ValasztVeletlenSzemelyeket(Random random, List<Szemely> szemelyek, int darabszam)
        {
            var maradek = new List<Szemely>(szemelyek); // másolat, hogy az eredeti lista ne sérüljön
            var kivalasztottak = new List<Szemely>();

            for (int i = 0; i < darabszam && maradek.Count > 0; i++)
            {
                int index = random.Next(maradek.Count);
                kivalasztottak.Add(maradek[index]);
                maradek.RemoveAt(index);
            }

            return kivalasztottak;
        }

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

        // Egy táblázatcellát ír ki, a létszámnak megfelelő háttérszínnel:
        // 0 fő = zöld, 1-19 fő = sárga, 20+ fő = piros.
        static void KiirSzinesCella(int letszam, int szelesseg)
        {
            ConsoleColor szin;
            if (letszam == 0) szin = ConsoleColor.Green;
            else if (letszam >= 20) szin = ConsoleColor.Red;
            else szin = ConsoleColor.Yellow;

            Console.BackgroundColor = szin;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(Kozepre(letszam.ToString(), szelesseg));
            Console.ResetColor();
        }

        // Egy szöveget középre igazítva ad vissza egy adott szélességű mezőben.
        // Ez teszi lehetővé, hogy 1 és 2 (vagy több) jegyű számok is szépen, egyformán illeszkedjenek.
        static string Kozepre(string szoveg, int szelesseg)
        {
            if (szoveg.Length >= szelesseg) return szoveg;

            int uresHely = szelesseg - szoveg.Length;
            int balOldal = uresHely / 2;
            int jobbOldal = uresHely - balOldal;

            return new string(' ', balOldal) + szoveg + new string(' ', jobbOldal);
        }

        // Táblázat: sorokban a mai naptól a hónap végéig eső napok, oszlopokban az 1 órás idősávok (8-9, 9-10, ..., 18-19).
        // Minden cellában az adott napon, adott idősávban aktív foglalások száma szerepel.
        static void KiirTablazat(List<Foglalas> foglalasok)
        {
            const int datumSzelesseg = 12;
            const int oraSzelesseg = 7;
            int oraSavokSzama = 19 - 8; // 8-9 ... 18-19 => 11 sáv

            DateTime maiNap = DateTime.Today;
            DateTime honapVege = new DateTime(maiNap.Year, maiNap.Month, DateTime.DaysInMonth(maiNap.Year, maiNap.Month));

            string elvalaszto = "+" + new string('-', datumSzelesseg)
                + string.Concat(Enumerable.Repeat("+" + new string('-', oraSzelesseg), oraSavokSzama))
                + "+";

            // Fejléc
            Console.WriteLine(elvalaszto);
            Console.Write("|" + Kozepre("Dátum", datumSzelesseg));
            for (int ora = 8; ora < 19; ora++)
                Console.Write("|" + Kozepre($"{ora}-{ora + 1}", oraSzelesseg));
            Console.WriteLine("|");
            Console.WriteLine(elvalaszto);

            // Sorok: napról napra a hónap végéig
            DateTime nap = maiNap;
            while (nap <= honapVege)
            {
                Console.Write("|" + Kozepre(nap.ToString("yyyy.MM.dd"), datumSzelesseg));

                for (int ora = 8; ora < 19; ora++)
                {
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
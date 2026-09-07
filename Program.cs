using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SD_gokart
{
    class Szemely
    {
        public string Vezeteknev { get; set; }
        public string Keresztnev { get; set; }
        public DateTime SzuletesiIdo { get; set; }
        public bool Elmulte18 { get; set; }
        public string VersenyzoAzonosito { get; set; }
        public string EmailCim { get; set; }

        public Szemely(string vezeteknev, string keresztnev, DateTime szuletesiIdo)
        {
            Vezeteknev = vezeteknev;
            Keresztnev = keresztnev;
            SzuletesiIdo = szuletesiIdo;

            // Kor pontos kiszámítása
            int age = DateTime.Now.Year - SzuletesiIdo.Year;
            if (DateTime.Now < SzuletesiIdo.AddYears(age)) { age--; }
            Elmulte18 = age >= 18;

            VersenyzoAzonosito = $"GO-{Vezeteknev}{Keresztnev}-{SzuletesiIdo:yyyyMMdd}";
            EmailCim = $"{Vezeteknev.ToLower()}.{Keresztnev.ToLower()}@gmail.com";
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();
            List<Szemely> Szemelyek = new List<Szemely>();

            string[] vezeteknevek = File.ReadAllText("vezeteknevek.txt", Encoding.UTF8)
                .Split(new char[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim(' ', '\''))
                .ToArray();

            string[] keresztnevek = File.ReadAllText("keresztnevek.txt", Encoding.UTF8)
                .Split(new char[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim(' ', '\''))
                .ToArray();

            // Beállítjuk, hogy fixen 3 embert generáljon
            int versenyzokSzama = random.Next(1, 151);

            DateTime start = new DateTime(1960, 1, 1);
            int range = (DateTime.Today - start).Days;

            for (int i = 0; i < versenyzokSzama; i++)
            {
                // Most már jól fog választani egy elemet a tömbből
                string randomVezeteknev = vezeteknevek[random.Next(vezeteknevek.Length)];
                string randomKeresztnev = keresztnevek[random.Next(keresztnevek.Length)];
                DateTime randomSzuletesiIdo = start.AddDays(random.Next(range));

                Szemely ujSzemely = new Szemely(randomVezeteknev, randomKeresztnev, randomSzuletesiIdo);
                Szemelyek.Add(ujSzemely);
            }
            
            Console.WriteLine($"Generált versenyzők száma: {Szemelyek.Count}");
            /*foreach (var item in Szemelyek)
            *{
            *    Console.WriteLine($"{item.VersenyzoAzonosito} | {item.EmailCim}");
            *}
            *
            *Console.ReadLine();
            */







        }
    }
}            /*
             A pályabérlés szabályai:
            - A pálya 8:00 és 19:00 között van nyitva.
            - A pályát minimum 1 óra időtartamra kell bérelni.
            - A pályán minimum 8 - maximum 20 versenyző lehet egyszerre.
            - Nem kell, hogy egy csoport tagjai legyenek a pályán, külsősként is be lehet csatlakozni a menetbe.
            - Az adott személy min. 1 , max. 2 órára foglaljon. A 2 órás foglalás összefüggő kell, hogy legyen
             */
using System;
using System.IO;
using System.Text.Json;

namespace Tamagotchi
{

    class Pet
    {
        public string imya { get; set; } = "";
        public string vid { get; set; } = "";
        public int golod { get; set; } = 50;
        public int zdorovie { get; set; } = 100;
        public int nastroenie { get; set; } = 50;
        public int energiya { get; set; } = 100;
        public int monety { get; set; } = 3;
        public long spitsya_do { get; set; } = 0;
        public long otdyhaet_do { get; set; } = 0;
        public string vremya { get; set; } = "";
        public bool mertviy { get; set; } = false;
    }

    class Program
    {
        static Pet pet = new Pet();
        static string fail = "pet.json";

        static void Main(string[] args)
        {

            Load();

            var app = WebApplication.CreateBuilder(args).Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();


            app.MapGet("/state", () =>
            {
                ObnovitVremya();
                Save();
                return pet;
            });


            app.MapGet("/pokormit", () =>
            {
                ObnovitVremya();
                if (Zanyat())
                {
                    Save();
                    return pet;
                }
                if (pet.monety <= 0)
                {
                    Save();
                    return pet;
                }
                pet.monety = pet.monety - 1;
                Pokormit();
                Save();
                return pet;
            });


            app.MapGet("/poigrat", () =>
            {
                ObnovitVremya();
                if (Zanyat())
                {
                    Save();
                    return pet;
                }
                Poigrat();
                Save();
                return pet;
            });


            app.MapGet("/igra", (string pobeda) =>
            {
                ObnovitVremya();
                if (Zanyat())
                {
                    Save();
                    return pet;
                }
                Poigrat();
                if (pobeda == "da")
                {
                    pet.monety = pet.monety + 1;
                }
                Save();
                return pet;
            });


            app.MapGet("/polechit", () =>
            {
                ObnovitVremya();
                if (Zanyat())
                {
                    Save();
                    return pet;
                }
                Polechit();
                Save();
                return pet;
            });


            app.MapGet("/spat", () =>
            {
                ObnovitVremya();
                if (Zanyat())
                {
                    Save();
                    return pet;
                }
                pet.spitsya_do = SejchasMS() + 8L * 60 * 60 * 1000;
                pet.otdyhaet_do = 0;
                Save();
                return pet;
            });


            app.MapGet("/otdyh", () =>
            {
                ObnovitVremya();
                if (Zanyat())
                {
                    Save();
                    return pet;
                }
                pet.otdyhaet_do = SejchasMS() + 5L * 60 * 1000;
                pet.spitsya_do = 0;
                Save();
                return pet;
            });


            app.MapGet("/sozdat", (string imya, int vid) =>
            {
                Sozdat(imya, vid);
                Save();
                return pet;
            });


            app.MapGet("/sbros", () =>
            {
                try { File.Delete(fail); } catch { }
                pet = new Pet();
                pet.mertviy = false;
                pet.imya = "Без имени";
                pet.vid = "кот";
                pet.golod = 50;
                pet.zdorovie = 100;
                pet.nastroenie = 50;
                pet.energiya = 100;
                pet.monety = 3;
                pet.vremya = DateTime.Now.ToString("o");
                Save();
                return pet;
            });

            app.Run();
        }


        static void Sozdat(string imya, int vid)
        {
            pet = new Pet();
            pet.mertviy = false;

            pet.imya = imya;
            if (pet.imya == "" || pet.imya == null)
            {
                pet.imya = "Без имени";
            }

            if (vid == 1) pet.vid = "кот";
            else if (vid == 2) pet.vid = "пёс";
            else if (vid == 3) pet.vid = "хомяк";
            else if (vid == 4) pet.vid = "дракон";
            else pet.vid = "кот";

            pet.golod = 50;
            pet.zdorovie = 100;
            pet.nastroenie = 50;
            pet.energiya = 100;
            pet.monety = 3;
            pet.vremya = DateTime.Now.ToString("o");
        }


        static void Pokormit()
        {
            pet.golod = pet.golod - 20;
            if (pet.golod < 0) pet.golod = 0;
            pet.nastroenie = pet.nastroenie + 5;
            if (pet.nastroenie > 100) pet.nastroenie = 100;
        }

        static void Poigrat()
        {
            pet.nastroenie = pet.nastroenie + 20;
            if (pet.nastroenie > 100) pet.nastroenie = 100;
            pet.energiya = pet.energiya - 15;
            if (pet.energiya < 0) pet.energiya = 0;
            pet.golod = pet.golod + 5;
            if (pet.golod > 100) pet.golod = 100;
        }

        static void Polechit()
        {
            pet.zdorovie = pet.zdorovie + 25;
            if (pet.zdorovie > 100) pet.zdorovie = 100;
            pet.nastroenie = pet.nastroenie - 10;
            if (pet.nastroenie < 0) pet.nastroenie = 0;
            pet.energiya = pet.energiya - 5;
            if (pet.energiya < 0) pet.energiya = 0;
        }


        static long SejchasMS()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }


        static bool Spit()
        {
            return SejchasMS() < pet.spitsya_do;
        }


        static bool Otdyhaet()
        {
            return SejchasMS() < pet.otdyhaet_do;
        }


        static bool Zanyat()
        {
            return Spit() || Otdyhaet();
        }


        static void ObnovitVremya()
        {
            DateTime seychas = DateTime.Now;
            long ms = SejchasMS();


            if (pet.spitsya_do != 0)
            {
                if (ms >= pet.spitsya_do)
                {

                    pet.energiya = 100;
                    pet.golod = 80;
                    pet.spitsya_do = 0;
                    pet.vremya = seychas.ToString("o");
                }
                else
                {

                    pet.vremya = seychas.ToString("o");
                    return;
                }
            }


            if (pet.otdyhaet_do != 0)
            {
                if (ms >= pet.otdyhaet_do)
                {

                    pet.energiya = pet.energiya + 30;
                    if (pet.energiya > 100) pet.energiya = 100;
                    pet.otdyhaet_do = 0;
                    pet.vremya = seychas.ToString("o");
                }
                else
                {

                    pet.vremya = seychas.ToString("o");
                    return;
                }
            }

            if (pet.vremya == "")
            {
                pet.vremya = seychas.ToString("o");
                return;
            }

            DateTime bylo = DateTime.Parse(pet.vremya);
            int sek = (int)(seychas - bylo).TotalSeconds;


            if (sek < 10)
            {
                return;
            }

            int shagov = sek / 10;

            pet.golod = pet.golod + shagov;
            if (pet.golod > 100) pet.golod = 100;

            pet.energiya = pet.energiya - shagov;
            if (pet.energiya < 0) pet.energiya = 0;

            pet.nastroenie = pet.nastroenie - shagov / 2;
            if (pet.nastroenie < 0) pet.nastroenie = 0;


            if (pet.golod >= 100)
            {
                pet.zdorovie = pet.zdorovie - shagov * 2;
            }

            if (pet.energiya <= 0)
            {
                pet.zdorovie = pet.zdorovie - shagov * 2;
            }

            if (pet.zdorovie < 0)
            {
                pet.zdorovie = 0;
            }

            if (pet.zdorovie <= 0)
            {
                pet.mertviy = true;
            }

            pet.vremya = seychas.ToString("o");
        }


        static void Save()
        {
            JsonSerializerOptions opt = new JsonSerializerOptions();
            opt.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            string json = JsonSerializer.Serialize(pet, opt);
            File.WriteAllText(fail, json);
        }


        static void Load()
        {
            if (!File.Exists(fail))
            {
                return;
            }

            try
            {
                string json = File.ReadAllText(fail);
                Pet zagruzka = JsonSerializer.Deserialize<Pet>(json) ?? new Pet();
                pet = zagruzka;

                pet.vremya = DateTime.Now.ToString("o");
            }
            catch
            {

            }
        }
    }
}
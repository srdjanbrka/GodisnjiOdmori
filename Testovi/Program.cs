using System;
using System.Collections.Generic;
using GodisnjiOdmori.Podaci;
using GodisnjiOdmori.Logika;

class Program
{
    static int broj;
    static void Provera(string naziv,bool uslov) { if(!uslov) throw new Exception("NEUSPEH: "+naziv); Console.WriteLine("OK: "+naziv); broj++; }
    static void Odbacuje(string naziv,Action a) { try { a(); } catch(InvalidOperationException) { Provera(naziv,true);return; } throw new Exception("NEUSPEH: "+naziv); }
    static int Main()
    {
        try {
            var pon=DateTime.Today.AddDays(7);
            while(pon.DayOfWeek!=DayOfWeek.Monday) pon=pon.AddDays(1);
            var uto=pon.AddDays(1);
            var dani=new[]{pon,uto};
            Provera("Bez prethodnih odsustava",!Pravilo.PrekoracenLimit(dani,new Odsustvo[0],3,2));
            var jedan=new[]{new Odsustvo{ZaposleniID=1,Datum=pon}};
            Provera("Tacno X je dozvoljeno",!Pravilo.PrekoracenLimit(dani,jedan,3,2));
            var dva=new[]{new Odsustvo{ZaposleniID=1,Datum=pon},new Odsustvo{ZaposleniID=2,Datum=pon}};
            Provera("X+1 ide na cekanje",Pravilo.PrekoracenLimit(dani,dva,3,2));
            var odvojeni=new[]{new Odsustvo{ZaposleniID=1,Datum=pon},new Odsustvo{ZaposleniID=2,Datum=uto}};
            Provera("Odvojeni dani se ne sabiraju",!Pravilo.PrekoracenLimit(dani,odvojeni,3,2));
            var duplikati=new[]{new Odsustvo{ZaposleniID=1,Datum=pon},new Odsustvo{ZaposleniID=1,Datum=pon}};
            Provera("Ista osoba se broji jednom",!Pravilo.PrekoracenLimit(dani,duplikati,3,2));
            Provera("Nema dvostrukog brojanja podnosioca",!Pravilo.PrekoracenLimit(dani,jedan,1,1));
            Provera("Granica poslednjeg dana ukljucena",Pravilo.PrekoracenLimit(dani,new[]{new Odsustvo{ZaposleniID=1,Datum=uto}},3,1));
            Provera("Odsustvo van perioda ne utice",!Pravilo.PrekoracenLimit(dani,new[]{new Odsustvo{ZaposleniID=1,Datum=uto.AddDays(1)}},3,1));
            Provera("Pet radnih dana",Pravilo.RadniDani(pon,pon.AddDays(6)).Count==5);
            Provera("Jedan radni dan",Pravilo.RadniDani(pon,pon).Count==1);
            Odbacuje("Obrnut period",()=>Pravilo.RadniDani(uto,pon));
            Odbacuje("Datum u prošlosti",()=>Pravilo.RadniDani(DateTime.Today.AddDays(-1),DateTime.Today));
            Odbacuje("Samo vikend",()=>Pravilo.RadniDani(pon.AddDays(5),pon.AddDays(6)));
            Odbacuje("Neispravan X",()=>Pravilo.PrekoracenLimit(dani,jedan,3,0));
            Console.WriteLine("Ukupno uspesnih provera: "+broj);return 0;
        } catch(Exception e) { Console.Error.WriteLine(e.Message);return 1; }
    }
}

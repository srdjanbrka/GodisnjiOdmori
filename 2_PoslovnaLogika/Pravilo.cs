using System;
using System.Collections.Generic;
using System.Linq;
using GodisnjiOdmori.Podaci;

namespace GodisnjiOdmori.Logika
{
    public static class Pravilo
    {
        public static List<DateTime> RadniDani(DateTime od,DateTime doDatuma)
        {
            od=od.Date; doDatuma=doDatuma.Date;
            if(od<DateTime.Today)
                throw new InvalidOperationException("Datum početka ne može biti pre današnjeg datuma.");
            if(doDatuma<od)
                throw new InvalidOperationException("Datum završetka mora biti jednak ili veći od datuma početka.");
            if(od.Year<2000 || doDatuma.Year>2100 || (doDatuma-od).Days>366)
                throw new InvalidOperationException("Unesite ispravan period između 2000. i 2100. godine, do 366 dana razlike.");
            var dani=new List<DateTime>();
            for(var d=od;d<=doDatuma;d=d.AddDays(1))
                if(d.DayOfWeek!=DayOfWeek.Saturday && d.DayOfWeek!=DayOfWeek.Sunday) dani.Add(d);
            if(dani.Count==0) throw new InvalidOperationException("Period mora sadržati makar jedan radni dan.");
            return dani;
        }
        public static bool PrekoracenLimit(IEnumerable<DateTime> dani,IEnumerable<Odsustvo> odsustva,int zaposleniId,int x)
        {
            if(x<1) throw new InvalidOperationException("Parametar X mora biti pozitivan.");
            var poDanu=odsustva.ToLookup(o=>o.Datum.Date);
            return dani.Any(d=>poDanu[d.Date].Select(o=>o.ZaposleniID).Concat(new[]{zaposleniId}).Distinct().Count()>x);
        }
    }
}

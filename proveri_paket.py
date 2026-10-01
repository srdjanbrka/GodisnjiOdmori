"""Statička provera paketa; nije zamena za MSBuild, C# ili SQL/IIS testove."""
from pathlib import Path
import base64
import hashlib
import re
import xml.etree.ElementTree as ET

root=Path(__file__).resolve().parent
ns={'m':'http://schemas.microsoft.com/developer/msbuild/2003'}

for path in root.rglob('*'):
    if path.suffix in ('.config','.xml','.csproj'):
        ET.parse(path)
        print('XML OK:',path.relative_to(root))

projects=list(root.rglob('*.csproj'))
assert len(projects)==5
for project in projects:
    tree=ET.parse(project)
    for node in tree.findall('.//m:Compile',ns)+tree.findall('.//m:ProjectReference',ns):
        target=project.parent/node.attrib['Include'].replace('\\','/')
        assert target.is_file(),f'Nedostaje {target}'
    includes={n.attrib['Include'].replace('\\','/') for n in tree.findall('.//m:Compile',ns)}
    actual={str(f.relative_to(project.parent)) for f in project.parent.rglob('*.cs')}
    assert includes==actual,(project,includes^actual)
    guid=tree.find('m:PropertyGroup/m:ProjectGuid',ns).text
    assert guid in (root/'GodisnjiOdmori.sln').read_text(encoding='utf-8')

def tekst(rel):
    return (root/rel).read_text(encoding='utf-8')

podaci=tekst('1_SlojPodataka/Podaci.csproj')
assert 'EntityFramework' in podaci
assert not (root/'1_SlojPodataka/Modeli.cs').exists()
assert not (root/'1_SlojPodataka/Repozitorijum.cs').exists()

repozitorijumi={
    'SektorRepozitorijum.cs':['DajSve','Daj(','Dodaj','Izmeni','Obrisi'],
    'ZaposleniRepozitorijum.cs':['DajSve','Daj(','Dodaj','Izmeni','Obrisi'],
    'ZahtevRepozitorijum.cs':['DajSve','Daj(','Dodaj','Izmeni','Obrisi'],
    'DanOdmoraRepozitorijum.cs':['DajSve','Daj(','Dodaj','Izmeni','Obrisi'],
    'KorisnikRepozitorijum.cs':['DajSve','Daj(','Dodaj','Izmeni','Obrisi']
}
for fajl,operacije in repozitorijumi.items():
    sadrzaj=tekst('1_SlojPodataka/'+fajl)
    for operacija in operacije:
        assert operacija in sadrzaj,(fajl,operacija)

assert ': Tabela' in tekst('1_SlojPodataka/SektorRepozitorijum.cs')
assert 'CommandType.StoredProcedure' in tekst('1_SlojPodataka/KorisnikRepozitorijum.cs')
assert 'DbContext' in tekst('1_SlojPodataka/GodisnjiOdmoriContext.cs')
assert 'Database.SqlQuery' in tekst('1_SlojPodataka/ZahtevRepozitorijum.cs')

mvc=tekst('4_MVC/MVC.csproj')
assert 'EntityFramework' in mvc
for model in ['PrijavaModel','ZahtevFormaModel','ZahtevStavkaModel','ZahteviListaModel','ZahtevDetaljiModel','ZaposleniStavkaModel']:
    assert (root/'4_MVC/ModeliPrikaza'/(model+'.cs')).is_file()
for view in ['Login/Index.cshtml','Zahtev/Index.cshtml','Zahtev/Detalji.cshtml','Zahtev/Forma.cshtml','Zahtev/Zaposleni.cshtml']:
    assert 'ModeliPrikaza' in tekst('4_MVC/Views/'+view)

sql=tekst('Baza/01_Baza.sql')
for procedura in ['Korisnik_DajSve','Korisnik_DajPoID','Korisnik_DajPoKorisnickomImenu','Korisnik_Dodaj','Korisnik_Izmeni','Korisnik_Obrisi','Zahtev_ImaPreklapanje','Zahtev_DajOdobrenaOdsustva']:
    assert procedura in sql
korisnik_tabela=sql[sql.index('CREATE TABLE dbo.Korisnik'):sql.index('CREATE INDEX IX_Zahtev')]
assert 'REFERENCES' not in korisnik_tabela
assert 'Štampaj listu' in tekst('4_MVC/Views/Zahtev/Index.cshtml')
assert all('DanOdmora' not in tekst('4_MVC/Views/'+view) for view in ['Zahtev/Index.cshtml','Zahtev/Detalji.cshtml','Zahtev/Forma.cshtml'])

parovi=[
    ('WmFwb3NsZW5pU2FsdDIwMjY=','eOiTbsKr4iVkAgBKKrRoim4YuG1WoZXNGkek5lXEsm4=',b'ZaposleniDemo2026!'),
    ('S2Fkcm92c2thU2FsdDIwMjY=','LGRyX8mr8zNzZ1Qh1Bylt7IGTHxYdPmurt+vbHVQLgw=',b'OdmorDemo2026!')
]
for salt_b64,hash_b64,lozinka in parovi:
    assert hashlib.pbkdf2_hmac('sha256',lozinka,base64.b64decode(salt_b64),100000)==base64.b64decode(hash_b64)

config=ET.parse(root/'4_MVC/Web.config')
settings={a.attrib['key']:a.attrib['value'] for a in config.findall('appSettings/add')}
assert settings['ParametriUrl']=='http://localhost:5101/api/parametri'
assert 'KadrovskaUser' not in settings and 'ZaposleniUser' not in settings
assert ET.parse(root/'3_RESTServis/App_Data/Parametri.xml').findtext('MaksimalnoOdsutnih')=='2'

print('OK: EF, ADO.NET procedure, DBUtils nasleđivanje, ViewModeli, CRUD, nezavisan korisnik, štampa i XML parametar.')
print('NAPOMENA: nije izvršen .NET build, C# test niti SQL/IIS integracija.')

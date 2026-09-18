"""Staticka provera paketa; nije zamena za MSBuild, C# ili SQL testove."""
from pathlib import Path
import base64
import hashlib
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parent
ns = {'m': 'http://schemas.microsoft.com/developer/msbuild/2003'}
for path in root.rglob('*'):
    if path.suffix in ('.config', '.xml', '.csproj'):
        ET.parse(path)
        print('XML OK:', path.relative_to(root))

projects = list(root.rglob('*.csproj'))
assert len(projects) == 5
for project in projects:
    tree = ET.parse(project)
    for node in tree.findall('.//m:Compile', ns) + tree.findall('.//m:ProjectReference', ns):
        target = project.parent / node.attrib['Include'].replace('\\', '/')
        assert target.is_file(), f'Nedostaje {target}'
    includes = {n.attrib['Include'].replace('\\', '/') for n in tree.findall('.//m:Compile', ns)}
    actual = {str(f.relative_to(project.parent)) for f in project.parent.rglob('*.cs')}
    assert includes == actual, (project, includes ^ actual)
    assert '{' + tree.find('m:PropertyGroup/m:ProjectGuid', ns).text.strip('{}') + '}' in (root/'GodisnjiOdmori.sln').read_text()

config = ET.parse(root/'4_MVC/Web.config')
settings = {a.attrib['key']: a.attrib['value'] for a in config.findall('appSettings/add')}
assert settings['ParametriUrl'] == 'http://localhost:5101/api/parametri'
assert 'Microsoft.CSharp' in (root/'4_MVC/MVC.csproj').read_text(), 'MVC mora imati Microsoft.CSharp referencu zbog ViewBag/dynamic poziva.'
for prefiks,lozinka in [('Kadrovska',b'OdmorDemo2026!'),('Zaposleni',b'ZaposleniDemo2026!')]:
    salt = base64.b64decode(settings[prefiks+'Salt'])
    expected = base64.b64decode(settings[prefiks+'Hash'])
    assert hashlib.pbkdf2_hmac('sha256', lozinka, salt, 100000) == expected
assert settings['ZaposleniID'] == '1'
assert ET.parse(root/'3_RESTServis/App_Data/Parametri.xml').findtext('MaksimalnoOdsutnih') == '2'
print('OK: reference projekata, svi C# fajlovi, solution GUID, REST URL, oba demo naloga i XML parametar.')
print('NAPOMENA: nije izvrsen .NET build, C# test niti SQL/IIS integracija.')

"""Build controlled native release layouts after installed-binary verification."""
from pathlib import Path
import argparse, importlib.util, json, os, re, shutil, subprocess, sys, tarfile, zipfile
sys.dont_write_bytecode=True
p=argparse.ArgumentParser(description=__doc__);p.add_argument('stage',type=Path);p.add_argument('target',choices=['linux-x64','windows-x64','macos-arm64']);p.add_argument('--version',required=True);args=p.parse_args()
assert re.fullmatch(r'0\.1\.0-preview',args.version),'Version must match source metadata'
source=Path(__file__).resolve().parents[1];stage=args.stage.resolve();binary=stage/'bin'/('orderlab.exe' if os.name=='nt' else 'orderlab')
def run(*arguments,code=0):
    r=subprocess.run([binary,*map(str,arguments)],capture_output=True,encoding='utf-8',timeout=40)
    assert r.returncode==code,r.stdout+r.stderr
    return r.stdout
assert run('--version').strip()=='orderlab '+args.version
for name in ['README.md','LICENSE','SECURITY.md','CONTRIBUTING.md','NuGet.Config']:shutil.copyfile(source/name,stage/name)
for name in ['docs','assets','examples']:shutil.copytree(source/name,stage/name,ignore=shutil.ignore_patterns('__pycache__','*.pyc','bin','obj'))
(stage/'licenses').mkdir()
sdk=Path(subprocess.check_output(['dotnet','--list-sdks'],text=True).splitlines()[-1].split('[',1)[1].rstrip(']')).parent
for name in ['LICENSE.txt','ThirdPartyNotices.txt']:shutil.copyfile(sdk/name,stage/'licenses'/('DOTNET-'+name))
spec=importlib.util.spec_from_file_location('fixtures',source/'examples/make_histories.py');fixtures=importlib.util.module_from_spec(spec);spec.loader.exec_module(fixtures)
capture=stage/'examples/synthetic';fixtures.create(capture)
good=json.loads(run('check',capture/'overlap-register.json','--format','json'));bad=json.loads(run('check',capture/'stale-register.json','--format','json',code=1))
assert good['witness']==[2,1] and bad['verdict']=='NotLinearizable'
(capture/'accepted-report.json').write_text(json.dumps(good,indent=2)+'\n',encoding='utf-8');(capture/'rejected-report.json').write_text(json.dumps(bad,indent=2)+'\n',encoding='utf-8')
run('check',capture/'overlap-register.json','--format','html','--output',capture/'accepted-report.html')
run('check',capture/'stale-register.json','--format','html','--output',capture/'rejected-report.html',code=1)
run('shrink',capture/'shrink-me.json','--output',capture/'reduced-history.json',code=1)
dest=source/'dist';dest.mkdir(exist_ok=True);archive=dest/('orderlab-'+args.version+'-'+args.target+('.zip' if args.target=='windows-x64' else '.tar.gz'))
files=sorted(stage.rglob('*'));assert all(not f.is_symlink() for f in files)
if archive.suffix=='.zip':
    with zipfile.ZipFile(archive,'x',compression=zipfile.ZIP_DEFLATED) as out:
        for f in files:out.write(f,f.relative_to(stage).as_posix())
else:
    with tarfile.open(archive,'x:gz') as out:
        for f in files:out.add(f,arcname=f.relative_to(stage).as_posix(),recursive=False)
print('Installed-layout witnesses, rejection and reduction passed:',archive.name)

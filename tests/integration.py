"""Independent installed-command, schema, digest, privacy and file-preservation checks."""
from pathlib import Path
import copy, hashlib, importlib.util, json, os, subprocess, sys, tempfile
sys.dont_write_bytecode=True
binary=Path(sys.argv[1]).resolve();source=Path(sys.argv[2]).resolve();checks=0
def expect(condition,message):
    global checks
    checks+=1
    if not condition:raise AssertionError(message)
def run(*args,code=0):
    r=subprocess.run([binary,*map(str,args)],capture_output=True,encoding='utf-8',timeout=40)
    expect(r.returncode==code,'exit '+str(r.returncode)+' '+r.stdout[-300:]+r.stderr[-300:]);return r.stdout
spec=importlib.util.spec_from_file_location('histories',source/'examples/make_histories.py');fixtures=importlib.util.module_from_spec(spec);spec.loader.exec_module(fixtures)
with tempfile.TemporaryDirectory(prefix='orderlab installed space ') as tmp:
    root=Path(tmp);folder=root/'invented';expected=fixtures.create(folder)
    expect(run('--version').strip()=='orderlab 0.1.0-preview','version')
    expect('orderlab check' in run('--help'),'help')
    for name,oracle in expected.items():
        path=folder/(name+'.json');original=path.read_bytes();exitcode=0 if oracle['verdict']=='Linearizable' else 1
        report=json.loads(run('check',path,'--format','json',code=exitcode))
        expect(report['schema']=='orderlab.report.v1','schema')
        expect(report['verdict']==oracle['verdict'] and len(report['operations'])==oracle['operations'],'fixture oracle '+name)
        expect(report['inputSha256']==hashlib.sha256(original).hexdigest(),'independent file hash')
        history=json.loads(original)
        for view,op in zip(report['operations'],history['operations']):
            expect((view['start'],view['end'],view['kind'])==(op['start'],op['end'],op['kind']),'exact intervals')
            for evidence,value in zip(view['arguments'],op['args']):
                expect(evidence=={'sha256':hashlib.sha256(value.encode()).hexdigest()},'argument digest and default omission')
            if op['result'] is not None:expect(view['result']['sha256']==hashlib.sha256(op['result'].encode()).hexdigest(),'result digest')
        partition=json.loads(run('check',path,'--format','json','--partition',code=exitcode))
        expect(partition['verdict']==report['verdict'],'partition agreement')
        if exitcode==0:
            order=','.join(map(str,report['witness']));expect('verified' in run('verify',path,'--witness',order),'witness replay')
        expect(path.read_bytes()==original,'source preserved')
    path=folder/'overlap-register.json';html=run('check',path,'--format','html')
    expect('<svg' in html and '<script' not in html and 'Content-Security-Policy' in html,'local report')
    expect('observed intervals' in html.lower(),'timeline')
    expect('rejected' in run('verify',path,'--witness','1,2',code=1),'forged order rejected')
    expect('rejected' in run('verify',path,'--witness','2,2',code=1),'duplicate order rejected')
    for flags in [('--max-states','1'),('--partition','--max-states','1')]:
        r=json.loads(run('check',path,'--format','json',*flags,code=2));expect(r['verdict']=='Inconclusive','budget is not negative')
    target=root/'report unicode é.json';run('check',path,'--format','json','--output',target)
    saved=target.read_bytes();expect(json.loads(saved)['verdict']=='Linearizable','Unicode path and new report')
    expect(run('check',path,'--output',target,code=2)=='','overwrite emits no report');expect(target.read_bytes()==saved,'existing report preserved')
    run('check',path,'--output',path,code=2)
    if os.name!='nt':expect(target.stat().st_mode&0o077==0,'private report mode')
    reduced=root/'reduced.json';text=run('shrink',folder/'shrink-me.json','--output',reduced,code=1)
    expect('one-deletion-minimal: True' in text,'shrink conclusion');expect(len(json.loads(reduced.read_text())['operations'])==2,'irrelevant operation removed')
    run('check',reduced,code=1)
    raw=json.loads(path.read_text());raw['initial']=['private-canary'];raw['operations'][0]['args']=['private-canary'];raw['operations'][1]['result']='private-canary';raw['operations'][0]['id']='identity-canary';raw['operations'][0]['client']='client-canary';raw['operations'][0]['object']='object-canary';raw['operations'][1]['object']='object-canary'
    private=root/'private.json';private.write_text(json.dumps(raw),encoding='utf-8')
    for format in ['text','json','html']:
        out=run('check',private,'--format',format);expect('canary' not in out,'default privacy '+format)
    expect('private-canary' in run('check',private,'--format','json','--include-values'),'explicit plaintext disclosure')
    for flags in [('--format','bogus'),('--format','json','--format','text'),('--max-states','0'),('--max-states','1000001'),('--timeout-ms','0'),('--max-checks','2'),('--no-such-flag',),('--output',)]:
        run('check',path,*flags,code=2)
    bad=root/'invalid.json'
    variants=[]
    for mutate in [lambda h:h.update(schema=2),lambda h:h.update(unknown=True),lambda h:h['operations'][0].update(end=0),lambda h:h['operations'][0].update(args=[True]),lambda h:h['operations'][1].update(id=h['operations'][0]['id']),lambda h:h['operations'][1].update(start=h['operations'][0]['start']),lambda h:h['operations'][1].update(client=h['operations'][0]['client']),lambda h:h['operations'][0].update(result='unexpected-ack'),lambda h:h['operations'][0].update(kind='execute-file')]:
        h=copy.deepcopy(json.loads(path.read_text()));mutate(h);variants.append(json.dumps(h))
    variants+=['null','[]','{}','{"schema":1,"schema":1}',path.read_text()[:-3]]
    for content in variants:
        bad.write_text(content,encoding='utf-8');expect(run('check',bad,'--format','json',code=2)=='','invalid input is not a clear report')
    huge=root/'huge.json'
    with huge.open('wb') as stream:stream.truncate(8*1024*1024+1)
    run('check',huge,code=2)
    recorded=root/'recorded.json';run('record-demo',recorded);record=json.loads(recorded.read_text());expect(len(record['operations'])==32,'actual concurrent workload count');run('check',recorded,'--format','json')
print('Independent CLI checks passed:',checks)

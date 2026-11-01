"""Generate invented complete histories. No targets or services are contacted."""
from pathlib import Path
import json, sys

def op(name,start,end,kind,result,*args,object='main'):
    return dict(id=name,client='client-'+name,object=object,start=start,end=end,kind=kind,args=list(args),result=result)

def create(destination):
    destination=Path(destination);destination.mkdir(parents=True,exist_ok=False)
    samples={
        'overlap-register':('register',['0'],[op('write',0,3,'write',None,'1'),op('read',1,2,'read','0')],'Linearizable'),
        'stale-register':('register',['0'],[op('write',0,1,'write',None,'1'),op('read',2,3,'read','0')],'NotLinearizable'),
        'lost-update':('counter',['0'],[op('first',0,3,'add','1','1'),op('second',1,2,'add','1','1')],'NotLinearizable'),
        'double-cas':('cas-register',['0'],[op('first',0,3,'cas','true','0','1'),op('second',1,2,'cas','true','0','2')],'NotLinearizable'),
        'fifo-inversion':('queue',['first','second'],[op('take',0,1,'dequeue','second')],'NotLinearizable'),
        'valid-queue':('queue',[],[op('put',0,1,'enqueue',None,'payload'),op('take',2,3,'dequeue','payload')],'Linearizable'),
        'valid-stack':('stack',['bottom'],[op('push',0,1,'push',None,'top'),op('pop',2,3,'pop','top')],'Linearizable'),
        'valid-set':('set',[],[op('add',0,1,'add','true','element'),op('contains',2,3,'contains','true','element')],'Linearizable'),
        'valid-map':('map',[],[op('put',0,1,'put',None,'key','value'),op('get',2,3,'get','value','key')],'Linearizable'),
        'two-objects':('register',['0'],[op('write',0,5,'write',None,'1',object='A'),op('read-b',1,2,'read','0',object='B'),op('read-a',3,4,'read','0',object='A')],'Linearizable'),
        'shrink-me':('register',['0'],[op('write',0,1,'write',None,'1'),op('unrelated',2,3,'write',None,'0',object='B'),op('read',4,5,'read','0')],'NotLinearizable'),
        'empty':('register',['0'],[],'Linearizable'),
    }
    expected={}
    for name,(model,initial,ops,verdict) in samples.items():
        history=dict(schema=1,model=model,initial=initial,operations=ops)
        (destination/(name+'.json')).write_text(json.dumps(history,indent=2)+'\n',encoding='utf-8')
        expected[name]=dict(verdict=verdict,operations=len(ops))
    (destination/'expected.json').write_text(json.dumps(expected,indent=2)+'\n',encoding='utf-8')
    return expected

if __name__=='__main__':
    if len(sys.argv)!=2:raise SystemExit('usage: make_histories.py NEW_DIRECTORY')
    create(sys.argv[1])

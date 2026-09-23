"""Local-only API fixture. Never connects to the production backend."""
from http.server import ThreadingHTTPServer, BaseHTTPRequestHandler
from pathlib import Path
import base64, json, time
from urllib.parse import urlparse, parse_qs
ROOT = Path(__file__).resolve().parents[1]
def enc(value): return base64.urlsafe_b64encode(json.dumps(value).encode()).decode().rstrip('=')
class Handler(BaseHTTPRequestHandler):
    mode = 'normal'
    def log_message(self, *args): pass
    def send(self, data, status=200):
        payload = json.dumps(data).encode(); self.send_response(status); self.send_header('Content-Type','application/json'); self.send_header('Content-Length',str(len(payload))); self.end_headers(); self.wfile.write(payload)
    def do_POST(self):
        body = json.loads(self.rfile.read(int(self.headers.get('Content-Length',0))) or '{}')
        if self.path == '/__mode': Handler.mode = body['mode']; return self.send({'ok':True})
        if self.path.lower() == '/api/auth/login':
            if body.get('Password') != 'fixture-password': return self.send({'isSuccess':False,'errors':['E-posta veya şifre hatalı.']},401)
            token=enc({'alg':'HS256','typ':'JWT'})+'.'+enc({'sub':'102' if body.get('Email','').startswith('admin') else '101','email':'customer@example.test','given_name':'Test','role':'1' if body.get('Email','').startswith('admin') else '3','exp':int(time.time())+3600})+'.'+base64.urlsafe_b64encode(b'fixture-signature-not-for-production').decode().rstrip('=')
            return self.send({'isSuccess':True,'data':token})
        self.send({'isSuccess':True,'data':True})
    def do_GET(self):
        url=urlparse(self.path); p=url.path.lower()
        if p.startswith('/images/katalog/'):
            data=(ROOT/'wwwroot/images/brand/category-ring.jpg').read_bytes();self.send_response(200);self.send_header('Content-Type','image/jpeg');self.end_headers();self.wfile.write(data);return
        if Handler.mode == 'failed': return self.send({'isSuccess':False,'errors':['Fixture unavailable']},503)
        if Handler.mode == 'empty': return self.send({'isSuccess':True,'data':[],'count':0})
        data=[]
        if p == '/api/products':
            cats=['Yüzük','Kolye','Küpe','Bileklik']
            data=[{'id':i,'code':f'AT-{i:03}','name':cats[(i-1)%4]+' Seçkisi','categoryNames':[cats[(i-1)%4]],'categoryIds':[(i-1)%4+1],'calculatedPrice':250+i*25+(1000 if Handler.mode=='premium' else 0),'gram':3.2,'metalPurityName':'14 Ayar','liveGoldPrice':80,'laborMultiplier':1,'polishingCost':0,'imageName':f'item-{i}.jpg','images':[],'productStones':[],'productMetals':[]} for i in range(1,9)]
            q=parse_qs(url.query)
            if q.get('Code'): data=[x for x in data if q['Code'][0].lower() in x['code'].lower()]
            if q.get('CategoryId'): data=[x for x in data if int(q['CategoryId'][0]) in x['categoryIds']]
        elif p == '/api/category': data=[{'id':i,'name':n,'parentId':0,'orderIndex':i} for i,n in enumerate(['BİLEKLİK','GERDANLIK','KELEPÇE','KOLYE','KÜPE','YÜZÜK','SET'],1)]
        elif p == '/api/metaltype': data=[{'id':1,'name':'Sarı Altın'}]
        elif p == '/api/metalpurity': data=[{'id':1,'name':'14 Ayar','purityValue':0.585}]
        self.send({'isSuccess':True,'data':data,'count':len(data),'statusCode':200})
if __name__=='__main__': ThreadingHTTPServer(('127.0.0.1',5210),Handler).serve_forever()

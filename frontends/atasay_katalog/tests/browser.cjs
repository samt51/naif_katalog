// Run against the local fixture API and Atasay app; see README.md.
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const assert = require('node:assert/strict');
const fs = require('node:fs');
(async () => {
 const browser=await chromium.launch({headless:true,executablePath:process.env.CHROMIUM_PATH || undefined});
 const page=await browser.newPage({viewport:{width:1440,height:1000}});
 const errors=[]; page.on('pageerror',error=>errors.push(error.message));
 // Translation is optional; do not mutate or depend on Google's service in tests.
 await page.route('**/translate.google.com/**',route=>route.abort());
 const base=process.env.ATASAY_TEST_URL || 'http://localhost:5208';
 await page.request.post('http://127.0.0.1:5210/__mode',{data:{mode:'normal'}});
 async function loadImages() {
   await page.locator('img').evaluateAll(images=>images.forEach(img=>img.loading='eager'));
   await page.waitForFunction(()=>Array.from(document.images).every(img=>img.complete));
   assert.deepEqual(await page.locator('img').evaluateAll(images=>images.filter(img=>!img.naturalWidth).map(img=>img.src)),[], 'broken images');
 }

 await page.goto(base);await page.screenshot({path:'/tmp/atasay-login-desktop.png',fullPage:true});
 assert.match(await page.title(),/ATASAY/);
 assert.doesNotMatch(await page.locator('body').innerText(),/naif/i);
 await page.locator('#email').fill('customer@example.test');await page.locator('#password').fill('wrong');await page.locator('.login-submit').click();await page.locator('[role=alert]').waitFor();
 await page.locator('#email').fill('customer@example.test');await page.locator('#password').fill('fixture-password');await page.locator('.login-submit').click();await page.waitForURL('**/Home/Welcome');
 await page.locator('.atasay-products').waitFor();await loadImages();await page.screenshot({path:'/tmp/atasay-home-desktop.png',fullPage:true});
 assert.equal(await page.locator('.atasay-product').count(),8);
 const categoryPhotos=await page.locator('.atasay-category img').evaluateAll(images=>images.map(img=>({src:img.src,width:img.naturalWidth,fill:img.clientWidth/img.parentElement.clientWidth})));
 assert.equal(categoryPhotos.length,7);
 assert.equal(new Set(categoryPhotos.map(img=>img.src)).size,7);
 assert.ok(categoryPhotos.every(img=>img.width>=768 && img.fill>.98),'category photos must be sharp and fill their cards');
 const setLabel=page.locator('.atasay-category .notranslate', {hasText:'SET'});
 assert.equal(await setLabel.getAttribute('translate'),'no');
 await page.evaluate(()=>changeLanguage('en'));
 assert.equal(await setLabel.innerText(),'SET');
 await page.evaluate(()=>changeLanguage('tr'));
 await page.locator('#categories').screenshot({path:'/tmp/atasay-categories-desktop.png'});

 assert.doesNotMatch(await page.locator('body').innerText(),/naif/i);
 for (const route of ['/Admin/Dashboard','/Category/Create','/Definition/Index','/HomeContent/Index','/Orders/Index','/Orders/Detail','/Product/Index','/Product/IndexData','/Product/ClearCache']) {
   const response=await page.request.get(base+route);assert.equal(response.status(),404,route);
 }
 for(const path of ['../secret.jpg','https://example.com/a.jpg','a/../b.png','test.svg','/etc/passwd']) {
   const response=await page.request.get(base+'/CatalogMedia/Image?path='+encodeURIComponent(path));assert.equal(response.status(),400,path);
 }
 const photo=await page.request.get(base+'/CatalogMedia/Image?path=item-1.jpg');assert.equal(photo.status(),200);
 await page.goto(base+'/Home/Index');await page.locator('.premium-card-link').first().click();await page.waitForURL('**/Home/Detail/**');
 await page.locator('#addToCartBtn').click();await page.locator('#catalogOffcanvas.show').waitFor();assert.equal(await page.locator('#catalogCount').innerText(),'1');
 await page.screenshot({path:'/tmp/atasay-cart-desktop.png',fullPage:true});
 await page.locator('#catalogOffcanvas [data-bs-dismiss=offcanvas]').click();
 await page.goto(base+'/Home/Welcome');
 await page.setViewportSize({width:390,height:844});await page.screenshot({path:'/tmp/atasay-home-mobile.png',fullPage:true});
 assert.ok(await page.evaluate(()=>document.documentElement.scrollWidth <= innerWidth),'mobile homepage overflows');
 await page.goto(base+'/Home/Index');assert.ok(await page.evaluate(()=>document.documentElement.scrollWidth <= innerWidth),'mobile catalog overflows');
 await page.screenshot({path:'/tmp/atasay-catalog-mobile.png',fullPage:true});
 await page.goto(base+'/Account/Logout');await page.screenshot({path:'/tmp/atasay-login-mobile.png',fullPage:true});
 assert.ok(await page.evaluate(()=>document.documentElement.scrollWidth <= innerWidth),'mobile login overflows');
 await page.locator('#email').fill('admin@example.test');await page.locator('#password').fill('fixture-password');await page.locator('.login-submit').click();await page.waitForURL('**/Home/Welcome');assert.equal(await page.locator('a[href*="/Admin"]').count(),0);
 const cookies=await page.context().cookies();
 assert.ok(cookies.some(c=>c.name==='Atasay.ApiToken'),'dedicated Atasay API token cookie');
 await page.request.post('http://127.0.0.1:5210/__mode',{data:{mode:'premium'}});
 await page.goto(base+'/Home/Detail/1');
 assert.equal(await page.evaluate(()=>basePrice),1275, 'customer pricing cache must not cross accounts');
 await page.request.post('http://127.0.0.1:5210/__mode',{data:{mode:'failed'}});await page.goto(base+'/Home/Welcome');await page.locator('.atasay-status').first().waitFor();assert.match(await page.locator('.atasay-status').first().innerText(),/yüklenemiyor/);
 await page.request.post('http://127.0.0.1:5210/__mode',{data:{mode:'empty'}});await page.reload();assert.match(await page.locator('.atasay-status').first().innerText(),/Yeni ürünler/);
 await page.request.post('http://127.0.0.1:5210/__mode',{data:{mode:'normal'}});
 assert.deepEqual(errors,[], 'Browser JavaScript errors');
 await browser.close();console.log('PASS: branding, login/error/admin-role, admin route exclusion, image proxy, catalog/detail/cart, mobile layouts, API empty/error states.');
})().catch(error=>{console.error(error);process.exit(1)});

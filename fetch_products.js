process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
fetch('https://localhost:3434/api/Products')
  .then(res => res.json())
  .then(data => require('fs').writeFileSync('products.json', JSON.stringify(data, null, 2)))
  .catch(err => console.error(err));

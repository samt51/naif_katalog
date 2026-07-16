process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
fetch('https://localhost:3434/api/ExchangeRate/HasAltin')
  .then(res => res.json())
  .then(data => console.log(JSON.stringify(data)))
  .catch(err => console.error(err));

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
fetch('https://localhost:3434/api/Users')
  .then(res => res.json())
  .then(data => require('fs').writeFileSync('users.json', JSON.stringify(data, null, 2)))
  .catch(err => console.error(err));

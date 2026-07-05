const fs = require('fs');
const content = fs.readFileSync('d:/prj/naif_katalog/Views/Admin/Products.cshtml', 'utf8');

const regex = /<script>([\s\S]*?)<\/script>/g;
let match;
let count = 1;
while ((match = regex.exec(content)) !== null) {
    let scriptContent = match[1];
    // Strip all @Html.Raw entirely for syntax checking
    scriptContent = scriptContent.replace(/@Html\.Raw[^\n]+/g, '[]');
    scriptContent = scriptContent.replace(/'@apiAddress'/g, '"http://localhost"');
    scriptContent = scriptContent.replace(/`@apiAddress/g, '`http://localhost');
    
    fs.writeFileSync(`d:/prj/naif_katalog/scratch${count}.js`, scriptContent);
    count++;
}

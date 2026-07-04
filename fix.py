import re

# Fix Products.cshtml
path_prod = r'd:\prj\naif_katalog\Views\Admin\Products.cshtml'
with open(path_prod, 'r', encoding='utf-8-sig') as f:
    content = f.read()

new_prod_func = '''    function deleteProduct(id) {
        Swal.fire({
            title: 'Emin misiniz?',
            text: 'Bu ürünü silmek istediğinize emin misiniz? (Ürün kalıcı olarak değil, sistemde gizlenecek şekilde işaretlenecektir)',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#ef4444',
            cancelButtonColor: '#6b7280',
            confirmButtonText: 'Evet, Sil!',
            cancelButtonText: 'İptal'
        }).then((result) => {
            if (result.isConfirmed) {
                fetch(@apiAddress/api/Products/, {
                    method: 'DELETE'
                })
                .then(res => res.json())
                .then(async data => {
                    if(data.isSuccess || data.success) {
                        await fetch('/Product/ClearCache', { method: 'POST' });
                        Swal.fire(
                            'Silindi!',
                            'Ürün başarıyla silindi.',
                            'success'
                        ).then(() => {
                            location.reload();
                        });
                    } else {
                        Swal.fire('Hata!', data.message || 'Ürün silinirken bir hata oluştu.', 'error');
                    }
                })
                .catch(err => {
                    console.error(err);
                    Swal.fire('Hata!', 'Sunucuya bağlanılamadı.', 'error');
                });
            }
        });
    }'''

content = re.sub(r'    function deleteProduct\(id\) \{.*?\n    \}', new_prod_func, content, flags=re.DOTALL)

with open(path_prod, 'w', encoding='utf-8-sig') as f:
    f.write(content)

# Fix Categories.cshtml
path_cat = r'd:\prj\naif_katalog\Views\Admin\Categories.cshtml'
with open(path_cat, 'r', encoding='utf-8-sig') as f:
    content = f.read()

new_cat_func = '''    function deleteCategory(id) {
        Swal.fire({
            title: 'Emin misiniz?',
            text: 'Bu kategoriyi silmek istediğinize emin misiniz?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Evet, Sil!',
            cancelButtonText: 'İptal'
        }).then((result) => {
            if (result.isConfirmed) {
                fetch(@apiAddress/api/Category/, {
                    method: 'DELETE'
                })
                .then(res => res.json())
                .then(data => {
                    if(data.isSuccess || data.success) {
                        Swal.fire(
                            'Silindi!',
                            'Kategori başarıyla silindi.',
                            'success'
                        ).then(() => {
                            location.reload();
                        });
                    } else {
                        Swal.fire('Hata!', data.message || 'Kategori silinirken bir hata oluştu.', 'error');
                    }
                })
                .catch(err => {
                    console.error(err);
                    Swal.fire('Hata!', 'Sunucuya bağlanılamadı.', 'error');
                });
            }
        });
    }'''

content = re.sub(r'    function deleteCategory\(id\) \{.*?\n    \}', new_cat_func, content, flags=re.DOTALL)

if '@using Microsoft.Extensions.Configuration' not in content:
    header_repl = '''@model List<naif_katalog.Models.CategoryDto>
@using Microsoft.Extensions.Configuration
@inject IConfiguration Configuration
@{
    var apiAddress = Configuration["ApiAdress"] ?? "https://apib2b.naifjewellery.com/";
    if (apiAddress.EndsWith("/")) apiAddress = apiAddress.Substring(0, apiAddress.Length - 1);'''
    content = re.sub(r'@model List<naif_katalog\.Models\.CategoryDto>\s*@\{', header_repl, content)

with open(path_cat, 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Done")

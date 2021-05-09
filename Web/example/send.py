import requests

# service
serviceURL = 'http://localhost:8080/html2pdf'
serviceUsername = 'user'
servicePassword = 'pass'

with open('main.html', 'r') as f:
    html = f.read()

with open('style.css', 'r') as f:
    css = f.read()

with open('header.html', 'r') as f:
    header = f.read()

with open('footer.html', 'r') as f:
    footer = f.read()

with open('cover.html', 'r') as f:
    cover = f.read()

req = {
    # 'bodyUrl': 'http://kmacademy.ir/html2pdf/index.html',
    'bodyUrl': None,
    'bodyHtml': html,
    #'bodyHtml': None,
    'bodyStyle': css,
    #'bodyStyle': None,
    'header': header,
    'footer': footer,
    'cover': cover,
    'paperSize': 'A4',
    'margin': '5% 5% 5% 5%',
    'fontFamilyRTL': None,
    'fontFamilyLTR': None,
    'fontSizeRTL': None,
    'fontSizeLTR': None,
    'defaultDirection': None,
    'pageNumberPosition': None,
    'password': None,
}

session = requests.Session()
session.auth = (serviceUsername, servicePassword)
res = session.post(serviceURL, json=req)
print('** status = {} **\n'.format(res.status_code))
if res.status_code == 200:
    with open('output.pdf', 'wb') as f:
        f.write(res.content)
        print('pdf saved in \'output.pdf\' file')
else:
    if res.status_code == 401:
        print('username or password is not correct')
    else:
        for key in res.json()['errors']:
            print(key + ':')
            for err in res.json()['errors'][key]:
                print('    ' + err)

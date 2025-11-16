### Ejecución

```bash
> docker compose down -v && docker compose up --build
```

## Documentación de endpoints

- Swagger: https://localhost:8443/api/songs/swagger/index.html
- Contrato WSDL (SOAP): https://localhost:8443/api/auth.asmx?wsdl

Puedes probar uno de los endopints xml mediante:

```bash
> curl --insecure --location 'https://localhost:8443/api/auth.asmx' \
--header 'Content-Type: text/xml; charset=utf-8' \
--header 'SOAPAction: http://tempuri.org/ISoapAuthService/Register' \
--data '<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/" xmlns:ser="http://schemas.datacontract.org/2004/07/service.src.auth.controllers.soap">
  <soap:Body>
    <tem:Register>
      <tem:request>
        <ser:Email>test@user.com</ser:Email>
        <ser:MasterPassword>password123</ser:MasterPassword>
      </tem:request>
    </tem:Register>
  </soap:Body>
</soap:Envelope>
'
```

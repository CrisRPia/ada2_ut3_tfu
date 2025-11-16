using System.ServiceModel;
namespace service.src.auth.controllers.soap;

[ServiceContract]
public interface ISoapAuthService
{
    [OperationContract]
    Task<SoapTokenResponse> Register(SoapRegisterRequest request);

    [OperationContract]
    Task<SoapTokenResponse> Login(SoapLoginRequest request);
}

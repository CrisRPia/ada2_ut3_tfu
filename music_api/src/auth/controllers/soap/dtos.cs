using System.Runtime.Serialization;

namespace service.src.auth.controllers.soap;

[DataContract]
public class SoapRegisterRequest
{
    [DataMember]
    public required string Email { get; set; }

    [DataMember]
    public required string MasterPassword { get; set; }
}

[DataContract]
public class SoapLoginRequest
{
    [DataMember]
    public required string Email { get; set; }

    [DataMember]
    public required string MasterPassword { get; set; }
}

[DataContract]
public class SoapTokenResponse
{
    [DataMember]
    public required string Token { get; set; }
}

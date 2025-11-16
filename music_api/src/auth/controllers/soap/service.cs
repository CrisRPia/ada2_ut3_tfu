using System.ServiceModel;
using Isopoh.Cryptography.Argon2;
using service.src.auth.services;
using service.src.common.models;

namespace service.src.auth.controllers.soap;

public class SoapAuthService(ApplicationDbContext context, JwtTokenGenerator tokenGenerator) : ISoapAuthService
{
    public async Task<SoapTokenResponse> Login(SoapLoginRequest request)
    {
        var user = context.Users.FirstOrDefault(u => u.Email == request.Email);

        if (user == null || !Argon2.Verify(user.PasswordHash, request.MasterPassword))
        {
            throw new FaultException("Invalid credentials.");
        }

        var token = tokenGenerator.GenerateToken(user);
        return new SoapTokenResponse { Token = token };
    }

    public async Task<SoapTokenResponse> Register(SoapRegisterRequest request)
    {
        if (context.Users.Any(u => u.Email == request.Email))
        {
            throw new FaultException("User with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = Argon2.Hash(request.MasterPassword),
        };

        context.Users.Add(user);
        context.SaveChanges();

        var token = tokenGenerator.GenerateToken(user);
        return new SoapTokenResponse { Token = token };
    }
}

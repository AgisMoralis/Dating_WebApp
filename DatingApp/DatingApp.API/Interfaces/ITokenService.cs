using System;

namespace DatingApp.API.Interfaces;

public interface ITokenService
{
    Task<string> CreateTokenAsync(Entities.Member user);
}

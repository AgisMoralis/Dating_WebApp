using System;

namespace DatingApp.API.Interfaces;

public interface ITokenService
{
    string CreateToken(Entities.Member user);
}
